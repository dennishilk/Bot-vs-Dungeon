using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSerializer : MonoBehaviour
{
    [Serializable]
    public class TilePrefabEntry
    {
        public TileType tileType;
        public GameObject prefab;
    }

    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private TrapBudgetManager trapBudgetManager;
    [SerializeField] private List<TilePrefabEntry> tilePrefabs = new();

    private readonly Dictionary<TileType, GameObject> _prefabMap = new();

    private void Awake()
    {
        RebuildPrefabMap();
    }

    public DungeonSaveData CaptureLayout(string saveName)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int usedBudget = trapBudgetManager != null ? trapBudgetManager.CurrentUsedBudget : 0;
        DungeonSaveData data = new()
        {
            saveName = saveName,
            createdUnixTime = now,
            trapBudgetUsed = usedBudget,
            metadata = new DungeonMetadata
            {
                dungeonName = saveName,
                creationDate = now,
                trapBudget = usedBudget,
                lastCertificationRating = string.Empty,
                timesTested = 0
            }
        };

        bool hasAnyTile = false;
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (KeyValuePair<Vector2Int, ArenaManager.TileEntry> pair in arenaManager.GetAllTiles())
        {
            hasAnyTile = true;
            Vector2Int pos = pair.Key;
            ArenaManager.TileEntry entry = pair.Value;

            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);

            if (entry.tileType == TileType.Start)
            {
                data.startPosition = SerializableVector2Int.From(pos);
            }
            else if (entry.tileType == TileType.Goal)
            {
                data.goalPosition = SerializableVector2Int.From(pos);
            }

            float rotationY = entry.instance != null ? entry.instance.transform.eulerAngles.y : 0f;
            data.placedObjects.Add(new PlacedObjectData
            {
                objectType = entry.tileType,
                gridPosition = SerializableVector2Int.From(pos),
                rotationY = rotationY
            });
        }

        if (hasAnyTile)
        {
            data.width = maxX - minX + 1;
            data.height = maxY - minY + 1;
        }

        return data;
    }

    public bool ApplyLayout(DungeonSaveData data, out string error)
    {
        error = string.Empty;
        if (data == null)
        {
            error = "Save data is empty.";
            return false;
        }

        if (arenaManager == null)
        {
            error = "ArenaManager reference missing.";
            return false;
        }

        arenaManager.ClearAll();

        foreach (PlacedObjectData item in data.placedObjects)
        {
            Vector2Int position = item.gridPosition.ToVector2Int();
            GameObject prefab = ResolvePrefab(item.objectType);

            GameObject instance = null;
            if (prefab != null)
            {
                Quaternion rotation = Quaternion.Euler(0f, item.rotationY, 0f);
                instance = Instantiate(prefab, new Vector3(position.x, 0f, position.y), rotation);
            }

            TrapBase trap = instance != null ? instance.GetComponent<TrapBase>() : null;
            arenaManager.SetTile(position, item.objectType, instance, trap);
        }

        trapBudgetManager?.OnArenaChanged();
        return true;
    }

    public string ToJson(DungeonSaveData data)
    {
        return JsonUtility.ToJson(data, true);
    }

    public bool TryFromJson(string json, out DungeonSaveData data)
    {
        data = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            data = JsonUtility.FromJson<DungeonSaveData>(json);
            return data != null;
        }
        catch
        {
            return false;
        }
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
