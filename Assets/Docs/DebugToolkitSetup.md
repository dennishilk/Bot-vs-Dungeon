# Debug Toolkit Setup (Unity 6 + C#)

This setup adds a developer-only AI testing lab to **Bot vs Dungeon** without changing gameplay systems.

## 1) Folder Structure

```text
Assets/
  Scripts/
    Debug/
      DebugOverlay.cs
      SimulationControlPanel.cs
      EventLogger.cs
      DebugPathVisualizer.cs
      TestArenaLoader.cs
    Data/
      BotPersonality.cs
```

## 2) Required C# Scripts

Minimum required scripts for the HUD loop:
- `DebugOverlay.cs`
- `SimulationControlPanel.cs`

Additional scripts included in this toolkit:
- `EventLogger.cs`
- `DebugPathVisualizer.cs`
- `TestArenaLoader.cs`

## 3) DebugCanvas UI Hierarchy

Create a new Canvas named `DebugCanvas` (Screen Space - Overlay) with this hierarchy:

```text
DebugCanvas
  DebugPanel
    BotInfoPanel
      BotHPText
      BotStateText
      TargetTileText
      PathLengthText
      CurrentTileText
      LastDecisionText
      DangerScoreText
      SimulationTimeText
    SimulationControls
      SpawnBotButton
      RunSimulationButton
      ResetRunButton
      ClearDungeonButton
      PauseSimulationButton
      ResumeSimulationButton
      SlowMotionToggle
      StepSimulationButton
      PersonalityDropdown
    DebugToggles
      ShowBotPathToggle
      ShowDangerMapToggle
      ShowTileGridToggle
      ShowTrapRangeToggle
    EventLogPanel
      Scroll View
        Viewport
          Content
            EventLogText
      ClearLogButton
```

Keep this panel compact and anchored to a corner (top-right recommended).

## 4) Example Gizmo Visualization

`DebugPathVisualizer.OnDrawGizmos()` draws:
- current bot path (`green`)
- danger tiles (`red`)
- goal tile (`bright green`)
- current target tile (`yellow`)

Attach `DebugPathVisualizer` to the same object as `ArenaManager` + `SimulationManager` references.

## 5) Example Event Logger Behavior

`EventLogger.Log(...)` timestamps each event and stores a capped queue.

Events now include:
- bot spawned
- path calculated
- bot entered trap zone
- trap activated
- bot took damage
- bot died
- bot reached goal

Use `maxEventCount` on `EventLogger` to cap memory/UI noise.

## 6) Inspector Setup Instructions

### Simulation tuning exposed
- `BotAgent`
  - `Move Speed`
  - `Careful/ Balanced/ Reckless Danger Multiplier`
- `BotHealth`
  - `Max Hp`
- `TrapBase` derived scripts
  - `damage`
- `BombTrap`
  - `triggerRadius`
- `ArcherTrap`
  - `range`
- `BotPathfinder`
  - `dangerCostMultiplier`

### Hook references
- `DebugOverlay`
  - assign `SimulationManager`
  - assign the 8 TMP text fields
- `SimulationControlPanel`
  - assign `SimulationManager`, `EventLogger`
  - assign all buttons/toggles/dropdown
- `EventLogger`
  - assign `EventLogText`
  - assign `ScrollRect`

## 7) TestArena Scene Setup Instructions

Create `Assets/Scenes/TestArena.unity`:
- Start tile on one side (example `(0,0)`)
- Goal tile opposite side (example `(10,0)`)
- open floor lane between them
- empty side space for trap placement experiments

Add optional layout buttons wired to `TestArenaLoader`:
- `SetupSawTest()`
- `SetupBombTest()`
- `SetupArcherLineTest()`
- `SetupPitMazeTest()`

Add `TestArena` to **Build Settings** so `LoadTestArenaScene()` can switch scenes quickly.
