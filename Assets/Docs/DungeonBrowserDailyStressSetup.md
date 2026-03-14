# Dungeon Browser + Daily Challenge + Stress Test Setup (Unity 6)

## 1) Dungeon Browser system
- Add `DungeonBrowserManager` to a scene object (example: `Systems/Persistence`).
- Assign:
  - `Save Manager` -> existing `DungeonSaveManager`
  - `Simulation Manager` -> existing `SimulationManager`
- Add `DungeonBrowserPanel` to `Canvas/HUD/DungeonBrowserPanel`.
- Assign:
  - `Browser Manager`
  - `Sort Dropdown`
  - `Dungeon Dropdown`
  - `Details Text`
  - `Status Text`
  - `Panel Root`

The browser lists local, imported share-code, and daily challenge files through `DungeonSaveManager.ListSaves` and supports sorting by name/date/rating.

## 2) Daily challenge generator
- Add `DailyChallengeGenerator` to `Systems/Challenges`.
- Inspector controls:
  - `Min Map Size` / `Max Map Size`
  - `Min Trap Budget` / `Max Trap Budget`
- Seed logic is deterministic:
  - `seed = YYYYMMDD`
  - Example: `2026-03-14 => 20260314`

## 3) Daily challenge tracking
- Add `DailyChallengeManager` and assign:
  - `Generator`
  - `Save Manager`
  - `Simulation Manager`
  - Optional `Replay Viewer`
  - UI labels (`titleText`, `objectiveText`, `budgetText`, `statusText`)
- Data stored locally in `Application.persistentDataPath/daily_challenge_results.json`:
  - completion status
  - best survival result
  - best completion time
  - attempts

## 4) Stress test manager
- Add `StressTestManager` to `Systems/Simulation`.
- Assign:
  - `Arena Manager`
  - `Bot Prefab`
  - `Heatmap Manager`
  - `Stress Test Report`

## 5) Stress test report UI
- Add `StressTestReport` on `Canvas/HUD/StressTestReportPanel`.
- Assign a TMP text field to `Report Text`.
- `StressTestManager` will push:
  - Bots Spawned / Survived / Died
  - Average Survival Time
  - Average Path Length
  - Most Lethal Tile
  - Most Common Cause of Death

## 6) Inspector setup instructions
### StressTestManager recommended defaults
- Max Bots: `50`
- Default Bot Count: `20`
- Spawn Interval: `0.12`
- Path Update Frequency: `0.1`
- Max Simulation Time: `90`

### DailyChallengeGenerator recommended defaults
- Min Map Size: `8`
- Max Map Size: `12`
- Min Trap Budget: `18`
- Max Trap Budget: `40`

## 7) UI hierarchy instructions
Suggested hierarchy:
- `Canvas`
  - `MainMenuPanel`
    - `OpenDungeonBrowserButton`
    - `OpenDailyChallengeButton`
    - `OpenStressTestButton`
  - `HUD`
    - `DungeonBrowserPanel`
      - `SortDropdown`
      - `DungeonDropdown`
      - `DetailsText`
      - `LoadButton`
      - `TestButton`
      - `DeleteButton`
    - `DailyChallengePanel`
      - `TodayTitle`
      - `ObjectiveText`
      - `BudgetText`
      - `PlayChallengeButton`
      - `ViewResultsButton`
      - `ReplayRunsButton`
    - `StressTestPanel`
      - `Run10Button`
      - `Run20Button`
      - `Run50Button`
      - `StopButton`
    - `StressTestReportPanel`
      - `ReportText`

## 8) Example daily challenge seed logic
```csharp
int seed = date.Year * 10000 + date.Month * 100 + date.Day;
UnityEngine.Random.InitState(seed);
```

## 9) Stress test statistics collection
`StressTestManager` records:
- total bots spawned
- bots survived
- bots died
- average survival time
- average path length
- most lethal tile (local run)
- most common cause of death

Deaths are forwarded to `DeathHeatmapManager.RecordDeathAtTile`, so stress tests feed the existing heatmap/bottleneck visualization.
