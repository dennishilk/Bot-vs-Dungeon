using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BotPathfinder))]
[RequireComponent(typeof(BotHealth))]
public class BotAgent : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Debug Personality")]
    [SerializeField] private BotPersonality personality = BotPersonality.Balanced;
    [SerializeField] private float carefulDangerMultiplier = 1.8f;
    [SerializeField] private float balancedDangerMultiplier = 1f;
    [SerializeField] private float recklessDangerMultiplier = 0.3f;

    private ArenaManager _arenaManager;
    private SimulationManager _simulationManager;
    private BotPathfinder _pathfinder;
    private BotHealth _health;

    private readonly List<Vector2Int> _path = new();
    private int _pathIndex;

    public BotState CurrentState { get; private set; } = BotState.Idle;
    public string LastDecision { get; private set; } = "Idle";
    public float LastDangerScore { get; private set; }
    public IReadOnlyList<Vector2Int> CurrentPath => _path;
    public Vector2Int CurrentTilePosition => new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    public Vector2Int CurrentTargetTile => _pathIndex >= 0 && _pathIndex < _path.Count ? _path[_pathIndex] : CurrentTilePosition;

    private void Awake()
    {
        _pathfinder = GetComponent<BotPathfinder>();
        _health = GetComponent<BotHealth>();
    }

    public void SetPersonality(BotPersonality newPersonality)
    {
        personality = newPersonality;
        LastDecision = $"Personality set: {newPersonality}";
    }

    public BotPersonality GetPersonality()
    {
        return personality;
    }

    public void Initialize(ArenaManager arenaManager, SimulationManager simulationManager, Vector2Int goal)
    {
        _arenaManager = arenaManager;
        _simulationManager = simulationManager;

        Vector2Int start = CurrentTilePosition;
        _path.Clear();
        _path.AddRange(_pathfinder.FindPath(arenaManager, start, goal, GetPersonalityDangerMultiplier()));
        _pathIndex = 0;
        CurrentState = BotState.Pathing;
        LastDangerScore = _pathfinder.LastPathDangerScore;
        LastDecision = _path.Count > 0 ? "Path calculated" : "No path found";

        EventLogger.Instance?.Log($"Path calculated (len={_path.Count}, danger={LastDangerScore:0.0})");

        if (_path.Count == 0)
        {
            _health.TakeDamage(999f);
        }
    }

    private void Update()
    {
        if (_simulationManager != null && !_simulationManager.CanBotAutoUpdate())
        {
            return;
        }

        SimulateStep(Time.deltaTime);
    }

    public void SimulateSingleStep(float stepDelta = 0.15f)
    {
        SimulateStep(stepDelta);
    }

    private void SimulateStep(float deltaTime)
    {
        if (CurrentState != BotState.Pathing || _pathIndex >= _path.Count)
        {
            return;
        }

        Vector2Int targetTile = _path[_pathIndex];
        Vector3 target = new(targetTile.x, transform.position.y, targetTile.y);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * deltaTime);

        if (Vector3.Distance(transform.position, target) > 0.05f)
        {
            LastDecision = $"Moving to {targetTile}";
            return;
        }

        _pathIndex++;
        LastDecision = $"Reached {targetTile}";

        if (_pathIndex >= _path.Count)
        {
            CurrentState = BotState.ReachedGoal;
            LastDecision = "Goal reached";
            EventLogger.Instance?.Log("Bot reached goal");
            _simulationManager.OnBotReachedGoal();
        }
    }

    private float GetPersonalityDangerMultiplier()
    {
        return personality switch
        {
            BotPersonality.Careful => carefulDangerMultiplier,
            BotPersonality.Reckless => recklessDangerMultiplier,
            _ => balancedDangerMultiplier
        };
    }
}
