using TMPro;
using UnityEngine;

public class AssignmentDetailsPanel : MonoBehaviour
{
    [SerializeField] private CampaignManager campaignManager;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text memoText;

    public void ShowAssignment(string assignmentId)
    {
        if (campaignManager == null)
        {
            return;
        }

        foreach (CampaignTier tier in campaignManager.GetTiers())
        {
            foreach (CampaignAssignment assignment in tier.assignments)
            {
                if (assignment.assignmentID != assignmentId)
                {
                    continue;
                }

                ApplyTexts(assignment);
                return;
            }
        }
    }

    private void ApplyTexts(CampaignAssignment assignment)
    {
        if (titleText != null)
        {
            titleText.text = assignment.title;
        }

        if (detailsText != null)
        {
            detailsText.text = $"Objective: {assignment.description}\n" +
                               $"Trap Budget: {assignment.trapBudget}\n" +
                               $"Required Rating: {assignment.requiredRating}\n" +
                               $"Reward: {assignment.bureauScoreReward} Bureau Score";
        }

        if (memoText != null)
        {
            memoText.text = assignment.flavorMemo;
        }
    }
}
