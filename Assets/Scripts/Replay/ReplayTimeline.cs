using System.Collections.Generic;
using UnityEngine;

public class ReplayTimeline : MonoBehaviour
{
    [SerializeField] private float chainReactionWindow = 1.5f;
    [SerializeField] private float defaultHighlightDuration = 2.2f;

    private readonly List<ReplayEventData> _events = new();
    private readonly List<ReplayHighlightData> _highlights = new();
    private readonly List<ReplayCameraEvent> _cameraEvents = new();
    private ReplayEventData _lastTrapEvent;

    public void Clear()
    {
        _events.Clear();
        _highlights.Clear();
        _cameraEvents.Clear();
        _lastTrapEvent = null;
    }

    public void AddEvent(ReplayEventData evt)
    {
        _events.Add(evt);

        if (evt.eventType == ReplayEventType.TrapActivated)
        {
            if (_lastTrapEvent != null && evt.timestamp - _lastTrapEvent.timestamp <= chainReactionWindow)
            {
                AddHighlight(new ReplayHighlightData
                {
                    timestamp = evt.timestamp,
                    duration = defaultHighlightDuration,
                    highlightType = ReplayEventType.TrapChain,
                    focusPoint = evt.worldPosition,
                    score = 0.8f + evt.intensity,
                    description = "Trap chain reaction"
                });
            }

            _lastTrapEvent = evt;
            _cameraEvents.Add(new ReplayCameraEvent
            {
                timestamp = evt.timestamp,
                behavior = CameraBehaviorType.ZoomToTrap,
                focusPoint = evt.worldPosition,
                blendDuration = 0.55f,
                zoom = 5.5f,
                slowMotion = true
            });
        }
    }

    public void AddHighlight(ReplayHighlightData highlight)
    {
        _highlights.Add(highlight);

        CameraBehaviorType behavior = highlight.highlightType switch
        {
            ReplayEventType.BotDeath => CameraBehaviorType.FocusOnDeath,
            ReplayEventType.GoalReached => CameraBehaviorType.GoalCelebrationShot,
            ReplayEventType.StressMultiDeath => CameraBehaviorType.WideDungeonView,
            ReplayEventType.TrapChain => CameraBehaviorType.ZoomToTrap,
            _ => CameraBehaviorType.FollowBot
        };

        _cameraEvents.Add(new ReplayCameraEvent
        {
            timestamp = highlight.timestamp,
            behavior = behavior,
            focusPoint = highlight.focusPoint,
            blendDuration = 0.7f,
            zoom = 6f,
            slowMotion = highlight.highlightType == ReplayEventType.BotDeath || highlight.highlightType == ReplayEventType.TrapChain
        });
    }

    public ReplayTimelineData Build()
    {
        ReplayTimelineData data = new()
        {
            events = new List<ReplayEventData>(_events),
            highlights = new List<ReplayHighlightData>(_highlights),
            cameraEvents = new List<ReplayCameraEvent>(_cameraEvents)
        };

        data.events.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
        data.highlights.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
        data.cameraEvents.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
        return data;
    }

    public int FindNearestHighlightIndex(float time)
    {
        if (_highlights.Count == 0)
        {
            return -1;
        }

        int bestIndex = 0;
        float bestDistance = Mathf.Abs(_highlights[0].timestamp - time);
        for (int i = 1; i < _highlights.Count; i++)
        {
            float d = Mathf.Abs(_highlights[i].timestamp - time);
            if (d < bestDistance)
            {
                bestDistance = d;
                bestIndex = i;
            }
        }

        return bestIndex;
    }
}
