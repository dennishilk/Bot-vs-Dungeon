using System.Collections;
using UnityEngine;

public class GoalFeedback : MonoBehaviour
{
    [SerializeField] private Light goalLight;
    [SerializeField] private ParticleSystem sparkleParticles;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float baseIntensity = 1f;
    [SerializeField] private float pulseAmplitude = 0.25f;
    [SerializeField] private float successBoost = 1.4f;
    [SerializeField] private float successDuration = 0.5f;

    private Coroutine _boostRoutine;

    private void Update()
    {
        if (goalLight == null)
        {
            return;
        }

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        goalLight.intensity = baseIntensity + pulse;
    }

    public void PlaySuccessFeedback()
    {
        if (sparkleParticles != null)
        {
            sparkleParticles.Play();
        }

        if (_boostRoutine != null)
        {
            StopCoroutine(_boostRoutine);
        }

        _boostRoutine = StartCoroutine(SuccessBoostRoutine());
    }

    private IEnumerator SuccessBoostRoutine()
    {
        float original = baseIntensity;
        baseIntensity *= successBoost;
        yield return new WaitForSeconds(successDuration);
        baseIntensity = original;
        _boostRoutine = null;
    }
}
