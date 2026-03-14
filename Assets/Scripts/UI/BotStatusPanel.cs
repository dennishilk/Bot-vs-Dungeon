using TMPro;
using UnityEngine;

namespace BotVsDungeon.UI
{
    public enum BotUIState
    {
        Idle,
        Moving,
        Hurt,
        Dead,
        Success
    }

    /// <summary>
    /// Lightweight bridge for showing bot HP/state in HUD.
    /// </summary>
    public class BotStatusPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private int maxHp = 100;

        private int currentHp;
        private BotUIState currentState = BotUIState.Idle;

        private void Awake()
        {
            currentHp = maxHp;
            Refresh();
        }

        public void SetBotHealth(int hp)
        {
            currentHp = Mathf.Max(0, hp);
            Refresh();
        }

        public void SetBotState(BotUIState state)
        {
            currentState = state;
            Refresh();
        }

        public void ResetStatus()
        {
            currentHp = maxHp;
            currentState = BotUIState.Idle;
            Refresh();
        }

        private void Refresh()
        {
            if (hpText != null)
            {
                hpText.text = $"HP: {currentHp}";
            }

            if (stateText != null)
            {
                stateText.text = $"State: {currentState}";
            }
        }
    }
}
