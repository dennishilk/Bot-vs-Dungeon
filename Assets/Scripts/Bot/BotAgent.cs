using System;
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
    [SerializeField] private float recklessDangerMultiplier = 0.35f;
    [SerializeField] private float panicDangerMultiplier = 1.1f;

    [Header("Trap Proximity Avoidance")]
    [SerializeField] private float carefulAdjacentTrapPenalty = 1.3f;
    [SerializeField] private float balancedAdjacentTrapPenalty = 0.6f;
    [SerializeField] private float recklessAdjacentTrapPenalty = 0.12f;
    [SerializeField] private float panicAdjacentTrapPenalty = 0.4f;

    [Header("Panic Personality")]
    [SerializeField] private float panicTriggerHp = 35f;
    [SerializeField] private float panicPathNoise = 1f;

    private ArenaManager _arenaManager;
    private SimulationManager _simulationManager;
    private BotPathfinder _pathfinder;
    private BotHealth _health;
    private AdaptiveLearningManager _adaptiveLearningManager;

    private readonly List<Vector2Int> _path = new();
    private readonly List<Vector2Int> _traversedTiles = new();
    private int _pathIndex;
    private Vector2Int _goalTile;
    private bool _panicTriggered;
    private float _manualUpdateInterval;
    private float _manualUpdateTimer;
    private bool _useAdaptiveLearning;
    private string _dungeonId;

    public event Action<BotAgent> OnGoalReached;

    public BotState CurrentState { get; private set; } = BotState.Idle;
    public string LastDecision { get; private set; } = "Idle";
    public float LastDangerScore { get; private set; }
    public IReadOnlyList<Vector2Int> CurrentPath => _path;
    public IReadOnlyList<Vector2Int> TraversedTiles => _traversedTiles;
    public int TraversedPathLength => Mathf.Max(0, _pathIndex);
    public Vector2Int CurrentTilePosition => new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    public Vector2Int CurrentTargetTile => _pathIndex >= 0 && _pathIndex < _path.Count ? _path[_pathIndex] : CurrentTilePosition;
    public string DungeonId => _dungeonId;

    private void Awake()
    {
        _pathfinder = GetComponent<BotPathfinder>();
        _health = GetComponent<BotHealth>();
        _health.OnBotDamaged += OnBotDamaged;
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnBotDamaged -= OnBotDamaged;
        }
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

    public void Initialize(
        ArenaManager arenaManager,
        SimulationManager simulationManager,
        Vector2Int goal,
        AdaptiveLearningManager adaptiveLearningManager = null,
        string dungeonId = null,
        bool useAdaptiveLearning = false)
    {
        _arenaManager = arenaManager;
        _simulationManager = simulationManager;
        _goalTile = goal;
        _panicTriggered = false;
        _manualUpdateTimer = 0f;
        _adaptiveLearningManager = adaptiveLearningManager;
        _dungeonId = dungeonId;
        _useAdaptiveLearning = useAdaptiveLearning;
        _traversedTiles.Clear();
        RecalculatePath();
    }

    public void ConfigureUpdateFrequency(float intervalSeconds)
    {
        _manualUpdateInterval = Mathf.Max(0f, intervalSeconds);
        _manualUpdateTimer = 0f;
    }

    private void RecalculatePath(float forcedNoise = 0f)
    {
        Vector2Int start = CurrentTilePosition;
        _path.Clear();

        float noise = forcedNoise;
        float trapPenalty = GetAdjacentTrapPenaltyMultiplier();
        float danger = GetPersonalityDangerMultiplier();
        BotLearningProfile profile = _adaptiveLearningManager != null
            ? _adaptiveLearningManager.GetLearningProfile(personality)
            : default;

        _path.AddRange(_pathfinder.FindPath(
            _arenaManager,
            start,
            _goalTile,
            danger,
            trapPenalty,
            noise,
            _adaptiveLearningManager,
            _dungeonId,
            profile,
            _useAdaptiveLearning));

        _pathIndex = 0;
        CurrentState = BotState.Pathing;
        LastDangerScore = _pathfinder.LastPathDangerScore;
        LastDecision = _path.Count > 0 ? "Path calculated" : "No path found";

        if (_adaptiveLearningManager != null)
        {
            _adaptiveLearningManager.RecordPathAvoidance(_dungeonId, _path);
        }

        EventLogger.Instance?.Log($"Path calculated ({personality}) len={_path.Count} danger={LastDangerScore:0.0}");

        if (_path.Count == 0)
        {
            _health.TakeDamage(999f, DamageSource.PathFailure);
        }
    }

    private void Update()
    {
        if (_simulationManager != null && !_simulationManager.CanBotAutoUpdate())
        {
            return;
        }

        if (personality == BotPersonality.Panic && !_panicTriggered && _health.CurrentHp <= panicTriggerHp)
        {
            _panicTriggered = true;
            LastDecision = "Panic mode triggered";
            RecalculatePath(panicPathNoise);
        }

        float step = Time.deltaTime;
        if (_manualUpdateInterval > 0f)
        {
            _manualUpdateTimer += Time.deltaTime;
            if (_manualUpdateTimer < _manualUpdateInterval)
            {
                return;
            }

            step = _manualUpdateTimer;
            _manualUpdateTimer = 0f;
        }

        SimulateStep(step);
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

        if (_traversedTiles.Count == 0 || _traversedTiles[_traversedTiles.Count - 1] != targetTile)
        {
            _traversedTiles.Add(targetTile);
        }

        _pathIndex++;
        LastDecision = $"Reached {targetTile}";

        if (_pathIndex >= _path.Count)
        {
            CurrentState = BotState.ReachedGoal;
            LastDecision = "Goal reached";
            EventLogger.Instance?.Log("Bot reached goal");
            OnGoalReached?.Invoke(this);
            _simulationManager?.OnBotReachedGoal();
        }
    }

    private float GetPersonalityDangerMultiplier()
    {
        return personality switch
        {
            BotPersonality.Careful => carefulDangerMultiplier,
            BotPersonality.Reckless => recklessDangerMultiplier,
            BotPersonality.Panic => panicDangerMultiplier,
            _ => balancedDangerMultiplier
        };
    }

    private float GetAdjacentTrapPenaltyMultiplier()
    {
        return personality switch
        {
            BotPersonality.Careful => carefulAdjacentTrapPenalty,
            BotPersonality.Reckless => recklessAdjacentTrapPenalty,
            BotPersonality.Panic => panicAdjacentTrapPenalty,
            _ => balancedAdjacentTrapPenalty
        };
    }

    private void OnBotDamaged(float amount, float _)
    {
        if (_adaptiveLearningManager == null)
        {
            return;
        }

        _adaptiveLearningManager.RecordDamage(_dungeonId, CurrentTilePosition, amount, personality);
    }
}
