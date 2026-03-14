using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private Transform flameVisual;
    [SerializeField] private float minIntensity = 0.75f;
    [SerializeField] private float maxIntensity = 1.15f;
    [SerializeField] private float flickerSpeed = 8f;
    [SerializeField] private float scaleJitter = 0.06f;

    private float _seed;
    private Vector3 _baseScale;

    private void Awake()
    {
        _seed = Random.Range(0f, 1000f);

        if (flameVisual != null)
        {
            _baseScale = flameVisual.localScale;
        }
    }

    private void Update()
    {
        float t = Time.time * flickerSpeed;
        float noise = Mathf.PerlinNoise(_seed, t);

        if (targetLight != null)
        {
            targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        }

        if (flameVisual != null)
        {
            float jitter = 1f + ((noise - 0.5f) * 2f * scaleJitter);
            flameVisual.localScale = _baseScale * jitter;
        }
    }
}
