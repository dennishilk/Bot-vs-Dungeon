# Challenge Gameplay Systems Setup (Unity 6)

This guide wires the new challenge gameplay layers on top of the existing build/simulation loop.

## 1) New Systems Added

Attach these scripts in the main scene:

- `TrapBudgetManager` (trap budget rules + HUD updates)
- `LevelObjectiveManager` (objective tracking + post-certification evaluation)
- `LevelSelectController` (challenge level loading)
- `DeathHeatmapManager` (death count collection)
- `HeatmapVisualizer` (heat overlay render + toggle/reset)

## 2) Trap Budget HUD Panel

Create `TrapBudgetPanel` in the HUD:

- `HeaderText`: `Trap Budget`
- `ValueText`: `0 / 10`

Wire to `TrapBudgetManager`:

- `budgetHeaderText` -> `HeaderText`
- `budgetValueText` -> `ValueText`
- `uiController` -> `UIController` (optional if using combined HUD text)

Color behavior:

- `withinBudgetColor`: green (within budget)
- `overBudgetColor`: red (budget exceeded)

> Placement systems should call `TrapBudgetManager.CanPlaceTile(...)` before placing traps and walls.

## 3) Objective HUD Panel

Create `ObjectivePanel` in the HUD:

- `ObjectiveHeaderText`: `Level Objective`
- `ObjectiveDescriptionText`: current objective text
- `ObjectiveStatusText`: starts as `Objective Pending`

Wire to `LevelObjectiveManager`:

- `objectiveHeaderText`
- `objectiveDescriptionText`
- `objectiveStatusText`
- `certificationManager`
- `uiController` (optional)

After certification completes, status auto-updates to:

- `Objective Complete`
- `Objective Failed`

## 4) Level Select Panel

Create `LevelSelectPanel` with:

- `LevelNameText`
- `LevelBudgetText`
- `LevelObjectiveText`
- `LevelCompletionText`
- `StartLevelButton`

Wire to `LevelSelectController` and populate `challengeLevels`.

Each `DungeonLevel` includes:

- level name
- description
- trap budget
- objective
- allowed trap types
- initial tile layout list (`position + tileType`)

Example seed levels:

1. **First Trap** — Budget 3 — Objective: `RecklessMustFail`
2. **Archer Corridor** — Budget 6 — Objective: `SurviveAtLeastOne`
3. **Pit Maze** — Budget 8 — Objective: `CarefulMustSurvive`
4. **Bomb Chamber** — Budget 10 — Objective: `ReachDungeonRating` (`Dangerous`)

## 5) Death Heatmap + Controls

Create a lightweight translucent quad prefab and assign to `HeatmapVisualizer.heatmapTilePrefab`.

Heatmap controls:

- Toggle: call `HeatmapVisualizer.ToggleHeatmap()`
- Reset button: call `HeatmapVisualizer.ResetHeatmap()`

Suggested gradient:

- low = faint yellow
- medium = orange
- high = red

Set `overlayHeight` to a low value (`~0.03`) so path history gizmos remain visible above it.

## 6) Inspector Configuration Checklist

### TrapBudgetManager

- `maxBudget`: per level default
- `trapCostEntries`:
  - Pit = 1
  - Saw = 2
  - Bomb = 3
  - Archer = 4
  - Wall = 1

### LevelObjectiveManager

- Pending/success/failure colors
- Objective text fields
- Certification manager reference

### LevelSelectController

- Arena, Budget Manager, Objective Manager, Simulation Manager refs
- Tile prefab table for level bootstrap
- Challenge level list data

### DeathHeatmapManager

- Simulation manager reference

### HeatmapVisualizer

- Heatmap manager reference
- `heatmapIntensityMultiplier`
- gradient keys
- show/hide default

## 7) Integration Notes

- Existing build mode, bot personalities, certification runs, report generation, and debug path history remain unchanged.
- New systems are additive and use events (`OnRunFinished`, `OnCertificationCompleted`, `OnArenaChanged`) to avoid replacing current flow.
