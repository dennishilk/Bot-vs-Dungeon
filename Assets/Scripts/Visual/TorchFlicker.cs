using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Light targetLight;
    [SerializeField] private Transform flameVisual;

    [Header("Intensity")]
    [SerializeField] private float minIntensity = 0.75f;
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float flickerSpeed = 7f;

    [Header("Range + Color Drift")]
    [SerializeField] private float minRange = 4f;
    [SerializeField] private float maxRange = 5.25f;
    [SerializeField] private Color warmA = new(1f, 0.56f, 0.28f, 1f);
    [SerializeField] private Color warmB = new(1f, 0.72f, 0.36f, 1f);

    [Header("Flame Visual")]
    [SerializeField] private float scaleJitter = 0.08f;
    [SerializeField] private Vector3 swayAmplitude = new(0.012f, 0.02f, 0.012f);

    private float _seed;
    private Vector3 _baseScale;
    private Vector3 _basePosition;

    private void Awake()
    {
        _seed = Random.Range(0f, 1000f);

        if (flameVisual != null)
        {
            _baseScale = flameVisual.localScale;
            _basePosition = flameVisual.localPosition;
        }
    }

    private void Update()
    {
        float t = Time.time * flickerSpeed;
        float n1 = Mathf.PerlinNoise(_seed, t);
        float n2 = Mathf.PerlinNoise(_seed + 47.23f, t * 0.7f);

        if (targetLight != null)
        {
            targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, n1);
            targetLight.range = Mathf.Lerp(minRange, maxRange, n2);
            targetLight.color = Color.Lerp(warmA, warmB, n2);
        }

        if (flameVisual != null)
        {
            float jitter = 1f + ((n1 - 0.5f) * 2f * scaleJitter);
            flameVisual.localScale = _baseScale * jitter;
            flameVisual.localPosition = _basePosition + new Vector3(
                Mathf.Sin(t * 0.9f) * swayAmplitude.x,
                Mathf.Sin(t * 1.7f) * swayAmplitude.y,
                Mathf.Cos(t * 1.1f) * swayAmplitude.z);
        }
    }
}
