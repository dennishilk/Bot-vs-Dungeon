using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMarkerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private GameObject successMarkerPrefab;
    [SerializeField] private float markerHeight = 0.2f;
    [SerializeField] private float pulseDuration = 0.45f;
    [SerializeField] private float pulseScaleMultiplier = 1.2f;

    private readonly List<GameObject> _spawnedMarkers = new();

    public void SpawnMarkerForRun(RunResult result)
    {
        if (result == null)
        {
            return;
        }

        if (result.survived)
        {
            if (successMarkerPrefab != null)
            {
                Vector3 successPosition = new(result.deathPosition.x, markerHeight, result.deathPosition.y);
                SpawnMarker(successMarkerPrefab, successPosition, GetPersonalityColor(result.personality));
            }

            return;
        }

        if (markerPrefab == null)
        {
            return;
        }

        Vector3 position = new(result.deathPosition.x, markerHeight, result.deathPosition.y);
        Color color = GetPersonalityColor(result.personality);
        SpawnMarker(markerPrefab, position, color);
    }

    public void ClearMarkers()
    {
        foreach (GameObject marker in _spawnedMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        _spawnedMarkers.Clear();
    }

    private void SpawnMarker(GameObject prefab, Vector3 position, Color color)
    {
        GameObject marker = Instantiate(prefab, position, Quaternion.identity, transform);
        Renderer renderer = marker.GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = color;
        }

        _spawnedMarkers.Add(marker);
        StartCoroutine(PulseMarker(marker.transform));
    }


    private IEnumerator PulseMarker(Transform markerTransform)
    {
        if (markerTransform == null)
        {
            yield break;
        }

        Vector3 baseScale = markerTransform.localScale;
        Vector3 pulseScale = baseScale * pulseScaleMultiplier;
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.PingPong(elapsed * 4f, 1f);
            markerTransform.localScale = Vector3.Lerp(baseScale, pulseScale, normalized);
            yield return null;
        }

        markerTransform.localScale = baseScale;
    }
    private static Color GetPersonalityColor(BotPersonality personality)
    {
        return personality switch
        {
            BotPersonality.Careful => new Color(0.3f, 0.6f, 1f),
            BotPersonality.Balanced => new Color(1f, 0.95f, 0.35f),
            BotPersonality.Reckless => new Color(1f, 0.3f, 0.25f),
            _ => new Color(0.8f, 0.45f, 1f)
        };
    }
}
