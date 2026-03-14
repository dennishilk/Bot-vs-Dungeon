using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonTilePlacement
{
    public Vector2Int position;
    public TileType tileType;
}

[System.Serializable]
public class DungeonLevel
{
    public string levelName = "New Challenge";
    [TextArea] public string levelDescription;
    public int trapBudget = 10;
    public LevelObjective objective = new();
    public List<TileType> allowedTrapTypes = new() { TileType.Pit, TileType.Saw, TileType.Bomb, TileType.Archer, TileType.Wall };
    public List<DungeonTilePlacement> initialDungeonLayout = new();
    public bool completed;
}
