using System.Collections.Generic;
using UnityEngine;

public class HeatmapVisualizer : MonoBehaviour
{
    [SerializeField] private DeathHeatmapManager deathHeatmapManager;
    [SerializeField] private GameObject heatmapTilePrefab;
    [SerializeField] private Gradient heatmapGradient;
    [SerializeField] private float heatmapIntensityMultiplier = 1f;
    [SerializeField] private float overlayHeight = 0.03f;
    [SerializeField] private bool showHeatmap = true;

    private readonly Dictionary<Vector2Int, Renderer> _tileRenderers = new();
    private readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
    private readonly int _colorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (heatmapGradient == null || heatmapGradient.colorKeys.Length == 0)
        {
            heatmapGradient = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(new Color(1f, 0.92f, 0.4f), 0f),
                    new GradientColorKey(new Color(1f, 0.57f, 0.15f), 0.5f),
                    new GradientColorKey(new Color(0.88f, 0.18f, 0.15f), 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0.15f, 0f),
                    new GradientAlphaKey(0.55f, 0.5f),
                    new GradientAlphaKey(0.85f, 1f)
                }
            };
        }

        if (deathHeatmapManager != null)
        {
            deathHeatmapManager.OnHeatmapChanged += RebuildHeatmap;
        }

        RebuildHeatmap();
    }

    private void OnDestroy()
    {
        if (deathHeatmapManager != null)
        {
            deathHeatmapManager.OnHeatmapChanged -= RebuildHeatmap;
        }
    }

    public void SetHeatmapVisible(bool visible)
    {
        showHeatmap = visible;
        foreach (Renderer renderer in _tileRenderers.Values)
        {
            if (renderer != null)
            {
                renderer.enabled = showHeatmap;
            }
        }
    }

    public void ToggleHeatmap()
    {
        SetHeatmapVisible(!showHeatmap);
    }

    public void ResetHeatmap()
    {
        deathHeatmapManager?.ResetHeatmap();
    }

    public void RebuildHeatmap()
    {
        if (deathHeatmapManager == null)
        {
            return;
        }

        int maxDeaths = Mathf.Max(1, deathHeatmapManager.GetMaxDeathCount());

        HashSet<Vector2Int> staleTiles = new(_tileRenderers.Keys);
        foreach (KeyValuePair<Vector2Int, int> entry in deathHeatmapManager.DeathCounts)
        {
            staleTiles.Remove(entry.Key);

            Renderer renderer = GetOrCreateTileRenderer(entry.Key);
            if (renderer == null)
            {
                continue;
            }

            float normalized = Mathf.Clamp01(entry.Value / (float)maxDeaths * heatmapIntensityMultiplier);
            Color heatColor = heatmapGradient.Evaluate(normalized);
            renderer.enabled = showHeatmap;

            MaterialPropertyBlock block = new();
            renderer.GetPropertyBlock(block);
            block.SetColor(_baseColorId, heatColor);
            block.SetColor(_colorId, heatColor);
            renderer.SetPropertyBlock(block);
        }

        foreach (Vector2Int tile in staleTiles)
        {
            if (_tileRenderers.TryGetValue(tile, out Renderer renderer) && renderer != null)
            {
                Destroy(renderer.gameObject);
            }
            _tileRenderers.Remove(tile);
        }
    }

    private Renderer GetOrCreateTileRenderer(Vector2Int tile)
    {
        if (_tileRenderers.TryGetValue(tile, out Renderer existing) && existing != null)
        {
            return existing;
        }

        if (heatmapTilePrefab == null)
        {
            return null;
        }

        Vector3 spawnPosition = new(tile.x, overlayHeight, tile.y);
        GameObject tileObject = Instantiate(heatmapTilePrefab, spawnPosition, Quaternion.identity, transform);
        tileObject.name = $"HeatmapTile_{tile.x}_{tile.y}";

        Renderer renderer = tileObject.GetComponentInChildren<Renderer>();
        _tileRenderers[tile] = renderer;
        return renderer;
    }
}
