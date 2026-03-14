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

            if (uiController != null)
            {
                uiController.ShowHUD();
            }
        }


        public void OpenChallengeLevels()
        {
            if (uiController != null)
            {
                uiController.ShowHUD();
            }
        }

        public void OpenSandboxMode()
        {
            if (uiController != null)
            {
                uiController.ShowHUD();
            }
        }


        public void OpenDungeonBrowser()
        {
            if (uiController != null)
            {
                uiController.ShowHUD();
            }

            dungeonBrowserPanel?.SetVisible(true);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
        }

        public void OpenDailyChallenge()
        {
            if (uiController != null)
            {
                uiController.ShowHUD();
            }

            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(true);
            stressTestPanel?.SetVisible(false);
        }

        public void OpenStressTest()
        {
            if (uiController != null)
            {
                uiController.ShowHUD();
            }

            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(true);
        }

        public void OpenReplayViewer()
        {
            Debug.Log("Replay Viewer selected.");
        }

        public void OpenCampaign()
        {
            if (uiController != null)
            {
                uiController.ShowHUD();
            }

            campaignScreenController?.SetVisible(true);
            profilePanelController?.Refresh();
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
        }

        public void OpenProfile()
        {
            if (uiController != null)
            {
                uiController.ShowHUD();
            }

            profilePanelController?.Refresh();
            campaignScreenController?.SetVisible(false);
            dungeonBrowserPanel?.SetVisible(false);
            dailyChallengePanel?.SetVisible(false);
            stressTestPanel?.SetVisible(false);
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
    }
}
