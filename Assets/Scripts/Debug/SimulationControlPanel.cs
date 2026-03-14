using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationControlPanel : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private EventLogger eventLogger;

    [Header("Buttons")]
    [SerializeField] private Button spawnBotButton;
    [SerializeField] private Button runSimulationButton;
    [SerializeField] private Button resetRunButton;
    [SerializeField] private Button clearDungeonButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button stepButton;
    [SerializeField] private Button clearLogButton;

    [Header("Toggles")]
    [SerializeField] private Toggle slowMotionToggle;
    [SerializeField] private Toggle showBotPathToggle;
    [SerializeField] private Toggle showDangerMapToggle;
    [SerializeField] private Toggle showTileGridToggle;
    [SerializeField] private Toggle showTrapRangeToggle;

    [Header("Personality")]
    [SerializeField] private TMP_Dropdown personalityDropdown;

    private void Awake()
    {
        Bind();
    }

    private void Bind()
    {
        spawnBotButton?.onClick.AddListener(simulationManager.SpawnBot);
        runSimulationButton?.onClick.AddListener(simulationManager.StartSimulation);
        resetRunButton?.onClick.AddListener(simulationManager.ResetRun);
        clearDungeonButton?.onClick.AddListener(simulationManager.ClearDungeon);
        pauseButton?.onClick.AddListener(simulationManager.PauseSimulation);
        resumeButton?.onClick.AddListener(simulationManager.ResumeSimulation);
        stepButton?.onClick.AddListener(simulationManager.StepSimulation);
        clearLogButton?.onClick.AddListener(() => eventLogger?.ClearLog());

        slowMotionToggle?.onValueChanged.AddListener(simulationManager.ToggleSlowMotion);

        showBotPathToggle?.onValueChanged.AddListener(v => DebugPathVisualizer.ShowBotPath = v);
        showDangerMapToggle?.onValueChanged.AddListener(v => DebugPathVisualizer.ShowDangerMap = v);
        showTileGridToggle?.onValueChanged.AddListener(v => DebugPathVisualizer.ShowTileGrid = v);
        showTrapRangeToggle?.onValueChanged.AddListener(v => DebugPathVisualizer.ShowTrapRange = v);

        if (personalityDropdown != null)
        {
            personalityDropdown.ClearOptions();
            personalityDropdown.AddOptions(new System.Collections.Generic.List<string> { "Careful", "Balanced", "Reckless" });
            personalityDropdown.value = 1;
            personalityDropdown.onValueChanged.AddListener(OnPersonalityChanged);
        }
    }

    private void OnPersonalityChanged(int index)
    {
        BotPersonality personality = (BotPersonality)Mathf.Clamp(index, 0, 2);
        simulationManager.SetBotPersonality(personality);
    }
}
