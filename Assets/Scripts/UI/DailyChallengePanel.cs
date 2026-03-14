using UnityEngine;

public class DailyChallengePanel : MonoBehaviour
{
    [SerializeField] private DailyChallengeManager dailyChallengeManager;
    [SerializeField] private GameObject panelRoot;

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    public void PlayChallenge() => dailyChallengeManager?.PlayChallenge();
    public void ViewResults() => dailyChallengeManager?.ViewResults();
    public void ReplayRuns() => dailyChallengeManager?.ReplayRuns();
}
