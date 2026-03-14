using UnityEngine;

public class AmbientDustSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustPrefab;
    [SerializeField] private Vector3 areaSize = new(10f, 3f, 10f);
    [SerializeField] private int spawnCount = 2;

    private void Start()
    {
        if (dustPrefab == null)
        {
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 offset = new(
                Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                Random.Range(0f, areaSize.y),
                Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f));

            Instantiate(dustPrefab, transform.position + offset, Quaternion.identity, transform);
        }
    }
}
