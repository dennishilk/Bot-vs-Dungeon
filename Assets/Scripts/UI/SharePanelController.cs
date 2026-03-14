using TMPro;
using UnityEngine;

public class SharePanelController : MonoBehaviour
{
    [SerializeField] private ShareCodeManager shareCodeManager;
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string exportedDungeonName = "shared_dungeon";
    [SerializeField] private GameObject panelRoot;

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    public void ExportCodeClicked()
    {
        if (shareCodeManager == null || codeInput == null)
        {
            return;
        }

        codeInput.text = shareCodeManager.ExportCurrentDungeonCode(exportedDungeonName);
        SetStatus("Share code exported.", true);
    }

    public void ImportCodeClicked()
    {
        if (shareCodeManager == null || codeInput == null)
        {
            return;
        }

        if (!shareCodeManager.TryImportCode(codeInput.text, out DungeonSaveData data, out string message))
        {
            SetStatus(message, false);
            return;
        }

        bool applied = shareCodeManager.ApplyImportedDungeon(data, out string applyMessage);
        SetStatus(applied ? "Share code imported successfully." : applyMessage, applied);
    }

    public void CopyCodeClicked()
    {
        if (codeInput == null)
        {
            return;
        }

        GUIUtility.systemCopyBuffer = codeInput.text;
        SetStatus("Share code copied to clipboard.", true);
    }

    public void PasteCodeClicked()
    {
        if (codeInput == null)
        {
            return;
        }

        codeInput.text = GUIUtility.systemCopyBuffer;
        SetStatus("Clipboard pasted.", true);
    }

    private void SetStatus(string message, bool success)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = success ? new Color(0.5f, 0.95f, 0.6f) : new Color(0.95f, 0.45f, 0.45f);
        }
    }
}
