using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonDirector : MonoBehaviour
{
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private GenerationEvaluator generationEvaluator;
    [SerializeField] private DungeonSaveManager dungeonSaveManager;
    [SerializeField] private DungeonSerializer dungeonSerializer;
    [SerializeField] private CertificationManager certificationManager;
    [SerializeField] private DirectorParameters parameters = new();

    [Header("Presets")]
    [SerializeField] private DirectorParameters fairPreset = new() { trapDensity = 0.25f, branchFrequency = 0.22f, maxTrapBudget = 14 };
    [SerializeField] private DirectorParameters brutalPreset = new() { trapDensity = 0.7f, branchFrequency = 0.45f, maxTrapBudget = 26 };
    [SerializeField] private DirectorParameters challengePreset = new() { trapDensity = 0.5f, branchFrequency = 0.32f, maxTrapBudget = 20 };

    public event Action<DirectorReport> OnDirectorReportReady;

    public DirectorParameters Parameters => parameters;

    public void GenerateBalanced() => StartCoroutine(GenerateRoutine(DirectorGoal.Balanced, false));
    public void GenerateWithAutoTest() => StartCoroutine(GenerateRoutine(DirectorGoal.Balanced, true));
    public void GenerateChallengeDungeon() => StartCoroutine(GenerateRoutine(DirectorGoal.Dangerous, false));
    public void GenerateBrutalDungeon() => StartCoroutine(GenerateRoutine(DirectorGoal.Brutal, false));
    public void GenerateFairDungeon() => StartCoroutine(GenerateRoutine(DirectorGoal.Fair, false));

    public void GenerateCustom(DirectorGoal goal, bool autoTest)
    {
        StartCoroutine(GenerateRoutine(goal, autoTest));
    }

    public void ApplyMapSize(int size)
    {
        parameters.mapWidth = Mathf.Max(6, size);
        parameters.mapHeight = Mathf.Max(6, size);
    }

    public void ApplyTrapDensity(float density)
    {
        parameters.trapDensity = Mathf.Clamp01(density);
    }

    private IEnumerator GenerateRoutine(DirectorGoal goal, bool autoTest)
    {
        if (dungeonGenerator == null || generationEvaluator == null || dungeonSerializer == null)
        {
            yield break;
        }

        DirectorParameters runParameters = CreateRunParameters(goal);
        DirectorReport bestReport = null;
        GeneratedDungeonLayout bestLayout = null;

        int attempts = Mathf.Max(1, runParameters.maxGenerationAttempts);
        for (int i = 0; i < attempts; i++)
        {
            GeneratedDungeonLayout layout = dungeonGenerator.GenerateLayout(goal, runParameters, i);
            ApplyLayout(layout, goal, persist:false);

            DirectorEvaluationSummary evaluation = null;
            List<RunResult> runs = null;
            yield return generationEvaluator.EvaluateLayoutRoutine(goal, runParameters.simulationDepthSeconds, runParameters.validationBotCount, (summary, allRuns) =>
            {
                evaluation = summary;
                runs = allRuns;
            });

            DirectorReport report = BuildReport(goal, i + 1, layout, evaluation, runs);
            if (bestReport == null || report.evaluation.goalMatchScore > bestReport.evaluation.goalMatchScore)
            {
                bestReport = report;
                bestLayout = layout;
            }

            if (report.evaluation.goalMatchScore >= 0.88f)
            {
                bestReport = report;
                bestLayout = layout;
                break;
            }
        }

        if (bestLayout != null)
        {
            ApplyLayout(bestLayout, goal, persist:true);
        }

        if (autoTest && certificationManager != null)
        {
            certificationManager.StartCertificationRun();
        }

        if (bestReport != null)
        {
            OnDirectorReportReady?.Invoke(bestReport);
        }
    }

    private DirectorReport BuildReport(DirectorGoal goal, int attempt, GeneratedDungeonLayout layout, DirectorEvaluationSummary evaluation, List<RunResult> runs)
    {
        float score = evaluation != null ? evaluation.goalMatchScore : 0f;

        return new DirectorReport
        {
            goal = goal,
            attemptsUsed = attempt,
            trapCount = layout != null ? layout.trapCount : 0,
            branchCount = layout != null ? layout.branchCount : 0,
            trapBudgetUsageRatio = layout != null ? layout.trapBudgetUsageRatio : 0f,
            generatedLayoutScore = score >= 0.82f ? "Excellent" : score >= 0.62f ? "Good" : score >= 0.42f ? "Average" : "Weak",
            verdict = BuildVerdict(goal, evaluation),
            evaluation = evaluation ?? new DirectorEvaluationSummary(),
            validationRuns = runs ?? new List<RunResult>()
        };
    }

    private static string BuildVerdict(DirectorGoal goal, DirectorEvaluationSummary evaluation)
    {
        if (evaluation == null)
        {
            return "No evaluation available.";
        }

        if (evaluation.goalMatchScore >= 0.82f)
        {
            return $"Successful {goal} Dungeon";
        }

        if (evaluation.goalMatchScore >= 0.55f)
        {
            return $"Partial {goal} Match";
        }

        return $"Low {goal} Match - consider regeneration";
    }

    private void ApplyLayout(GeneratedDungeonLayout layout, DirectorGoal goal, bool persist)
    {
        if (layout == null)
        {
            return;
        }

        string saveName = $"Director_{goal}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        DungeonSaveData saveData = layout.ToSaveData(saveName);
        dungeonSerializer.ApplyLayout(saveData, out _);

        certificationManager?.SetDungeonName(saveName);

        if (persist && dungeonSaveManager != null)
        {
            dungeonSaveManager.SaveImportedLayout(saveName, saveData, true, out _);
        }
    }

    private DirectorParameters CreateRunParameters(DirectorGoal goal)
    {
        DirectorParameters source = parameters;
        if (goal == DirectorGoal.Fair)
        {
            source = fairPreset;
        }
        else if (goal == DirectorGoal.Brutal)
        {
            source = brutalPreset;
        }
        else if (goal == DirectorGoal.Dangerous)
        {
            source = challengePreset;
        }

        return new DirectorParameters
        {
            mapWidth = source.mapWidth,
            mapHeight = source.mapHeight,
            trapDensity = source.trapDensity,
            branchFrequency = source.branchFrequency,
            maxTrapBudget = source.maxTrapBudget,
            maxGenerationAttempts = source.maxGenerationAttempts,
            simulationDepthSeconds = source.simulationDepthSeconds,
            validationBotCount = source.validationBotCount,
            deterministic = source.deterministic,
            deterministicSeed = source.deterministicSeed
        };
    }
}
