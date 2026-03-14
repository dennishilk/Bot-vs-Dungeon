using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private DirectorParameters defaultParameters = new();

    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public DirectorParameters DefaultParameters => defaultParameters;

    public GeneratedDungeonLayout GenerateLayout(DirectorGoal goal, DirectorParameters parameters, int attemptIndex)
    {
        DirectorParameters p = parameters ?? defaultParameters;
        int seed = p.deterministic ? p.deterministicSeed + (attemptIndex * 17) + (int)goal * 101 : Environment.TickCount + attemptIndex;
        System.Random rng = new(seed);

        int width = Mathf.Max(6, p.mapWidth);
        int height = Mathf.Max(6, p.mapHeight);

        Vector2Int start = new(1, 1);
        Vector2Int goalTile = new(width - 2, height - 2);

        HashSet<Vector2Int> mainPath = CreateMainPath(start, goalTile, width, height, rng, GetPathComplexity(goal));
        HashSet<Vector2Int> branches = CreateBranches(mainPath, width, height, rng, p.branchFrequency, goal);

        HashSet<Vector2Int> walkable = new(mainPath);
        walkable.UnionWith(branches);
        walkable.Add(start);
        walkable.Add(goalTile);

        List<PlacedObjectData> placed = new();
        foreach (Vector2Int floor in walkable)
        {
            placed.Add(new PlacedObjectData
            {
                objectType = TileType.Floor,
                gridPosition = SerializableVector2Int.From(floor),
                rotationY = 0f
            });
        }

        List<PlacedObjectData> traps = PlaceTraps(goal, p, rng, mainPath, branches, start, goalTile, width, height);
        placed.AddRange(traps);

        placed.Add(new PlacedObjectData { objectType = TileType.Start, gridPosition = SerializableVector2Int.From(start) });
        placed.Add(new PlacedObjectData { objectType = TileType.Goal, gridPosition = SerializableVector2Int.From(goalTile) });

        int maxTrapBudget = Mathf.Max(1, p.maxTrapBudget);
        float usageRatio = Mathf.Clamp01((float)traps.Count / maxTrapBudget);

        return new GeneratedDungeonLayout
        {
            goal = goal,
            placedObjects = placed,
            start = start,
            goalTile = goalTile,
            width = width,
            height = height,
            branchCount = branches.Count,
            trapCount = traps.Count,
            trapBudgetUsageRatio = usageRatio,
            seedUsed = seed
        };
    }

    private static HashSet<Vector2Int> CreateMainPath(Vector2Int start, Vector2Int goal, int width, int height, System.Random rng, float complexity)
    {
        HashSet<Vector2Int> path = new() { start };
        Vector2Int cursor = start;
        int safety = width * height * 3;

        while (cursor != goal && safety-- > 0)
        {
            List<Vector2Int> candidates = new();
            foreach (Vector2Int direction in CardinalDirections)
            {
                Vector2Int next = cursor + direction;
                if (next.x < 1 || next.y < 1 || next.x >= width - 1 || next.y >= height - 1)
                {
                    continue;
                }

                candidates.Add(next);
            }

            candidates.Sort((a, b) => DistanceScore(a, goal, rng, complexity).CompareTo(DistanceScore(b, goal, rng, complexity)));
            cursor = candidates[0];
            path.Add(cursor);
        }

        path.Add(goal);
        return path;
    }

    private static HashSet<Vector2Int> CreateBranches(HashSet<Vector2Int> mainPath, int width, int height, System.Random rng, float branchFrequency, DirectorGoal goal)
    {
        HashSet<Vector2Int> branches = new();
        int branchLength = goal == DirectorGoal.Puzzle ? 4 : 2;

        foreach (Vector2Int pivot in mainPath)
        {
            if (rng.NextDouble() > branchFrequency)
            {
                continue;
            }

            Vector2Int direction = CardinalDirections[rng.Next(0, CardinalDirections.Length)];
            Vector2Int cursor = pivot;
            for (int i = 0; i < branchLength; i++)
            {
                cursor += direction;
                if (cursor.x < 1 || cursor.y < 1 || cursor.x >= width - 1 || cursor.y >= height - 1)
                {
                    break;
                }

                if (!mainPath.Contains(cursor))
                {
                    branches.Add(cursor);
                }
            }
        }

        return branches;
    }

    private static List<PlacedObjectData> PlaceTraps(
        DirectorGoal goal,
        DirectorParameters parameters,
        System.Random rng,
        HashSet<Vector2Int> mainPath,
        HashSet<Vector2Int> branches,
        Vector2Int start,
        Vector2Int goalTile,
        int width,
        int height)
    {
        List<PlacedObjectData> traps = new();
        HashSet<Vector2Int> candidates = new(mainPath);
        candidates.UnionWith(branches);
        candidates.Remove(start);
        candidates.Remove(goalTile);

        float densityMultiplier = goal switch
        {
            DirectorGoal.Fair => 0.55f,
            DirectorGoal.Dangerous => 0.9f,
            DirectorGoal.Brutal => 1.2f,
            DirectorGoal.Puzzle => 0.75f,
            DirectorGoal.StressTest => 1.3f,
            _ => 0.8f
        };

        int desiredCount = Mathf.Clamp(
            Mathf.RoundToInt(candidates.Count * parameters.trapDensity * densityMultiplier),
            1,
            Mathf.Max(1, parameters.maxTrapBudget));

        List<Vector2Int> ordered = new(candidates);
        ordered.Sort((a, b) => (a.x + a.y).CompareTo(b.x + b.y));

        int index = 0;
        while (traps.Count < desiredCount && index < ordered.Count)
        {
            Vector2Int tile = ordered[index++];
            TileType trapType = SelectTrapType(goal, rng, tile, width, height);

            traps.Add(new PlacedObjectData
            {
                objectType = trapType,
                gridPosition = SerializableVector2Int.From(tile),
                rotationY = 0f
            });
        }

        return traps;
    }

    private static TileType SelectTrapType(DirectorGoal goal, System.Random rng, Vector2Int tile, int width, int height)
    {
        bool corridor = tile.x <= 2 || tile.y <= 2 || tile.x >= width - 3 || tile.y >= height - 3;

        return goal switch
        {
            DirectorGoal.Fair => corridor ? TileType.Pit : (rng.NextDouble() < 0.6 ? TileType.Saw : TileType.Pit),
            DirectorGoal.Brutal => corridor ? TileType.Bomb : (rng.NextDouble() < 0.55 ? TileType.Archer : TileType.Bomb),
            DirectorGoal.Dangerous => rng.NextDouble() < 0.45 ? TileType.Bomb : TileType.Saw,
            DirectorGoal.Puzzle => rng.NextDouble() < 0.5 ? TileType.Pit : TileType.Archer,
            DirectorGoal.StressTest => rng.NextDouble() < 0.5 ? TileType.Bomb : TileType.Archer,
            _ => rng.NextDouble() < 0.5 ? TileType.Saw : TileType.Bomb
        };
    }

    private static float GetPathComplexity(DirectorGoal goal)
    {
        return goal switch
        {
            DirectorGoal.Fair => 0.2f,
            DirectorGoal.Balanced => 0.35f,
            DirectorGoal.Dangerous => 0.45f,
            DirectorGoal.Puzzle => 0.7f,
            DirectorGoal.StressTest => 0.5f,
            DirectorGoal.Brutal => 0.6f,
            _ => 0.4f
        };
    }

    private static float DistanceScore(Vector2Int candidate, Vector2Int goal, System.Random rng, float complexity)
    {
        int distance = Mathf.Abs(goal.x - candidate.x) + Mathf.Abs(goal.y - candidate.y);
        float noise = (float)rng.NextDouble() * complexity * 6f;
        return distance + noise;
    }
}

[Serializable]
public class GeneratedDungeonLayout
{
    public DirectorGoal goal;
    public List<PlacedObjectData> placedObjects = new();
    public Vector2Int start;
    public Vector2Int goalTile;
    public int width;
    public int height;
    public int trapCount;
    public int branchCount;
    public float trapBudgetUsageRatio;
    public int seedUsed;

    public DungeonSaveData ToSaveData(string saveName)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return new DungeonSaveData
        {
            saveName = saveName,
            width = width,
            height = height,
            createdUnixTime = now,
            trapBudgetUsed = trapCount,
            startPosition = SerializableVector2Int.From(start),
            goalPosition = SerializableVector2Int.From(goalTile),
            placedObjects = new List<PlacedObjectData>(placedObjects),
            metadata = new DungeonMetadata
            {
                dungeonName = saveName,
                creationDate = now,
                trapBudget = trapCount,
                timesTested = 0,
                lastCertificationRating = string.Empty
            }
        };
    }
}
