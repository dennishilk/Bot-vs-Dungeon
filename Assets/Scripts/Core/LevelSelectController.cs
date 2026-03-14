using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [System.Serializable]
    private class TilePrefabEntry
    {
        public TileType tileType;
        public GameObject prefab;
    }

    [Header("Core References")]
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private TrapBudgetManager trapBudgetManager;
    [SerializeField] private LevelObjectiveManager objectiveManager;
    [SerializeField] private SimulationManager simulationManager;

    [Header("UI")]
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text levelBudgetText;
    [SerializeField] private TMP_Text levelObjectiveText;
    [SerializeField] private TMP_Text levelCompletionText;
    [SerializeField] private Button startLevelButton;

    [Header("Level Data")]
    [SerializeField] private List<DungeonLevel> challengeLevels = new();
    [SerializeField] private List<TilePrefabEntry> tilePrefabs = new();

    private readonly Dictionary<TileType, GameObject> _prefabMap = new();
    private int _selectedLevelIndex;

    private void Awake()
    {
        RebuildPrefabMap();

        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveEvaluated += HandleObjectiveEvaluated;
        }

        if (startLevelButton != null)
        {
            startLevelButton.onClick.AddListener(() => StartLevel(_selectedLevelIndex));
        }

        RefreshLevelPreview();
    }


    private void OnDestroy()
    {
        if (objectiveManager != null)
        {
            objectiveManager.OnObjectiveEvaluated -= HandleObjectiveEvaluated;
        }
    }

    public void SelectLevel(int index)
    {
        _selectedLevelIndex = Mathf.Clamp(index, 0, Mathf.Max(0, challengeLevels.Count - 1));
        RefreshLevelPreview();
    }

    public void StartLevel(int index)
    {
        if (challengeLevels.Count == 0 || arenaManager == null)
        {
            return;
        }

        DungeonLevel level = challengeLevels[Mathf.Clamp(index, 0, challengeLevels.Count - 1)];

        if (simulationManager != null && simulationManager.IsSimulationRunning)
        {
            simulationManager.ResetRun();
        }

        arenaManager.ClearAll();
        foreach (DungeonTilePlacement tile in level.initialDungeonLayout)
        {
            GameObject prefab = ResolvePrefab(tile.tileType);
            GameObject instance = prefab != null
                ? Instantiate(prefab, new Vector3(tile.position.x, 0f, tile.position.y), Quaternion.identity)
                : null;

            TrapBase trap = instance != null ? instance.GetComponent<TrapBase>() : null;
            arenaManager.SetTile(tile.position, tile.tileType, instance, trap);
        }

        trapBudgetManager?.SetMaxBudget(level.trapBudget);
        trapBudgetManager?.SetAllowedTrapTypes(level.allowedTrapTypes);
        trapBudgetManager?.OnArenaChanged();
        objectiveManager?.SetObjective(level.objective);

        _selectedLevelIndex = index;
        RefreshLevelPreview();
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
    }

    public void SetLevelSelectVisible(bool visible)
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(visible);
        }
    }


    private void HandleObjectiveEvaluated(bool success)
    {
        if (!success || challengeLevels.Count == 0)
        {
            return;
        }

        challengeLevels[Mathf.Clamp(_selectedLevelIndex, 0, challengeLevels.Count - 1)].completed = true;
        RefreshLevelPreview();
    }

    private void RefreshLevelPreview()
    {
        if (challengeLevels.Count == 0)
        {
            if (levelNameText != null) levelNameText.text = "No Levels Configured";
            if (levelBudgetText != null) levelBudgetText.text = "Budget: --";
            if (levelObjectiveText != null) levelObjectiveText.text = "Objective: --";
            if (levelCompletionText != null) levelCompletionText.text = string.Empty;
            return;
        }

        DungeonLevel level = challengeLevels[Mathf.Clamp(_selectedLevelIndex, 0, challengeLevels.Count - 1)];

        if (levelNameText != null) levelNameText.text = level.levelName;
        if (levelBudgetText != null) levelBudgetText.text = $"Budget: {level.trapBudget}";
        if (levelObjectiveText != null) levelObjectiveText.text = $"Objective: {level.objective.GetDisplayText()}";
        if (levelCompletionText != null) levelCompletionText.text = level.completed ? "Completed ✓" : "Not Completed";
    }

    private void RebuildPrefabMap()
    {
        _prefabMap.Clear();
        foreach (TilePrefabEntry entry in tilePrefabs)
        {
            _prefabMap[entry.tileType] = entry.prefab;
        }
    }

    private GameObject ResolvePrefab(TileType tileType)
    {
        return _prefabMap.TryGetValue(tileType, out GameObject prefab) ? prefab : null;
    }
}
