using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ReplayFrameData
{
    public float timestamp;
    public Vector3 botPosition;
    public float botHP;
    public BotState currentState;
}

[Serializable]
public class RunReplayData
{
    public BotPersonality personality;
    public bool survived;
    public string causeOfDeath;
    public Vector3 deathPosition;
    public float completionTime;
    public List<ReplayFrameData> frames = new();

    public string ToSummaryLine()
    {
        string outcome = survived ? "Survived" : $"Died ({causeOfDeath})";
        return $"{personality}: {outcome} | {completionTime:0.00}s | Frames {frames.Count}";
    }
}

[Serializable]
public class CertificationReplayData
{
    public string sessionName;
    public long createdUnixTime;
    public List<RunReplayData> runReplays = new();
}
