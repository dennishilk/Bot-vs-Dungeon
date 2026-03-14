using System;
using System.Collections.Generic;
using UnityEngine;

public enum ReplayEventType
{
    BotSpawn,
    BotDamage,
    BotNearDeath,
    BotDeath,
    TrapActivated,
    TrapChain,
    GoalReached,
    CameraFocus,
    StressMultiDeath,
    RareSurvival
}

public enum CameraBehaviorType
{
    FollowBot,
    ZoomToTrap,
    FocusOnDeath,
    WideDungeonView,
    GoalCelebrationShot,
    IntroPan
}

[Serializable]
public class ReplayFrameData
{
    public float timestamp;
    public Vector3 botPosition;
    public float botHP;
    public BotState currentState;
}

[Serializable]
public class ReplayEventData
{
    public float timestamp;
    public ReplayEventType eventType;
    public Vector3 worldPosition;
    public string sourceId;
    public float intensity;
    public string details;
}

[Serializable]
public class ReplayHighlightData
{
    public float timestamp;
    public float duration = 2f;
    public ReplayEventType highlightType;
    public Vector3 focusPoint;
    public float score;
    public string description;
}

[Serializable]
public class ReplayCameraFocusPoint
{
    public float timestamp;
    public Vector3 worldPosition;
    public float weight;
    public CameraBehaviorType suggestedBehavior;
}

[Serializable]
public class ReplayCameraEvent
{
    public float timestamp;
    public CameraBehaviorType behavior;
    public Vector3 focusPoint;
    public float blendDuration;
    public float zoom;
    public bool slowMotion;
}

[Serializable]
public class ReplayTimelineData
{
    public List<ReplayEventData> events = new();
    public List<ReplayHighlightData> highlights = new();
    public List<ReplayCameraEvent> cameraEvents = new();

    public bool HasHighlights => highlights.Count > 0;
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
    public List<ReplayEventData> events = new();
    public List<ReplayCameraFocusPoint> cameraFocusPoints = new();
    public ReplayTimelineData timeline = new();

    public string ToSummaryLine()
    {
        string outcome = survived ? "Survived" : $"Died ({causeOfDeath})";
        int highlightCount = timeline != null ? timeline.highlights.Count : 0;
        return $"{personality}: {outcome} | {completionTime:0.00}s | Frames {frames.Count} | Highlights {highlightCount}";
    }
}

[Serializable]
public class CertificationReplayData
{
    public string sessionName;
    public long createdUnixTime;
    public List<RunReplayData> runReplays = new();
}
