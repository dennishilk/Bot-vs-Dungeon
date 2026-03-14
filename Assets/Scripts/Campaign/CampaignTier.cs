using System;
using System.Collections.Generic;

[Serializable]
public class CampaignTier
{
    public string tierID;
    public string title;
    public int unlockBureauScore;
    public List<CampaignAssignment> assignments = new();

    [TextArea]
    public string tierFlavor;
}
