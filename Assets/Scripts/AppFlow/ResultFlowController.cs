using TMPro;
using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    public class ResultFlowController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private GameFlowController gameFlowController;
        [SerializeField] private SaveLoadMenuController saveLoadMenuController;

        private DungeonReport _lastReport;

        public void Show(DungeonReport report)
        {
            _lastReport = report;
            panelRoot?.SetActive(true);
            if (summaryText != null)
            {
                summaryText.text = report == null
                    ? "No run report available."
                    : $"Rating: {report.rating}\nVerdict: {report.verdict}\nRuns: {report.totalRuns}\nSurvivals: {report.totalSurvivals}";
            }
        }

        public void Hide()
        {
            panelRoot?.SetActive(false);
        }

        public void WatchReplayClicked() => gameFlowController?.WatchReplay();
        public void WatchHighlightsClicked() => gameFlowController?.WatchHighlights();
        public void EditDungeonClicked() => AppStateManager.Instance?.ChangeState(AppState.Sandbox);
        public void SaveDungeonClicked() => saveLoadMenuController?.SetVisible(true);
        public void RunAgainClicked() => gameFlowController?.StartSingleRun();
        public void ReturnToMenuClicked() => gameFlowController?.ReturnToMenu();
    }
}
