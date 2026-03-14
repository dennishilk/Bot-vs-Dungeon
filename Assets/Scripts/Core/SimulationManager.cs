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

    private BotAgent _activeBot;
    private float _simulationTime;
    private bool _isPaused;

    public bool IsSimulationRunning { get; private set; }
    public float SimulationTime => _simulationTime;
    public BotAgent ActiveBot => _activeBot;

    public event Action<BotAgent> OnBotSpawned;

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
        if (IsSimulationRunning)
        {
            return;
        }

        if (!arenaManager.TryFindTile(TileType.Start, out Vector2Int startPos) ||
            !arenaManager.TryFindTile(TileType.Goal, out Vector2Int goalPos))
        {
            uiController.SetStatus("Place both START and GOAL.");
            return;
        }

        buildModeController.SetBuildMode(false);
        IsSimulationRunning = true;
        _isPaused = false;
        _simulationTime = 0f;
        uiController.ClearResult();

        Vector3 spawnPos = new(startPos.x, 0.6f, startPos.y);
        GameObject botObj = Instantiate(botPrefab, spawnPos, Quaternion.identity);

        _activeBot = botObj.GetComponent<BotAgent>();
        BotHealth health = botObj.GetComponent<BotHealth>();

        _activeBot.SetPersonality(defaultPersonality);
        _activeBot.Initialize(arenaManager, this, goalPos);
        health.OnBotDied += OnBotDied;

        EventLogger.Instance?.Log("Bot spawned");
        OnBotSpawned?.Invoke(_activeBot);

        AudioManager.Instance?.PlayUISound(SoundCue.SimulationStart);
        uiController.SetStatus("Simulation running...");
    }

    public void SpawnBot()
    {
        if (IsSimulationRunning)
        {
            return;
        }

        StartSimulation();
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

    public void StopSimulation(string result)
    {
        if (!IsSimulationRunning)
        {
            return;
        }

        IsSimulationRunning = false;
        _isPaused = false;
        Time.timeScale = 1f;

        if (_activeBot != null)
        {
            StartCoroutine(DestroyBotAfterDelay(_activeBot.gameObject, cleanupDelay));
            _activeBot = null;
        }

        buildModeController.SetBuildMode(true);

        bool success = result == "BOT SURVIVED";
        uiController.SetStatus(result);
        uiController.ShowResult(success);

        AudioManager.Instance?.PlayResultSound(success ? SoundCue.ResultVictory : SoundCue.ResultFail);
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
        StopSimulation("BOT DIED");
    }

    public void OnBotReachedGoal()
    {
        goalFeedback?.PlaySuccessFeedback();
        AudioManager.Instance?.PlayResultSound(SoundCue.BotSuccess);
        StopSimulation("BOT SURVIVED");
    }
}
