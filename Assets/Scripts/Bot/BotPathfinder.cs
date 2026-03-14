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

    public List<Vector2Int> FindPath(
        ArenaManager arenaManager,
        Vector2Int start,
        Vector2Int goal,
        float personalityDangerMultiplier = 1f,
        float adjacentTrapPenaltyMultiplier = 0f,
        float deterministicNoiseWeight = 0f,
        AdaptiveLearningManager adaptiveLearningManager = null,
        string dungeonId = null,
        BotLearningProfile learningProfile = default,
        bool useAdaptiveLearning = false)
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
                float learnedDanger = useAdaptiveLearning && adaptiveLearningManager != null
                    ? adaptiveLearningManager.GetLearnedDangerModifier(dungeonId, next, learningProfile)
                    : 0f;
                float adjacencyPenalty = GetAdjacentTrapCount(arenaManager, next) * adjacentTrapPenaltyMultiplier;
                float noise = GetDeterministicTileNoise(next) * deterministicNoiseWeight;
                float tentative = gScore[current] + 1f + danger + learnedDanger + adjacencyPenalty + noise;

                if (!gScore.ContainsKey(next) || tentative < gScore[next])
                {
                    gScore[next] = tentative;
                    cameFrom[next] = current;
                    float priority = tentative + Manhattan(next, goal);
                    open.Enqueue(next, priority);
                }
            }
        }

        LastPathDangerScore = 999f;
        return new List<Vector2Int>();
    }

    private static int Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new() { current };
        while (cameFrom.TryGetValue(current, out Vector2Int parent))
        {
            current = parent;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private static float ComputePathDanger(ArenaManager arenaManager, List<Vector2Int> path, float personalityMultiplier)
    {
        float total = 0f;
        foreach (Vector2Int tile in path)
        {
            total += arenaManager.GetDangerCost(tile) * personalityMultiplier;
        }

        return total;
    }

    private static int GetAdjacentTrapCount(ArenaManager arenaManager, Vector2Int tile)
    {
        int count = 0;
        foreach (Vector2Int dir in Directions)
        {
            if (arenaManager.GetDangerCost(tile + dir) > 0f)
            {
                count++;
            }
        }

        return count;
    }

    private static float GetDeterministicTileNoise(Vector2Int tile)
    {
        float hash = Mathf.Sin(tile.x * 12.9898f + tile.y * 78.233f) * 43758.5453f;
        return Mathf.Abs(hash - Mathf.Floor(hash));
    }
}
