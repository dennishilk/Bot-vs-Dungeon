# Certification System Setup (Unity 6)

This project now supports personality-driven simulation runs and certification reporting.

## 1) New Runtime Data

- `BotPersonality` now includes: `Careful`, `Balanced`, `Reckless`, `Panic`.
- `RunResult` stores per-run output:
  - personality
  - survived/died
  - cause of death
  - completion time
  - remaining HP
  - death position
  - path length
- `DungeonReport` aggregates certification run output and computes:
  - survival totals
  - average HP/time/path length
  - dungeon rating
  - certification verdict

## 2) Scene Hookup

### SimulationControlPanel
Assign references in the inspector:
- `Simulation Manager`
- `Certification Manager`
- existing buttons
- new buttons:
  - `Certification Run Button`
  - `Clear Markers Button`
- personality dropdown (`Careful/Balanced/Reckless/Panic`)
- optional path history toggle (`Show Path History Toggle`)

### CertificationManager
Assign:
- `Simulation Manager`
- `Dungeon Report Panel`
- `Death Marker Spawner`

Tune:
- `Delay Between Runs` (default 0.75)
- `Show Path History By Default`
- `Safe Hp Threshold` (default 50)
- `Fair Hp Threshold` (default 30)

### DungeonReportPanel
Assign text references:
- run summary text
- stats text
- rating text
- verdict text
- flavor text

### DeathMarkerSpawner
Assign:
- `Marker Prefab` (death marker)
- optional `Success Marker Prefab`
- `Marker Height`

### DebugOverlay (optional)
Assign new fields:
- `Certification Manager`
- personality text
- certification run progress text

## 3) Bot AI Tuning

`BotAgent` exposes personality tuning:
- Danger multipliers:
  - careful (high)
  - balanced (medium)
  - reckless (low)
  - panic (slightly unsafe default)
- Adjacent trap penalties:
  - careful avoids trap-near tiles strongly
  - balanced moderate avoidance
  - reckless minimal avoidance
  - panic weak avoidance
- Panic settings:
  - trigger HP
  - panic path noise

## 4) Single Run vs Certification Run

- **Single Run**: use dropdown personality + existing run button.
- **Certification Run**: press `Certification Run`; system auto-runs:
  1. Careful
  2. Balanced
  3. Reckless

Each run reuses the same dungeon and records a `RunResult`.

## 5) Death Markers + Path History

- Death markers spawn at bot death tile per run.
- Marker color is personality-coded.
- Optional success markers spawn at goal tile.
- Path history draws gizmo lines for previous runs.
- Use `Show Path History` toggle and `Clear Markers` button to manage visibility.

## 6) Example Rating + Verdict Rules

Current defaults in `DungeonReport.Build`:

- 3/3 survive:
  - `Safe` if avg HP >= safe threshold
  - otherwise `Fair`
  - verdict `Certified` or `Certified With Risk`
- 2/3 survive:
  - rating `Dangerous`
  - verdict `Certified With Risk` or `Unsafe` (if trap activity is high)
- 1/3 survive:
  - rating `Brutal`
  - verdict `Bot Fatality Zone`
- 0/3 survive:
  - rating `Impossible` (or `Brutal` without trap activity)
  - verdict `Impossible Layout`

Adjust thresholds and wording as needed for your target difficulty scale.
