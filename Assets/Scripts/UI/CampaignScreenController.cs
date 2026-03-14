using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CampaignScreenController : MonoBehaviour
{
    [SerializeField] private CampaignManager campaignManager;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text bureauScoreText;
    [SerializeField] private TMP_Text promotionProgressText;
    [SerializeField] private TMP_Text assignmentListText;

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }

        if (visible)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (campaignManager == null)
        {
            return;
        }

        PlayerCareerData data = campaignManager.CareerData;
        if (rankText != null)
        {
            rankText.text = $"Rank: {data.currentRank}";
        }

        if (bureauScoreText != null)
        {
            bureauScoreText.text = $"Bureau Score: {data.bureauScore}";
        }

        if (promotionProgressText != null)
        {
            promotionProgressText.text = BuildPromotionProgress();
        }

        if (assignmentListText != null)
        {
            assignmentListText.text = BuildAssignmentList(campaignManager.GetTiers());
        }
    }

    private string BuildPromotionProgress()
    {
        return $"Assignments Completed: {campaignManager.CareerData.completedAssignments.Count}\n" +
               $"Unlocked Tiers: {campaignManager.CareerData.unlockedTiers.Count}";
    }

    private static string BuildAssignmentList(IReadOnlyList<CampaignTier> tiers)
    {
        System.Text.StringBuilder sb = new();
        foreach (CampaignTier tier in tiers)
        {
            sb.AppendLine($"[{tier.title}]");
            foreach (CampaignAssignment assignment in tier.assignments)
            {
                string status = assignment.completed ? "Completed" : assignment.unlocked ? "Available" : "Locked";
                sb.AppendLine($" - {assignment.title} ({status})");
            }
        }

        return sb.ToString();
    }
}
