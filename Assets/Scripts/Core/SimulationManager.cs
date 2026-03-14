using System;
using System.Collections;
using BotVsDungeon.UI;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private BuildModeController buildModeController;
    [SerializeField] private UIController uiController;
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private GoalFeedback goalFeedback;
    [SerializeField] private float cleanupDelay = 0.5f;
    [SerializeField] private BotPersonality defaultPersonality = BotPersonality.Balanced;
    [SerializeField] private AdaptiveLearningManager adaptiveLearningManager;
    [SerializeField] private bool adaptiveModeEnabled = true;

    private BotAgent _activeBot;
    private float _simulationTime;
    private bool _isPaused;
    private BotPersonality _activeRunPersonality;
    private bool _updateUiForActiveRun = true;
    private string _activeDungeonId;

    public bool IsSimulationRunning { get; private set; }
    public float SimulationTime => _simulationTime;
    public BotAgent ActiveBot => _activeBot;
    public bool AdaptiveModeEnabled => adaptiveModeEnabled;
    public string ActiveDungeonId => _activeDungeonId;

    public event Action<BotAgent> OnBotSpawned;
    public event Action<RunResult> OnRunFinished;

    private void Update()
    {
        if (!IsSimulationRunning || _isPaused)
        {
            return;
        }

        _simulationTime += Time.deltaTime;
    }

    public void StartSimulation()
    {
        StartSimulationWithPersonality(defaultPersonality, true);
    }

    public bool StartSimulationWithPersonality(BotPersonality personality, bool updateUi)
    {
        if (IsSimulationRunning)
        {
            return false;
        }

        if (!arenaManager.TryFindTile(TileType.Start, out Vector2Int startPos) ||
            !arenaManager.TryFindTile(TileType.Goal, out Vector2Int goalPos))
        {
            if (updateUi)
            {
                uiController.SetStatus("Place both START and GOAL.");
            }
            return false;
        }

        _activeDungeonId = adaptiveLearningManager != null
            ? adaptiveLearningManager.ComputeDungeonIdentifier(arenaManager)
            : "runtime_dungeon";

        buildModeController.SetBuildMode(false);
        IsSimulationRunning = true;
        _isPaused = false;
        _simulationTime = 0f;
        _activeRunPersonality = personality;
        _updateUiForActiveRun = updateUi;

        if (updateUi)
        {
            uiController.ClearResult();
            uiController.SetStatus($"Simulation running ({personality})...");
        }

        Vector3 spawnPos = new(startPos.x, 0.6f, startPos.y);
        GameObject botObj = Instantiate(botPrefab, spawnPos, Quaternion.identity);

        _activeBot = botObj.GetComponent<BotAgent>();
        BotHealth health = botObj.GetComponent<BotHealth>();

        _activeBot.SetPersonality(personality);
        _activeBot.Initialize(arenaManager, this, goalPos, adaptiveLearningManager, _activeDungeonId, adaptiveModeEnabled);
        health.OnBotDied += OnBotDied;

        EventLogger.Instance?.Log($"Bot spawned ({personality})");
        OnBotSpawned?.Invoke(_activeBot);

        AudioManager.Instance?.PlayBotEvent(BotAudioEvent.Spawn, personality, spawnPos);
        AudioManager.Instance?.PlayUISound(SoundCue.SimulationStart);
        AudioManager.Instance?.PlayMusicTrack(MusicTrackType.Gameplay);
        AudioManager.Instance?.QueueAnnouncement(AnnouncerEvent.CertificationInitiated);
        return true;
    }

    public void SpawnBot()
    {
        if (IsSimulationRunning)
        {
            return;
        }

        StartSimulation();
    }

    public void SetAdaptiveMode(bool enabled)
    {
        adaptiveModeEnabled = enabled;
        adaptiveLearningManager?.SetRunMode(enabled);
        EventLogger.Instance?.Log(enabled ? "Adaptive mode enabled" : "Fresh mode enabled");
    }

    public void ResetCurrentDungeonLearning()
    {
        string dungeonId = adaptiveLearningManager != null ? adaptiveLearningManager.ComputeDungeonIdentifier(arenaManager) : _activeDungeonId;
        adaptiveLearningManager?.ClearCurrentDungeonLearning(dungeonId);
    }

    public void ResetAllLearning()
    {
        adaptiveLearningManager?.ClearAllLearning();
    }

    public AdaptiveLearningSummaryData GetLearningSummary()
    {
        string dungeonId = adaptiveLearningManager != null ? adaptiveLearningManager.ComputeDungeonIdentifier(arenaManager) : _activeDungeonId;
        return adaptiveLearningManager != null ? adaptiveLearningManager.BuildSummary(dungeonId) : null;
    }

    public void SetBotPersonality(BotPersonality personality)
    {
        defaultPersonality = personality;
        if (_activeBot != null)
        {
            _activeBot.SetPersonality(personality);
        }

        EventLogger.Instance?.Log($"Bot personality: {personality}");
    }

    public void StopSimulation(string result, bool updateUi = true)
    {
        if (!IsSimulationRunning)
        {
            return;
        }

        IsSimulationRunning = false;
        _isPaused = false;
        Time.timeScale = 1f;

        RunResult runResult = BuildRunResult(result == "BOT SURVIVED");
        OnRunFinished?.Invoke(runResult);

        if (_activeBot != null)
        {
            StartCoroutine(DestroyBotAfterDelay(_activeBot.gameObject, cleanupDelay));
            _activeBot = null;
        }

        buildModeController.SetBuildMode(true);

        if (updateUi)
        {
            bool success = runResult.survived;
            uiController.SetStatus(result);
            uiController.ShowResult(success);
            AudioManager.Instance?.PlayResultSound(success ? SoundCue.ResultVictory : SoundCue.ResultFail);
            AudioManager.Instance?.PlayMusicTrack(MusicTrackType.Results);
            if (success)
            {
                AudioManager.Instance?.QueueAnnouncement(AnnouncerEvent.ComplianceRatingApproved);
            }
        }
    }

    public void ResetRun()
    {
        StopSimulation("SIM RESET");
        uiController.ClearResult();
        uiController.SetStatus("Simulation reset");
        EventLogger.Instance?.Log("Run reset");
    }

    public void ClearDungeon()
    {
        if (IsSimulationRunning)
        {
            ResetRun();
        }

        arenaManager.ClearAll();
        DebugPathVisualizer.ClearPathHistory();
        EventLogger.Instance?.Log("Dungeon cleared");
    }

    public void PauseSimulation()
    {
        if (!IsSimulationRunning)
        {
            return;
        }

        _isPaused = true;
        Time.timeScale = 0f;
        EventLogger.Instance?.Log("Simulation paused");
    }

    public void ResumeSimulation()
    {
        if (!IsSimulationRunning)
        {
            return;
        }

        _isPaused = false;
        Time.timeScale = 1f;
        EventLogger.Instance?.Log("Simulation resumed");
    }

    public void ToggleSlowMotion(bool enabled)
    {
        if (!IsSimulationRunning)
        {
            return;
        }

        Time.timeScale = enabled ? 0.2f : 1f;
        EventLogger.Instance?.Log(enabled ? "Slow motion enabled" : "Slow motion disabled");
    }

    public void StepSimulation()
    {
        if (!IsSimulationRunning || _activeBot == null)
        {
            return;
        }

        _isPaused = true;
        Time.timeScale = 0f;
        _activeBot.SimulateSingleStep();
        _simulationTime += 0.15f;
        EventLogger.Instance?.Log("Simulation stepped");
    }

    public bool CanBotAutoUpdate()
    {
        return IsSimulationRunning && !_isPaused;
    }

    private IEnumerator DestroyBotAfterDelay(GameObject bot, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (bot != null)
        {
            Destroy(bot);
        }
    }

    private void OnBotDied(BotHealth _)
    {
        StopSimulation("BOT DIED", _updateUiForActiveRun);
    }

    public void OnBotReachedGoal()
    {
        goalFeedback?.PlaySuccessFeedback();
        AudioManager.Instance?.PlayBotEvent(BotAudioEvent.Success, _activeRunPersonality, _activeBot != null ? _activeBot.transform.position : (Vector3?)null);
        AudioManager.Instance?.PlayResultSound(SoundCue.BotSuccess);
        AudioManager.Instance?.QueueAnnouncement(AnnouncerEvent.SurvivabilityThresholdAchieved);
        StopSimulation("BOT SURVIVED", _updateUiForActiveRun);
    }

    private RunResult BuildRunResult(bool survived)
    {
        RunResult runResult = new()
        {
            personality = _activeRunPersonality,
            survived = survived,
            completionTime = _simulationTime,
            dungeonId = _activeDungeonId,
            usedAdaptiveMode = adaptiveModeEnabled
        };

        if (_activeBot == null)
        {
            runResult.causeOfDeath = survived ? "None" : "Unknown";
            return runResult;
        }

        BotHealth health = _activeBot.GetComponent<BotHealth>();
        runResult.pathLength = _activeBot.TraversedPathLength;
        runResult.remainingHP = health != null ? health.CurrentHp : 0f;

        if (survived)
        {
            runResult.causeOfDeath = "None";
            Vector2Int goalTile = _activeBot.CurrentTilePosition;
            runResult.deathPosition = new Vector2(goalTile.x, goalTile.y);
        }
        else
        {
            Vector2Int tile = _activeBot.CurrentTilePosition;
            runResult.deathPosition = new Vector2(tile.x, tile.y);
            runResult.causeOfDeath = health != null ? MapDamageSource(health.LastDamageSource) : "Unknown";
        }

        if (adaptiveLearningManager != null)
        {
            adaptiveLearningManager.RecordRunOutcome(_activeDungeonId, survived, adaptiveModeEnabled);
            if (survived)
            {
                adaptiveLearningManager.RecordSuccessPath(_activeDungeonId, _activeBot.TraversedTiles, runResult.personality);
            }
            else
            {
                adaptiveLearningManager.RecordDeath(_activeDungeonId, _activeBot.CurrentTilePosition, runResult.personality);
            }
        }

        DebugPathVisualizer.RecordPathHistory(runResult.personality, _activeBot.CurrentPath, survived);
        return runResult;
    }

    private static string MapDamageSource(DamageSource source)
    {
        return source switch
        {
            DamageSource.SawTrap => "Saw Trap",
            DamageSource.BombTrap => "Bomb Trap",
            DamageSource.ArcherTrap => "Archer Trap",
            DamageSource.PathFailure => "No Valid Path",
            DamageSource.Multiple => "Multiple Damage Sources",
            _ => "Unknown"
        };
    }
}
