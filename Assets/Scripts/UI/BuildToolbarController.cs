using System.Collections.Generic;
using UnityEngine;

namespace BotVsDungeon.UI
{
    /// <summary>
    /// Keeps only one build toolbar button selected at a time.
    /// </summary>
    public class BuildToolbarController : MonoBehaviour
    {
        [SerializeField] private List<BuildToolbarButton> buttons = new List<BuildToolbarButton>();

        private void Start()
        {
            if (buttons.Count > 0)
            {
                SelectButton(buttons[0]);
                buttons[0].HandleClick();
            }
        }

        public void SelectButton(BuildToolbarButton selected)
        {
            foreach (BuildToolbarButton button in buttons)
            {
                if (button != null)
                {
                    button.SetSelected(button == selected);
                }
            }
        }
    }
}
