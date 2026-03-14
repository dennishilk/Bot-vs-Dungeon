using System.Text;
using TMPro;
using UnityEngine;

public class DungeonReportPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text runSummaryText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text ratingText;
    [SerializeField] private TMP_Text verdictText;
    [SerializeField] private TMP_Text flavorText;

    public void ShowReport(DungeonReport report)
    {
        if (report == null)
        {
            return;
        }

        panelRoot?.SetActive(true);

        if (runSummaryText != null)
        {
            StringBuilder sb = new();
            sb.AppendLine("Run Summary");
            foreach (RunResult run in report.runResults)
            {
                sb.AppendLine($"- {run.ToSummaryLine()}");
            }
            runSummaryText.text = sb.ToString();
        }

        if (statsText != null)
        {
            statsText.text =
                "Statistics\n" +
                $"- Survival Rate: {report.totalSurvivals}/{report.totalRuns}\n" +
                $"- Avg HP: {report.averageRemainingHP:0.0}\n" +
                $"- Avg Time: {report.averageCompletionTime:0.00}s\n" +
                $"- Avg Path Length: {report.averagePathLength:0.0}";
        }

        if (ratingText != null)
        {
            ratingText.text = $"Dungeon Rating: {report.rating}";
        }

        if (verdictText != null)
        {
            verdictText.text = $"Certification Verdict: {report.verdict}";
        }

        if (flavorText != null)
        {
            flavorText.text = report.verdict switch
            {
                "Certified" => "The dungeon passed minimum survivability standards.",
                "Certified With Risk" => "This layout is passable, but expect paperwork.",
                "Bot Fatality Zone" => "This dungeon appears technically possible, but extremely rude.",
                "Impossible Layout" => "No bot returned. Further paperwork has been postponed indefinitely.",
                _ => "Proceed with caution."
            };
        }
    }

    public void Hide()
    {
        panelRoot?.SetActive(false);
    }
}
