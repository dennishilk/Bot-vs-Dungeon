using System;

public static class CampaignMath
{
    public static int GetRatingRank(string rating)
    {
        if (string.IsNullOrWhiteSpace(rating))
        {
            return 0;
        }

        return rating.Trim().ToLowerInvariant() switch
        {
            "unsafe" => 1,
            "fair" => 2,
            "safe" => 3,
            "impossible" => 4,
            _ => 0
        };
    }

    public static string BuildPromotionMessage(string rankTitle)
    {
        return $"Promotion approved. You are now '{rankTitle}'. Please sign Form 44-B before your next lethal review.";
    }
}
