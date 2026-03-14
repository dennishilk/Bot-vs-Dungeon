using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StressAdaptiveLearningMode
{
    SharedLearningPool,
    IndividualLearning
}

public class StressTestManager : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private DeathHeatmapManager heatmapManager;
    [SerializeField] private StressTestReport stressTestReport;
    [SerializeField] private int maxBots = 50;
    [SerializeField] private int defaultBotCount = 20;
    [SerializeField] private float spawnInterval = 0.12f;
    [SerializeField] private float maxSimulationTime = 90f;
    [SerializeField] private float pathUpdateFrequency = 0.1f;
    [SerializeField] private bool colorByPersonality = true;
    [SerializeField] private AdaptiveLearningManager adaptiveLearningManager;
    [SerializeField] private bool adaptiveStressMode = true;
    [SerializeField] private StressAdaptiveLearningMode stressAdaptiveLearningMode = StressAdaptiveLearningMode.SharedLearningPool;

    private readonly List<TrackedStressBot> _activeBots = new();
    private readonly Dictionary<Vector2Int, int> _localDeaths = new();
    private readonly Dictionary<string, int> _deathCauses = new();
    private readonly HashSet<string> _learningIdsUsed = new();

    private int _targetBotCount;
    private int _completed;
    private int _survived;
    private int _died;
    private float _accumulatedSurvivalTime;
    private float _accumulatedPathLength;
    private bool _running;
    private Coroutine _runRoutine;
    private string _stressDungeonId;

    public bool IsRunning => _running;

    public void StartStressTest10() => StartStressTest(10);
    public void StartStressTest20() => StartStressTest(20);
    public void StartStressTest50() => StartStressTest(50);

    public void StartStressTest(int count)
    {
        if (_running)
        {
            return;
        }

        _targetBotCount = Mathf.Clamp(count <= 0 ? defaultBotCount : count, 1, maxBots);
        _runRoutine = StartCoroutine(RunStressTestRoutine());
    }

    public void StopStressTest()
    {
        if (!_running)
        {
            return;
        }

        if (_runRoutine != null)
        {
            StopCoroutine(_runRoutine);
            _runRoutine = null;
        }

        FinishAndReport();
    }

    private IEnumerator RunStressTestRoutine()
    {
        ResetStats();
        _running = true;

        if (!arenaManager.TryFindTile(TileType.Start, out Vector2Int startPos) ||
            !arenaManager.TryFindTile(TileType.Goal, out Vector2Int goalPos))
        {
            _running = false;
            yield break;
        }

        _stressDungeonId = adaptiveLearningManager != null
            ? adaptiveLearningManager.ComputeDungeonIdentifier(arenaManager, "Stress")
            : "stress_runtime";

        if (adaptiveStressMode && stressAdaptiveLearningMode == StressAdaptiveLearningMode.SharedLearningPool)
        {
            adaptiveLearningManager?.ClearCurrentDungeonLearning(_stressDungeonId);
        }

        float elapsed = 0f;
        int spawned = 0;

        while (spawned < _targetBotCount && elapsed < maxSimulationTime)
        {
            SpawnBot(startPos, goalPos, spawned);
            spawned++;
            elapsed += spawnInterval;
            yield return new WaitForSeconds(spawnInterval);
        }

        while (_completed < _targetBotCount && elapsed < maxSimulationTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishAndReport();
    }

    private void SpawnBot(Vector2Int startPos, Vector2Int goalPos, int index)
    {
        GameObject botObj = Instantiate(botPrefab, new Vector3(startPos.x, 0.6f, startPos.y), Quaternion.identity);
        botObj.name = $"StressBot_{index + 1}";

        BotAgent agent = botObj.GetComponent<BotAgent>();
        BotHealth health = botObj.GetComponent<BotHealth>();
        if (agent == null || health == null)
        {
            Destroy(botObj);
            return;
        }

        BotPersonality personality = (BotPersonality)(index % Enum.GetValues(typeof(BotPersonality)).Length);
        agent.SetPersonality(personality);

        string botDungeonId = stressAdaptiveLearningMode == StressAdaptiveLearningMode.IndividualLearning
            ? $"{_stressDungeonId}_bot_{index}"
            : _stressDungeonId;

        _learningIdsUsed.Add(botDungeonId);
        agent.Initialize(arenaManager, null, goalPos, adaptiveLearningManager, botDungeonId, adaptiveStressMode);
        agent.ConfigureUpdateFrequency(pathUpdateFrequency);

        if (colorByPersonality)
        {
            ApplyBotColor(botObj, personality);
        }

        TrackedStressBot tracked = new()
        {
            bot = botObj,
            agent = agent,
            health = health,
            spawnTime = Time.time,
            finished = false
        };

        health.OnBotDied += _ => OnBotDied(tracked);
        agent.OnGoalReached += _ => OnBotSurvived(tracked);
        _activeBots.Add(tracked);
    }

    private void OnBotSurvived(TrackedStressBot tracked)
    {
        if (tracked == null || tracked.finished)
        {
            return;
        }

        tracked.finished = true;
        _completed++;
        _survived++;
        ReplayEventStream.Emit(ReplayEventType.GoalReached, tracked.bot != null ? tracked.bot.transform.position : Vector3.zero, "StressGoal", 1f, "Stress bot survived");
        _accumulatedSurvivalTime += Mathf.Max(0f, Time.time - tracked.spawnTime);
        _accumulatedPathLength += tracked.agent != null ? tracked.agent.TraversedPathLength : 0f;

        if (tracked.agent != null && adaptiveLearningManager != null)
        {
            adaptiveLearningManager.RecordRunOutcome(tracked.agent.DungeonId, true, adaptiveStressMode);
            adaptiveLearningManager.RecordSuccessPath(tracked.agent.DungeonId, tracked.agent.TraversedTiles, tracked.agent.GetPersonality());
        }
    }

    private void OnBotDied(TrackedStressBot tracked)
    {
        if (tracked == null || tracked.finished)
        {
            return;
        }

        tracked.finished = true;
        _completed++;
        _died++;
        ReplayEventStream.Emit(ReplayEventType.BotDeath, tracked.bot != null ? tracked.bot.transform.position : Vector3.zero, "StressDeath", 1f, "Stress bot died");
        _accumulatedSurvivalTime += Mathf.Max(0f, Time.time - tracked.spawnTime);
        _accumulatedPathLength += tracked.agent != null ? tracked.agent.TraversedPathLength : 0f;

        if (tracked.agent != null)
        {
            Vector2Int deathTile = tracked.agent.CurrentTilePosition;
            _localDeaths.TryGetValue(deathTile, out int count);
            _localDeaths[deathTile] = count + 1;
            heatmapManager?.RecordDeathAtTile(deathTile);

            if (adaptiveLearningManager != null)
            {
                adaptiveLearningManager.RecordRunOutcome(tracked.agent.DungeonId, false, adaptiveStressMode);
                adaptiveLearningManager.RecordDeath(tracked.agent.DungeonId, deathTile, tracked.agent.GetPersonality());
            }
        }

        string cause = tracked.health != null ? tracked.health.LastDamageSource.ToString() : "Unknown";
        _deathCauses.TryGetValue(cause, out int causeCount);
        _deathCauses[cause] = causeCount + 1;
    }

    private void FinishAndReport()
    {
        _running = false;

        foreach (TrackedStressBot tracked in _activeBots)
        {
            if (tracked.bot != null)
            {
                Destroy(tracked.bot);
            }
        }

        StressTestResultData result = new()
        {
            botsSpawned = _targetBotCount,
            botsSurvived = _survived,
            botsDied = _died,
            averageSurvivalTime = _targetBotCount > 0 ? _accumulatedSurvivalTime / _targetBotCount : 0f,
            averagePathLength = _targetBotCount > 0 ? _accumulatedPathLength / _targetBotCount : 0f,
            mostLethalTile = FindMostLethalTile(),
            mostCommonCauseOfDeath = FindMostCommonCause(),
            adaptiveModeUsed = adaptiveStressMode,
            adaptiveLearningPool = stressAdaptiveLearningMode.ToString(),
            learningSummary = adaptiveLearningManager != null ? adaptiveLearningManager.BuildAggregateSummary(_learningIdsUsed) : null
        };

        stressTestReport?.Show(result);
        _activeBots.Clear();
        _runRoutine = null;
    }

    private Vector2Int FindMostLethalTile()
    {
        int max = 0;
        Vector2Int tile = Vector2Int.zero;
        foreach (KeyValuePair<Vector2Int, int> pair in _localDeaths)
        {
            if (pair.Value <= max)
            {
                continue;
            }

            max = pair.Value;
            tile = pair.Key;
        }

        return tile;
    }

    private string FindMostCommonCause()
    {
        int max = 0;
        string cause = "None";
        foreach (KeyValuePair<string, int> pair in _deathCauses)
        {
            if (pair.Value <= max)
            {
                continue;
            }

            max = pair.Value;
            cause = pair.Key;
        }

        return cause;
    }

    private void ResetStats()
    {
        _completed = 0;
        _survived = 0;
        _died = 0;
        _accumulatedSurvivalTime = 0f;
        _accumulatedPathLength = 0f;
        _localDeaths.Clear();
        _deathCauses.Clear();
        _learningIdsUsed.Clear();
    }

    private static void ApplyBotColor(GameObject botObj, BotPersonality personality)
    {
        Color color = personality switch
        {
            BotPersonality.Careful => new Color(0.35f, 0.55f, 1f),
            BotPersonality.Reckless => new Color(1f, 0.35f, 0.35f),
            _ => Color.white
        };

        foreach (Renderer r in botObj.GetComponentsInChildren<Renderer>())
        {
            if (r.material != null)
            {
                r.material.color = color;
            }
        }
    }

    [Serializable]
    private class TrackedStressBot
    {
        public GameObject bot;
        public BotAgent agent;
        public BotHealth health;
        public float spawnTime;
        public bool finished;
    }
}
