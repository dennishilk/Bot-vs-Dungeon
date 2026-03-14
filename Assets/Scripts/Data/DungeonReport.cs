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

    public static DungeonReport Build(List<RunResult> runs, int trapActivations, float safeHpThreshold = 50f, float fairHpThreshold = 30f)
    {
        DungeonReport report = new();
        report.runResults = runs;
        report.totalRuns = runs.Count;
        report.totalSurvivals = runs.Count(r => r.survived);
        report.averageRemainingHP = report.totalRuns > 0 ? runs.Average(r => r.remainingHP) : 0f;
        report.averageCompletionTime = runs.Where(r => r.survived).Any() ? runs.Where(r => r.survived).Average(r => r.completionTime) : 0f;
        report.averagePathLength = report.totalRuns > 0 ? runs.Average(r => r.pathLength) : 0f;

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
