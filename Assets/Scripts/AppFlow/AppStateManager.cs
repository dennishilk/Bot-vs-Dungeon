using System;
using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    public enum AppState
    {
        Boot,
        MainMenu,
        Campaign,
        Sandbox,
        DailyChallenge,
        DirectorMode,
        EvolutionLab,
        ReplayViewer,
        Settings,
        ProfileSelection,
        Credits,
        ExitConfirmation,
        Loading,
        Result,
        LevelComplete,
        Promotion
    }

    public class AppStateManager : MonoBehaviour
    {
        public static AppStateManager Instance { get; private set; }

        [Header("Startup")]
        [SerializeField] private AppState defaultState = AppState.Boot;
        [SerializeField] private bool autoEnterMainMenuFromBoot = true;
        [SerializeField] private float bootDurationSeconds = 1.1f;

        [Header("Application")]
        [SerializeField] private bool enableQuitConfirmation = true;
        [SerializeField] private bool autoSaveOnStateChange = true;

        public AppState CurrentState { get; private set; } = AppState.Boot;
        public AppState PreviousState { get; private set; } = AppState.Boot;
        public bool EnableQuitConfirmation => enableQuitConfirmation;

        public event Action<AppState, AppState> OnStateChanged;

        private float _bootTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentState = defaultState;
            PreviousState = defaultState;
        }

        private void Update()
        {
            if (CurrentState != AppState.Boot || !autoEnterMainMenuFromBoot)
            {
                return;
            }

            _bootTimer += Time.unscaledDeltaTime;
            if (_bootTimer >= Mathf.Max(0f, bootDurationSeconds))
            {
                ChangeState(AppState.MainMenu);
            }
        }

        public void ChangeState(AppState nextState)
        {
            if (nextState == CurrentState)
            {
                return;
            }

            PreviousState = CurrentState;
            CurrentState = nextState;

            if (autoSaveOnStateChange)
            {
                PlayerProfileManager.Instance?.SaveActiveProfile();
            }

            OnStateChanged?.Invoke(CurrentState, PreviousState);
        }

        public void ReturnToMenu()
        {
            ChangeState(AppState.MainMenu);
        }

        public void RequestQuit()
        {
            if (enableQuitConfirmation)
            {
                ChangeState(AppState.ExitConfirmation);
                return;
            }

            QuitNow();
        }

        public void QuitNow()
        {
            PlayerProfileManager.Instance?.SaveActiveProfile();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
