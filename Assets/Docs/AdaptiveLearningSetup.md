# Adaptive Learning Setup (Unity 6 + C#)

## Added scripts
- `Assets/Scripts/Core/AdaptiveLearningManager.cs`
- `Assets/Scripts/Data/AdaptiveTileMemory.cs`
- `Assets/Scripts/Data/DungeonLearningMemory.cs`
- `Assets/Scripts/Data/BotLearningProfile.cs`
- `Assets/Scripts/Debug/LearningOverlayVisualizer.cs`
- `Assets/Scripts/UI/LearningSummaryPanel.cs`

## Inspector configuration
On `AdaptiveLearningManager`:
- **Base Death Learning Weight**: `3.0`
- **Base Damage Learning Weight**: `0.24`
- **Success Path Reduction Weight**: `0.16`
- **Neighbor Spread Factor**: `0.45`
- **Forgetfulness Rate**: `0.02`
- **Careful Learning Multiplier**: `1.35`
- **Balanced Learning Multiplier**: `1.0`
- **Reckless Learning Multiplier**: `0.55`
- **Panic Learning Multiplier**: `1.65`

## Example formulas
- **Death learning**
  - `danger += baseDeathWeight * learningStrength * spread * (1 + deathCount * repeatedFailureAmplification)`
- **Damage learning**
  - `danger += baseDamageWeight * learningStrength * max(0.1, damage * 0.1)`
- **Success learning**
  - `danger = max(0, danger - successReductionWeight * learningStrength)`
- **Forgetfulness**
  - `danger *= (1 - forgetfulnessRate)` each observed run

## Scene hookup
1. Add `AdaptiveLearningManager` to a persistent scene object (e.g. `GameSystems`).
2. Assign it to:
   - `SimulationManager.adaptiveLearningManager`
   - `StressTestManager.adaptiveLearningManager`
   - `LearningOverlayVisualizer.adaptiveLearningManager`
3. Add `LearningOverlayVisualizer` to any gizmo/debug object and wire:
   - `arenaManager`
   - `simulationManager`
4. Update `SimulationControlPanel` references:
   - `adaptiveModeToggle`
   - `adaptiveCertificationToggle`
   - `showLearnedDangerToggle`
   - `showSuccessfulPathsToggle`
   - `showLearningHeatmapToggle`
   - `resetLearningButton`
   - `resetAllLearningButton`
5. Add `LearningSummaryPanel` UI text object and link `simulationManager` + `summaryText`.

## Modes
- **Fresh Run Mode**: bot ignores learned danger while pathing.
- **Adaptive Run Mode**: pathing cost includes learned danger modifiers.

## Stress test options
- **SharedLearningPool**: all stress bots contribute to one memory.
- **IndividualLearning**: each stress bot gets its own memory namespace.
