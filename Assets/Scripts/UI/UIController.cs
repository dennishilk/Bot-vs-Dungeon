using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Text modeText;
    [SerializeField] private Text selectionText;
    [SerializeField] private Text statusText;

    public void SetMode(string mode)
    {
        if (modeText != null)
        {
            modeText.text = mode;
        }
    }

    public void SetSelection(string selected)
    {
        if (selectionText != null)
        {
            selectionText.text = $"Selected: {selected}";
        }
    }

    public void SetStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
    }
}
