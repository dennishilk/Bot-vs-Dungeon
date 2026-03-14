using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BotVsDungeon.UI
{
    public enum UIMode
    {
        Build,
        Simulation
    }

    public enum BuildItemType
    {
        Floor,
        Wall,
        Pit,
        Saw,
        Bomb,
        Archer,
        Start,
        Goal
    }

    /// <summary>
    /// Coordinates menu/HUD visibility and status text updates without owning gameplay logic.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject resultBannerPanel;

        [Header("Top Bar")]
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private TMP_Text simulationStatusText;

        [Header("Selection")]
        [SerializeField] private TMP_Text selectedItemText;
        [SerializeField] private TMP_Text trapDescriptionText;

        [Header("Control Buttons")]
        [SerializeField] private Button buildModeButton;
        [SerializeField] private Button simulateButton;
        [SerializeField] private Button clearDungeonButton;
        [SerializeField] private Button returnToMenuButton;

        [Header("Events")]
        [SerializeField] private UnityEvent onBuildModeRequested;
        [SerializeField] private UnityEvent onSimulateRequested;
        [SerializeField] private UnityEvent onClearDungeonRequested;
        [SerializeField] private UnityEvent onReturnToMenuRequested;

        [Header("Optional Components")]
        [SerializeField] private ResultBannerController resultBannerController;

        private UIMode currentMode = UIMode.Build;
        private BuildItemType currentItem = BuildItemType.Floor;

        private void Awake()
        {
            HookButtons();
            ApplyState();
            ShowMainMenu();
        }

        private void HookButtons()
        {
            if (buildModeButton != null)
            {
                buildModeButton.onClick.AddListener(() =>
                {
                    SetMode(UIMode.Build);
                    onBuildModeRequested?.Invoke();
                });
            }

            if (simulateButton != null)
            {
                simulateButton.onClick.AddListener(() =>
                {
                    SetMode(UIMode.Simulation);
                    SetSimulationStatus("Simulation Running");
                    onSimulateRequested?.Invoke();
                });
            }

            if (clearDungeonButton != null)
            {
                clearDungeonButton.onClick.AddListener(() =>
                {
                    SetSimulationStatus("Dungeon Cleared");
                    ClearResult();
                    onClearDungeonRequested?.Invoke();
                });
            }

            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(() =>
                {
                    ShowMainMenu();
                    onReturnToMenuRequested?.Invoke();
                });
            }
        }

        public void ShowMainMenu()
        {
            SetPanelState(mainMenuPanel, true);
            SetPanelState(hudPanel, false);
            ClearResult();
            SetSimulationStatus("Awaiting orders");
        }

        public void ShowHUD()
        {
            SetPanelState(mainMenuPanel, false);
            SetPanelState(hudPanel, true);
            SetMode(UIMode.Build);
            SetSimulationStatus("Build your dungeon");
        }

        public void SetMode(UIMode mode)
        {
            currentMode = mode;
            if (modeText != null)
            {
                modeText.text = mode == UIMode.Build ? "Mode: Build" : "Mode: Simulation";
            }
        }

        public void SetSelectedItem(BuildItemType item)
        {
            currentItem = item;
            if (selectedItemText != null)
            {
                selectedItemText.text = $"Selected: {item}";
            }

            if (trapDescriptionText != null)
            {
                trapDescriptionText.text = GetItemDescription(item);
            }
        }

        public void SetSimulationStatus(string status)
        {
            if (simulationStatusText != null)
            {
                simulationStatusText.text = $"Status: {status}";
            }
        }

        public void ShowResult(bool botSurvived)
        {
            SetPanelState(resultBannerPanel, true);
            if (resultBannerController != null)
            {
                resultBannerController.ShowResult(botSurvived ? "BOT SURVIVED" : "BOT DIED", botSurvived);
            }
        }

        public void ClearResult()
        {
            if (resultBannerController != null)
            {
                resultBannerController.HideResult();
            }
            else
            {
                SetPanelState(resultBannerPanel, false);
            }
        }

        private void ApplyState()
        {
            SetMode(currentMode);
            SetSelectedItem(currentItem);
            SetSimulationStatus("Build your dungeon");
            ClearResult();
        }

        private static void SetPanelState(GameObject panel, bool enabled)
        {
            if (panel != null)
            {
                panel.SetActive(enabled);
            }
        }

        private static string GetItemDescription(BuildItemType item)
        {
            switch (item)
            {
                case BuildItemType.Floor:
                    return "Walkable stone tile. Keeps paths clean and open.";
                case BuildItemType.Wall:
                    return "Solid obstacle. Use to force longer, riskier routes.";
                case BuildItemType.Pit:
                    return "Impassable hazard. One wrong step ends the journey.";
                case BuildItemType.Saw:
                    return "Continuous contact damage. Efficient, rude, and loud.";
                case BuildItemType.Bomb:
                    return "Explodes once when the bot gets close.";
                case BuildItemType.Archer:
                    return "Fires periodically in a straight line.";
                case BuildItemType.Start:
                    return "Bot spawn point. Required before simulation starts.";
                case BuildItemType.Goal:
                    return "Exit tile. Reach it and the bot survives.";
                default:
                    return string.Empty;
            }
        }
    }
}
