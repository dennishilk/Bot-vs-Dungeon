using TMPro;
using UnityEngine;

public class StressTestPanel : MonoBehaviour
{
    [SerializeField] private StressTestManager stressTestManager;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject panelRoot;

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    public void Run10Bots() => Run(10);
    public void Run20Bots() => Run(20);
    public void Run50Bots() => Run(50);

    public void StopRun()
    {
        stressTestManager?.StopStressTest();
        SetStatus("Stress test stopped.", false);
    }

    private void Run(int count)
    {
        if (stressTestManager == null)
        {
            SetStatus("StressTestManager is missing.", false);
            return;
        }

        stressTestManager.StartStressTest(count);
        SetStatus($"Started stress test ({count} bots).", true);
    }

    private void SetStatus(string message, bool success)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = success ? new Color(0.5f, 0.95f, 0.6f) : new Color(0.95f, 0.45f, 0.45f);
        }
    }
}
