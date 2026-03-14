# Bot vs Dungeon - Standalone Desktop App Flow Integration

## 1) Standalone App Flow Architecture

Top-level application states are managed by `AppStateManager`:

- Boot
- Main Menu
- Campaign
- Sandbox
- Daily Challenge
- Director Mode
- Evolution Lab
- Replay Viewer
- Settings
- Profile Selection
- Credits
- Exit Confirmation
- (optional operational states) Loading / Result / Level Complete / Promotion

The game is now routed as an app-like flow where all major systems are reachable via UI menus.

## 2) Central App State Management System

### `AppStateManager.cs`

Responsibilities:
- global current + previous app state tracking
- boot-to-menu transition timing
- transition event dispatch (`OnStateChanged`)
- quit request flow with optional confirmation
- save active profile metadata during transitions

Inspector options:
- `defaultState`
- `autoEnterMainMenuFromBoot`
- `bootDurationSeconds`
- `enableQuitConfirmation`
- `autoSaveOnStateChange`

## 3) Mode Routing / Scene Routing Plan

### `SceneRouter.cs`

Supports two routing styles:

1. **Scene routing** (optional): map state -> scene name and use async scene loading.
2. **Panel routing**: map state -> panel groups and toggle active panels.

This allows either:
- multi-scene desktop flow (Boot/MainMenu/Game/Replay), or
- single-scene UI panel routing.

Loading overlay support:
- `loadingOverlayPanel`
- `loadingOverlayMinimumTime`

## 4) Main Menu Integration

### `MainMenuController.cs`

Main menu actions now set explicit app states and keep existing systems connected.

Required menu entries supported:
- Campaign
- Sandbox
- Daily Challenge
- Director Mode
- Evolution Lab
- Replay Viewer
- Settings
- Profile
- Credits
- Exit

Optional:
- Continue last session can call `StartGame()` or directly switch to Sandbox/Campaign.

## 5) Profile Selection System

### `PlayerProfileManager.cs`
Stores profile metadata in `Application.persistentDataPath/profiles.json`.

Profile operations:
- create
- select
- delete
- rename

### `ProfileSelectionController.cs`
UI layer for profile management with status messaging and menu return flow.

Profile data intentionally represents app-facing progression ownership:
- campaign progression linkage
- achievements/settings ownership
- save/replay history ownership (extendable by profile ID tagging)

## 6) Settings Integration

### `SettingsMenuController.cs`
Player-facing settings sections:
- Audio (master/music/ambient/sfx)
- Display (fullscreen, resolution, vSync)
- Gameplay (default replay speed)
- Accessibility placeholder path (UI scale provided)

Storage:
- Unity `PlayerPrefs`
- immediate save (`saveOnEachChange`) or explicit apply

## 7) Save / Load UX Integration

### `SaveLoadMenuController.cs`
UI flow includes:
- save / overwrite
- load
- delete
- share code export/import
- player-facing status + error popup handling

Save behavior uses existing `DungeonSaveManager` and Unity persistent path storage.

## 8) Replay Browser Integration

### `ReplayBrowserController.cs`
Replay viewer is promoted to first-class mode:
- list replays
- filter (all/survived/failed)
- play replay
- play highlights
- delete replay
- return to menu

### `ReplayRecorder.cs`
Added replay management APIs:
- `RemoveReplayAt(...)`
- `ClearReplayHistory()`

## 9) Result Flow Integration

### `ResultFlowController.cs`
Post-run flow actions:
- Watch Replay
- Watch Highlights
- Edit Dungeon
- Save Dungeon
- Run Again
- Return to Menu

`GameFlowController` coordinates mode action calls and result-state transition.

## 10) Error Popup System

### `ErrorPopupController.cs`
User-facing, non-technical error messaging for:
- invalid/missing save
- share code parse/import failures
- replay operations
- other UI flow issues

## 11) UI Hierarchy Instructions

Recommended root hierarchy:

- `AppRoot`
  - `MainMenuPanel`
  - `CampaignPanel`
  - `SandboxPanel`
  - `DailyChallengePanel`
  - `DirectorModePanel`
  - `EvolutionLabPanel`
  - `ReplayBrowserPanel`
  - `ProfileSelectionPanel`
  - `SettingsPanel`
  - `CreditsPanel`
  - `ResultPanel`
  - `ErrorPopupPanel`
  - `ConfirmDialogPanel`
  - `LoadingOverlayPanel`

Route them through `SceneRouter` panel lists by app state.

## 12) Inspector Setup Instructions

### App bootstrap object (`AppSystems`)
Add components:
- `AppStateManager`
- `SceneRouter`
- `PlayerProfileManager`

### Gameplay flow object (`GameFlow`)
Add components:
- `GameFlowController`
- `ResultFlowController`
- `SaveLoadMenuController`
- `ReplayBrowserController`
- `SettingsMenuController`
- `ProfileSelectionController`
- `ErrorPopupController`

Wire references to existing systems:
- `SimulationManager`
- `CertificationManager`
- `StressTestManager`
- `ReplayViewer`
- `ReplayRecorder`
- `HighlightReplayPlayer`
- `DungeonSaveManager`
- `ShareCodeManager`

## 13) Linux-friendly Persistence / Runtime Guidelines

Implemented assumptions:
- all app data under `Application.persistentDataPath`
- no Windows-only path assumptions
- no CLI dependency for normal gameplay
- no manual JSON editing required for core loops
- clean in-app quit behavior via `AppStateManager`

For Linux desktop builds:
- keep save/profile/replay storage in Unity-managed persistent directories
- avoid shell command dependencies
- ensure all mode entry points exist in UI

## 14) Complete User Flow (Launch -> Play -> Exit)

1. Boot splash/title (`Boot`)
2. Main menu (`MainMenu`)
3. Choose mode (Campaign/Sandbox/etc.)
4. Build/test dungeon via mode UI
5. View result/report panel
6. Watch replay/highlights or iterate
7. Save/load/share from save UI
8. Adjust settings and profile in dedicated menus
9. Exit through in-game confirmation flow

This provides a full standalone desktop-game user journey suitable for Linux and NixOS-based development/testing workflows.
