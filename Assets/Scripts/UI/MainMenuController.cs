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

        [Header("Events")]
        [SerializeField] private UnityEvent onStartGame;
        [SerializeField] private UnityEvent onOpenSettingsPlaceholder;

        public void StartGame()
        {
            onStartGame?.Invoke();

            if (useSceneLoading)
            {
                SceneManager.LoadScene(gameSceneName);
                return;
            }

            if (uiController != null)
            {
                uiController.ShowHUD();
            }
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
