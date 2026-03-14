using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayViewer : MonoBehaviour
{
    [SerializeField] private ReplayRecorder replayRecorder;
    [SerializeField] private GameObject replayBotPrefab;
    [SerializeField] private Transform replayRoot;
    [SerializeField] private LineRenderer replayPathLine;
    [SerializeField] private GameObject deathMarkerPrefab;
    [SerializeField] private float defaultReplaySpeed = 1f;

    private GameObject _replayBotInstance;
    private GameObject _deathMarkerInstance;
    private Coroutine _playRoutine;
    private RunReplayData _activeReplay;
    private int _frameIndex;

    public float ReplaySpeed { get; private set; } = 1f;
    public bool IsPlaying => _playRoutine != null;
    public RunReplayData ActiveReplay => _activeReplay;
    public int CurrentFrameIndex => _frameIndex;

    private void Awake()
    {
        ReplaySpeed = Mathf.Max(0.1f, defaultReplaySpeed);
    }

    public IReadOnlyList<RunReplayData> GetRunHistory()
    {
        return replayRecorder != null ? replayRecorder.RunHistory : new RunReplayData[0];
    }

    public bool SelectReplay(int index, out string message)
    {
        message = string.Empty;
        IReadOnlyList<RunReplayData> runs = GetRunHistory();
        if (runs.Count == 0)
        {
            message = "No replay history available yet.";
            return false;
        }

        if (index < 0 || index >= runs.Count)
        {
            message = "Replay index out of range.";
            return false;
        }

        StopReplay();
        _activeReplay = runs[index];
        _frameIndex = 0;
        EnsureReplayObjects();
        ApplyFrame(0);
        DrawPath();
        ShowDeathMarkerIfNeeded();
        return true;
    }

    public void Play()
    {
        if (_activeReplay == null || _activeReplay.frames.Count == 0 || IsPlaying)
        {
            return;
        }

        _playRoutine = StartCoroutine(PlayRoutine());
    }

    public void Pause()
    {
        if (_playRoutine == null)
        {
            return;
        }

        StopCoroutine(_playRoutine);
        _playRoutine = null;
    }

    public void StopReplay()
    {
        Pause();
        _frameIndex = 0;
        if (_activeReplay != null && _activeReplay.frames.Count > 0)
        {
            ApplyFrame(0);
        }
    }

    public void StepForward()
    {
        if (_activeReplay == null || _activeReplay.frames.Count == 0)
        {
            return;
        }

        _frameIndex = Mathf.Min(_frameIndex + 1, _activeReplay.frames.Count - 1);
        ApplyFrame(_frameIndex);
    }

    public void SetReplaySpeed(float speed)
    {
        ReplaySpeed = Mathf.Max(0.1f, speed);
    }

    private IEnumerator PlayRoutine()
    {
        while (_activeReplay != null && _frameIndex < _activeReplay.frames.Count - 1)
        {
            int next = _frameIndex + 1;
            float dt = Mathf.Max(0.01f, _activeReplay.frames[next].timestamp - _activeReplay.frames[_frameIndex].timestamp);
            yield return new WaitForSecondsRealtime(dt / ReplaySpeed);
            _frameIndex = next;
            ApplyFrame(_frameIndex);
        }

        _playRoutine = null;
    }

    private void EnsureReplayObjects()
    {
        if (_replayBotInstance == null && replayBotPrefab != null)
        {
            _replayBotInstance = Instantiate(replayBotPrefab, Vector3.zero, Quaternion.identity, replayRoot);
            _replayBotInstance.name = "ReplayBot";
        }
    }

    private void ApplyFrame(int index)
    {
        if (_activeReplay == null || _activeReplay.frames.Count == 0)
        {
            return;
        }

        EnsureReplayObjects();
        ReplayFrameData frame = _activeReplay.frames[Mathf.Clamp(index, 0, _activeReplay.frames.Count - 1)];
        if (_replayBotInstance != null)
        {
            _replayBotInstance.transform.position = frame.botPosition;
        }
    }

    private void DrawPath()
    {
        if (replayPathLine == null || _activeReplay == null)
        {
            return;
        }

        replayPathLine.positionCount = _activeReplay.frames.Count;
        for (int i = 0; i < _activeReplay.frames.Count; i++)
        {
            replayPathLine.SetPosition(i, _activeReplay.frames[i].botPosition + new Vector3(0f, 0.05f, 0f));
        }
    }

    private void ShowDeathMarkerIfNeeded()
    {
        if (_deathMarkerInstance != null)
        {
            Destroy(_deathMarkerInstance);
            _deathMarkerInstance = null;
        }

        if (_activeReplay == null || _activeReplay.survived || deathMarkerPrefab == null)
        {
            return;
        }

        _deathMarkerInstance = Instantiate(deathMarkerPrefab, _activeReplay.deathPosition, Quaternion.identity, replayRoot);
    }
}
