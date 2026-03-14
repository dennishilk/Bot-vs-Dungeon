using System.Collections;
using UnityEngine;

public class HitFlashEffect : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color hitColor = new(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private ParticleSystem hitParticle;

    private Material _material;
    private Color _baseColor;

    private void Awake()
    {
        if (targetRenderer != null)
        {
            _material = targetRenderer.material;
            _baseColor = _material.color;
        }
    }

    public void PlayHitFlash(Vector3 hitPosition)
    {
        if (hitParticle != null)
        {
            hitParticle.transform.position = hitPosition;
            hitParticle.Play();
        }

        if (_material != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        _material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        _material.color = _baseColor;
    }
}
