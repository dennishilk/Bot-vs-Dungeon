using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BotVsDungeon.UI
{
    /// <summary>
    /// Displays end-of-run outcome and optional tinting.
    /// </summary>
    public class ResultBannerController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Image bannerBackground;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Theme Colors")]
        [SerializeField] private Color successColor = new(0.23f, 0.5f, 0.3f, 0.92f);
        [SerializeField] private Color failColor = new(0.55f, 0.17f, 0.17f, 0.92f);
        [SerializeField] private float fadeDuration = 0.22f;

        private Coroutine _animateRoutine;

        public void ShowResult(string text, bool wasSuccess)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (resultText != null)
            {
                resultText.text = text;
            }

            if (bannerBackground != null)
            {
                bannerBackground.color = wasSuccess ? successColor : failColor;
            }

            if (canvasGroup != null)
            {
                if (_animateRoutine != null)
                {
                    StopCoroutine(_animateRoutine);
                }

                _animateRoutine = StartCoroutine(FadeInRoutine(wasSuccess));
            }
        }

        public void HideResult()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private IEnumerator FadeInRoutine(bool wasSuccess)
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            Color pulseFrom = wasSuccess ? successColor : failColor;
            Color pulseTo = pulseFrom * 1.15f;
            pulseTo.a = pulseFrom.a;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = t;

                if (bannerBackground != null)
                {
                    bannerBackground.color = Color.Lerp(pulseTo, pulseFrom, t);
                }

                yield return null;
            }

            canvasGroup.alpha = 1f;
            if (bannerBackground != null)
            {
                bannerBackground.color = pulseFrom;
            }

            _animateRoutine = null;
        }
    }
}
