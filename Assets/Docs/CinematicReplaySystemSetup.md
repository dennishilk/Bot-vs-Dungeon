# Cinematic Replay System Setup

## Architecture
- `ReplayRecorder` now records frame snapshots, structured replay events, camera focus points, and a generated `ReplayTimelineData` payload.
- `ReplayEventStream` is a lightweight event bus for trap activations, deaths, damage spikes, and goals.
- `ReplayTimeline` stores event ordering, creates chain-reaction highlights, and emits camera cues.
- `HighlightDetector` applies heuristic highlight detection for deaths, near-death saves, trap chains, goals, stress multi-death spikes, and rare survival moments.
- `ReplayViewer` supports full-run playback, rewind/step controls, jump-to-highlight, timeline-aware camera behavior switching, and cinematic intro.
- `HighlightReplayPlayer` plays only highlighted slices for clip-friendly output.
- `ReplayCameraController` keeps replay camera movement smooth, readable, and separate from gameplay camera.
- `DevlogCaptureController` automates simulation -> replay selection -> cinematic intro -> highlight playback loop.

## Inspector Configuration

### ReplayRecorder
- `Record Frame Interval`: 0.08 - 0.12 for smooth capture.
- `Near Death Threshold`: tune around 15 - 25 HP.
- Assign `ReplayTimeline` + `HighlightDetector` references.

### ReplayTimeline
- `Chain Reaction Window`: 1.2 - 1.8 seconds.
- `Default Highlight Duration`: 2.0 - 2.8 seconds.

### HighlightDetector
- `Sensitivity`: 0.8 (strict) to 1.2 (more highlights).
- `Stress Death Window`: 1.5 - 2.5 seconds.
- `Stress Deaths For Highlight`: 3+ to avoid noise.

### ReplayCameraController
- `Move Speed`: 3.5 - 5.5 for soft tracking.
- `Rotation Speed`: 4 - 6 for stable look-at.
- `Slow Motion Factor`: 0.25 - 0.45.
- `Default Zoom`: 58 - 65 FOV.
- `Highlight Zoom`: 38 - 48 FOV.

### DevlogCaptureController
- `Delay Before Replay`: 0.5 - 1.0 sec.
- `Devlog Replay Length`: 15 - 30 sec.
- `Auto Start On Enable`: optional for one-button capture workflow.

## Replay Viewer UI Additions
- Hook buttons to:
  - `ReplayPanelController.PlayHighlightsClicked`
  - `ReplayPanelController.CinematicReplayClicked`
  - `ReplayPanelController.JumpToHighlightClicked`
  - `ReplayPanelController.RewindClicked`
- Hook a toggle to `devlogToggle` for Devlog Capture Mode.
- Details label now shows event and highlight counts for timeline visibility.

## Notes
- No gameplay systems are modified; this is playback/capture instrumentation only.
- Export remains screen-capture friendly (no external encoding integration).
