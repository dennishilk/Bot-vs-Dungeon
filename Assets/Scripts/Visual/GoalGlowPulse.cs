using UnityEngine;

public class GoalGlowPulse : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string emissionProperty = "_EmissionColor";
    [SerializeField] private Color baseEmission = new(0.1f, 0.5f, 0.2f);
    [SerializeField] private Color pulseEmission = new(0.25f, 0.85f, 0.45f);
    [SerializeField] private float pulseSpeed = 1.8f;

    private Material _instanceMaterial;

    private void Awake()
    {
        if (targetRenderer != null)
        {
            _instanceMaterial = targetRenderer.material;
        }
    }

    private void Update()
    {
        if (_instanceMaterial == null)
        {
            return;
        }

        float pulse = 0.5f + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
        _instanceMaterial.SetColor(emissionProperty, Color.Lerp(baseEmission, pulseEmission, pulse));
    }
}
