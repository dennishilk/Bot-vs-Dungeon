using System.Collections;
using UnityEngine;

public class GoalFeedback : MonoBehaviour
{
    [SerializeField] private Light goalLight;
    [SerializeField] private ParticleSystem sparkleParticles;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float baseIntensity = 1f;
    [SerializeField] private float pulseAmplitude = 0.25f;
    [SerializeField] private float successBoost = 1.6f;
    [SerializeField] private float successDuration = 0.65f;
    [SerializeField] private float successPulseSpeedMultiplier = 1.8f;

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
        float originalIntensity = baseIntensity;
        float originalSpeed = pulseSpeed;
        baseIntensity *= successBoost;
        pulseSpeed *= successPulseSpeedMultiplier;

        float elapsed = 0f;
        while (elapsed < successDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        baseIntensity = originalIntensity;
        pulseSpeed = originalSpeed;
        _boostRoutine = null;
    }
}
