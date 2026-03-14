using System.Collections;
using UnityEngine;

public class DevlogCaptureController : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private ReplayViewer replayViewer;
    [SerializeField] private HighlightReplayPlayer highlightReplayPlayer;
    [SerializeField] private bool autoStartOnEnable;
    [SerializeField] private float delayBeforeReplay = 0.75f;
    [SerializeField] private float devlogReplayLength = 20f;

    private Coroutine _captureRoutine;

    private void OnEnable()
    {
        if (autoStartOnEnable)
        {
            StartDevlogCapture();
        }
    }

    public void StartDevlogCapture()
    {
        if (_captureRoutine != null)
        {
            StopCoroutine(_captureRoutine);
        }

        _captureRoutine = StartCoroutine(DevlogRoutine());
    }

    private IEnumerator DevlogRoutine()
    {
        if (simulationManager != null && !simulationManager.IsSimulationRunning)
        {
            simulationManager.StartSimulation();
        }

        while (simulationManager != null && simulationManager.IsSimulationRunning)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(delayBeforeReplay);

        if (replayViewer == null)
        {
            yield break;
        }

        if (!replayViewer.SelectReplay(0, out _))
        {
            yield break;
        }

        replayViewer.PlayCinematicIntro();
        highlightReplayPlayer?.PlayHighlights();

        float elapsed = 0f;
        while (elapsed < devlogReplayLength)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        replayViewer.Pause();
        _captureRoutine = null;
    }
}
