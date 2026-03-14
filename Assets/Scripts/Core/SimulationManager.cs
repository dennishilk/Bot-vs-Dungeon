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

    private BotAgent _activeBot;

    public bool IsSimulationRunning { get; private set; }

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
        uiController.ClearResult();

        Vector3 spawnPos = new(startPos.x, 0.6f, startPos.y);
        GameObject botObj = Instantiate(botPrefab, spawnPos, Quaternion.identity);

        _activeBot = botObj.GetComponent<BotAgent>();
        BotHealth health = botObj.GetComponent<BotHealth>();

        _activeBot.Initialize(arenaManager, this, goalPos);
        health.OnBotDied += OnBotDied;

        AudioManager.Instance?.PlayUISound(SoundCue.SimulationStart);
        uiController.SetStatus("Simulation running...");
    }

    public void StopSimulation(string result)
    {
        if (!IsSimulationRunning)
        {
            return;
        }

        IsSimulationRunning = false;

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

    private IEnumerator DestroyBotAfterDelay(GameObject bot, float delay)
    {
        yield return new WaitForSeconds(delay);
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
