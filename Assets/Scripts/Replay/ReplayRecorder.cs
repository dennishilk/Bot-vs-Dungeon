using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayRecorder : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private float recordFrameInterval = 0.1f;
    [SerializeField] private int maxReplayHistoryCount = 30;

    private readonly List<RunReplayData> _runHistory = new();
    private readonly List<CertificationReplayData> _certificationHistory = new();

    private Coroutine _recordingRoutine;
    private RunReplayData _activeRun;
    private CertificationReplayData _activeCertification;
    private BotAgent _trackedBot;

    public IReadOnlyList<RunReplayData> RunHistory => _runHistory;
    public IReadOnlyList<CertificationReplayData> CertificationHistory => _certificationHistory;

    private void Awake()
    {
        if (simulationManager != null)
        {
            simulationManager.OnBotSpawned += HandleBotSpawned;
            simulationManager.OnRunFinished += HandleRunFinished;
        }
    }

    private void OnDestroy()
    {
        if (simulationManager != null)
        {
            simulationManager.OnBotSpawned -= HandleBotSpawned;
            simulationManager.OnRunFinished -= HandleRunFinished;
        }
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
        _activeRun.frames.Add(new ReplayFrameData
        {
            timestamp = simulationManager != null ? simulationManager.SimulationTime : Time.time,
            botPosition = _trackedBot.transform.position,
            botHP = health != null ? health.CurrentHp : 0f,
            currentState = _trackedBot.CurrentState
        });
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

        _runHistory.Insert(0, _activeRun);
        if (_runHistory.Count > maxReplayHistoryCount)
        {
            _runHistory.RemoveAt(_runHistory.Count - 1);
        }

        if (_activeCertification != null)
        {
            _activeCertification.runReplays.Add(_activeRun);
        }

        _activeRun = null;
        _trackedBot = null;
    }
}
