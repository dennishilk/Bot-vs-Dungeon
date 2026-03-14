using System;
using System.Collections.Generic;

[Serializable]
public class CampaignAssignment
{
    public string assignmentID;
    public string title;
    public string description;
    public int trapBudget = 8;
    public string requiredRating = "Fair";
    public float minimumSurvivalRate;
    public bool requireAtLeastOneFatality;
    public bool requireDailyChallengeCompletion;
    public int bureauScoreReward = 100;
    public bool unlocked;
    public bool completed;

    [TextArea]
    public string flavorMemo;

    public bool MeetsCompletion(DungeonReport report, IReadOnlyList<RunResult> runs)
    {
        if (report == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(requiredRating))
        {
            int required = CampaignMath.GetRatingRank(requiredRating);
            int actual = CampaignMath.GetRatingRank(report.rating);
            if (actual < required)
            {
                return false;
            }
        }

        if (minimumSurvivalRate > 0f && report.survivalRate < minimumSurvivalRate)
        {
            return false;
        }

        if (requireAtLeastOneFatality && report.deaths <= 0)
        {
            return false;
        }

        return true;
    }
}
