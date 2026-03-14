# Director Mode Setup (Optional Procedural Dungeon Director)

## Overview
Director Mode is an **optional** rule-based generation workflow that layers on top of existing Bot vs Dungeon systems.
It does not replace build mode, certification, progression, save/load, share, replay, stress testing, campaign, or adaptive learning.

## 1) Director System Architecture

### Core scripts
- `Assets/Scripts/Director/DungeonDirector.cs`
  - Orchestrates generation attempts.
  - Applies generated layout through `DungeonSerializer`.
  - Evaluates each candidate through `GenerationEvaluator`.
  - Optionally triggers certification runs.
  - Emits `OnDirectorReportReady` for UI/reporting.

- `Assets/Scripts/Director/DungeonGenerator.cs`
  - Deterministic, rule-based layout generation.
  - Builds start/goal, main path, branches, trap corridors.
  - Applies goal presets and trap density patterns.

- `Assets/Scripts/Director/GenerationEvaluator.cs`
  - Runs bot simulations using existing `SimulationManager`.
  - Uses Careful/Balanced/Reckless runs (1-3 bots configurable).
  - Computes survival, triggers, path length, completion time, and goal-match score.

- `Assets/Scripts/Director/DirectorReport.cs`
  - Holds report structures: parameters, evaluation summary, and verdict fields.

- `Assets/Scripts/Director/DirectorModePanel.cs`
  - Director menu panel controller.
  - Exposes Generate / Generate+AutoTest and preset actions.
  - Displays attempts, simulation stats, goal match score, and verdict text.

## 2) Dungeon Generation Rules
Generation is intentionally lightweight and deterministic:
1. Define map bounds.
2. Place Start and Goal.
3. Create main path from Start to Goal.
4. Create side branches based on branch frequency.
5. Place traps by rule patterns and goal profile.
6. Validate with bot simulation.
7. Iterate up to `maxGenerationAttempts`.

## 3) Path Generation Approach
- Main path built with Manhattan-distance preference plus complexity noise.
- Branches generated from path pivots.
- Guaranteed playable baseline by always retaining a connected Start→Goal floor path.

## 4) Trap Placement Logic
Rule-based trap placement (not pure random):
- Edge/corridor bias for corridor traps.
- Goal-based trap profile:
  - Fair: lighter traps, lower density.
  - Dangerous/Brutal: bomb/archer heavy and denser.
  - Puzzle: pit/archer interactions with branches and detours.
  - StressTest: high density and pressure zones.

## 5) Evaluation & Scoring Logic
Validation runs use existing bot simulation personalities:
- Careful
- Balanced
- Reckless

Collected metrics:
- Survival rate
- Trap triggers (death count proxy)
- Average path length
- Average completion time

Goal-match score (0-1) is computed via weighted formulas per `DirectorGoal`.
Director stops early if score reaches strong threshold, otherwise keeps best attempt.

## 6) Director Mode UI
`DirectorModePanel` expected controls:
- Goal dropdown
- Generate Dungeon
- Generate + Auto Test
- Generate Challenge Dungeon
- Generate Brutal Dungeon
- Generate Fair Dungeon
- Map size slider
- Trap density slider

Display fields:
- Generation attempts
- Simulation summary
- Goal match score
- Director report summary text

## 7) Inspector Parameters
`DirectorParameters`:
- map size (width/height)
- trap density
- branch frequency
- max trap budget
- generation attempts
- simulation depth
- validation bot count
- deterministic seed mode

## 8) Example Presets
Configured in `DungeonDirector`:
- Fair preset
  - low trap density
  - lower branch complexity
  - lower budget
- Brutal preset
  - high trap density
  - high branch complexity
  - high budget
- Challenge (Dangerous) preset
  - medium-high density
  - medium branches
  - medium-high budget

## 9) Director Report System
`DirectorReport` output includes:
- Director Goal
- Attempts used
- Generated Layout Score
- Survival Statistics
- Most Lethal Cause
- Average Survival Time
- Goal Match Score
- Verdict (e.g., Successful Brutal Dungeon)

## Save / Share / Replay / Campaign Integration
- Generated layouts are applied through existing serializer flow.
- Generated layouts are persisted into save storage (`SaveImportedLayout`) so they are browseable and shareable.
- Auto test uses existing certification pipeline.
- Replays/campaign/progression hooks continue to function through existing systems.

## Performance Safety
Safety constraints are built-in:
- Attempt cap (`maxGenerationAttempts`)
- Simulation depth cap (`simulationDepthSeconds`)
- Validation bot count cap (1-3)

No machine learning, no server-side generation, and no heavy procedural libraries.
