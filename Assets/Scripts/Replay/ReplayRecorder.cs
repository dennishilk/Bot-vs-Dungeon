using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayRecorder : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private StressTestManager stressTestManager;
    [SerializeField] private ReplayTimeline replayTimeline;
    [SerializeField] private HighlightDetector highlightDetector;
    [SerializeField] private float recordFrameInterval = 0.1f;
    [SerializeField] private int maxReplayHistoryCount = 30;
    [SerializeField] private float nearDeathThreshold = 20f;

    private readonly List<RunReplayData> _runHistory = new();
    private readonly List<CertificationReplayData> _certificationHistory = new();

    private Coroutine _recordingRoutine;
    private RunReplayData _activeRun;
    private CertificationReplayData _activeCertification;
    private BotAgent _trackedBot;
    private BotHealth _trackedHealth;

    public IReadOnlyList<RunReplayData> RunHistory => _runHistory;
    public IReadOnlyList<CertificationReplayData> CertificationHistory => _certificationHistory;

    private void Awake()
    {
        if (simulationManager != null)
        {
            simulationManager.OnBotSpawned += HandleBotSpawned;
            simulationManager.OnRunFinished += HandleRunFinished;
        }

        ReplayEventStream.OnReplayEvent += HandleReplayEvent;
    }

    private void OnDestroy()
    {
        if (simulationManager != null)
        {
            simulationManager.OnBotSpawned -= HandleBotSpawned;
            simulationManager.OnRunFinished -= HandleRunFinished;
        }

        ReplayEventStream.OnReplayEvent -= HandleReplayEvent;
        UnsubscribeHealth();
    }

    public void BeginCertificationSession(string sessionName)
    {
        _activeCertification = new CertificationReplayData
        {
            sessionName = string.IsNullOrWhiteSpace(sessionName) ? "Certification Session" : sessionName,
            createdUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public void EndCertificationSession()
    {
        if (_activeCertification == null)
        {
            return;
        }

        if (_activeCertification.runReplays.Count > 0)
        {
            _certificationHistory.Add(_activeCertification);
        }

        _activeCertification = null;
    }

    private void HandleBotSpawned(BotAgent bot)
    {
        _trackedBot = bot;
        _activeRun = new RunReplayData
        {
            personality = bot.GetPersonality()
        };

        replayTimeline?.Clear();
        ReplayEventStream.Emit(ReplayEventType.BotSpawn, bot.transform.position, bot.name, 1f, "Bot spawned");

        _trackedHealth = bot.GetComponent<BotHealth>();
        if (_trackedHealth != null)
        {
            _trackedHealth.OnBotDamaged += HandleBotDamaged;
        }

        if (_recordingRoutine != null)
        {
            StopCoroutine(_recordingRoutine);
        }

        _recordingRoutine = StartCoroutine(RecordRoutine());
    }

    private IEnumerator RecordRoutine()
    {
        WaitForSeconds wait = new(recordFrameInterval);
        while (_trackedBot != null && simulationManager != null && simulationManager.IsSimulationRunning)
        {
            CaptureFrame();
            yield return wait;
        }

        CaptureFrame();
        _recordingRoutine = null;
    }

    private void CaptureFrame()
    {
        if (_trackedBot == null || _activeRun == null)
        {
            return;
        }

        BotHealth health = _trackedBot.GetComponent<BotHealth>();
        float now = simulationManager != null ? simulationManager.SimulationTime : Time.time;
        Vector3 position = _trackedBot.transform.position;
        float hp = health != null ? health.CurrentHp : 0f;

        _activeRun.frames.Add(new ReplayFrameData
        {
            timestamp = now,
            botPosition = position,
            botHP = hp,
            currentState = _trackedBot.CurrentState
        });

        _activeRun.cameraFocusPoints.Add(new ReplayCameraFocusPoint
        {
            timestamp = now,
            worldPosition = position,
            weight = Mathf.Clamp01(1f - (hp / 100f)),
            suggestedBehavior = CameraBehaviorType.FollowBot
        });
    }

    private void HandleBotDamaged(float _, float currentHp)
    {
        if (_trackedBot == null)
        {
            return;
        }

        if (currentHp <= nearDeathThreshold)
        {
            ReplayEventStream.Emit(
                ReplayEventType.BotNearDeath,
                _trackedBot.transform.position,
                _trackedBot.name,
                nearDeathThreshold,
                "Bot near death");
        }
    }

    private void HandleReplayEvent(ReplayEventData evt)
    {
        if (_activeRun == null)
        {
            return;
        }

        ReplayEventData adjusted = new()
        {
            timestamp = simulationManager != null ? simulationManager.SimulationTime : evt.timestamp,
            eventType = evt.eventType,
            worldPosition = evt.worldPosition,
            sourceId = evt.sourceId,
            intensity = evt.intensity,
            details = evt.details
        };

        _activeRun.events.Add(adjusted);
        replayTimeline?.AddEvent(adjusted);
        highlightDetector?.AnalyzeEvent(adjusted);
    }

    private void HandleRunFinished(RunResult result)
    {
        if (_activeRun == null)
        {
            return;
        }

        _activeRun.survived = result.survived;
        _activeRun.causeOfDeath = result.causeOfDeath;
        _activeRun.completionTime = result.completionTime;
        _activeRun.deathPosition = new Vector3(result.deathPosition.x, 0.6f, result.deathPosition.y);

        if (result.survived)
        {
            ReplayEventStream.Emit(ReplayEventType.GoalReached, _activeRun.deathPosition, "Goal", 1f, "Goal reached");
            if (result.remainingHP <= nearDeathThreshold)
            {
                ReplayEventStream.Emit(ReplayEventType.RareSurvival, _activeRun.deathPosition, "Goal", result.remainingHP, "Survived with very low HP");
            }
        }
        else
        {
            ReplayEventStream.Emit(ReplayEventType.BotDeath, _activeRun.deathPosition, result.causeOfDeath, 1.2f, result.causeOfDeath);
        }

        _activeRun.timeline = replayTimeline != null ? replayTimeline.Build() : new ReplayTimelineData();

        _runHistory.Insert(0, _activeRun);
        if (_runHistory.Count > maxReplayHistoryCount)
        {
            _runHistory.RemoveAt(_runHistory.Count - 1);
        }

        if (_activeCertification != null)
        {
            _activeCertification.runReplays.Add(_activeRun);
        }

        UnsubscribeHealth();
        _activeRun = null;
        _trackedBot = null;
    }

    private void UnsubscribeHealth()
    {
        if (_trackedHealth != null)
        {
            _trackedHealth.OnBotDamaged -= HandleBotDamaged;
            _trackedHealth = null;
        }
    }
}
