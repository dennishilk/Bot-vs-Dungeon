using System;
using System.Collections.Generic;
using UnityEngine;

namespace BotVsDungeon.Evolution
{
    public enum EvolutionGoal
    {
        Brutal,
        Fair,
        Puzzle,
        Balanced,
        StressTest
    }

    public enum EvolutionSpeed
    {
        Normal,
        Fast,
        VeryFast
    }

    [Serializable]
    public class EvolutionFitnessWeights
    {
        [Range(-2f, 2f)] public float survivalRateWeight = 0.5f;
        [Range(-2f, 2f)] public float completionTimeWeight = -0.25f;
        [Range(-2f, 2f)] public float trapTriggersWeight = 0.6f;
        [Range(-2f, 2f)] public float pathLengthWeight = 0.4f;
        [Range(-2f, 2f)] public float trafficConflictWeight = 0.4f;
    }

    [Serializable]
    public class EvolutionSettings
    {
        [Range(10, 20)] public int populationSize = 12;
        [Range(1, 200)] public int maxGenerations = 40;
        [Range(0.05f, 0.8f)] public float mutationProbability = 0.3f;
        [Range(0f, 0.8f)] public float recombinationProbability = 0.45f;
        [Range(0.1f, 0.8f)] public float survivorRatio = 0.3f;
        [Range(2f, 45f)] public float simulationDepthSeconds = 14f;
        [Range(3, 5)] public int botCount = 3;
        public bool deterministic = true;
        public int deterministicSeed = 1807;
        public EvolutionSpeed simulationSpeed = EvolutionSpeed.Fast;
        public bool skipBotVisuals = true;
        public EvolutionGoal goal = EvolutionGoal.Balanced;
        public EvolutionFitnessWeights customWeights = new();
    }

    [Serializable]
    public class EvolutionMetrics
    {
        public float survivalRate;
        public float averageDamageTaken;
        public float averageCompletionTime;
        public float averagePathLength;
        public int trapTriggers;
        public int botDeaths;
        public List<Vector2> deathTilePositions = new();
        public int trafficConflictCount;
    }

    [Serializable]
    public class EvolutionCandidate
    {
        public DungeonGenome genome;
        public EvolutionMetrics metrics = new();
        public float fitness;
    }

    [Serializable]
    public class EvolutionGenerationReport
    {
        public int generationNumber;
        public int populationSize;
        public float bestFitness;
        public float averageFitness;
        public float bestSurvivalRate;
        public EvolutionGoal goal;
        public EvolutionCandidate bestCandidate;
    }
}
