# Presentation + Progression Setup (Unity 6)

This setup adds polish systems without changing the core dungeon gameplay loop.

## 1) New managers (recommended scene wiring)

Create/assign these objects in your main scene:

- `ProgressionManager` (empty GameObject)
  - `saveFileName`: `progression_save.json`
  - Optional: enable `verboseLogging` while testing.
- `AchievementManager`
  - Assign `ArenaManager`, `CertificationManager`, `ProgressionManager`, `AchievementPopup`.
- `SceneTransitionController` (persisted singleton)
  - Assign `transitionCanvasGroup` from `SceneTransitionPanel`.
- `ResultScreenController`
  - Assign all text fields + banner + canvas group from `ResultScreenPanel`.
- `LevelCompleteScreen`
  - Assign texts + wire button events (`NextLevel`, `Retry`, `Return to Menu`).

## 2) Updated existing component wiring

### CertificationManager
- Assign `resultScreenController`.
- Set `dungeonName` for the current challenge/sandbox layout.
- Tune `resultScreenDelay` (e.g., `0.25`).

### LevelSelectController
- Assign `progressionManager`.
- Assign `certificationManager`.
- Assign `levelCompleteScreen`.

### MainMenuController
- Assign `sceneTransitionController` (or rely on singleton instance).
- Use button bindings:
  - `StartGame` → Play
  - `OpenChallengeLevels` → Challenge Levels
  - `OpenSandboxMode` → Sandbox Mode
  - `OpenReplayViewer` → Replay Viewer
  - `OpenSettingsPlaceholder` → Settings
  - `QuitGame` → Exit

## 3) UI hierarchy additions

Under your main Canvas, add:

- `SceneTransitionPanel`
  - Full-screen Image (black)
  - `CanvasGroup` (alpha 0 by default)
  - Referenced by `SceneTransitionController`
- `ResultScreenPanel`
  - Banner image + text block area
  - Fields for dungeon name, bot outcomes, stats, rating, verdict, flavor text
  - `CanvasGroup` for fade-in
- `AchievementPopup`
  - Anchored panel in top-left or top-right
  - `RectTransform`, `CanvasGroup`, title text, description text, icon image
- `LevelCompletePanel`
  - Level name, objective complete text, dungeon rating, stars text
  - Buttons: Next Level / Retry / Return to Menu

## 4) Inspector tuning guidance

- `AchievementPopup`
  - `slideDuration`: ~`0.28`
  - `visibleDuration`: `2.0 - 2.5`
  - `fadeOutDuration`: `0.3 - 0.5`
- `SceneTransitionController`
  - `transitionSpeed`: `2.0 - 3.0`
- `ResultScreenController`
  - `fadeInDuration`: `0.3 - 0.45`
- `DeathMarkerSpawner`
  - `pulseDuration`: `0.35 - 0.5`
  - `pulseScaleMultiplier`: `1.15 - 1.3`

## 5) What is saved locally

`ProgressionManager` stores a JSON save in `Application.persistentDataPath`:

- unlocked challenge levels
- completed challenge levels
- unlocked achievements (`achievementID`, title, description, unlocked flag, unlock date)

Use `Reset Progression Save` context menu on `ProgressionManager` for QA resets.
