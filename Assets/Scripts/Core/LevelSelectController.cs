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
    [SerializeField] private CertificationManager certificationManager;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private LevelCompleteScreen levelCompleteScreen;

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

        progressionManager?.Initialize(challengeLevels.Count);
        SyncProgressionFlags();
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

        int clampedIndex = Mathf.Clamp(index, 0, challengeLevels.Count - 1);
        if (progressionManager != null && !progressionManager.IsLevelUnlocked(clampedIndex))
        {
            return;
        }

        DungeonLevel level = challengeLevels[clampedIndex];

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

        _selectedLevelIndex = clampedIndex;
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

        int index = Mathf.Clamp(_selectedLevelIndex, 0, challengeLevels.Count - 1);
        challengeLevels[index].completed = true;
        progressionManager?.MarkLevelCompleted(index);
        SyncProgressionFlags();

        string rating = certificationManager != null && certificationManager.LastReport != null
            ? certificationManager.LastReport.rating
            : "Unrated";
        int stars = CalculateStars(rating);
        levelCompleteScreen?.Show(challengeLevels[index].levelName, "Objective Completed", rating, stars);
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
        bool unlocked = progressionManager == null || progressionManager.IsLevelUnlocked(Mathf.Clamp(_selectedLevelIndex, 0, challengeLevels.Count - 1));
        if (levelCompletionText != null)
        {
            if (!unlocked)
            {
                levelCompletionText.text = "Locked";
            }
            else
            {
                levelCompletionText.text = level.completed ? "Completed ✓" : "Not Completed";
            }
        }

        if (startLevelButton != null)
        {
            startLevelButton.interactable = unlocked;
        }
    }


    private void SyncProgressionFlags()
    {
        if (progressionManager == null)
        {
            return;
        }

        for (int i = 0; i < challengeLevels.Count; i++)
        {
            challengeLevels[i].completed = progressionManager.IsLevelCompleted(i);
        }
    }

    private static int CalculateStars(string rating)
    {
        return rating switch
        {
            "Safe" => 3,
            "Fair" => 2,
            _ => 1
        };
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
