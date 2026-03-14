using System.Collections;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private ParticleSystem smokeParticle;
    [SerializeField] private Light flashLight;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float flashIntensity = 6f;

    public void PlayAt(Vector3 worldPosition)
    {
        transform.position = worldPosition;

        if (explosionParticle != null)
        {
            explosionParticle.Play();
        }

        if (smokeParticle != null)
        {
            smokeParticle.Play();
        }

        if (flashLight != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        flashLight.enabled = true;
        flashLight.intensity = flashIntensity;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(flashIntensity, 0f, elapsed / flashDuration);
            yield return null;
        }

        flashLight.intensity = 0f;
        flashLight.enabled = false;
    }
}
