using UnityEngine;

public class MainMenuAtmosphereMotion : MonoBehaviour
{
    [SerializeField] private Vector3 motionAmplitude = new(0.08f, 0.05f, 0.08f);
    [SerializeField] private float motionSpeed = 0.3f;
    [SerializeField] private Light menuLight;
    [SerializeField] private float lightFlickerAmount = 0.12f;

    private Vector3 _startPosition;
    private float _baseLightIntensity;

    private void Awake()
    {
        _startPosition = transform.position;
        if (menuLight != null)
        {
            _baseLightIntensity = menuLight.intensity;
        }
    }

    private void Update()
    {
        float t = Time.time * motionSpeed;
        transform.position = _startPosition + new Vector3(
            Mathf.Sin(t) * motionAmplitude.x,
            Mathf.Sin(t * 1.3f) * motionAmplitude.y,
            Mathf.Cos(t * 0.8f) * motionAmplitude.z);

        if (menuLight != null)
        {
            menuLight.intensity = _baseLightIntensity + Mathf.Sin(t * 2.2f) * lightFlickerAmount;
        }
    }
}
