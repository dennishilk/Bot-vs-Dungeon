using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class DungeonReport
{
    public int totalRuns;
    public int totalSurvivals;
    public float averageRemainingHP;
    public float averageCompletionTime;
    public float averagePathLength;
    public string rating;
    public string verdict;
    public List<RunResult> runResults = new();

    public Vector2IntSerializable mostLethalTile;
    public Vector2IntSerializable mostAvoidedTile;
    public Vector2IntSerializable mostLearnedDangerousTile;
    public int learnedDangerousTileCount;
    public float freshSuccessRate;
    public float adaptiveSuccessRate;
    public float adaptiveImprovement;

    public static DungeonReport Build(
        List<RunResult> runs,
        int trapActivations,
        float safeHpThreshold = 50f,
        float fairHpThreshold = 30f,
        AdaptiveLearningSummaryData adaptiveSummary = null)
    {
        DungeonReport report = new();
        report.runResults = runs;
        report.totalRuns = runs.Count;
        report.totalSurvivals = runs.Count(r => r.survived);
        report.averageRemainingHP = report.totalRuns > 0 ? runs.Average(r => r.remainingHP) : 0f;
        report.averageCompletionTime = runs.Where(r => r.survived).Any() ? runs.Where(r => r.survived).Average(r => r.completionTime) : 0f;
        report.averagePathLength = report.totalRuns > 0 ? runs.Average(r => r.pathLength) : 0f;

        if (adaptiveSummary != null)
        {
            report.mostLethalTile = Vector2IntSerializable.From(adaptiveSummary.mostLethalTile);
            report.mostAvoidedTile = Vector2IntSerializable.From(adaptiveSummary.mostAvoidedTile);
            report.mostLearnedDangerousTile = Vector2IntSerializable.From(adaptiveSummary.mostLearnedDangerousTile);
            report.learnedDangerousTileCount = adaptiveSummary.learnedDangerousTiles;
            report.freshSuccessRate = adaptiveSummary.preAdaptationSuccessRate;
            report.adaptiveSuccessRate = adaptiveSummary.postAdaptationSuccessRate;
            report.adaptiveImprovement = adaptiveSummary.adaptiveImprovement;
        }

        if (report.totalSurvivals == 3)
        {
            report.rating = report.averageRemainingHP >= safeHpThreshold ? "Safe" : "Fair";
            report.verdict = report.averageRemainingHP >= fairHpThreshold ? "Certified" : "Certified With Risk";
        }
        else if (report.totalSurvivals == 2)
        {
            report.rating = "Dangerous";
            report.verdict = trapActivations >= 4 ? "Unsafe" : "Certified With Risk";
        }
        else if (report.totalSurvivals == 1)
        {
            report.rating = "Brutal";
            report.verdict = "Bot Fatality Zone";
        }
        else
        {
            report.rating = trapActivations > 0 ? "Impossible" : "Brutal";
            report.verdict = "Impossible Layout";
        }

        return report;
    }
}

[System.Serializable]
public struct Vector2IntSerializable
{
    public int x;
    public int y;

    public static Vector2IntSerializable From(UnityEngine.Vector2Int value)
    {
        return new Vector2IntSerializable { x = value.x, y = value.y };
    }
}
