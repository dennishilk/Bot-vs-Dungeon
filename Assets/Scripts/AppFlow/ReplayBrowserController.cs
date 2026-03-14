using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    public enum ReplayFilterMode
    {
        All,
        Survived,
        Failed
    }

    public class ReplayBrowserController : MonoBehaviour
    {
        [SerializeField] private ReplayViewer replayViewer;
        [SerializeField] private ReplayRecorder replayRecorder;
        [SerializeField] private HighlightReplayPlayer highlightReplayPlayer;
        [SerializeField] private TMP_Dropdown replayDropdown;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TMP_Text detailsText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject panelRoot;

        private readonly List<RunReplayData> _cached = new();

        private void OnEnable()
        {
            Refresh();
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }

            if (visible)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            _cached.Clear();
            IReadOnlyList<RunReplayData> history = replayViewer != null ? replayViewer.GetRunHistory() : Array.Empty<RunReplayData>();
            ReplayFilterMode filter = filterDropdown != null ? (ReplayFilterMode)Mathf.Clamp(filterDropdown.value, 0, 2) : ReplayFilterMode.All;

            foreach (RunReplayData replay in history)
            {
                if (filter == ReplayFilterMode.Survived && !replay.survived) continue;
                if (filter == ReplayFilterMode.Failed && replay.survived) continue;
                _cached.Add(replay);
            }

            if (replayDropdown != null)
            {
                replayDropdown.ClearOptions();
                List<string> options = new();
                foreach (RunReplayData replay in _cached)
                {
                    options.Add(replay.ToSummaryLine());
                }

                if (options.Count == 0)
                {
                    options.Add("No replays found");
                }

                replayDropdown.AddOptions(options);
                replayDropdown.value = 0;
            }

            UpdateDetails();
        }

        public void PlayReplayClicked()
        {
            if (!SelectReplay()) return;
            replayViewer?.Play();
            SetStatus("Replay playing.", true);
        }

        public void PlayHighlightsClicked()
        {
            if (!SelectReplay()) return;
            highlightReplayPlayer?.PlayHighlights();
            SetStatus("Highlights started.", true);
        }

        public void DeleteReplayClicked()
        {
            if (replayRecorder == null)
            {
                SetStatus("Replay recorder missing.", false);
                return;
            }

            RunReplayData selected = _cached.Count > 0 ? _cached[Mathf.Clamp(replayDropdown.value, 0, _cached.Count - 1)] : null;
            bool ok = replayRecorder.RemoveReplay(selected, out string message);
            SetStatus(message, ok);
            Refresh();
        }

        public void ReturnToMenuClicked()
        {
            AppStateManager.Instance?.ReturnToMenu();
        }

        public void UpdateDetails()
        {
            if (detailsText == null)
            {
                return;
            }

            if (_cached.Count == 0)
            {
                detailsText.text = "No replay selected.";
                return;
            }

            RunReplayData replay = _cached[Mathf.Clamp(replayDropdown.value, 0, _cached.Count - 1)];
            detailsText.text =
                $"Mode: {(replay.survived ? "Successful run" : "Failed run")}\n" +
                $"Personality: {replay.personality}\n" +
                $"Date: {DateTime.Now:yyyy-MM-dd}\n" +
                $"Completion: {replay.completionTime:0.00}s\n" +
                $"Highlights: {(replay.timeline != null ? replay.timeline.highlights.Count : 0)}";
        }

        private bool SelectReplay()
        {
            if (_cached.Count == 0)
            {
                SetStatus("No replay selected.", false);
                return false;
            }

            bool ok = replayViewer != null && replayViewer.SelectReplay(Mathf.Clamp(replayDropdown.value, 0, _cached.Count - 1), out string message);
            SetStatus(ok ? "Replay selected." : message, ok);
            return ok;
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
}
