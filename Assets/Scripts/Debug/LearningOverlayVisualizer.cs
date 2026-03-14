using UnityEngine;

public class LearningOverlayVisualizer : MonoBehaviour
{
    public static bool ShowLearnedDanger = true;
    public static bool ShowSuccessfulPaths;
    public static bool ShowLearningHeatmap = true;

    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private AdaptiveLearningManager adaptiveLearningManager;
    [SerializeField] private Color safeColor = new(0.2f, 0.55f, 1f, 0.45f);
    [SerializeField] private Color dangerColor = new(1f, 0.22f, 0.1f, 0.6f);

    private void OnDrawGizmos()
    {
        if (!ShowLearnedDanger || arenaManager == null || adaptiveLearningManager == null)
        {
            return;
        }

        string dungeonId = simulationManager != null && !string.IsNullOrWhiteSpace(simulationManager.ActiveDungeonId)
            ? simulationManager.ActiveDungeonId
            : adaptiveLearningManager.ComputeDungeonIdentifier(arenaManager);

        foreach (AdaptiveTileMemory tile in adaptiveLearningManager.GetTileMemories(dungeonId))
        {
            float intensity = Mathf.Clamp01(tile.learnedDangerModifier / 8f);
            Color tileColor = Color.Lerp(safeColor, dangerColor, intensity);
            tileColor.a = ShowLearningHeatmap ? tileColor.a : 0.3f;
            Gizmos.color = tileColor;
            Vector2Int pos = tile.Position;
            Gizmos.DrawCube(new Vector3(pos.x, 0.07f, pos.y), new Vector3(0.84f, 0.02f, 0.84f));

            if (ShowSuccessfulPaths && tile.successfulPassCount > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(new Vector3(pos.x, 0.11f, pos.y), new Vector3(0.52f, 0.03f, 0.52f));
            }
        }
    }
}
