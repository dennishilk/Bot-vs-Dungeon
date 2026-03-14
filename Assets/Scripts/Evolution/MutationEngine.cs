using System;
using System.Collections.Generic;
using UnityEngine;

namespace BotVsDungeon.Evolution
{
    public class MutationEngine : MonoBehaviour
    {
        [SerializeField] private Vector2Int mutationOffsetMin = new(-2, -2);
        [SerializeField] private Vector2Int mutationOffsetMax = new(2, 2);

        private static readonly TileType[] TrapTypes = { TileType.Saw, TileType.Bomb, TileType.Archer, TileType.Pit };

        public DungeonGenome Mutate(DungeonGenome source, float mutationProbability, System.Random rng)
        {
            DungeonGenome genome = source.Clone();
            if (rng.NextDouble() > mutationProbability)
            {
                return genome;
            }

            int action = rng.Next(0, 6);
            switch (action)
            {
                case 0: MoveTrap(genome, rng); break;
                case 1: ReplaceTrapType(genome, rng); break;
                case 2: AddTrap(genome, rng); break;
                case 3: RemoveTrap(genome, rng); break;
                case 4: RotateTrap(genome, rng); break;
                default: MutateBranchCorridor(genome, rng); break;
            }

            genome.trapBudget = CountTraps(genome);
            return genome;
        }

        private static void MoveTrap(DungeonGenome genome, System.Random rng)
        {
            List<int> indexes = new(genome.TrapIndexes());
            if (indexes.Count == 0) return;
            int idx = indexes[rng.Next(indexes.Count)];
            PlacedObjectData trap = genome.placedObjects[idx];
            Vector2Int pos = trap.gridPosition.ToVector2Int();
            pos += new Vector2Int(rng.Next(-1, 2), rng.Next(-1, 2));
            trap.gridPosition = SerializableVector2Int.From(ClampInsideMap(pos, genome.width, genome.height));
            genome.placedObjects[idx] = trap;
        }

        private static void ReplaceTrapType(DungeonGenome genome, System.Random rng)
        {
            List<int> indexes = new(genome.TrapIndexes());
            if (indexes.Count == 0) return;
            int idx = indexes[rng.Next(indexes.Count)];
            PlacedObjectData trap = genome.placedObjects[idx];
            trap.objectType = TrapTypes[rng.Next(TrapTypes.Length)];
            genome.placedObjects[idx] = trap;
        }

        private static void AddTrap(DungeonGenome genome, System.Random rng)
        {
            Vector2Int pos = new(rng.Next(1, Mathf.Max(2, genome.width - 1)), rng.Next(1, Mathf.Max(2, genome.height - 1)));
            if (pos == genome.startTile || pos == genome.goalTile) return;
            genome.placedObjects.Add(new PlacedObjectData
            {
                objectType = TrapTypes[rng.Next(TrapTypes.Length)],
                gridPosition = SerializableVector2Int.From(pos),
                rotationY = rng.Next(0, 4) * 90f
            });
        }

        private static void RemoveTrap(DungeonGenome genome, System.Random rng)
        {
            List<int> indexes = new(genome.TrapIndexes());
            if (indexes.Count == 0) return;
            genome.placedObjects.RemoveAt(indexes[rng.Next(indexes.Count)]);
        }

        private static void RotateTrap(DungeonGenome genome, System.Random rng)
        {
            List<int> indexes = new(genome.TrapIndexes());
            if (indexes.Count == 0) return;
            int idx = indexes[rng.Next(indexes.Count)];
            PlacedObjectData trap = genome.placedObjects[idx];
            trap.rotationY = (trap.rotationY + (rng.Next(0, 2) == 0 ? 90f : 270f)) % 360f;
            genome.placedObjects[idx] = trap;
        }

        private void MutateBranchCorridor(DungeonGenome genome, System.Random rng)
        {
            bool addFloor = rng.Next(0, 2) == 0;
            Vector2Int pos = new(rng.Next(1, Mathf.Max(2, genome.width - 1)), rng.Next(1, Mathf.Max(2, genome.height - 1)));
            if (addFloor)
            {
                genome.placedObjects.Add(new PlacedObjectData
                {
                    objectType = TileType.Floor,
                    gridPosition = SerializableVector2Int.From(pos),
                    rotationY = 0f
                });
            }
            else
            {
                for (int i = genome.placedObjects.Count - 1; i >= 0; i--)
                {
                    PlacedObjectData item = genome.placedObjects[i];
                    if (item.objectType == TileType.Floor && item.gridPosition.ToVector2Int() == pos)
                    {
                        genome.placedObjects.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private static Vector2Int ClampInsideMap(Vector2Int point, int width, int height)
        {
            return new Vector2Int(Mathf.Clamp(point.x, 1, Mathf.Max(1, width - 2)), Mathf.Clamp(point.y, 1, Mathf.Max(1, height - 2)));
        }

        private static int CountTraps(DungeonGenome genome)
        {
            int count = 0;
            foreach (int _ in genome.TrapIndexes()) count++;
            return count;
        }
    }
}
