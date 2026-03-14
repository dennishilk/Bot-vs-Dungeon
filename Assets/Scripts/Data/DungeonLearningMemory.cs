using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DungeonLearningMemory
{
    public string dungeonID;
    public int totalRunsObserved;
    public int totalDeaths;
    public int totalSuccesses;
    public List<AdaptiveTileMemory> tileMemory = new();

    private Dictionary<Vector2Int, AdaptiveTileMemory> _tileMap;

    public void Initialize(string id)
    {
        dungeonID = id;
        _tileMap = new Dictionary<Vector2Int, AdaptiveTileMemory>();
        foreach (AdaptiveTileMemory memory in tileMemory)
        {
            _tileMap[memory.Position] = memory;
        }
    }

    public AdaptiveTileMemory GetOrCreateTile(Vector2Int position)
    {
        _tileMap ??= tileMemory.ToDictionary(t => t.Position, t => t);
        if (_tileMap.TryGetValue(position, out AdaptiveTileMemory memory))
        {
            return memory;
        }

        memory = new AdaptiveTileMemory(position);
        _tileMap[position] = memory;
        tileMemory.Add(memory);
        return memory;
    }

    public bool TryGetTile(Vector2Int position, out AdaptiveTileMemory memory)
    {
        _tileMap ??= tileMemory.ToDictionary(t => t.Position, t => t);
        return _tileMap.TryGetValue(position, out memory);
    }

    public AdaptiveTileMemory GetMostLethalTile()
    {
        return tileMemory.OrderByDescending(t => t.deathCount).FirstOrDefault();
    }

    public AdaptiveTileMemory GetMostAvoidedTile()
    {
        return tileMemory.OrderByDescending(t => t.avoidedCount).FirstOrDefault();
    }

    public int LearnedDangerousTiles(float threshold)
    {
        return tileMemory.Count(t => t.learnedDangerModifier >= threshold);
    }
}
