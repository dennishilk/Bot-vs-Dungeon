using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum AdaptiveRunMode
{
    Fresh,
    Adaptive
}

[Serializable]
public class AdaptiveLearningSummaryData
{
    public string dungeonId;
    public int observedRuns;
    public int learnedDangerousTiles;
    public Vector2Int mostLethalTile;
    public Vector2Int mostAvoidedTile;
    public float preAdaptationSuccessRate;
    public float postAdaptationSuccessRate;
    public float adaptiveImprovement;
}

public class AdaptiveLearningManager : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private AdaptiveRunMode runMode = AdaptiveRunMode.Adaptive;
    [SerializeField] private bool allowFreshRunsToRecordLearning;
    [SerializeField] private bool persistenceEnabled;

    [Header("Learning Weights")]
    [SerializeField] private float baseDeathLearningWeight = 3f;
    [SerializeField] private float baseDamageLearningWeight = 0.24f;
    [SerializeField] private float successPathReductionWeight = 0.16f;
    [SerializeField] private float neighborSpreadFactor = 0.45f;
    [SerializeField] private float forgetfulnessRate = 0.02f;
    [SerializeField] private float repeatedFailureAmplification = 0.35f;

    [Header("Personality Learning Multipliers")]
    [SerializeField] private float carefulLearningMultiplier = 1.35f;
    [SerializeField] private float balancedLearningMultiplier = 1f;
    [SerializeField] private float recklessLearningMultiplier = 0.55f;
    [SerializeField] private float panicLearningMultiplier = 1.65f;

    private readonly Dictionary<string, DungeonLearningMemory> _memoryByDungeon = new();
    private readonly Dictionary<string, int> _freshRunOutcomes = new();
    private readonly Dictionary<string, int> _adaptiveRunOutcomes = new();

    public AdaptiveRunMode CurrentRunMode => runMode;
    public bool IsAdaptiveModeEnabled => runMode == AdaptiveRunMode.Adaptive;

    public void SetRunMode(bool adaptiveEnabled)
    {
        runMode = adaptiveEnabled ? AdaptiveRunMode.Adaptive : AdaptiveRunMode.Fresh;
    }

    public string ComputeDungeonIdentifier(ArenaManager arenaManager, string saveName = null)
    {
        if (arenaManager == null)
        {
            return string.Empty;
        }

        StringBuilder sb = new();
        if (!string.IsNullOrWhiteSpace(saveName))
        {
            sb.Append(saveName.Trim());
            sb.Append('|');
        }

        foreach (KeyValuePair<Vector2Int, ArenaManager.TileEntry> tile in arenaManager.GetAllTiles().OrderBy(t => t.Key.x).ThenBy(t => t.Key.y))
        {
            sb.Append(tile.Key.x).Append(',').Append(tile.Key.y).Append(':').Append((int)tile.Value.tileType).Append(';');
        }

        int hash = Animator.StringToHash(sb.ToString());
        string prefix = string.IsNullOrWhiteSpace(saveName) ? "Dungeon" : saveName.Trim();
        return $"{prefix}_{Mathf.Abs(hash)}";
    }

    public BotLearningProfile GetLearningProfile(BotPersonality personality)
    {
        return BotLearningProfile.FromPersonality(
            personality,
            carefulLearningMultiplier,
            balancedLearningMultiplier,
            recklessLearningMultiplier,
            panicLearningMultiplier,
            forgetfulnessRate);
    }

    public float GetLearnedDangerModifier(string dungeonId, Vector2Int tile, BotLearningProfile profile)
    {
        if (!IsAdaptiveModeEnabled || string.IsNullOrWhiteSpace(dungeonId))
        {
            return 0f;
        }

        if (!_memoryByDungeon.TryGetValue(dungeonId, out DungeonLearningMemory memory) ||
            !memory.TryGetTile(tile, out AdaptiveTileMemory tileMemory))
        {
            return 0f;
        }

        return Mathf.Max(0f, tileMemory.learnedDangerModifier * profile.riskToleranceModifier);
    }

    public void RecordDamage(string dungeonId, Vector2Int tile, float damage, BotPersonality personality)
    {
        if (!ShouldRecordLearning())
        {
            return;
        }

        DungeonLearningMemory memory = GetOrCreateMemory(dungeonId);
        BotLearningProfile profile = GetLearningProfile(personality);
        AdaptiveTileMemory tileMemory = memory.GetOrCreateTile(tile);
        tileMemory.damageCount++;
        tileMemory.totalDamageTaken += Mathf.Max(0f, damage);

        float scaledDelta = baseDamageLearningWeight * profile.EffectiveLearningMultiplier * Mathf.Max(0.1f, damage * 0.1f);
        tileMemory.learnedDangerModifier += scaledDelta;
    }

    public void RecordDeath(string dungeonId, Vector2Int tile, BotPersonality personality)
    {
        if (!ShouldRecordLearning())
        {
            return;
        }

        DungeonLearningMemory memory = GetOrCreateMemory(dungeonId);
        BotLearningProfile profile = GetLearningProfile(personality);
        memory.totalDeaths++;

        ApplyDeathToTile(memory, tile, profile, 1f);

        Vector2Int[] neighbors =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        foreach (Vector2Int offset in neighbors)
        {
            ApplyDeathToTile(memory, tile + offset, profile, neighborSpreadFactor);
        }
    }

    public void RecordSuccessPath(string dungeonId, IReadOnlyList<Vector2Int> path, BotPersonality personality)
    {
        if (!ShouldRecordLearning() || path == null || path.Count == 0)
        {
            return;
        }

        DungeonLearningMemory memory = GetOrCreateMemory(dungeonId);
        BotLearningProfile profile = GetLearningProfile(personality);
        memory.totalSuccesses++;

        foreach (Vector2Int tile in path)
        {
            AdaptiveTileMemory tileMemory = memory.GetOrCreateTile(tile);
            tileMemory.successfulPassCount++;
            tileMemory.learnedDangerModifier = Mathf.Max(
                0f,
                tileMemory.learnedDangerModifier - (successPathReductionWeight * profile.EffectiveLearningMultiplier));
        }
    }

    public void RecordPathAvoidance(string dungeonId, IReadOnlyList<Vector2Int> path)
    {
        if (!ShouldRecordLearning() || path == null)
        {
            return;
        }

        DungeonLearningMemory memory = GetOrCreateMemory(dungeonId);
        foreach (Vector2Int tile in path)
        {
            AdaptiveTileMemory tileMemory = memory.GetOrCreateTile(tile);
            if (tileMemory.learnedDangerModifier > 0.35f)
            {
                tileMemory.avoidedCount++;
            }
        }
    }

    public void RecordRunOutcome(string dungeonId, bool survived, bool adaptiveModeUsed)
    {
        if (string.IsNullOrWhiteSpace(dungeonId))
        {
            return;
        }

        DungeonLearningMemory memory = GetOrCreateMemory(dungeonId);
        memory.totalRunsObserved++;

        if (adaptiveModeUsed)
        {
            _adaptiveRunOutcomes.TryGetValue(dungeonId, out int adaptiveCount);
            _adaptiveRunOutcomes[dungeonId] = adaptiveCount + (survived ? 1 : 0);
        }
        else
        {
            _freshRunOutcomes.TryGetValue(dungeonId, out int freshCount);
            _freshRunOutcomes[dungeonId] = freshCount + (survived ? 1 : 0);
        }

        ApplyForgetfulness(memory);
    }

    public AdaptiveLearningSummaryData BuildSummary(string dungeonId)
    {
        if (!_memoryByDungeon.TryGetValue(dungeonId, out DungeonLearningMemory memory))
        {
            return new AdaptiveLearningSummaryData { dungeonId = dungeonId };
        }

        int freshRuns = Mathf.Max(1, memory.totalRunsObserved / 2);
        int adaptiveRuns = Mathf.Max(1, memory.totalRunsObserved - freshRuns);
        float freshRate = _freshRunOutcomes.TryGetValue(dungeonId, out int freshSuccesses) ? (float)freshSuccesses / freshRuns : 0f;
        float adaptiveRate = _adaptiveRunOutcomes.TryGetValue(dungeonId, out int adaptiveSuccesses) ? (float)adaptiveSuccesses / adaptiveRuns : 0f;

        return new AdaptiveLearningSummaryData
        {
            dungeonId = dungeonId,
            observedRuns = memory.totalRunsObserved,
            learnedDangerousTiles = memory.LearnedDangerousTiles(1f),
            mostLethalTile = memory.GetMostLethalTile()?.Position ?? Vector2Int.zero,
            mostAvoidedTile = memory.GetMostAvoidedTile()?.Position ?? Vector2Int.zero,
            preAdaptationSuccessRate = freshRate,
            postAdaptationSuccessRate = adaptiveRate,
            adaptiveImprovement = adaptiveRate - freshRate
        };
    }

    public IEnumerable<AdaptiveTileMemory> GetTileMemories(string dungeonId)
    {
        if (_memoryByDungeon.TryGetValue(dungeonId, out DungeonLearningMemory memory))
        {
            return memory.tileMemory;
        }

        return Array.Empty<AdaptiveTileMemory>();
    }

    public void ClearCurrentDungeonLearning(string dungeonId)
    {
        if (string.IsNullOrWhiteSpace(dungeonId))
        {
            return;
        }

        _memoryByDungeon.Remove(dungeonId);
        _freshRunOutcomes.Remove(dungeonId);
        _adaptiveRunOutcomes.Remove(dungeonId);
    }

    public void ClearAllLearning()
    {
        _memoryByDungeon.Clear();
        _freshRunOutcomes.Clear();
        _adaptiveRunOutcomes.Clear();
    }

    private DungeonLearningMemory GetOrCreateMemory(string dungeonId)
    {
        if (string.IsNullOrWhiteSpace(dungeonId))
        {
            dungeonId = "runtime_dungeon";
        }

        if (_memoryByDungeon.TryGetValue(dungeonId, out DungeonLearningMemory memory))
        {
            return memory;
        }

        memory = new DungeonLearningMemory();
        memory.Initialize(dungeonId);
        _memoryByDungeon[dungeonId] = memory;
        return memory;
    }

    private void ApplyDeathToTile(DungeonLearningMemory memory, Vector2Int tile, BotLearningProfile profile, float spread)
    {
        AdaptiveTileMemory tileMemory = memory.GetOrCreateTile(tile);
        tileMemory.deathCount++;
        float amplification = 1f + (tileMemory.deathCount * repeatedFailureAmplification);
        tileMemory.learnedDangerModifier += baseDeathLearningWeight * profile.EffectiveLearningMultiplier * spread * amplification;
    }

    private void ApplyForgetfulness(DungeonLearningMemory memory)
    {
        float retained = Mathf.Clamp01(1f - forgetfulnessRate);
        foreach (AdaptiveTileMemory tile in memory.tileMemory)
        {
            tile.learnedDangerModifier *= retained;
        }
    }

    private bool ShouldRecordLearning()
    {
        return IsAdaptiveModeEnabled || allowFreshRunsToRecordLearning || persistenceEnabled;
    }
}
