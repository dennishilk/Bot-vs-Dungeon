using UnityEngine;

public enum ObjectiveType
{
    SurviveAtLeastOne,
    KillAllBots,
    CarefulMustSurvive,
    RecklessMustFail,
    ReachDungeonRating
}

[System.Serializable]
public class LevelObjective
{
    public ObjectiveType objectiveType = ObjectiveType.SurviveAtLeastOne;
    public string targetRating = "Dangerous";
    [TextArea] public string objectiveDescription;
    [HideInInspector] public bool success;

    public string GetDisplayText()
    {
        if (!string.IsNullOrWhiteSpace(objectiveDescription))
        {
            return objectiveDescription;
        }

        return objectiveType switch
        {
            ObjectiveType.SurviveAtLeastOne => "Build a dungeon where at least one bot survives.",
            ObjectiveType.KillAllBots => "Build a dungeon where all bots die.",
            ObjectiveType.CarefulMustSurvive => "Build a dungeon where the Careful bot survives.",
            ObjectiveType.RecklessMustFail => "Build a dungeon where the Reckless bot fails.",
            ObjectiveType.ReachDungeonRating => $"Build a dungeon rated \"{targetRating}\".",
            _ => "Complete the challenge objective."
        };
    }
}
