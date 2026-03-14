using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private SimulationManager simulationManager;
        [SerializeField] private CertificationManager certificationManager;
        [SerializeField] private StressTestManager stressTestManager;
        [SerializeField] private ReplayViewer replayViewer;
        [SerializeField] private HighlightReplayPlayer highlightReplayPlayer;
        [SerializeField] private SaveLoadMenuController saveLoadMenuController;
        [SerializeField] private ResultFlowController resultFlowController;

        [Header("Defaults")]
        [SerializeField] private int defaultStressTestRuns = 20;

        public void EnterMode(AppState mode)
        {
            AppStateManager.Instance?.ChangeState(mode);
        }

        public void StartSingleRun()
        {
            simulationManager?.StartSimulation();
        }

        public void RunCertification()
        {
            certificationManager?.StartCertificationRun();
        }

        public void RunStressTest()
        {
            stressTestManager?.StartStressTest(defaultStressTestRuns);
        }

        public void WatchReplay()
        {
            AppStateManager.Instance?.ChangeState(AppState.ReplayViewer);
            replayViewer?.Play();
        }

        public void WatchHighlights()
        {
            AppStateManager.Instance?.ChangeState(AppState.ReplayViewer);
            highlightReplayPlayer?.PlayHighlights();
        }

        public void OpenSaveLoad()
        {
            saveLoadMenuController?.SetVisible(true);
        }

        public void ReturnToMenu()
        {
            saveLoadMenuController?.SetVisible(false);
            resultFlowController?.Hide();
            AppStateManager.Instance?.ReturnToMenu();
        }

        public void HandleRunFinished(DungeonReport report)
        {
            AppStateManager.Instance?.ChangeState(AppState.Result);
            resultFlowController?.Show(report);
        }
    }
}
