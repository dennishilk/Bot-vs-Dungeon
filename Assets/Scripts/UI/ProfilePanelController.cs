using TMPro;
using UnityEngine;

public class ProfilePanelController : MonoBehaviour
{
    [SerializeField] private CampaignManager campaignManager;
    [SerializeField] private TMP_Text profileText;

    public void Refresh()
    {
        if (campaignManager == null || profileText == null)
        {
            return;
        }

        PlayerCareerData data = campaignManager.CareerData;
        int tested = data.totalSurvivals + data.totalBotFatalities;

        profileText.text =
            $"Current Rank: {data.currentRank}\n" +
            $"Bureau Score: {data.bureauScore}\n" +
            $"Assignments Completed: {data.completedAssignments.Count}\n" +
            $"Total Bots Tested: {tested}\n" +
            $"Total Bot Fatalities: {data.totalBotFatalities}\n" +
            $"Average Survival Rate: {data.averageSurvivalRate:P0}\n" +
            $"Best Dungeon Rating: {data.bestDungeonRating}";
    }
}
