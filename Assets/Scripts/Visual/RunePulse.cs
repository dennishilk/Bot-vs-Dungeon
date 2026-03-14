using UnityEngine;

public class RunePulse : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string emissionProperty = "_EmissionColor";
    [SerializeField] private Color lowEmission = new(0.07f, 0.18f, 0.35f, 1f);
    [SerializeField] private Color highEmission = new(0.22f, 0.45f, 0.88f, 1f);
    [SerializeField] private float pulseSpeed = 1.4f;
    [SerializeField] private bool affectScale = true;
    [SerializeField] private float scalePulse = 0.025f;

    private Material _materialInstance;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;

        if (targetRenderer != null)
        {
            _materialInstance = targetRenderer.material;
        }
    }

    private void Update()
    {
        float pulse = 0.5f + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;

        if (_materialInstance != null)
        {
            _materialInstance.SetColor(emissionProperty, Color.Lerp(lowEmission, highEmission, pulse));
        }

        if (affectScale)
        {
            float s = 1f + (pulse - 0.5f) * 2f * scalePulse;
            transform.localScale = _baseScale * s;
        }
    }
}
