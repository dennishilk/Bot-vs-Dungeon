using System;
using System.Collections.Generic;
using System.Linq;
using BotVsDungeon.UI;
using TMPro;
using UnityEngine;

public class LevelObjectiveManager : MonoBehaviour
{
    [SerializeField] private CertificationManager certificationManager;
    [SerializeField] private TMP_Text objectiveHeaderText;
    [SerializeField] private UIController uiController;
    [SerializeField] private TMP_Text objectiveDescriptionText;
    [SerializeField] private TMP_Text objectiveStatusText;
    [SerializeField] private Color pendingColor = new(0.9f, 0.85f, 0.7f);
    [SerializeField] private Color successColor = new(0.4f, 0.95f, 0.5f);
    [SerializeField] private Color failureColor = new(0.95f, 0.35f, 0.35f);

    public event Action<bool> OnObjectiveEvaluated;

    public LevelObjective ActiveObjective { get; private set; }

    private void Awake()
    {
        if (certificationManager != null)
        {
            certificationManager.OnCertificationCompleted += HandleCertificationCompleted;
        }

        SetObjective(null);
    }

    private void OnDestroy()
    {
        if (certificationManager != null)
        {
            certificationManager.OnCertificationCompleted -= HandleCertificationCompleted;
        }
    }

    public void SetObjective(LevelObjective objective)
    {
        ActiveObjective = objective;

        if (objectiveHeaderText != null)
        {
            objectiveHeaderText.text = "Level Objective";
        }

        if (objectiveDescriptionText != null)
        {
            objectiveDescriptionText.text = objective != null
                ? objective.GetDisplayText()
                : "No active objective.";
        }

        SetStatusText("Objective Pending", pendingColor);
        uiController?.SetObjectiveText(objective != null ? objective.GetDisplayText() : "No active objective.");
    }

    public bool EvaluateObjective(DungeonReport report, IReadOnlyList<RunResult> runResults)
    {
        if (ActiveObjective == null)
        {
            SetStatusText("Objective Pending", pendingColor);
            uiController?.SetObjectiveText("No active objective.");
            return false;
        }

        bool success = ActiveObjective.objectiveType switch
        {
            ObjectiveType.SurviveAtLeastOne => runResults.Any(r => r.survived),
            ObjectiveType.KillAllBots => runResults.Count > 0 && runResults.All(r => !r.survived),
            ObjectiveType.CarefulMustSurvive => runResults.Any(r => r.personality == BotPersonality.Careful && r.survived),
            ObjectiveType.RecklessMustFail => runResults.Any(r => r.personality == BotPersonality.Reckless && !r.survived),
            ObjectiveType.ReachDungeonRating => report != null && string.Equals(report.rating, ActiveObjective.targetRating, System.StringComparison.OrdinalIgnoreCase),
            _ => false
        };

        ActiveObjective.success = success;
        SetStatusText(success ? "Objective Complete" : "Objective Failed", success ? successColor : failureColor);
        OnObjectiveEvaluated?.Invoke(success);
        return success;
    }

    private void HandleCertificationCompleted(DungeonReport report, IReadOnlyList<RunResult> runs)
    {
        EvaluateObjective(report, runs);
    }

    private void SetStatusText(string value, Color color)
    {
        if (objectiveStatusText != null)
        {
            objectiveStatusText.text = value;
            objectiveStatusText.color = color;
        }
    }
}
