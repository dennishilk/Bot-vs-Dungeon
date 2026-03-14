using System.Collections;
using UnityEngine;

public class LightCameraShake : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 0.15f;
    [SerializeField] private float defaultMagnitude = 0.08f;

    private Vector3 _origin;
    private Coroutine _shakeRoutine;

    public static LightCameraShake Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _origin = transform.localPosition;
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
        }

        _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 jitter = Random.insideUnitCircle * magnitude;
            transform.localPosition = _origin + new Vector3(jitter.x, 0f, jitter.y);
            yield return null;
        }

        transform.localPosition = _origin;
        _shakeRoutine = null;
    }
}
