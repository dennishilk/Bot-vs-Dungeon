using System;
using UnityEngine;

public static class ReplayEventStream
{
    public static event Action<ReplayEventData> OnReplayEvent;

    public static void Emit(
        ReplayEventType eventType,
        Vector3 worldPosition,
        string sourceId,
        float intensity = 1f,
        string details = "")
    {
        ReplayEventData evt = new()
        {
            timestamp = Time.time,
            eventType = eventType,
            worldPosition = worldPosition,
            sourceId = sourceId,
            intensity = intensity,
            details = details
        };

        OnReplayEvent?.Invoke(evt);
    }
}
