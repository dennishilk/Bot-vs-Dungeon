using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BotPathfinder))]
[RequireComponent(typeof(BotHealth))]
public class BotAgent : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private ArenaManager _arenaManager;
    private SimulationManager _simulationManager;
    private BotPathfinder _pathfinder;
    private BotHealth _health;

    private List<Vector2Int> _path = new();
    private int _pathIndex;

    public BotState CurrentState { get; private set; } = BotState.Idle;

    private void Awake()
    {
        _pathfinder = GetComponent<BotPathfinder>();
        _health = GetComponent<BotHealth>();
    }

    public void Initialize(ArenaManager arenaManager, SimulationManager simulationManager, Vector2Int goal)
    {
        _arenaManager = arenaManager;
        _simulationManager = simulationManager;

        Vector2Int start = new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        _path = _pathfinder.FindPath(arenaManager, start, goal);
        _pathIndex = 0;
        CurrentState = BotState.Pathing;

        if (_path.Count == 0)
        {
            _health.TakeDamage(999f);
        }
    }

    private void Update()
    {
        if (CurrentState != BotState.Pathing || _pathIndex >= _path.Count)
        {
            return;
        }

        Vector3 target = new(_path[_pathIndex].x, transform.position.y, _path[_pathIndex].y);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) > 0.05f)
        {
            return;
        }

        _pathIndex++;
        if (_pathIndex >= _path.Count)
        {
            CurrentState = BotState.ReachedGoal;
            _simulationManager.OnBotReachedGoal();
        }
    }
}
