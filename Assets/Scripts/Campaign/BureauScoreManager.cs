using System;
using UnityEngine;

public class BureauScoreManager : MonoBehaviour
{
    [Header("Base Rewards")]
    [SerializeField] private int scorePerAssignment = 120;
    [SerializeField] private int scorePerDailyChallenge = 60;
    [SerializeField] private int scorePerAchievement = 30;
    [SerializeField] private int scorePerCertification = 20;
    [SerializeField] private float qualityMultiplier = 1.1f;

    public event Action<int, string> OnScoreAwarded;

    public int ComputeAssignmentReward(CampaignAssignment assignment, DungeonReport report)
    {
        int baseReward = assignment != null ? Mathf.Max(0, assignment.bureauScoreReward) : scorePerAssignment;
        if (report == null)
        {
            return baseReward;
        }

        float multiplier = 1f + Mathf.Clamp01(report.survivalRate);
        multiplier += report.rating.Equals("Impossible", StringComparison.OrdinalIgnoreCase) ? 0.25f : 0f;
        multiplier += report.rating.Equals("Safe", StringComparison.OrdinalIgnoreCase) ? 0.1f : 0f;

        return Mathf.RoundToInt(baseReward * multiplier * qualityMultiplier);
    }

    public int GetCertificationReward() => scorePerCertification;
    public int GetDailyChallengeReward() => scorePerDailyChallenge;
    public int GetAchievementReward() => scorePerAchievement;

    public void RaiseScoreAwarded(int amount, string reason)
    {
        OnScoreAwarded?.Invoke(Mathf.Max(0, amount), reason);
    }
}
