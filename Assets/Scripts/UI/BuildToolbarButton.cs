using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BotVsDungeon.UI
{
    /// <summary>
    /// Handles selected/hover visual state for one build button and informs UIController.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BuildToolbarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UIController uiController;
        [SerializeField] private BuildToolbarController toolbarController;
        [SerializeField] private BuildItemType itemType;
        [SerializeField] private Image background;
        [SerializeField] private Image highlightBorder;
        [SerializeField] private TMP_Text label;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new(0.16f, 0.17f, 0.2f);
        [SerializeField] private Color hoverColor = new(0.22f, 0.24f, 0.28f);
        [SerializeField] private Color selectedColor = new(0.17f, 0.42f, 0.56f);
        [SerializeField] private Color normalTextColor = new(0.88f, 0.85f, 0.78f);
        [SerializeField] private Color selectedTextColor = new(0.97f, 0.97f, 0.97f);

        [Header("Events")]
        [SerializeField] private UnityEvent<BuildItemType> onBuildItemSelected;

        private Button button;
        private bool isSelected;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
            RefreshVisualState();
        }

        public void HandleClick()
        {
            AudioManager.Instance?.PlayUISound(SoundCue.UIButtonClick);
            toolbarController?.SelectButton(this);
            uiController?.SetSelectedItem(itemType);
            onBuildItemSelected?.Invoke(itemType);
            SetSelected(true);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            RefreshVisualState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelected && background != null)
            {
                background.color = hoverColor;
            }

            transform.localScale = Vector3.one * 1.03f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected)
            {
                RefreshVisualState();
            }

            transform.localScale = Vector3.one;
        }

        private void RefreshVisualState()
        {
            if (background != null)
            {
                background.color = isSelected ? selectedColor : normalColor;
            }

            if (label != null)
            {
                label.color = isSelected ? selectedTextColor : normalTextColor;
            }

            if (highlightBorder != null)
            {
                highlightBorder.enabled = isSelected;
                Color borderColor = selectedColor;
                borderColor.a = 0.9f;
                highlightBorder.color = borderColor;
            }
        }
    }
}
