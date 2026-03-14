using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonSaveData
{
    public string version = "BVD_SAVE_V1";
    public string saveName;
    public int width;
    public int height;
    public SerializableVector2Int startPosition;
    public SerializableVector2Int goalPosition;
    public int trapBudgetUsed;
    public long createdUnixTime;
    public List<PlacedObjectData> placedObjects = new();
}

[Serializable]
public class PlacedObjectData
{
    public TileType objectType;
    public SerializableVector2Int gridPosition;
    public float rotationY;
}

[Serializable]
public struct SerializableVector2Int
{
    public int x;
    public int y;

    public SerializableVector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int ToVector2Int() => new(x, y);

    public static SerializableVector2Int From(Vector2Int value) => new(value.x, value.y);
}
