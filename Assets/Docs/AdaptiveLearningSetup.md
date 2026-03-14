# Adaptive Learning Setup (Unity 6 + C#)

This prototype now includes a deterministic, rule-based adaptive memory layer so bots can "learn from pain" across repeated runs without ML.

## 1) Adaptive learning data structures
- `Assets/Scripts/Data/AdaptiveTileMemory.cs`
  - Stores per-tile signals:
    - `gridPosition`
    - `deathCount`
    - `damageCount`
    - `totalDamageTaken`
    - `successfulPassCount`
    - `learnedDangerModifier`
    - `avoidedCount`
- `Assets/Scripts/Data/DungeonLearningMemory.cs`
  - Stores dungeon-scoped memory:
    - `dungeonID`
    - `tileMemory` list
    - `totalRunsObserved`
    - `totalDeaths`
    - `totalSuccesses`
  - Helper queries:
    - most lethal tile
    - most avoided tile
    - most learned-dangerous tile
- `Assets/Scripts/Data/BotLearningProfile.cs`
  - Personality-influenced adaptation profile:
    - `personality`
    - `riskToleranceModifier`
    - `learningStrength`
    - `forgetfulnessFactor`

## 2) Adaptive pathfinding cost integration
- `Assets/Scripts/Bot/BotPathfinder.cs`
  - Final step cost:
    - `1`
    - `+ base trap danger * personality multiplier`
    - `+ learned danger modifier`
    - `+ adjacent trap penalty`
    - `+ deterministic noise (panic/debug)`
- `Assets/Scripts/Bot/BotAgent.cs`
  - Passes personality profile + dungeon ID into pathfinder.
  - Can run in fresh mode (`useAdaptiveLearning=false`) or adaptive mode (`true`).

## 3) Learning update logic after runs
- `Assets/Scripts/Core/AdaptiveLearningManager.cs`
  - **Death learning** with neighbor spread and repeated-failure amplification.
  - **Damage learning** proportional to damage amount.
  - **Success learning** reduces danger on successful route tiles.
  - **Path avoidance tracking** increases `avoidedCount` for high-danger tiles skipped by chosen routes.
  - **Forgetfulness** decays learned danger each observed run.
  - **Dungeon identifier** is deterministic from ordered tile layout hash (+ optional prefix).

## 4) UI for adaptive mode + reset controls
- `Assets/Scripts/Debug/SimulationControlPanel.cs`
  - Toggles:
    - Adaptive mode (Fresh vs Adaptive)
    - Adaptive certification mode
    - Show learned danger
    - Show successful paths
    - Show learning heatmap
  - Buttons:
    - Reset current dungeon learning
    - Reset all learning
- `Assets/Scripts/UI/LearningSummaryPanel.cs`
  - Displays live dungeon learning summary, including improvement metrics.

## 5) Overlay visualization logic
- `Assets/Scripts/Debug/LearningOverlayVisualizer.cs`
  - Gizmo overlays:
    - learned danger heatmap (safe→danger colors)
    - successful path markers (optional)
  - Global toggles support debug filtering.

## 6) Report integration for adaptive statistics
- `Assets/Scripts/Data/DungeonReport.cs`
- `Assets/Scripts/UI/DungeonReportPanel.cs`
- `Assets/Scripts/UI/StressTestReport.cs`
- `Assets/Scripts/Core/StressTestManager.cs`
  - Reports now include:
    - learned dangerous tile count
    - most lethal tile
    - most avoided tile
    - most learned-dangerous tile
    - fresh success rate vs adaptive success rate
    - adaptive improvement

## 7) Inspector setup instructions
### AdaptiveLearningManager
- **Mode**
  - `Run Mode`: Adaptive (default) or Fresh
  - `Allow Fresh Runs To Record Learning`: optional baseline collection
  - `Persistence Enabled`: optional local memory persistence
  - `Persistence File Name`: e.g. `adaptive_learning_memory.json`
- **Learning Weights**
  - `Base Death Learning Weight`
  - `Base Damage Learning Weight`
  - `Success Path Reduction Weight`
  - `Neighbor Spread Factor`
  - `Forgetfulness Rate`
  - `Repeated Failure Amplification`
- **Personality Learning Multipliers**
  - `Careful Learning Multiplier`
  - `Balanced Learning Multiplier`
  - `Reckless Learning Multiplier`
  - `Panic Learning Multiplier`

### SimulationManager
- Link `adaptiveLearningManager`
- Configure `adaptiveModeEnabled` default

### StressTestManager
- Link `adaptiveLearningManager`
- Set `adaptiveStressMode`
- Set `stressAdaptiveLearningMode`:
  - `SharedLearningPool` (recommended default)
  - `IndividualLearning`

### CertificationManager
- Use `adaptiveCertificationMode` toggle:
  - OFF => each certification bot starts fresh
  - ON => later bots benefit from earlier outcomes

## 8) Example formulas
- Death learning:
  - `danger += baseDeathWeight * effectiveLearning * spread * (1 + deathCount * repeatedFailureAmplification)`
- Damage learning:
  - `danger += baseDamageWeight * effectiveLearning * max(0.1, damage * 0.1)`
- Success path learning:
  - `danger = max(0, danger - successReductionWeight * effectiveLearning)`
- Final pathfinding tile cost:
  - `cost = base + trap + personalityWeight + learnedDanger + adjacencyPenalty + deterministicNoise`
- Forgetfulness:
  - `danger *= clamp01(1 - forgetfulnessRate)` per run outcome

## 9) Scene hookup instructions
1. Add `AdaptiveLearningManager` to a persistent object (e.g. `GameSystems`).
2. Wire references:
   - `SimulationManager.adaptiveLearningManager`
   - `StressTestManager.adaptiveLearningManager`
   - `LearningOverlayVisualizer.adaptiveLearningManager`
3. Add `LearningOverlayVisualizer` to a debug gizmo object and link:
   - `arenaManager`
   - `simulationManager`
4. In debug UI (`SimulationControlPanel`), wire:
   - Adaptive mode + certification toggles
   - Overlay toggles
   - Reset learning buttons
5. Add `LearningSummaryPanel` and assign:
   - `simulationManager`
   - `summaryText`
6. (Optional persistence)
   - Enable `AdaptiveLearningManager.persistenceEnabled` to save/load per-dungeon learning from `Application.persistentDataPath`.

## Notes
- No ML/NN/external AI is used.
- Behavior is deterministic, rule-based, and inspectable.
- Existing systems remain intact and are extended via additive integration points.
