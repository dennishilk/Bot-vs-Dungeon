using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [System.Serializable]
    public class TileEntry
    {
        public TileType tileType;
        public GameObject instance;
        public TrapBase trap;
    }

    private readonly Dictionary<Vector2Int, TileEntry> _grid = new();

    public bool TryGetTile(Vector2Int gridPos, out TileEntry tile) => _grid.TryGetValue(gridPos, out tile);

    public void SetTile(Vector2Int gridPos, TileType tileType, GameObject instance, TrapBase trap = null)
    {
        RemoveTile(gridPos);
        _grid[gridPos] = new TileEntry
        {
            tileType = tileType,
            instance = instance,
            trap = trap
        };
    }

    public void RemoveTile(Vector2Int gridPos)
    {
        if (!_grid.TryGetValue(gridPos, out TileEntry existing))
        {
            return;
        }

        if (existing.instance != null)
        {
            Destroy(existing.instance);
        }

        _grid.Remove(gridPos);
    }

    public void ClearAll()
    {
        foreach (TileEntry tile in _grid.Values)
        {
            if (tile.instance != null)
            {
                Destroy(tile.instance);
            }
        }

        _grid.Clear();
    }

    public IEnumerable<KeyValuePair<Vector2Int, TileEntry>> GetAllTiles()
    {
        return _grid;
    }

    public bool IsImpassable(Vector2Int gridPos)
    {
        if (!_grid.TryGetValue(gridPos, out TileEntry tile))
        {
            return false;
        }

        return tile.tileType is TileType.Wall or TileType.Pit;
    }

    public float GetDangerCost(Vector2Int gridPos)
    {
        if (!_grid.TryGetValue(gridPos, out TileEntry tile) || tile.trap == null)
        {
            return 0f;
        }

        return tile.trap.PathCostPenalty;
    }


    public IEnumerable<Vector2Int> GetDangerTiles()
    {
        foreach (KeyValuePair<Vector2Int, TileEntry> pair in _grid)
        {
            if (pair.Value.trap != null)
            {
                yield return pair.Key;
            }
        }
    }
    public bool TryFindTile(TileType type, out Vector2Int pos)
    {
        foreach (KeyValuePair<Vector2Int, TileEntry> pair in _grid)
        {
            if (pair.Value.tileType == type)
            {
                pos = pair.Key;
                return true;
            }
        }

        pos = default;
        return false;
    }
}
