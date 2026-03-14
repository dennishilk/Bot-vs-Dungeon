using TMPro;
using UnityEngine;

public class StressTestReport : MonoBehaviour
{
    [SerializeField] private TMP_Text reportText;
    [SerializeField] private GameObject panelRoot;

    public void Show(StressTestResultData data)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (reportText != null)
        {
            reportText.text =
                "Stress Test Report\n" +
                $"Bots Spawned: {data.botsSpawned}\n" +
                $"Bots Survived: {data.botsSurvived}\n" +
                $"Bots Died: {data.botsDied}\n\n" +
                $"Average Survival Time: {data.averageSurvivalTime:0.00}s\n" +
                $"Average Path Length: {data.averagePathLength:0.0}\n" +
                $"Most Lethal Tile: ({data.mostLethalTile.x}, {data.mostLethalTile.y})\n" +
                $"Most Common Cause of Death: {data.mostCommonCauseOfDeath}";
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}
