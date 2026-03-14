using TMPro;
using UnityEngine;

public class DirectorModePanel : MonoBehaviour
{
    [SerializeField] private DungeonDirector dungeonDirector;
    [SerializeField] private TMP_Dropdown goalDropdown;
    [SerializeField] private TMP_Text attemptsText;
    [SerializeField] private TMP_Text simulationResultsText;
    [SerializeField] private TMP_Text goalMatchText;
    [SerializeField] private TMP_Text reportText;
    [SerializeField] private GameObject panelRoot;

    private void Awake()
    {
        if (goalDropdown != null && goalDropdown.options.Count == 0)
        {
            goalDropdown.ClearOptions();
            goalDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                DirectorGoal.Fair.ToString(),
                DirectorGoal.Dangerous.ToString(),
                DirectorGoal.Brutal.ToString(),
                DirectorGoal.Puzzle.ToString(),
                DirectorGoal.StressTest.ToString(),
                DirectorGoal.Balanced.ToString()
            });
        }

        if (dungeonDirector != null)
        {
            dungeonDirector.OnDirectorReportReady += HandleReportReady;
        }
    }

    private void OnDestroy()
    {
        if (dungeonDirector != null)
        {
            dungeonDirector.OnDirectorReportReady -= HandleReportReady;
        }
    }

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    public void GenerateDungeon()
    {
        dungeonDirector?.GenerateCustom(GetSelectedGoal(), false);
    }

    public void GenerateAndAutoTest()
    {
        dungeonDirector?.GenerateCustom(GetSelectedGoal(), true);
    }

    public void GenerateChallengeDungeon()
    {
        dungeonDirector?.GenerateChallengeDungeon();
    }

    public void GenerateBrutalDungeon()
    {
        dungeonDirector?.GenerateBrutalDungeon();
    }

    public void GenerateFairDungeon()
    {
        dungeonDirector?.GenerateFairDungeon();
    }

    public void OnMapSizeChanged(float value)
    {
        dungeonDirector?.ApplyMapSize(Mathf.RoundToInt(value));
    }

    public void OnTrapDensityChanged(float value)
    {
        dungeonDirector?.ApplyTrapDensity(value);
    }

    private DirectorGoal GetSelectedGoal()
    {
        if (goalDropdown == null)
        {
            return DirectorGoal.Balanced;
        }

        int value = Mathf.Clamp(goalDropdown.value, 0, System.Enum.GetValues(typeof(DirectorGoal)).Length - 1);
        return (DirectorGoal)value;
    }

    private void HandleReportReady(DirectorReport report)
    {
        if (report == null)
        {
            return;
        }

        if (attemptsText != null)
        {
            attemptsText.text = $"Generation Attempts: {report.attemptsUsed}";
        }

        if (simulationResultsText != null)
        {
            simulationResultsText.text =
                $"Survival: {report.evaluation.botsSurvived}/{report.evaluation.botsTotal}\n" +
                $"Avg Time: {report.evaluation.averageCompletionTime:0.0}s\n" +
                $"Trap Triggers: {report.evaluation.trapTriggers}";
        }

        if (goalMatchText != null)
        {
            goalMatchText.text = $"Goal Match Score: {report.evaluation.goalMatchScore:0.00}";
        }

        if (reportText != null)
        {
            reportText.text = report.ToSummaryText();
        }
    }
}
