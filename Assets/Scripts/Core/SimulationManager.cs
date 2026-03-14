using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private BuildModeController buildModeController;
    [SerializeField] private UIController uiController;
    [SerializeField] private GameObject botPrefab;

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

        Vector3 spawnPos = new(startPos.x, 0.6f, startPos.y);
        GameObject botObj = Instantiate(botPrefab, spawnPos, Quaternion.identity);

        _activeBot = botObj.GetComponent<BotAgent>();
        BotHealth health = botObj.GetComponent<BotHealth>();

        _activeBot.Initialize(arenaManager, this, goalPos);
        health.OnBotDied += OnBotDied;

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
            Destroy(_activeBot.gameObject);
            _activeBot = null;
        }

        buildModeController.SetBuildMode(true);
        uiController.SetStatus(result);
    }

    private void OnBotDied(BotHealth _)
    {
        StopSimulation("BOT DIED");
    }

    public void OnBotReachedGoal()
    {
        StopSimulation("BOT SURVIVED");
    }
}
