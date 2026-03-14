using TMPro;
using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private TMP_Text botHpText;
    [SerializeField] private TMP_Text botStateText;
    [SerializeField] private TMP_Text targetTileText;
    [SerializeField] private TMP_Text pathLengthText;
    [SerializeField] private TMP_Text currentTileText;
    [SerializeField] private TMP_Text decisionText;
    [SerializeField] private TMP_Text dangerScoreText;
    [SerializeField] private TMP_Text simulationTimeText;

    private void Update()
    {
        BotAgent bot = simulationManager != null ? simulationManager.ActiveBot : null;
        BotHealth health = bot != null ? bot.GetComponent<BotHealth>() : null;

        SetText(botHpText, $"Bot HP: {(health != null ? health.CurrentHp.ToString("0") : "-")}");
        SetText(botStateText, $"Bot State: {(bot != null ? bot.CurrentState.ToString() : "None")}");
        SetText(targetTileText, $"Current Target Tile: {(bot != null ? bot.CurrentTargetTile.ToString() : "-")}");
        SetText(pathLengthText, $"Current Path Length: {(bot != null ? bot.CurrentPath.Count.ToString() : "0")}");
        SetText(currentTileText, $"Current Tile Position: {(bot != null ? bot.CurrentTilePosition.ToString() : "-")}");
        SetText(decisionText, $"Last Decision: {(bot != null ? bot.LastDecision : "Idle")}");
        SetText(dangerScoreText, $"Danger Score: {(bot != null ? bot.LastDangerScore.ToString("0.0") : "0")}");
        SetText(simulationTimeText, $"Simulation Time: {(simulationManager != null ? simulationManager.SimulationTime.ToString("0.00") : "0.00")}s");
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
