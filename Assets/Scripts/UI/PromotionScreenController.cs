using TMPro;
using UnityEngine;

public class PromotionScreenController : MonoBehaviour
{
    [SerializeField] private CampaignManager campaignManager;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text unlocksText;

    private void Awake()
    {
        if (campaignManager != null)
        {
            campaignManager.OnPromotionGranted += ShowPromotion;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        AudioManager.Instance?.PlayUI(UIAudioEvent.PanelClose);
    }

    private void OnDestroy()
    {
        if (campaignManager != null)
        {
            campaignManager.OnPromotionGranted -= ShowPromotion;
        }
    }

    public void ShowPromotion(string rankTitle, string message, System.Collections.Generic.IReadOnlyList<string> unlocks)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        AudioManager.Instance?.PlayUI(UIAudioEvent.PromotionEarned);
        AudioManager.Instance?.PlayMusicTrack(MusicTrackType.PromotionFanfare);

        if (titleText != null)
        {
            titleText.text = $"Promotion: {rankTitle}";
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (unlocksText != null)
        {
            unlocksText.text = unlocks == null || unlocks.Count == 0
                ? "No new bureau privileges issued."
                : "New Unlocks:\n - " + string.Join("\n - ", unlocks);
        }
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        AudioManager.Instance?.PlayUI(UIAudioEvent.PanelClose);
    }
}
