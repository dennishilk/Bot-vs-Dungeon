using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayPanelController : MonoBehaviour
{
    [SerializeField] private ReplayViewer replayViewer;
    [SerializeField] private HighlightReplayPlayer highlightReplayPlayer;
    [SerializeField] private DevlogCaptureController devlogCaptureController;
    [SerializeField] private Toggle devlogToggle;
    [SerializeField] private TMP_Dropdown replayListDropdown;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_Text speedLabel;
    [SerializeField] private GameObject panelRoot;

    private readonly List<RunReplayData> _cachedReplays = new();

    private void OnEnable()
    {
        RefreshReplayList();
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveListener(OnSpeedChanged);
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(speedSlider.value);
        }

        if (devlogToggle != null)
        {
            devlogToggle.onValueChanged.RemoveListener(OnDevlogToggleChanged);
            devlogToggle.onValueChanged.AddListener(OnDevlogToggleChanged);
        }
    }

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    public void RefreshReplayList()
    {
        _cachedReplays.Clear();
        IReadOnlyList<RunReplayData> history = replayViewer.GetRunHistory();
        foreach (RunReplayData run in history)
        {
            _cachedReplays.Add(run);
        }

        if (replayListDropdown == null)
        {
            return;
        }

        replayListDropdown.ClearOptions();
        List<string> options = new();
        foreach (RunReplayData run in _cachedReplays)
        {
            options.Add(run.ToSummaryLine());
        }

        if (options.Count == 0)
        {
            options.Add("No replays available");
        }

        replayListDropdown.AddOptions(options);
        replayListDropdown.value = 0;
        UpdateSelectedReplayDetails();
    }

    public void SelectReplayClicked()
    {
        if (_cachedReplays.Count == 0)
        {
            SetStatus("No replay selected.", false);
            return;
        }

        bool success = replayViewer.SelectReplay(Mathf.Clamp(replayListDropdown.value, 0, _cachedReplays.Count - 1), out string message);
        SetStatus(success ? "Replay loaded." : message, success);
        UpdateSelectedReplayDetails();
    }

    public void PlayClicked()
    {
        replayViewer.Play();
        SetStatus("Replay playing.", true);
    }

    public void PauseClicked()
    {
        replayViewer.Pause();
        SetStatus("Replay paused.", true);
    }

    public void StopClicked()
    {
        replayViewer.StopReplay();
        SetStatus("Replay stopped.", true);
    }

    public void StepForwardClicked()
    {
        replayViewer.StepForward();
        SetStatus("Replay stepped forward.", true);
    }

    public void RewindClicked()
    {
        replayViewer.RewindStep();
        SetStatus("Replay rewound.", true);
    }

    public void JumpToHighlightClicked()
    {
        replayViewer.JumpToHighlight(1);
        SetStatus("Jumped to highlight.", true);
    }

    public void PlayHighlightsClicked()
    {
        highlightReplayPlayer?.PlayHighlights();
        SetStatus("Highlight replay started.", true);
    }

    public void CinematicReplayClicked()
    {
        replayViewer.PlayCinematicIntro();
        replayViewer.Play();
        SetStatus("Cinematic replay started.", true);
    }

    private void UpdateSelectedReplayDetails()
    {
        if (detailsText == null)
        {
            return;
        }

        if (_cachedReplays.Count == 0)
        {
            detailsText.text = "No run data.";
            return;
        }

        RunReplayData run = _cachedReplays[Mathf.Clamp(replayListDropdown.value, 0, _cachedReplays.Count - 1)];
        detailsText.text =
            $"Personality: {run.personality}\n" +
            $"Result: {(run.survived ? "Survived" : "Died")}\n" +
            $"Cause: {run.causeOfDeath}\n" +
            $"Replay Time: {run.completionTime:0.00}s\n" +
            $"Frames: {run.frames.Count}\n" +
            $"Events: {run.events.Count}\n" +
            $"Highlights: {(run.timeline != null ? run.timeline.highlights.Count : 0)}";
    }

    private void OnSpeedChanged(float value)
    {
        replayViewer.SetReplaySpeed(value);
        if (speedLabel != null)
        {
            speedLabel.text = $"Speed: {value:0.0}x";
        }
    }

    private void OnDevlogToggleChanged(bool enabled)
    {
        if (enabled)
        {
            devlogCaptureController?.StartDevlogCapture();
            SetStatus("Devlog capture started.", true);
        }
    }

    private void SetStatus(string message, bool success)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = success ? new Color(0.5f, 0.95f, 0.6f) : new Color(0.95f, 0.45f, 0.45f);
        }
    }
}
