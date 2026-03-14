using System.Collections;
using UnityEngine;

public class HighlightReplayPlayer : MonoBehaviour
{
    [SerializeField] private ReplayViewer replayViewer;
    [SerializeField] private float preRollSeconds = 1.1f;
    [SerializeField] private float postRollSeconds = 0.8f;

    private Coroutine _highlightRoutine;

    public void PlayHighlights()
    {
        if (replayViewer == null || replayViewer.ActiveReplay == null)
        {
            return;
        }

        StopHighlights();
        _highlightRoutine = StartCoroutine(PlayHighlightsRoutine());
    }

    public void StopHighlights()
    {
        if (_highlightRoutine == null)
        {
            return;
        }

        StopCoroutine(_highlightRoutine);
        _highlightRoutine = null;
    }

    private IEnumerator PlayHighlightsRoutine()
    {
        RunReplayData replay = replayViewer.ActiveReplay;
        if (replay == null || replay.timeline == null || replay.timeline.highlights.Count == 0)
        {
            yield break;
        }

        foreach (ReplayHighlightData highlight in replay.timeline.highlights)
        {
            float start = Mathf.Max(0f, highlight.timestamp - preRollSeconds);
            float end = highlight.timestamp + highlight.duration + postRollSeconds;

            replayViewer.JumpToTime(start);
            replayViewer.Play();

            while (replayViewer.ActiveReplay != null && replayViewer.CurrentTime < end)
            {
                yield return null;
            }

            replayViewer.Pause();
            yield return new WaitForSecondsRealtime(0.12f);
        }

        _highlightRoutine = null;
    }
}
