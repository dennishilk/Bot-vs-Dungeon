using UnityEngine;

namespace BotVsDungeon.Evolution
{
    public static class EvolutionGoalPresets
    {
        public static EvolutionSettings BuildPreset(EvolutionGoal goal)
        {
            return goal switch
            {
                EvolutionGoal.Brutal => new EvolutionSettings
                {
                    goal = goal,
                    populationSize = 14,
                    maxGenerations = 50,
                    mutationProbability = 0.38f,
                    recombinationProbability = 0.52f,
                    survivorRatio = 0.3f,
                    simulationDepthSeconds = 12f,
                    botCount = 3,
                    simulationSpeed = EvolutionSpeed.Fast,
                    customWeights = new EvolutionFitnessWeights
                    {
                        survivalRateWeight = -1.25f,
                        completionTimeWeight = -0.6f,
                        trapTriggersWeight = 0.85f,
                        pathLengthWeight = 0.1f,
                        trafficConflictWeight = 0.25f
                    }
                },
                EvolutionGoal.Fair => new EvolutionSettings
                {
                    goal = goal,
                    populationSize = 12,
                    maxGenerations = 36,
                    mutationProbability = 0.22f,
                    recombinationProbability = 0.42f,
                    survivorRatio = 0.35f,
                    simulationDepthSeconds = 16f,
                    botCount = 3,
                    simulationSpeed = EvolutionSpeed.Normal,
                    customWeights = new EvolutionFitnessWeights
                    {
                        survivalRateWeight = 0.9f,
                        completionTimeWeight = -0.3f,
                        trapTriggersWeight = -0.3f,
                        pathLengthWeight = 0.2f,
                        trafficConflictWeight = -0.1f
                    }
                },
                EvolutionGoal.Puzzle => new EvolutionSettings
                {
                    goal = goal,
                    populationSize = 16,
                    maxGenerations = 70,
                    mutationProbability = 0.45f,
                    recombinationProbability = 0.6f,
                    survivorRatio = 0.28f,
                    simulationDepthSeconds = 18f,
                    botCount = 3,
                    simulationSpeed = EvolutionSpeed.Fast,
                    customWeights = new EvolutionFitnessWeights
                    {
                        survivalRateWeight = -0.1f,
                        completionTimeWeight = -0.1f,
                        trapTriggersWeight = 0.55f,
                        pathLengthWeight = 1.2f,
                        trafficConflictWeight = 0.25f
                    }
                },
                EvolutionGoal.StressTest => new EvolutionSettings
                {
                    goal = goal,
                    populationSize = 20,
                    maxGenerations = 48,
                    mutationProbability = 0.35f,
                    recombinationProbability = 0.55f,
                    survivorRatio = 0.25f,
                    simulationDepthSeconds = 10f,
                    botCount = 5,
                    simulationSpeed = EvolutionSpeed.VeryFast,
                    customWeights = new EvolutionFitnessWeights
                    {
                        survivalRateWeight = -0.6f,
                        completionTimeWeight = -0.25f,
                        trapTriggersWeight = 0.75f,
                        pathLengthWeight = 0.35f,
                        trafficConflictWeight = 1.15f
                    }
                },
                _ => new EvolutionSettings
                {
                    goal = EvolutionGoal.Balanced,
                    populationSize = 12,
                    maxGenerations = 40,
                    mutationProbability = 0.3f,
                    recombinationProbability = 0.45f,
                    survivorRatio = 0.3f,
                    simulationDepthSeconds = 14f,
                    botCount = 3,
                    simulationSpeed = EvolutionSpeed.Fast,
                    customWeights = new EvolutionFitnessWeights
                    {
                        survivalRateWeight = 0.3f,
                        completionTimeWeight = -0.2f,
                        trapTriggersWeight = 0.35f,
                        pathLengthWeight = 0.3f,
                        trafficConflictWeight = 0.35f
                    }
                }
            };
        }
    }
}
