using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BotVsDungeon.AppFlow
{
    public class SceneRouter : MonoBehaviour
    {
        [System.Serializable]
        public class StateSceneRoute
        {
            public AppState state;
            public string sceneName;
        }

        [Header("Optional Scene Routing")]
        [SerializeField] private bool useSceneRouting;
        [SerializeField] private List<StateSceneRoute> stateSceneRoutes = new();

        [Header("Panel Routing")]
        [SerializeField] private GameObject loadingOverlayPanel;
        [SerializeField] private float loadingOverlayMinimumTime = 0.2f;
        [SerializeField] private List<GameObject> hideForAllStates = new();
        [SerializeField] private List<GameObject> mainMenuPanels = new();
        [SerializeField] private List<GameObject> campaignPanels = new();
        [SerializeField] private List<GameObject> sandboxPanels = new();
        [SerializeField] private List<GameObject> dailyChallengePanels = new();
        [SerializeField] private List<GameObject> directorModePanels = new();
        [SerializeField] private List<GameObject> evolutionLabPanels = new();
        [SerializeField] private List<GameObject> replayViewerPanels = new();
        [SerializeField] private List<GameObject> settingsPanels = new();
        [SerializeField] private List<GameObject> profilePanels = new();
        [SerializeField] private List<GameObject> creditsPanels = new();
        [SerializeField] private List<GameObject> resultPanels = new();
        [SerializeField] private List<GameObject> exitPanels = new();

        private readonly Dictionary<AppState, string> _sceneMap = new();
        private Coroutine _loadingRoutine;

        private void Awake()
        {
            foreach (StateSceneRoute route in stateSceneRoutes)
            {
                if (!string.IsNullOrWhiteSpace(route.sceneName))
                {
                    _sceneMap[route.state] = route.sceneName;
                }
            }
        }

        private void OnEnable()
        {
            if (AppStateManager.Instance != null)
            {
                AppStateManager.Instance.OnStateChanged += HandleStateChanged;
                HandleStateChanged(AppStateManager.Instance.CurrentState, AppStateManager.Instance.PreviousState);
            }
        }

        private void OnDisable()
        {
            if (AppStateManager.Instance != null)
            {
                AppStateManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(AppState state, AppState _)
        {
            if (_loadingRoutine != null)
            {
                StopCoroutine(_loadingRoutine);
            }

            _loadingRoutine = StartCoroutine(ApplyStateRoutine(state));
        }

        private IEnumerator ApplyStateRoutine(AppState state)
        {
            SetLoadingOverlay(true);
            float startedAt = Time.unscaledTime;

            if (useSceneRouting && _sceneMap.TryGetValue(state, out string sceneName) && SceneManager.GetActiveScene().name != sceneName)
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
                while (!load.isDone)
                {
                    yield return null;
                }
            }

            HideAllPanels();
            SetPanelListVisible(GetPanelsForState(state), true);

            float elapsed = Time.unscaledTime - startedAt;
            if (elapsed < loadingOverlayMinimumTime)
            {
                yield return new WaitForSecondsRealtime(loadingOverlayMinimumTime - elapsed);
            }

            SetLoadingOverlay(false);
            _loadingRoutine = null;
        }

        private List<GameObject> GetPanelsForState(AppState state)
        {
            return state switch
            {
                AppState.Campaign => campaignPanels,
                AppState.Sandbox => sandboxPanels,
                AppState.DailyChallenge => dailyChallengePanels,
                AppState.DirectorMode => directorModePanels,
                AppState.EvolutionLab => evolutionLabPanels,
                AppState.ReplayViewer => replayViewerPanels,
                AppState.Settings => settingsPanels,
                AppState.ProfileSelection => profilePanels,
                AppState.Credits => creditsPanels,
                AppState.Result => resultPanels,
                AppState.ExitConfirmation => exitPanels,
                _ => mainMenuPanels
            };
        }

        private void HideAllPanels()
        {
            SetPanelListVisible(hideForAllStates, false);
            SetPanelListVisible(mainMenuPanels, false);
            SetPanelListVisible(campaignPanels, false);
            SetPanelListVisible(sandboxPanels, false);
            SetPanelListVisible(dailyChallengePanels, false);
            SetPanelListVisible(directorModePanels, false);
            SetPanelListVisible(evolutionLabPanels, false);
            SetPanelListVisible(replayViewerPanels, false);
            SetPanelListVisible(settingsPanels, false);
            SetPanelListVisible(profilePanels, false);
            SetPanelListVisible(creditsPanels, false);
            SetPanelListVisible(resultPanels, false);
            SetPanelListVisible(exitPanels, false);
        }

        private static void SetPanelListVisible(List<GameObject> panels, bool visible)
        {
            if (panels == null)
            {
                return;
            }

            foreach (GameObject panel in panels)
            {
                if (panel != null)
                {
                    panel.SetActive(visible);
                }
            }
        }

        private void SetLoadingOverlay(bool visible)
        {
            if (loadingOverlayPanel != null)
            {
                loadingOverlayPanel.SetActive(visible);
            }
        }
    }
}
