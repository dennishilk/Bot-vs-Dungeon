using UnityEngine;
using UnityEngine.SceneManagement;

public class TestArenaLoader : MonoBehaviour
{
    [SerializeField] private string testArenaSceneName = "TestArena";
    [SerializeField] private ArenaManager arenaManager;

    [Header("Optional Prefabs For Quick Layouts")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject sawPrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject pitPrefab;

    public void LoadTestArenaScene()
    {
        SceneManager.LoadScene(testArenaSceneName);
    }

    public void SetupSawTest()
    {
        SetupBasicLane();
        PlaceTrap(new Vector2Int(5, 0), TileType.Saw, sawPrefab);
        EventLogger.Instance?.Log("TestArena layout loaded: Saw Test");
    }

    public void SetupBombTest()
    {
        SetupBasicLane();
        PlaceTrap(new Vector2Int(5, 0), TileType.Bomb, bombPrefab);
        EventLogger.Instance?.Log("TestArena layout loaded: Bomb Test");
    }

    public void SetupArcherLineTest()
    {
        SetupBasicLane();
        PlaceTrap(new Vector2Int(5, 1), TileType.Archer, archerPrefab);
        EventLogger.Instance?.Log("TestArena layout loaded: Archer Line Test");
    }

    public void SetupPitMazeTest()
    {
        SetupBasicLane();
        PlaceTrap(new Vector2Int(4, 0), TileType.Pit, pitPrefab);
        PlaceTrap(new Vector2Int(6, 0), TileType.Pit, pitPrefab);
        EventLogger.Instance?.Log("TestArena layout loaded: Pit Maze Test");
    }

    private void SetupBasicLane()
    {
        if (arenaManager == null)
        {
            return;
        }

        arenaManager.ClearAll();

        for (int x = 0; x <= 10; x++)
        {
            Vector2Int pos = new(x, 0);
            GameObject instance = floorPrefab != null ? Instantiate(floorPrefab, new Vector3(pos.x, 0f, pos.y), Quaternion.identity) : null;
            arenaManager.SetTile(pos, TileType.Floor, instance);
        }

        arenaManager.SetTile(new Vector2Int(0, 0), TileType.Start, null);
        arenaManager.SetTile(new Vector2Int(10, 0), TileType.Goal, null);
    }

    private void PlaceTrap(Vector2Int position, TileType tileType, GameObject trapPrefab)
    {
        if (arenaManager == null)
        {
            return;
        }

        GameObject instance = trapPrefab != null ? Instantiate(trapPrefab, new Vector3(position.x, 0f, position.y), Quaternion.identity) : null;
        TrapBase trap = instance != null ? instance.GetComponent<TrapBase>() : null;
        arenaManager.SetTile(position, tileType, instance, trap);
    }
}
