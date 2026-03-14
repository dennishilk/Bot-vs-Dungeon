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

        [Header("Theme Colors")]
        [SerializeField] private Color successColor = new Color(0.23f, 0.5f, 0.3f, 0.92f);
        [SerializeField] private Color failColor = new Color(0.55f, 0.17f, 0.17f, 0.92f);

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
        }

        public void HideResult()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}
