using System;
using System.Collections.Generic;
using UnityEngine;

namespace BotVsDungeon.Evolution
{
    [Serializable]
    public class DungeonGenome
    {
        public int width;
        public int height;
        public int trapBudget;
        public Vector2Int startTile;
        public Vector2Int goalTile;
        public List<PlacedObjectData> placedObjects = new();
        public int sourceSeed;

        public DungeonGenome Clone()
        {
            return new DungeonGenome
            {
                width = width,
                height = height,
                trapBudget = trapBudget,
                startTile = startTile,
                goalTile = goalTile,
                sourceSeed = sourceSeed,
                placedObjects = new List<PlacedObjectData>(placedObjects)
            };
        }

        public DungeonSaveData ToSaveData(string saveName)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new DungeonSaveData
            {
                saveName = saveName,
                width = width,
                height = height,
                createdUnixTime = now,
                trapBudgetUsed = trapBudget,
                startPosition = SerializableVector2Int.From(startTile),
                goalPosition = SerializableVector2Int.From(goalTile),
                placedObjects = new List<PlacedObjectData>(placedObjects),
                metadata = new DungeonMetadata
                {
                    dungeonName = saveName,
                    creationDate = now,
                    trapBudget = trapBudget,
                    timesTested = 0,
                    lastCertificationRating = string.Empty
                }
            };
        }

        public static DungeonGenome FromLayout(GeneratedDungeonLayout layout)
        {
            return new DungeonGenome
            {
                width = layout.width,
                height = layout.height,
                trapBudget = layout.trapCount,
                startTile = layout.start,
                goalTile = layout.goalTile,
                sourceSeed = layout.seedUsed,
                placedObjects = new List<PlacedObjectData>(layout.placedObjects)
            };
        }

        public IEnumerable<int> TrapIndexes()
        {
            for (int i = 0; i < placedObjects.Count; i++)
            {
                TileType type = placedObjects[i].objectType;
                if (type is TileType.Saw or TileType.Bomb or TileType.Archer or TileType.Pit)
                {
                    yield return i;
                }
            }
        }
    }
}
