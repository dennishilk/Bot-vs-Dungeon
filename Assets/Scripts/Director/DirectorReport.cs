using System;
using System.Collections.Generic;
using UnityEngine;

public enum DirectorGoal
{
    Fair,
    Dangerous,
    Brutal,
    Puzzle,
    StressTest,
    Balanced
}

[Serializable]
public class DirectorParameters
{
    [Min(6)] public int mapWidth = 12;
    [Min(6)] public int mapHeight = 12;
    [Range(0.05f, 0.95f)] public float trapDensity = 0.35f;
    [Range(0f, 0.8f)] public float branchFrequency = 0.3f;
    [Min(1)] public int maxTrapBudget = 18;
    [Range(1, 20)] public int maxGenerationAttempts = 10;
    [Range(2f, 40f)] public float simulationDepthSeconds = 16f;
    [Range(1, 3)] public int validationBotCount = 3;
    public bool deterministic = true;
    public int deterministicSeed = 420;
}

[Serializable]
public class DirectorEvaluationSummary
{
    public int botsSurvived;
    public int botsTotal;
    public float survivalRate;
    public float averageCompletionTime;
    public float averagePathLength;
    public int trapTriggers;
    public string mostLethalCause;
    public float goalMatchScore;
}

[Serializable]
public class DirectorReport
{
    public DirectorGoal goal;
    public int attemptsUsed;
    public int trapCount;
    public int branchCount;
    public float trapBudgetUsageRatio;
    public string generatedLayoutScore;
    public string verdict;
    public DirectorEvaluationSummary evaluation = new();
    public List<RunResult> validationRuns = new();

    public string ToSummaryText()
    {
        return
            $"Director Goal: {goal}\n" +
            $"Attempts Used: {attemptsUsed}\n" +
            $"Generated Layout Score: {generatedLayoutScore}\n" +
            $"Survival Rate: {evaluation.botsSurvived} / {evaluation.botsTotal}\n" +
            $"Most Lethal Tile: {evaluation.mostLethalCause}\n" +
            $"Average Survival Time: {evaluation.averageCompletionTime:0.0}s\n" +
            $"Goal Match Score: {evaluation.goalMatchScore:0.00}\n" +
            $"Verdict: {verdict}";
    }
}
