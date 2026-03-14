using System.Collections.Generic;
using UnityEngine;

public class BotPathfinder : MonoBehaviour
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    [Header("Debug + Tuning")]
    [SerializeField] private float dangerCostMultiplier = 1f;

    public float LastPathDangerScore { get; private set; }

    public List<Vector2Int> FindPath(ArenaManager arenaManager, Vector2Int start, Vector2Int goal, float personalityDangerMultiplier = 1f)
    {
        PriorityQueue<Vector2Int> open = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new();

        open.Enqueue(start, 0f);
        gScore[start] = 0f;
        LastPathDangerScore = 0f;

        while (open.TryDequeue(out Vector2Int current))
        {
            if (current == goal)
            {
                List<Vector2Int> path = Reconstruct(cameFrom, current);
                LastPathDangerScore = ComputePathDanger(arenaManager, path, personalityDangerMultiplier);
                return path;
            }

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next = current + dir;
                if (arenaManager.IsImpassable(next))
                {
                    continue;
                }

                float danger = arenaManager.GetDangerCost(next) * dangerCostMultiplier * personalityDangerMultiplier;
                float tentative = gScore[current] + 1f + danger;
                if (!gScore.ContainsKey(next) || tentative < gScore[next])
                {
                    gScore[next] = tentative;
                    cameFrom[next] = current;
                    float priority = tentative + Manhattan(next, goal);
                    open.Enqueue(next, priority);
                }
            }
        }

        return new List<Vector2Int>();
    }

    private float ComputePathDanger(ArenaManager arenaManager, IEnumerable<Vector2Int> path, float personalityDangerMultiplier)
    {
        float score = 0f;
        foreach (Vector2Int tile in path)
        {
            score += arenaManager.GetDangerCost(tile) * dangerCostMultiplier * personalityDangerMultiplier;
        }

        return score;
    }

    private static float Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new() { current };

        while (cameFrom.TryGetValue(current, out Vector2Int prev))
        {
            current = prev;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private class PriorityQueue<T>
    {
        private readonly List<(T item, float priority)> _items = new();

        public void Enqueue(T item, float priority)
        {
            _items.Add((item, priority));
        }

        public bool TryDequeue(out T item)
        {
            if (_items.Count == 0)
            {
                item = default;
                return false;
            }

            int bestIndex = 0;
            float bestPriority = _items[0].priority;
            for (int i = 1; i < _items.Count; i++)
            {
                if (!(_items[i].priority < bestPriority))
                {
                    continue;
                }

                bestPriority = _items[i].priority;
                bestIndex = i;
            }

            item = _items[bestIndex].item;
            _items.RemoveAt(bestIndex);
            return true;
        }
    }
}
