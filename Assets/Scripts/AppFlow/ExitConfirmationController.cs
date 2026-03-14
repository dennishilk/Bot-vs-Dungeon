using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    public class ExitConfirmationController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        public void Open()
        {
            panelRoot?.SetActive(true);
            AppStateManager.Instance?.ChangeState(AppState.ExitConfirmation);
        }

        public void ConfirmExit()
        {
            panelRoot?.SetActive(false);
            AppStateManager.Instance?.QuitNow();
        }

        public void CancelExit()
        {
            panelRoot?.SetActive(false);
            AppStateManager.Instance?.ReturnToMenu();
        }
    }
}
