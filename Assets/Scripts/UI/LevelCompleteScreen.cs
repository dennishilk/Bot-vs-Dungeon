using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelCompleteScreen : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text ratingText;
    [SerializeField] private TMP_Text starsText;

    [Header("Button Events")]
    [SerializeField] private UnityEvent onNextLevel;
    [SerializeField] private UnityEvent onRetry;
    [SerializeField] private UnityEvent onReturnToMenu;

    public void Show(string levelName, string objective, string rating, int stars)
    {
        panelRoot?.SetActive(true);
        if (levelNameText != null) levelNameText.text = levelName;
        if (objectiveText != null) objectiveText.text = objective;
        if (ratingText != null) ratingText.text = $"Dungeon Rating: {rating}";
        if (starsText != null) starsText.text = new string('★', Mathf.Clamp(stars, 1, 3));
    }

    public void Hide()
    {
        panelRoot?.SetActive(false);
    }

    public void NextLevel() => onNextLevel?.Invoke();
    public void Retry() => onRetry?.Invoke();
    public void ReturnToMenu() => onReturnToMenu?.Invoke();
}
