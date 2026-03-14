using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using BotVsDungeon.Evolution;
using BotVsDungeon.AppFlow;

namespace BotVsDungeon.UI
{
    /// <summary>
    /// Central entry for top-level menu actions and app-state navigation.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private bool useSceneLoading = false;
        [SerializeField] private string gameSceneName = "Main";
        [SerializeField] private UIController uiController;
        [SerializeField] private SceneTransitionController sceneTransitionController;
        [SerializeField] private DungeonBrowserPanel dungeonBrowserPanel;
        [SerializeField] private DailyChallengePanel dailyChallengePanel;
        [SerializeField] private StressTestPanel stressTestPanel;
        [SerializeField] private CampaignScreenController campaignScreenController;
        [SerializeField] private ProfilePanelController profilePanelController;
        [SerializeField] private DirectorModePanel directorModePanel;
        [SerializeField] private EvolutionUIController evolutionUIController;
        [SerializeField] private SaveLoadMenuController saveLoadMenuController;
        [SerializeField] private SettingsMenuController settingsMenuController;
        [SerializeField] private ProfileSelectionController profileSelectionController;

        [Header("Events")]
        [SerializeField] private UnityEvent onStartGame;
        [SerializeField] private UnityEvent onOpenSettingsPlaceholder;

        public void StartGame()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.ButtonClick);
            onStartGame?.Invoke();
            AppStateManager.Instance?.ChangeState(AppState.Sandbox);

            if (useSceneLoading)
            {
                SceneTransitionController transition = sceneTransitionController != null ? sceneTransitionController : SceneTransitionController.Instance;
                if (transition != null)
                {
                    transition.FadeAndLoadScene(gameSceneName);
                }
                else
                {
                    SceneManager.LoadScene(gameSceneName);
                }
                return;
            }

            uiController?.ShowHUD();
        }

        public void OpenChallengeLevels()
        {
            AppStateManager.Instance?.ChangeState(AppState.Campaign);
            uiController?.ShowHUD();
            HideOptionalPanels();
        }

        public void OpenSandboxMode()
        {
            AppStateManager.Instance?.ChangeState(AppState.Sandbox);
            uiController?.ShowHUD();
            HideOptionalPanels();
        }

        public void OpenDungeonBrowser()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.Sandbox);
            uiController?.ShowHUD();
            dungeonBrowserPanel?.SetVisible(true);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
            evolutionUIController?.SetVisible(false);
        }

        public void OpenDailyChallenge()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.DailyChallenge);
            uiController?.ShowHUD();
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(true);
            stressTestPanel?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
            evolutionUIController?.SetVisible(false);
        }

        public void OpenStressTest()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.Sandbox);
            uiController?.ShowHUD();
            AudioManager.Instance?.SetStressTestTension(true);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(true);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
            evolutionUIController?.SetVisible(false);
        }

        public void OpenReplayViewer()
        {
            AppStateManager.Instance?.ChangeState(AppState.ReplayViewer);
        }

        public void OpenDirectorMode()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.DirectorMode);
            uiController?.ShowHUD();
            directorModePanel?.SetVisible(true);
            evolutionUIController?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
        }

        public void OpenEvolutionLab()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.EvolutionLab);
            uiController?.ShowHUD();
            evolutionUIController?.SetVisible(true);
            directorModePanel?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
        }

        public void OpenCampaign()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.Campaign);
            uiController?.ShowHUD();
            campaignScreenController?.SetVisible(true);
            profilePanelController?.Refresh();
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            directorModePanel?.SetVisible(false);
            evolutionUIController?.SetVisible(false);
        }

        public void OpenProfile()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.ProfileSelection);
            uiController?.ShowHUD();
            profilePanelController?.Refresh();
            profileSelectionController?.Refresh();
            campaignScreenController?.SetVisible(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            directorModePanel?.SetVisible(false);
            evolutionUIController?.SetVisible(false);
        }

        public void OpenSaveLoadPanel()
        {
            AppStateManager.Instance?.ChangeState(AppState.Sandbox);
            saveLoadMenuController?.SetVisible(true);
        }

        public void QuitGame()
        {
            AppStateManager.Instance?.RequestQuit();
        }

        public void OpenSettingsPlaceholder()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            AppStateManager.Instance?.ChangeState(AppState.Settings);
            settingsMenuController?.SetVisible(true);
            onOpenSettingsPlaceholder?.Invoke();
            AudioManager.Instance?.PlayMusicTrack(MusicTrackType.Menu);
        }

        public void OpenCredits()
        {
            AppStateManager.Instance?.ChangeState(AppState.Credits);
        }

        private void HideOptionalPanels()
        {
            AudioManager.Instance?.SetStressTestTension(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
            evolutionUIController?.SetVisible(false);
            settingsMenuController?.SetVisible(false);
            saveLoadMenuController?.SetVisible(false);
        }
    }
}
