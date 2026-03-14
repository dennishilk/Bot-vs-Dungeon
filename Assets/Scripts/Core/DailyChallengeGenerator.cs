using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyChallengeGenerator : MonoBehaviour
{
    [SerializeField] private int minMapSize = 8;
    [SerializeField] private int maxMapSize = 12;
    [SerializeField] private int minTrapBudget = 18;
    [SerializeField] private int maxTrapBudget = 40;

    public DungeonSaveData GenerateForDate(DateTime date)
    {
        int seed = date.Year * 10000 + date.Month * 100 + date.Day;
        UnityEngine.Random.InitState(seed);

        int width = UnityEngine.Random.Range(minMapSize, maxMapSize + 1);
        int height = UnityEngine.Random.Range(minMapSize, maxMapSize + 1);
        int budget = UnityEngine.Random.Range(minTrapBudget, maxTrapBudget + 1);

        DungeonSaveData data = new()
        {
            saveName = $"daily_{seed}",
            width = width,
            height = height,
            createdUnixTime = new DateTimeOffset(date.Date, TimeSpan.Zero).ToUnixTimeSeconds(),
            trapBudgetUsed = budget,
            startPosition = new SerializableVector2Int(0, 0),
            goalPosition = new SerializableVector2Int(width - 1, height - 1),
            metadata = new DungeonMetadata
            {
                dungeonName = $"Daily Challenge {date:yyyy-MM-dd}",
                creationDate = new DateTimeOffset(date.Date, TimeSpan.Zero).ToUnixTimeSeconds(),
                trapBudget = budget,
                lastCertificationRating = string.Empty,
                timesTested = 0
            }
        };

        BuildBasicLayout(data, width, height);
        PlaceDeterministicTraps(data, budget);
        return data;
    }

    private static void BuildBasicLayout(DungeonSaveData data, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                data.placedObjects.Add(new PlacedObjectData
                {
                    objectType = TileType.Floor,
                    gridPosition = new SerializableVector2Int(x, y),
                    rotationY = 0f
                });
            }
        }

        data.placedObjects.Add(new PlacedObjectData { objectType = TileType.Start, gridPosition = data.startPosition, rotationY = 0f });
        data.placedObjects.Add(new PlacedObjectData { objectType = TileType.Goal, gridPosition = data.goalPosition, rotationY = 0f });
    }

    private static void PlaceDeterministicTraps(DungeonSaveData data, int budget)
    {
        int trapsToPlace = Mathf.Clamp(budget / 4, 4, 16);
        List<TileType> trapPool = new() { TileType.Saw, TileType.Bomb, TileType.Archer, TileType.Pit };
        for (int i = 0; i < trapsToPlace; i++)
        {
            int x = UnityEngine.Random.Range(1, Mathf.Max(2, data.width - 1));
            int y = UnityEngine.Random.Range(1, Mathf.Max(2, data.height - 1));
            TileType trap = trapPool[UnityEngine.Random.Range(0, trapPool.Count)];
            data.placedObjects.Add(new PlacedObjectData
            {
                objectType = trap,
                gridPosition = new SerializableVector2Int(x, y),
                rotationY = UnityEngine.Random.Range(0, 4) * 90f
            });
        }
    }

    public static int BuildDateSeed(DateTime date)
    {
        return date.Year * 10000 + date.Month * 100 + date.Day;
    }
}
