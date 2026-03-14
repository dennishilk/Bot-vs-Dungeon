# Bot vs Dungeon UI Layer Setup (Unity 6)

This guide adds a compact, dark-fantasy UI presentation layer without changing the core gameplay loop.

---

## 1) UI Folder Structure

```text
Assets/
  Scripts/
    UI/
      UIController.cs
      MainMenuController.cs
      BuildToolbarController.cs
      BuildToolbarButton.cs
      BotStatusPanel.cs
      ResultBannerController.cs
  UI/
    Docs/
      UISetupGuide.md
    Prefabs/
    Styles/
```

---

## 2) Scene Hierarchy (Menu + HUD)

```text
Canvas (Screen Space - Overlay)
  MainMenuPanel
    BackdropImage
    TitleText                     (BOT VS DUNGEON)
    TaglineText                   (Build a dungeon. Launch a bot. Watch it fail.)
    StartButton
    SettingsButtonPlaceholder
    QuitButton
    VersionText

  HUDPanel
    TopBar
      ModeText
      SimulationStatusText
      ReturnToMenuButton

    BuildToolbar
      FloorButton
      WallButton
      PitButton
      SawButton
      BombButton
      ArcherButton
      StartButton
      GoalButton

    SidePanel
      SelectedItemText
      TrapDescriptionText
      BotStatusPanel
        BotHPText
        BotStateText

    BottomControls
      BuildModeButton
      SimulateButton
      ClearDungeonButton

    ResultBanner
      ResultBackground
      ResultText
```

> If you prefer scene loading, create `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Main.unity`. If not, keep both panels in one scene and switch visibility.

---

## 3) Required Buttons and Exact Functions

### Main Menu
- **StartButton** → `MainMenuController.StartGame()`
- **SettingsButtonPlaceholder** → `MainMenuController.OpenSettingsPlaceholder()`
- **QuitButton** → `MainMenuController.QuitGame()`

### Top-Level HUD Controls
- **BuildModeButton** → `UIController.SetMode(Build)` + your build-mode system event.
- **SimulateButton** → `UIController.SetMode(Simulation)` + simulation start event.
- **ClearDungeonButton** → clear map event + `UIController.ClearResult()`.
- **ReturnToMenuButton** → `UIController.ShowMainMenu()` (or load menu scene).

### Build Toolbar
Each build button uses `BuildToolbarButton` with `itemType` set to:
- Floor
- Wall
- Pit
- Saw
- Bomb
- Archer
- Start
- Goal

Each click should:
1. Highlight selected button.
2. Set selected item text.
3. Forward selection to your placement system.

Use `BuildToolbarController` to ensure one active highlight at a time.

---

## 4) Panel Visibility Rules

- **Startup:** MainMenuPanel = ON, HUDPanel = OFF, ResultBanner = OFF.
- **Start Game:** MainMenuPanel = OFF, HUDPanel = ON, Mode = Build.
- **Simulating:** HUD remains ON, SimulationStatus updates.
- **Result:** ResultBanner = ON with success/fail color + text.
- **Clear Dungeon:** ResultBanner = OFF, status set to neutral.
- **Return to Menu:** MainMenuPanel = ON, HUDPanel = OFF.

---

## 5) Inspector Setup Instructions

### Canvas
- Render Mode: **Screen Space - Overlay**
- Canvas Scaler: **Scale With Screen Size**
- Reference Resolution: **1920 x 1080**
- Match: **0.5**

### Anchors and Layout
- **TopBar:** stretch top, height ~86.
- **BuildToolbar:** left-center (or bottom-center), fixed width, `HorizontalLayoutGroup` or `VerticalLayoutGroup`.
- **SidePanel:** right-center, medium width (280–340).
- **BottomControls:** bottom-center.
- **ResultBanner:** top-center (or center), anchored so it stays visible over arena.

### Script Wiring
1. Create `UIRoot` object and attach **UIController**.
2. Assign panel references (`MainMenuPanel`, `HUDPanel`, `ResultBanner`).
3. Assign text refs (`ModeText`, `SimulationStatusText`, `SelectedItemText`, `TrapDescriptionText`).
4. Assign buttons (`BuildModeButton`, `SimulateButton`, `ClearDungeonButton`, `ReturnToMenuButton`).
5. Attach **ResultBannerController** to ResultBanner and assign text/background.
6. Attach **BotStatusPanel** to BotStatusPanel object and assign `BotHPText` + `BotStateText`.
7. Attach **MainMenuController** to MainMenuPanel and assign `UIController`.
8. Add **BuildToolbarController** to BuildToolbar and list all 8 `BuildToolbarButton` components.
9. On each toolbar button:
   - attach `BuildToolbarButton`
   - assign `UIController`, `BuildToolbarController`, label text, and background image
   - set `itemType`

### Gameplay Event Hooks
Use UnityEvents in `UIController` and `BuildToolbarButton`:
- `onBuildModeRequested` → BuildModeController method.
- `onSimulateRequested` → SimulationManager start method.
- `onClearDungeonRequested` → clear/reset method.
- `onBuildItemSelected` → placement system selection method.

---

## 6) Recommended UI Colors (Dark Fantasy Prototype)

| Role | Hex | Notes |
|---|---|---|
| Panel background | `#1E2128` | Charcoal base |
| Panel alt | `#252A33` | Layer separation |
| Border/accent metal | `#8A7760` | Iron/bronze feel |
| Primary text | `#E3DCCB` | Off-white parchment tone |
| Secondary text | `#B8B2A3` | Less prominent copy |
| Build mode accent | `#2B6B8F` | Cool cyan-blue |
| Warning accent | `#D28B3C` | amber/orange |
| Danger accent | `#A63A3A` | red trap/result fail |
| Success accent | `#4E8E52` | green result success |
| Button hover | `#343A46` | subtle lift |
| Selected tool | `#2D6E95` | clear active tool highlight |

Recommended alpha for panels: `0.85 - 0.93`.

---

## 7) Typography and Hierarchy

- Title: 58–72 pt, bold.
- Tagline: 24–30 pt.
- Section labels (`Mode`, `Status`): 24–28 pt.
- Button labels: 20–24 pt.
- Side-panel data (`HP`, `State`): 22–26 pt.
- Result banner: 52–64 pt, bold uppercase.

Use TMP default or other clean sans-serif for readability.

---

## 8) Optional Light Polish (No New Systems)

- Add button color transitions (normal/hover/pressed) via `Button.Colors`.
- Add mild `CanvasGroup` fade to result banner.
- Add subtle `Shadow` or `Outline` components to key labels.
- Keep motion gentle and short (`0.15 - 0.25s`).

---

## 9) Why This UI Supports Readability + Dungeon Mood

- **Readability first:** clear zoning (top status, build toolbar, side info, bottom controls) reduces search time during rapid build/sim loop.
- **Strong tool feedback:** selected tool highlighting and selected-item text make placement intent explicit.
- **Simulation clarity:** persistent status line + result banner prevent ambiguous outcomes.
- **Thematic consistency:** charcoal panels, muted metal accents, and parchment text create a dark-fantasy control-room feel without ornate clutter.
- **Prototype-safe scope:** all additions are presentational and event-driven, preserving the existing gameplay architecture.
