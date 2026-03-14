using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance { get; private set; }

    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private float transitionSpeed = 2.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetAlpha(0f);
    }

    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadRoutine(sceneName));
    }

    public IEnumerator FadeOutIn()
    {
        yield return FadeRoutine(0f, 1f);
        yield return FadeRoutine(1f, 0f);
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        yield return FadeRoutine(0f, 1f);
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return FadeRoutine(1f, 0f);
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        float elapsed = 0f;
        float duration = 1f / Mathf.Max(0.01f, transitionSpeed);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (transitionCanvasGroup == null)
        {
            return;
        }

        transitionCanvasGroup.alpha = alpha;
        transitionCanvasGroup.blocksRaycasts = alpha > 0.9f;
        transitionCanvasGroup.interactable = alpha > 0.9f;
    }
}
