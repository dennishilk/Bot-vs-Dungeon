using UnityEngine;

[System.Serializable]
public struct BotLearningProfile
{
    public BotPersonality personality;
    public float riskToleranceModifier;
    public float learningStrength;
    public float forgetfulnessFactor;

    public float EffectiveLearningMultiplier => Mathf.Max(0f, learningStrength * riskToleranceModifier);

    public static BotLearningProfile FromPersonality(
        BotPersonality personality,
        float carefulLearningMultiplier,
        float balancedLearningMultiplier,
        float recklessLearningMultiplier,
        float panicLearningMultiplier,
        float forgetfulnessRate)
    {
        float learning = personality switch
        {
            BotPersonality.Careful => carefulLearningMultiplier,
            BotPersonality.Reckless => recklessLearningMultiplier,
            BotPersonality.Panic => panicLearningMultiplier,
            _ => balancedLearningMultiplier
        };

        float riskTolerance = personality switch
        {
            BotPersonality.Careful => 1.15f,
            BotPersonality.Reckless => 0.65f,
            BotPersonality.Panic => 1.4f,
            _ => 1f
        };

        return new BotLearningProfile
        {
            personality = personality,
            riskToleranceModifier = riskTolerance,
            learningStrength = learning,
            forgetfulnessFactor = forgetfulnessRate
        };
    }
}
