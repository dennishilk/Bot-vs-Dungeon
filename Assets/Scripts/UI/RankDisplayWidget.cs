using TMPro;
using UnityEngine;

public class RankDisplayWidget : MonoBehaviour
{
    [SerializeField] private CampaignManager campaignManager;
    [SerializeField] private TMP_Text rankText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (campaignManager == null || rankText == null)
        {
            return;
        }

        rankText.text = $"{campaignManager.CareerData.currentRank} | Score {campaignManager.CareerData.bureauScore}";
    }
}
