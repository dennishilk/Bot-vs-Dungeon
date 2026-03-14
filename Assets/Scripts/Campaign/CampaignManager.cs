using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CampaignManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CertificationManager certificationManager;
    [SerializeField] private BureauScoreManager bureauScoreManager;
    [SerializeField] private DailyChallengeManager dailyChallengeManager;

    [Header("Progression")]
    [SerializeField] private string campaignSaveFileName = "career_save.json";
    [SerializeField] private List<CampaignTier> tiers = new();
    [SerializeField] private List<string> rankTitles = new() { "Junior Architect", "Certified Architect", "Senior Hazard Planner", "Brutality Compliance Officer", "Grand Auditor of Peril" };
    [SerializeField] private List<int> promotionThresholds = new() { 0, 350, 900, 1700, 3000 };

    [Header("Runtime")]
    [SerializeField] private string activeAssignmentId;

    public event Action<PlayerCareerData> OnCareerDataChanged;
    public event Action<string, string, IReadOnlyList<string>> OnPromotionGranted;
    public event Action<CampaignAssignment, int> OnAssignmentCompleted;

    public PlayerCareerData CareerData { get; private set; } = new();
    public string SavePath => Path.Combine(Application.persistentDataPath, campaignSaveFileName);

    private void Awake()
    {
        LoadCareer();
        EnsureDefaultCampaign();
        EnsureUnlockState();

        if (certificationManager != null)
        {
            certificationManager.OnCertificationCompleted += HandleCertificationCompleted;
        }
    }

    private void OnDestroy()
    {
        if (certificationManager != null)
        {
            certificationManager.OnCertificationCompleted -= HandleCertificationCompleted;
        }
    }

    public IReadOnlyList<CampaignTier> GetTiers() => tiers;

    public CampaignAssignment GetActiveAssignment()
    {
        return FindAssignment(activeAssignmentId);
    }

    public bool SetActiveAssignment(string assignmentId)
    {
        CampaignAssignment assignment = FindAssignment(assignmentId);
        if (assignment == null || !assignment.unlocked || assignment.completed)
        {
            return false;
        }

        activeAssignmentId = assignmentId;
        SaveCareer();
        return true;
    }

    public void AwardDailyChallengeCompletion()
    {
        int reward = bureauScoreManager != null ? bureauScoreManager.GetDailyChallengeReward() : 60;
        ApplyBureauScore(reward, "Daily challenge certified");
    }

    public void AwardAchievementMilestone(string achievementId)
    {
        if (string.IsNullOrWhiteSpace(achievementId))
        {
            return;
        }

        int reward = bureauScoreManager != null ? bureauScoreManager.GetAchievementReward() : 30;
        ApplyBureauScore(reward, $"Achievement filed: {achievementId}");
    }

    private void HandleCertificationCompleted(DungeonReport report, IReadOnlyList<RunResult> runs)
    {
        if (report == null)
        {
            return;
        }

        UpdateCareerStats(report, runs);

        int certReward = bureauScoreManager != null ? bureauScoreManager.GetCertificationReward() : 20;
        ApplyBureauScore(certReward, "Certification run filed");

        CampaignAssignment active = GetActiveAssignment();
        if (active == null)
        {
            return;
        }

        if (active.requireDailyChallengeCompletion && !HasUnlockedFeature("daily_certified"))
        {
            return;
        }

        if (!active.MeetsCompletion(report, runs))
        {
            return;
        }

        active.completed = true;
        if (!CareerData.completedAssignments.Contains(active.assignmentID))
        {
            CareerData.completedAssignments.Add(active.assignmentID);
        }

        int assignmentReward = bureauScoreManager != null
            ? bureauScoreManager.ComputeAssignmentReward(active, report)
            : active.bureauScoreReward;

        ApplyBureauScore(assignmentReward, $"Assignment completed: {active.title}");
        UnlockProgressionFeatures();
        UnlockNextAssignments();

        OnAssignmentCompleted?.Invoke(active, assignmentReward);
        SaveCareer();
        OnCareerDataChanged?.Invoke(CareerData);
    }

    private void UpdateCareerStats(DungeonReport report, IReadOnlyList<RunResult> runs)
    {
        CareerData.totalCertifications++;
        CareerData.totalBotFatalities += report.deaths;
        CareerData.totalSurvivals += report.survivors;

        int totalBots = Mathf.Max(1, CareerData.totalSurvivals + CareerData.totalBotFatalities);
        CareerData.averageSurvivalRate = CareerData.totalSurvivals / (float)totalBots;

        if (CampaignMath.GetRatingRank(report.rating) > CampaignMath.GetRatingRank(CareerData.bestDungeonRating))
        {
            CareerData.bestDungeonRating = report.rating;
        }
    }

    private void ApplyBureauScore(int amount, string reason)
    {
        int positive = Mathf.Max(0, amount);
        CareerData.bureauScore += positive;
        bureauScoreManager?.RaiseScoreAwarded(positive, reason);
        EvaluatePromotion();
    }

    private void EvaluatePromotion()
    {
        int rankIndex = 0;
        for (int i = 0; i < promotionThresholds.Count; i++)
        {
            if (CareerData.bureauScore >= promotionThresholds[i])
            {
                rankIndex = i;
            }
        }

        rankIndex = Mathf.Clamp(rankIndex, 0, rankTitles.Count - 1);
        string newRank = rankTitles[rankIndex];
        if (string.Equals(newRank, CareerData.currentRank, StringComparison.Ordinal))
        {
            return;
        }

        CareerData.currentRank = newRank;
        List<string> unlocks = UnlockProgressionFeatures();
        OnPromotionGranted?.Invoke(newRank, CampaignMath.BuildPromotionMessage(newRank), unlocks);
    }

    private List<string> UnlockProgressionFeatures()
    {
        List<string> unlocks = new();

        TryUnlockFeature("campaign_reports", CareerData.bureauScore >= 250, "Bureau report annex");
        TryUnlockFeature("decor_theme_iron_keep", CareerData.bureauScore >= 600, "Decor theme: Iron Keep");
        TryUnlockFeature("advanced_assignments", CareerData.bureauScore >= 1000, "Advanced assignment pool");
        TryUnlockFeature("stress_test_cert_mode", CareerData.bureauScore >= 1400, "Stress test certification mode");
        TryUnlockFeature("daily_certified", CareerData.totalCertifications >= 3, "Daily challenge certification credit");

        void TryUnlockFeature(string id, bool condition, string label)
        {
            if (!condition || CareerData.unlockedFeatures.Contains(id))
            {
                return;
            }

            CareerData.unlockedFeatures.Add(id);
            unlocks.Add(label);
        }

        return unlocks;
    }

    private void EnsureUnlockState()
    {
        if (tiers.Count == 0)
        {
            return;
        }

        if (CareerData.unlockedTiers.Count == 0)
        {
            CareerData.unlockedTiers.Add(tiers[0].tierID);
        }

        foreach (CampaignTier tier in tiers)
        {
            bool tierUnlocked = CareerData.unlockedTiers.Contains(tier.tierID) || CareerData.bureauScore >= tier.unlockBureauScore;
            if (tierUnlocked && !CareerData.unlockedTiers.Contains(tier.tierID))
            {
                CareerData.unlockedTiers.Add(tier.tierID);
            }

            for (int i = 0; i < tier.assignments.Count; i++)
            {
                CampaignAssignment assignment = tier.assignments[i];
                bool unlockedByOrder = i == 0 || tier.assignments[i - 1].completed;
                assignment.unlocked = tierUnlocked && (assignment.unlocked || unlockedByOrder);
                assignment.completed = CareerData.completedAssignments.Contains(assignment.assignmentID) || assignment.completed;
            }
        }

        UnlockNextAssignments();
        UnlockProgressionFeatures();
        SaveCareer();
    }

    private void UnlockNextAssignments()
    {
        foreach (CampaignTier tier in tiers)
        {
            bool tierUnlocked = CareerData.unlockedTiers.Contains(tier.tierID);
            if (!tierUnlocked)
            {
                if (CareerData.bureauScore >= tier.unlockBureauScore)
                {
                    CareerData.unlockedTiers.Add(tier.tierID);
                }
                else
                {
                    continue;
                }
            }

            bool unlockNext = true;
            foreach (CampaignAssignment assignment in tier.assignments)
            {
                if (unlockNext)
                {
                    assignment.unlocked = true;
                }

                if (!assignment.completed)
                {
                    unlockNext = false;
                }
            }
        }
    }

    private CampaignAssignment FindAssignment(string assignmentId)
    {
        if (string.IsNullOrWhiteSpace(assignmentId))
        {
            return null;
        }

        foreach (CampaignTier tier in tiers)
        {
            CampaignAssignment found = tier.assignments.FirstOrDefault(a => a.assignmentID == assignmentId);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void EnsureDefaultCampaign()
    {
        if (tiers.Count > 0)
        {
            return;
        }

        tiers = new List<CampaignTier>
        {
            new()
            {
                tierID = "tier_1",
                title = "Junior Architect",
                unlockBureauScore = 0,
                tierFlavor = "You are licensed to place hazards under direct supervision.",
                assignments = new List<CampaignAssignment>
                {
                    new() { assignmentID = "A-101", title = "Introductory Saw Placement Audit", description = "Place legal saw spacing and earn at least a Fair rating.", requiredRating = "Fair", trapBudget = 8, bureauScoreReward = 120, flavorMemo = "Memo: This corridor meets minimum saw-spacing requirements." },
                    new() { assignmentID = "A-102", title = "Bomb Corridor Compliance Review", description = "Demonstrate controlled volatility with one documented fatality.", requiredRating = "Unsafe", requireAtLeastOneFatality = true, trapBudget = 10, bureauScoreReward = 140, flavorMemo = "Memo: Bomb placement remains technically legal, though deeply frowned upon." }
                }
            },
            new()
            {
                tierID = "tier_2",
                title = "Certified Architect",
                unlockBureauScore = 450,
                tierFlavor = "You may now certify multi-bot lethality studies.",
                assignments = new List<CampaignAssignment>
                {
                    new() { assignmentID = "B-201", title = "Multi-Bot Survivability Assessment", description = "Hit a Safe rating with strong survivability evidence.", requiredRating = "Safe", minimumSurvivalRate = 0.66f, trapBudget = 12, bureauScoreReward = 180, flavorMemo = "Notice: At least one bot should survive so legal can claim due process." },
                    new() { assignmentID = "B-202", title = "Reckless Bot Fatality Investigation", description = "Force a reckless run failure while maintaining bureau standards.", requiredRating = "Fair", requireAtLeastOneFatality = true, trapBudget = 13, bureauScoreReward = 190, flavorMemo = "No surviving bot signatures were recovered. Filing continues." }
                }
            },
            new()
            {
                tierID = "tier_3",
                title = "Senior Hazard Planner",
                unlockBureauScore = 1100,
                tierFlavor = "Regional offices now trust your paperwork and your cruelty.",
                assignments = new List<CampaignAssignment>
                {
                    new() { assignmentID = "C-301", title = "Pit Maze Licensing Trial", description = "Publish an Impossible-rated dungeon for elite review.", requiredRating = "Impossible", trapBudget = 15, bureauScoreReward = 260, flavorMemo = "Directive: Any pit wider than one tile requires a decorative warning skull." },
                    new() { assignmentID = "C-302", title = "Daily Compliance Drill", description = "Complete a daily challenge and submit one full certification packet.", requiredRating = "Fair", requireDailyChallengeCompletion = true, trapBudget = 15, bureauScoreReward = 230, flavorMemo = "Attachment missing: witness statement from surviving personnel." }
                }
            }
        };
    }

    [ContextMenu("Reset Career Save")]
    public void ResetCareer()
    {
        CareerData = new PlayerCareerData();
        activeAssignmentId = string.Empty;
        foreach (CampaignTier tier in tiers)
        {
            foreach (CampaignAssignment assignment in tier.assignments)
            {
                assignment.completed = false;
                assignment.unlocked = false;
            }
        }

        EnsureUnlockState();
        SaveCareer();
        OnCareerDataChanged?.Invoke(CareerData);
    }

    private void LoadCareer()
    {
        if (!File.Exists(SavePath))
        {
            CareerData = new PlayerCareerData();
            return;
        }

        try
        {
            CareerData = JsonUtility.FromJson<PlayerCareerData>(File.ReadAllText(SavePath)) ?? new PlayerCareerData();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not load career data. Using defaults. {ex.Message}");
            CareerData = new PlayerCareerData();
        }
    }

    private void SaveCareer()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(CareerData, true));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to save career data. {ex.Message}");
        }
    }
}
