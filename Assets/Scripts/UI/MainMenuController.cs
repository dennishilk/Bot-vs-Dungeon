using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using BotVsDungeon.Evolution;

namespace BotVsDungeon.UI
{
    /// <summary>
    /// Owns menu button behavior. Supports both scene loading and panel-based flow.
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

        [Header("Events")]
        [SerializeField] private UnityEvent onStartGame;
        [SerializeField] private UnityEvent onOpenSettingsPlaceholder;

        public void StartGame()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.ButtonClick);
            onStartGame?.Invoke();

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
            uiController?.ShowHUD();
            HideOptionalPanels();
        }

        public void OpenSandboxMode()
        {
            uiController?.ShowHUD();
            HideOptionalPanels();
        }

        public void OpenDungeonBrowser()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
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
            Debug.Log("Replay Viewer selected.");
        }

        public void OpenDirectorMode()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
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
            uiController?.ShowHUD();
            profilePanelController?.Refresh();
            campaignScreenController?.SetVisible(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            directorModePanel?.SetVisible(false);
            evolutionUIController?.SetVisible(false);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OpenSettingsPlaceholder()
        {
            AudioManager.Instance?.PlayUI(UIAudioEvent.PanelOpen);
            onOpenSettingsPlaceholder?.Invoke();
            Debug.Log("Settings placeholder selected. No settings menu implemented.");
            AudioManager.Instance?.PlayMusicTrack(MusicTrackType.Menu);
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
        }
    }
}
