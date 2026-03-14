using UnityEngine;

public class PitTrap : TrapBase
{
    public override float PathCostPenalty => 9999f;

    public override void HandleBot(BotHealth botHealth)
    {
        // Pit is impassable and should not be traversed.
    }
}
