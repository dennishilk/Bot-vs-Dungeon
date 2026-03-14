using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationEvaluator : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;

    public bool IsEvaluating { get; private set; }

    public IEnumerator EvaluateLayoutRoutine(DirectorGoal goal, float simulationDepthSeconds, int validationBotCount, Action<DirectorEvaluationSummary, List<RunResult>> onComplete)
    {
        if (simulationManager == null)
        {
            onComplete?.Invoke(new DirectorEvaluationSummary
            {
                botsTotal = 0,
                mostLethalCause = "SimulationManager Missing",
                goalMatchScore = 0f
            }, new List<RunResult>());
            yield break;
        }

        IsEvaluating = true;
        List<RunResult> runs = new();
        List<BotPersonality> personalities = new() { BotPersonality.Careful, BotPersonality.Balanced, BotPersonality.Reckless };
        int count = Mathf.Clamp(validationBotCount, 1, 3);

        for (int i = 0; i < count; i++)
        {
            BotPersonality personality = personalities[i];
            bool finished = false;
            RunResult latest = null;

            void OnFinished(RunResult result)
            {
                latest = result;
                finished = true;
            }

            simulationManager.OnRunFinished += OnFinished;
            bool started = simulationManager.StartSimulationWithPersonality(personality, false);
            if (!started)
            {
                simulationManager.OnRunFinished -= OnFinished;
                continue;
            }

            float elapsed = 0f;
            while (!finished && elapsed < simulationDepthSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!finished && simulationManager.IsSimulationRunning)
            {
                simulationManager.StopSimulation("SIM DEPTH LIMIT", false);
            }

            simulationManager.OnRunFinished -= OnFinished;

            if (latest != null)
            {
                runs.Add(latest);
            }

            yield return null;
        }

        DirectorEvaluationSummary summary = BuildSummary(goal, runs);
        IsEvaluating = false;
        onComplete?.Invoke(summary, runs);
    }

    private static DirectorEvaluationSummary BuildSummary(DirectorGoal goal, List<RunResult> runs)
    {
        DirectorEvaluationSummary summary = new()
        {
            botsTotal = runs.Count,
            mostLethalCause = "None"
        };

        if (runs.Count == 0)
        {
            summary.goalMatchScore = 0f;
            return summary;
        }

        int survived = 0;
        float completionTime = 0f;
        float pathLength = 0f;
        int trapTriggers = 0;
        Dictionary<string, int> causes = new();

        foreach (RunResult run in runs)
        {
            if (run.survived)
            {
                survived++;
            }
            else
            {
                trapTriggers++;
                string cause = string.IsNullOrWhiteSpace(run.causeOfDeath) ? "Unknown" : run.causeOfDeath;
                causes.TryGetValue(cause, out int existing);
                causes[cause] = existing + 1;
            }

            completionTime += run.completionTime;
            pathLength += run.pathLength;
        }

        summary.botsSurvived = survived;
        summary.survivalRate = (float)survived / runs.Count;
        summary.averageCompletionTime = completionTime / runs.Count;
        summary.averagePathLength = pathLength / runs.Count;
        summary.trapTriggers = trapTriggers;

        int max = 0;
        foreach (KeyValuePair<string, int> pair in causes)
        {
            if (pair.Value > max)
            {
                max = pair.Value;
                summary.mostLethalCause = pair.Key;
            }
        }

        summary.goalMatchScore = ScoreGoalMatch(goal, summary);
        return summary;
    }

    private static float ScoreGoalMatch(DirectorGoal goal, DirectorEvaluationSummary summary)
    {
        float survival = summary.survivalRate;
        float completion = Mathf.Clamp01(1f - (summary.averageCompletionTime / 25f));
        float traps = Mathf.Clamp01(summary.trapTriggers / Mathf.Max(1f, summary.botsTotal));

        return goal switch
        {
            DirectorGoal.Fair => Mathf.Clamp01((1f - Mathf.Abs(survival - 0.5f)) * 0.6f + completion * 0.2f + (1f - traps) * 0.2f),
            DirectorGoal.Brutal => Mathf.Clamp01((1f - survival) * 0.6f + traps * 0.25f + (1f - completion) * 0.15f),
            DirectorGoal.Puzzle => Mathf.Clamp01(completion * 0.25f + (1f - survival) * 0.15f + Mathf.Clamp01(summary.averagePathLength / 20f) * 0.6f),
            DirectorGoal.StressTest => Mathf.Clamp01((1f - survival) * 0.45f + traps * 0.35f + Mathf.Clamp01(summary.averagePathLength / 18f) * 0.2f),
            DirectorGoal.Dangerous => Mathf.Clamp01((1f - survival) * 0.45f + traps * 0.4f + completion * 0.15f),
            _ => Mathf.Clamp01((1f - Mathf.Abs(survival - 0.4f)) * 0.5f + completion * 0.3f + traps * 0.2f)
        };
    }
}
