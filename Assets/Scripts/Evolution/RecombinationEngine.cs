using UnityEngine;

namespace BotVsDungeon.Evolution
{
    public class RecombinationEngine : MonoBehaviour
    {
        public DungeonGenome Recombine(DungeonGenome a, DungeonGenome b, System.Random rng)
        {
            DungeonGenome child = a.Clone();
            int splitX = rng.Next(1, Mathf.Max(2, a.width - 1));

            child.placedObjects.Clear();
            foreach (PlacedObjectData item in a.placedObjects)
            {
                if (item.gridPosition.x <= splitX)
                {
                    child.placedObjects.Add(item);
                }
            }

            foreach (PlacedObjectData item in b.placedObjects)
            {
                if (item.gridPosition.x > splitX)
                {
                    child.placedObjects.Add(item);
                }
            }

            child.startTile = a.startTile;
            child.goalTile = b.goalTile;
            child.trapBudget = CountTraps(child);
            return child;
        }

        private static int CountTraps(DungeonGenome genome)
        {
            int count = 0;
            foreach (int _ in genome.TrapIndexes()) count++;
            return count;
        }
    }
}
