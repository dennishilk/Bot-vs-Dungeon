using UnityEngine;

public class PitMistController : MonoBehaviour
{
    [SerializeField] private ParticleSystem mistParticles;
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobSpeed = 0.45f;
    [SerializeField] private float alphaMin = 0.25f;
    [SerializeField] private float alphaMax = 0.45f;

    private Transform _cachedTransform;
    private Vector3 _basePos;
    private Material _materialInstance;
    private Color _baseColor;

    private void Awake()
    {
        _cachedTransform = transform;
        _basePos = _cachedTransform.localPosition;

        if (mistParticles != null)
        {
            var renderer = mistParticles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.material != null)
            {
                _materialInstance = renderer.material;
                _baseColor = _materialInstance.color;
            }
        }
    }

    private void Update()
    {
        float t = Time.time * bobSpeed;
        _cachedTransform.localPosition = _basePos + new Vector3(0f, Mathf.Sin(t) * bobAmplitude, 0f);

        if (_materialInstance == null)
        {
            return;
        }

        float alphaT = 0.5f + Mathf.Sin(t * 1.8f + 1.37f) * 0.5f;
        Color c = _baseColor;
        c.a = Mathf.Lerp(alphaMin, alphaMax, alphaT);
        _materialInstance.color = c;
    }
}
