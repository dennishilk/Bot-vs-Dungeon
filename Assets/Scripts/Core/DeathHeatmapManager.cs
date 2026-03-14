using System;
using System.Collections.Generic;
using UnityEngine;

public class DeathHeatmapManager : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;

    private readonly Dictionary<Vector2Int, int> _deathCounts = new();

    public event Action OnHeatmapChanged;
    public IReadOnlyDictionary<Vector2Int, int> DeathCounts => _deathCounts;

    private void Awake()
    {
        if (simulationManager != null)
        {
            simulationManager.OnRunFinished += OnRunFinished;
        }
    }

    private void OnDestroy()
    {
        if (simulationManager != null)
        {
            simulationManager.OnRunFinished -= OnRunFinished;
        }
    }

    public int GetDeathCount(Vector2Int tilePosition)
    {
        return _deathCounts.TryGetValue(tilePosition, out int count) ? count : 0;
    }

    public int GetMaxDeathCount()
    {
        int max = 0;
        foreach (KeyValuePair<Vector2Int, int> entry in _deathCounts)
        {
            if (entry.Value > max)
            {
                max = entry.Value;
            }
        }

        return max;
    }

    public void ResetHeatmap()
    {
        _deathCounts.Clear();
        OnHeatmapChanged?.Invoke();
    }

    private void OnRunFinished(RunResult runResult)
    {
        if (runResult == null || runResult.survived)
        {
            return;
        }

        Vector2Int deathTile = new(Mathf.RoundToInt(runResult.deathPosition.x), Mathf.RoundToInt(runResult.deathPosition.y));
        _deathCounts.TryGetValue(deathTile, out int existing);
        _deathCounts[deathTile] = existing + 1;
        OnHeatmapChanged?.Invoke();
    }
}
