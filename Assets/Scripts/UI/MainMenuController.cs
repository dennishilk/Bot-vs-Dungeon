using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

        [Header("Events")]
        [SerializeField] private UnityEvent onStartGame;
        [SerializeField] private UnityEvent onOpenSettingsPlaceholder;

        public void StartGame()
        {
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
            uiController?.ShowHUD();
            dungeonBrowserPanel?.SetVisible(true);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
        }

        public void OpenDailyChallenge()
        {
            uiController?.ShowHUD();
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(true);
            stressTestPanel?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
        }

        public void OpenStressTest()
        {
            uiController?.ShowHUD();
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(true);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
        }

        public void OpenReplayViewer()
        {
            Debug.Log("Replay Viewer selected.");
        }

        public void OpenDirectorMode()
        {
            uiController?.ShowHUD();
            directorModePanel?.SetVisible(true);
            campaignScreenController?.SetVisible(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
        }

        public void OpenCampaign()
        {
            uiController?.ShowHUD();
            campaignScreenController?.SetVisible(true);
            profilePanelController?.Refresh();
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            directorModePanel?.SetVisible(false);
        }

        public void OpenProfile()
        {
            uiController?.ShowHUD();
            profilePanelController?.Refresh();
            campaignScreenController?.SetVisible(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            directorModePanel?.SetVisible(false);
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
            onOpenSettingsPlaceholder?.Invoke();
            Debug.Log("Settings placeholder selected. No settings menu implemented.");
        }

        private void HideOptionalPanels()
        {
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
            campaignScreenController?.SetVisible(false);
            directorModePanel?.SetVisible(false);
        }
    }
}
