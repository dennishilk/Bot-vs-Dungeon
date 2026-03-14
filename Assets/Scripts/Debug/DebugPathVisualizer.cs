using UnityEngine;

public class DebugPathVisualizer : MonoBehaviour
{
    public static bool ShowBotPath = true;
    public static bool ShowDangerMap = true;
    public static bool ShowTileGrid = false;
    public static bool ShowTrapRange = false;

    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private SimulationManager simulationManager;

    [Header("Colors")]
    [SerializeField] private Color pathColor = Color.green;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private Color goalColor = new(0.2f, 1f, 0.2f, 1f);
    [SerializeField] private Color targetColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (arenaManager == null)
        {
            return;
        }

        if (ShowTileGrid)
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
            foreach (var tile in arenaManager.GetAllTiles())
            {
                Gizmos.DrawWireCube(new Vector3(tile.Key.x, 0.05f, tile.Key.y), new Vector3(1f, 0.05f, 1f));
            }
        }

        if (ShowDangerMap)
        {
            Gizmos.color = dangerColor;
            foreach (Vector2Int dangerTile in arenaManager.GetDangerTiles())
            {
                Gizmos.DrawCube(new Vector3(dangerTile.x, 0.03f, dangerTile.y), new Vector3(0.7f, 0.05f, 0.7f));
            }
        }

        if (arenaManager.TryFindTile(TileType.Goal, out Vector2Int goal))
        {
            Gizmos.color = goalColor;
            Gizmos.DrawSphere(new Vector3(goal.x, 0.4f, goal.y), 0.2f);
        }

        BotAgent bot = simulationManager != null ? simulationManager.ActiveBot : null;
        if (bot == null)
        {
            return;
        }

        if (ShowBotPath)
        {
            Gizmos.color = pathColor;
            var path = bot.CurrentPath;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 from = new(path[i].x, 0.2f, path[i].y);
                Vector3 to = new(path[i + 1].x, 0.2f, path[i + 1].y);
                Gizmos.DrawLine(from, to);
            }
        }

        Gizmos.color = targetColor;
        Vector2Int target = bot.CurrentTargetTile;
        Gizmos.DrawSphere(new Vector3(target.x, 0.45f, target.y), 0.12f);

        if (ShowTrapRange)
        {
            DrawTrapRangeGizmos();
        }
    }

    private void DrawTrapRangeGizmos()
    {
        foreach (var tile in arenaManager.GetAllTiles())
        {
            if (tile.Value.trap == null)
            {
                continue;
            }

            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.35f);
            Gizmos.DrawWireSphere(new Vector3(tile.Key.x, 0.25f, tile.Key.y), 1f);
        }
    }
}
