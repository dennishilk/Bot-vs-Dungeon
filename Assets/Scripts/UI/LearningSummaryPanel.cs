using TMPro;
using UnityEngine;

public class LearningSummaryPanel : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private GameObject panelRoot;

    private void Update()
    {
        if (simulationManager == null || summaryText == null)
        {
            return;
        }

        AdaptiveLearningSummaryData summary = simulationManager.GetLearningSummary();
        if (summary == null)
        {
            return;
        }

        summaryText.text =
            "Dungeon Learning Summary\n" +
            $"Observed Runs: {summary.observedRuns}\n" +
            $"Learned Dangerous Tiles: {summary.learnedDangerousTiles}\n" +
            $"Most Lethal Tile: ({summary.mostLethalTile.x}, {summary.mostLethalTile.y})\n" +
            $"Most Avoided Tile: ({summary.mostAvoidedTile.x}, {summary.mostAvoidedTile.y})\n" +
            $"Success Improvement: {(summary.adaptiveImprovement * 100f):+0;-0;0}%";
    }

    public void SetVisible(bool visible)
    {
        panelRoot?.SetActive(visible);
    }
}
