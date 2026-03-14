using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite defaultIcon;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.28f;
    [SerializeField] private float visibleDuration = 2.2f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private Vector2 hiddenAnchorPosition = new(-420f, -24f);
    [SerializeField] private Vector2 shownAnchorPosition = new(24f, -24f);

    private Coroutine _popupRoutine;

    private void Awake()
    {
        HideImmediate();
    }

    public void Show(AchievementData data)
    {
        if (data == null)
        {
            return;
        }

        if (_popupRoutine != null)
        {
            StopCoroutine(_popupRoutine);
        }

        gameObject.SetActive(true);
        titleText.text = data.title;
        descriptionText.text = data.description;
        if (iconImage != null)
        {
            iconImage.sprite = defaultIcon;
        }

        _popupRoutine = StartCoroutine(PopupRoutine());
    }

    public void HideImmediate()
    {
        if (popupRect != null)
        {
            popupRect.anchoredPosition = hiddenAnchorPosition;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator PopupRoutine()
    {
        canvasGroup.alpha = 1f;
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            popupRect.anchoredPosition = Vector2.Lerp(hiddenAnchorPosition, shownAnchorPosition, t);
            yield return null;
        }

        popupRect.anchoredPosition = shownAnchorPosition;

        yield return new WaitForSecondsRealtime(visibleDuration);

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        HideImmediate();
        _popupRoutine = null;
    }
}
