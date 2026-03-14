using System.Collections.Generic;
using BotVsDungeon.UI;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TrapCostEntry
{
    public TileType tileType;
    public int cost;
}

public class TrapBudgetManager : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private TMP_Text budgetHeaderText;
    [SerializeField] private UIController uiController;
    [SerializeField] private TMP_Text budgetValueText;
    [SerializeField] private Color withinBudgetColor = new(0.45f, 0.9f, 0.55f);
    [SerializeField] private Color overBudgetColor = new(0.95f, 0.35f, 0.35f);
    [SerializeField] private int maxBudget = 10;
    [SerializeField] private List<TrapCostEntry> trapCostEntries = new()
    {
        new TrapCostEntry { tileType = TileType.Pit, cost = 1 },
        new TrapCostEntry { tileType = TileType.Saw, cost = 2 },
        new TrapCostEntry { tileType = TileType.Bomb, cost = 3 },
        new TrapCostEntry { tileType = TileType.Archer, cost = 4 },
        new TrapCostEntry { tileType = TileType.Wall, cost = 1 }
    };

    private readonly Dictionary<TileType, int> _costMap = new();
    private HashSet<TileType> _allowedTrapTypes;

    public int CurrentUsedBudget { get; private set; }
    public int MaxBudget => maxBudget;
    public int RemainingBudget => Mathf.Max(0, maxBudget - CurrentUsedBudget);

    private void Awake()
    {
        RebuildCostMap();
        if (arenaManager != null)
        {
            arenaManager.OnArenaChanged += OnArenaChanged;
        }

        RecalculateBudgetUsage();
    }

    private void OnDestroy()
    {
        if (arenaManager != null)
        {
            arenaManager.OnArenaChanged -= OnArenaChanged;
        }
    }

    public void SetMaxBudget(int budget)
    {
        maxBudget = Mathf.Max(0, budget);
        RefreshBudgetUI();
    }

    public void SetAllowedTrapTypes(IEnumerable<TileType> allowed)
    {
        _allowedTrapTypes = allowed != null ? new HashSet<TileType>(allowed) : null;
    }

    public bool CanPlaceTile(TileType tileType, out string reason)
    {
        if (_allowedTrapTypes != null && IsBudgetedTile(tileType) && !_allowedTrapTypes.Contains(tileType))
        {
            reason = $"{tileType} is not allowed for this level.";
            return false;
        }

        int cost = GetCost(tileType);
        if (cost <= 0)
        {
            reason = string.Empty;
            return true;
        }

        if (CurrentUsedBudget + cost > maxBudget)
        {
            reason = $"Not enough trap budget. Need {cost}, have {RemainingBudget}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void OnArenaChanged()
    {
        RecalculateBudgetUsage();
    }

    public int GetCost(TileType tileType)
    {
        return _costMap.TryGetValue(tileType, out int cost) ? Mathf.Max(0, cost) : 0;
    }

    private void RebuildCostMap()
    {
        _costMap.Clear();
        foreach (TrapCostEntry entry in trapCostEntries)
        {
            _costMap[entry.tileType] = Mathf.Max(0, entry.cost);
        }
    }

    private void RecalculateBudgetUsage()
    {
        CurrentUsedBudget = 0;
        if (arenaManager == null)
        {
            RefreshBudgetUI();
            return;
        }

        foreach (KeyValuePair<Vector2Int, ArenaManager.TileEntry> pair in arenaManager.GetAllTiles())
        {
            CurrentUsedBudget += GetCost(pair.Value.tileType);
        }

        RefreshBudgetUI();
    }

    private void RefreshBudgetUI()
    {
        if (budgetHeaderText != null)
        {
            budgetHeaderText.text = "Trap Budget";
        }

        if (budgetValueText != null)
        {
            budgetValueText.text = $"{CurrentUsedBudget} / {maxBudget}";
            budgetValueText.color = CurrentUsedBudget <= maxBudget ? withinBudgetColor : overBudgetColor;
        }

        uiController?.SetTrapBudget(CurrentUsedBudget, maxBudget, CurrentUsedBudget <= maxBudget ? withinBudgetColor : overBudgetColor);
    }

    private bool IsBudgetedTile(TileType tileType) => GetCost(tileType) > 0;
}
