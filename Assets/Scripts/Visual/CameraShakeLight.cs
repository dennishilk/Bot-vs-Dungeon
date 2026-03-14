using System.Collections;
using UnityEngine;

public class CameraShakeLight : MonoBehaviour
{
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float magnitude = 0.08f;

    private Vector3 _originalPos;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        _originalPos = transform.localPosition;
    }

    public void Shake()
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
        }

        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 jitter = Random.insideUnitCircle * magnitude;
            transform.localPosition = _originalPos + new Vector3(jitter.x, 0f, jitter.y);
            yield return null;
        }

        transform.localPosition = _originalPos;
        _shakeRoutine = null;
    }
}
