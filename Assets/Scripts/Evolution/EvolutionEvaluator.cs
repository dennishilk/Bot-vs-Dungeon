using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BotVsDungeon.Evolution
{
    public class EvolutionEvaluator : MonoBehaviour
    {
        [SerializeField] private GenerationEvaluator generationEvaluator;
        [SerializeField] private DungeonSerializer dungeonSerializer;

        public IEnumerator EvaluateRoutine(DungeonGenome genome, EvolutionGoal goal, EvolutionSettings settings, Action<EvolutionMetrics, float> onComplete)
        {
            if (genome == null || dungeonSerializer == null || generationEvaluator == null)
            {
                onComplete?.Invoke(new EvolutionMetrics(), 0f);
                yield break;
            }

            DungeonSaveData saveData = genome.ToSaveData($"EvolutionEval_{DateTime.UtcNow:HHmmssfff}");
            dungeonSerializer.ApplyLayout(saveData, out _);

            DirectorEvaluationSummary summary = null;
            List<RunResult> runs = null;
            int botCount = Mathf.Clamp(settings.botCount, 3, 5);
            yield return generationEvaluator.EvaluateLayoutRoutine(MapGoal(goal), settings.simulationDepthSeconds, Mathf.Min(3, botCount), (s, r) =>
            {
                summary = s;
                runs = r;
            });

            EvolutionMetrics metrics = BuildMetrics(summary, runs, settings, botCount);
            float fitness = ScoreFitness(goal, metrics, settings.customWeights);
            onComplete?.Invoke(metrics, fitness);
        }

        private static EvolutionMetrics BuildMetrics(DirectorEvaluationSummary summary, List<RunResult> runs, EvolutionSettings settings, int requestedBotCount)
        {
            EvolutionMetrics metrics = new();
            if (summary == null)
            {
                return metrics;
            }

            metrics.survivalRate = summary.survivalRate;
            metrics.averageCompletionTime = summary.averageCompletionTime;
            metrics.averagePathLength = summary.averagePathLength;
            metrics.trapTriggers = summary.trapTriggers;
            metrics.botDeaths = Mathf.Max(0, summary.botsTotal - summary.botsSurvived);
            metrics.trafficConflictCount = requestedBotCount >= 5 ? summary.trapTriggers + Mathf.RoundToInt(summary.averagePathLength * 0.2f) : 0;

            if (runs != null)
            {
                float totalDamage = 0f;
                foreach (RunResult run in runs)
                {
                    totalDamage += Mathf.Max(0f, 100f - run.remainingHP);
                    if (!run.survived)
                    {
                        metrics.deathTilePositions.Add(run.deathPosition);
                    }
                }

                metrics.averageDamageTaken = runs.Count > 0 ? totalDamage / runs.Count : 0f;
            }

            return metrics;
        }

        private static float ScoreFitness(EvolutionGoal goal, EvolutionMetrics metrics, EvolutionFitnessWeights customWeights)
        {
            EvolutionFitnessWeights weights = ResolveWeights(goal, customWeights);
            float normalizedTime = Mathf.Clamp01(1f - (metrics.averageCompletionTime / 30f));
            float normalizedPath = Mathf.Clamp01(metrics.averagePathLength / 25f);
            float normalizedTriggers = Mathf.Clamp01(metrics.trapTriggers / 5f);
            float normalizedTraffic = Mathf.Clamp01(metrics.trafficConflictCount / 8f);

            float score =
                (metrics.survivalRate * weights.survivalRateWeight) +
                (normalizedTime * weights.completionTimeWeight) +
                (normalizedTriggers * weights.trapTriggersWeight) +
                (normalizedPath * weights.pathLengthWeight) +
                (normalizedTraffic * weights.trafficConflictWeight);

            return Mathf.Clamp(score, -2f, 2f);
        }

        private static EvolutionFitnessWeights ResolveWeights(EvolutionGoal goal, EvolutionFitnessWeights custom)
        {
            if (custom != null)
            {
                return custom;
            }

            return goal switch
            {
                EvolutionGoal.Brutal => new EvolutionFitnessWeights { survivalRateWeight = -1.2f, completionTimeWeight = -0.6f, trapTriggersWeight = 0.8f, pathLengthWeight = 0.1f, trafficConflictWeight = 0.3f },
                EvolutionGoal.Fair => new EvolutionFitnessWeights { survivalRateWeight = 0.8f, completionTimeWeight = -0.25f, trapTriggersWeight = -0.4f, pathLengthWeight = 0.15f, trafficConflictWeight = -0.1f },
                EvolutionGoal.Puzzle => new EvolutionFitnessWeights { survivalRateWeight = -0.2f, completionTimeWeight = -0.1f, trapTriggersWeight = 0.6f, pathLengthWeight = 1.1f, trafficConflictWeight = 0.2f },
                EvolutionGoal.StressTest => new EvolutionFitnessWeights { survivalRateWeight = -0.55f, completionTimeWeight = -0.3f, trapTriggersWeight = 0.7f, pathLengthWeight = 0.35f, trafficConflictWeight = 1.1f },
                _ => new EvolutionFitnessWeights { survivalRateWeight = 0.25f, completionTimeWeight = -0.15f, trapTriggersWeight = 0.35f, pathLengthWeight = 0.25f, trafficConflictWeight = 0.25f }
            };
        }

        private static DirectorGoal MapGoal(EvolutionGoal goal)
        {
            return goal switch
            {
                EvolutionGoal.Brutal => DirectorGoal.Brutal,
                EvolutionGoal.Fair => DirectorGoal.Fair,
                EvolutionGoal.Puzzle => DirectorGoal.Puzzle,
                EvolutionGoal.StressTest => DirectorGoal.StressTest,
                _ => DirectorGoal.Balanced
            };
        }
    }
}
