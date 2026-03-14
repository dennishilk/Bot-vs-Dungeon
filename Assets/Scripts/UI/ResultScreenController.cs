using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultScreenController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image resultBanner;
    [SerializeField] private TMP_Text dungeonNameText;
    [SerializeField] private TMP_Text carefulResultText;
    [SerializeField] private TMP_Text balancedResultText;
    [SerializeField] private TMP_Text recklessResultText;
    [SerializeField] private TMP_Text survivalRateText;
    [SerializeField] private TMP_Text averageHPText;
    [SerializeField] private TMP_Text averageTimeText;
    [SerializeField] private TMP_Text ratingText;
    [SerializeField] private TMP_Text verdictText;
    [SerializeField] private TMP_Text flavorText;

    [Header("Presentation")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private Color successColor = new(0.24f, 0.8f, 0.38f, 0.95f);
    [SerializeField] private Color riskyColor = new(0.95f, 0.78f, 0.25f, 0.95f);
    [SerializeField] private Color failureColor = new(0.92f, 0.29f, 0.29f, 0.95f);

    private Coroutine _fadeRoutine;

    public void Show(string dungeonName, DungeonReport report, IReadOnlyList<RunResult> runs)
    {
        if (report == null)
        {
            return;
        }

        panelRoot?.SetActive(true);
        if (dungeonNameText != null)
        {
            dungeonNameText.text = $"Dungeon: {dungeonName}";
        }

        SetPersonalityResult(carefulResultText, FindRun(runs, BotPersonality.Careful));
        SetPersonalityResult(balancedResultText, FindRun(runs, BotPersonality.Balanced));
        SetPersonalityResult(recklessResultText, FindRun(runs, BotPersonality.Reckless));

        if (survivalRateText != null)
        {
            float pct = report.totalRuns > 0 ? (report.totalSurvivals / (float)report.totalRuns) * 100f : 0f;
            survivalRateText.text = $"Survival Rate: {pct:0}% ({report.totalSurvivals}/{report.totalRuns})";
        }

        if (averageHPText != null)
        {
            averageHPText.text = $"Average HP: {report.averageRemainingHP:0.0}";
        }

        if (averageTimeText != null)
        {
            averageTimeText.text = $"Average Completion Time: {report.averageCompletionTime:0.00}s";
        }

        if (ratingText != null)
        {
            ratingText.text = $"Dungeon Rating: {report.rating}";
            ratingText.color = GetResultColor(report);
        }

        if (verdictText != null)
        {
            verdictText.text = $"Certification Verdict: {report.verdict}";
            verdictText.color = GetResultColor(report);
        }

        if (flavorText != null)
        {
            flavorText.text = GetFlavorText(report);
        }

        if (resultBanner != null)
        {
            resultBanner.color = GetResultColor(report);
        }

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeInRoutine());
    }

    public void Hide()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        panelRoot?.SetActive(false);
    }

    private IEnumerator FadeInRoutine()
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        _fadeRoutine = null;
    }

    private static RunResult FindRun(IReadOnlyList<RunResult> runs, BotPersonality personality)
    {
        if (runs == null)
        {
            return null;
        }

        foreach (RunResult run in runs)
        {
            if (run.personality == personality)
            {
                return run;
            }
        }

        return null;
    }

    private static void SetPersonalityResult(TMP_Text text, RunResult run)
    {
        if (text == null)
        {
            return;
        }

        if (run == null)
        {
            text.text = "N/A";
            return;
        }

        text.text = $"{run.personality}: {(run.survived ? "Survived" : "Died")}";
    }

    private Color GetResultColor(DungeonReport report)
    {
        if (report.totalSurvivals == report.totalRuns && report.totalRuns > 0)
        {
            return successColor;
        }

        if (report.totalSurvivals > 0)
        {
            return riskyColor;
        }

        return failureColor;
    }

    private static string GetFlavorText(DungeonReport report)
    {
        return report.verdict switch
        {
            "Certified" => "This dungeon meets minimum survivability standards.",
            "Certified With Risk" => "Technically possible, but deeply unpleasant.",
            "Impossible Layout" => "No bot returned. Investigation ongoing.",
            _ => "The certification board demands additional caution."
        };
    }
}
