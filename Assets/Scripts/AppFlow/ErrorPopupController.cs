using TMPro;
using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    public class ErrorPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        public void ShowError(string title, string body)
        {
            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(title) ? "Something went wrong" : title;
            }

            if (bodyText != null)
            {
                bodyText.text = string.IsNullOrWhiteSpace(body)
                    ? "Please try again. If this keeps happening, return to the main menu."
                    : body;
            }

            panelRoot?.SetActive(true);
        }

        public void Close()
        {
            panelRoot?.SetActive(false);
        }
    }
}
