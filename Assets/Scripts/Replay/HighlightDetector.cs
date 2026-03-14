using System.Collections.Generic;
using UnityEngine;

public class HighlightDetector : MonoBehaviour
{
    [SerializeField] private ReplayTimeline replayTimeline;
    [SerializeField] private float nearDeathThreshold = 20f;
    [SerializeField] private float rareSurvivalHealthThreshold = 12f;
    [SerializeField] private float stressDeathWindow = 2f;
    [SerializeField] private int stressDeathsForHighlight = 3;
    [SerializeField] private float baseHighlightDuration = 2.4f;
    [SerializeField] private float sensitivity = 1f;

    private readonly Queue<float> _recentDeathTimestamps = new();

    public void AnalyzeEvent(ReplayEventData evt)
    {
        if (replayTimeline == null)
        {
            return;
        }

        switch (evt.eventType)
        {
            case ReplayEventType.BotDeath:
                RegisterDeath(evt);
                break;
            case ReplayEventType.BotNearDeath:
                if (evt.intensity >= nearDeathThreshold * Mathf.Max(0.15f, sensitivity))
                {
                    AddHighlight(evt, ReplayEventType.BotNearDeath, "Near death recovery", 0.9f);
                }
                break;
            case ReplayEventType.GoalReached:
                AddHighlight(evt, ReplayEventType.GoalReached, "Goal reached", 1f);
                break;
            case ReplayEventType.TrapChain:
                AddHighlight(evt, ReplayEventType.TrapChain, "Trap chain reaction", 0.95f);
                break;
            case ReplayEventType.RareSurvival:
                if (evt.intensity <= rareSurvivalHealthThreshold * Mathf.Max(0.15f, sensitivity))
                {
                    AddHighlight(evt, ReplayEventType.RareSurvival, "Rare survival", 1.1f);
                }
                break;
        }
    }

    private void RegisterDeath(ReplayEventData evt)
    {
        AddHighlight(evt, ReplayEventType.BotDeath, "Bot death", 1.25f);

        _recentDeathTimestamps.Enqueue(evt.timestamp);
        while (_recentDeathTimestamps.Count > 0 && evt.timestamp - _recentDeathTimestamps.Peek() > stressDeathWindow)
        {
            _recentDeathTimestamps.Dequeue();
        }

        if (_recentDeathTimestamps.Count >= stressDeathsForHighlight)
        {
            AddHighlight(evt, ReplayEventType.StressMultiDeath, "Multiple bot deaths", 1.35f);
            _recentDeathTimestamps.Clear();
        }
    }

    private void AddHighlight(ReplayEventData evt, ReplayEventType type, string description, float score)
    {
        replayTimeline.AddHighlight(new ReplayHighlightData
        {
            timestamp = evt.timestamp,
            duration = baseHighlightDuration,
            highlightType = type,
            focusPoint = evt.worldPosition,
            score = score,
            description = description
        });
    }
}
