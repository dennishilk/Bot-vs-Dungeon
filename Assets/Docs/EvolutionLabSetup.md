# Evolution Lab (Experimental Mode) - Setup & Architecture

This document adds an **optional experimental mode** called **Evolution Lab** for Unity 6 + C# project *Bot vs Dungeon*.

The implementation is fully rule-based, deterministic when desired, and designed to build on existing systems without redesigning or removing them.

---

## 1) Evolutionary Architecture

Core loop implemented in `EvolutionManager`:

1. Generate initial population via existing `DungeonGenerator`.
2. Evaluate each candidate with existing simulation infrastructure (`GenerationEvaluator` + `SimulationManager`).
3. Score fitness based on selected `EvolutionGoal`.
4. Select top survivors (default top 30%).
5. Create next generation via mutation and recombination.
6. Repeat until max generations, target fitness reached, or user stops.

### Key scripts

- `Assets/Scripts/Evolution/EvolutionManager.cs`
- `Assets/Scripts/Evolution/DungeonGenome.cs`
- `Assets/Scripts/Evolution/PopulationManager.cs`
- `Assets/Scripts/Evolution/EvolutionEvaluator.cs`
- `Assets/Scripts/Evolution/MutationEngine.cs`
- `Assets/Scripts/Evolution/RecombinationEngine.cs`
- `Assets/Scripts/Evolution/EvolutionUIController.cs`
- `Assets/Scripts/Evolution/EvolutionTypes.cs`
- `Assets/Scripts/Evolution/EvolutionGoalPresets.cs`

---

## 2) Genome Representation

`DungeonGenome` stores evolvable dungeon state:

- map dimensions (`width`, `height`)
- trap budget count (`trapBudget`)
- `startTile` and `goalTile`
- full placed object list (`placedObjects`) including floor/traps/objectives
- source seed (`sourceSeed`)

The genome can be cloned and converted into `DungeonSaveData` for load/save/share integration.

---

## 3) Population Management Logic

`PopulationManager` handles:

- sorting candidates by fitness descending
- selecting survivors by ratio
- computing average population fitness

Default settings use population size in the requested 10-20 range.

---

## 4) Mutation Rule System

`MutationEngine` supports configurable mutation probability and these mutation operations:

- move trap to a nearby tile
- replace trap type
- add trap
- remove trap
- rotate trap
- add/remove branch corridor floor segment

All mutations are bounded to valid map interior coordinates.

---

## 5) Recombination Rule System

`RecombinationEngine` merges two successful parents by split crossover:

- split the layout on X axis
- copy left side from parent A
- copy right side from parent B
- retain valid start/goal and recalc trap budget

This creates offspring layout variants from two parents.

---

## 6) Fitness Evaluation + Bot Simulation Integration

`EvolutionEvaluator` applies genome to arena using `DungeonSerializer`, then runs validation with existing bot simulation flow via `GenerationEvaluator`.

Metrics collected:

- survival rate
- average damage taken
- average completion time
- average path length
- trap trigger count
- death count + death tile positions
- stress traffic conflict proxy for 5-bot mode

Goals supported:

- `Brutal`
- `Fair`
- `Puzzle`
- `Balanced`
- `StressTest`

Fitness is rule-based and configurable through weights (`EvolutionFitnessWeights`).

---

## 7) Evolution UI Panel Instructions

Add an **Evolution Lab** panel to your menu canvas and bind `EvolutionUIController`.

### Required controls

- Goal selector (`TMP_Dropdown`)
- Population size slider
- Mutation rate slider
- Recombination rate slider
- Speed selector (`Normal`, `Fast`, `Very Fast`)
- Start Evolution button
- Stop Evolution button
- View Best Dungeon button
- View Current Generation button
- Save Best Dungeon button

### Required readouts

- generation counter
- best fitness
- average fitness
- best preview text

### Main Menu integration

`MainMenuController` now has `OpenEvolutionLab()` and optional `EvolutionUIController` reference. Wire a new main menu button to this method.

---

## 8) Inspector Configuration Setup

`EvolutionManager` inspector exposes experimental parameters:

- population size
- max generations
- mutation probability
- recombination probability
- survivor ratio
- simulation depth
- bot count (3-5)
- deterministic seed toggle/seed
- speed mode
- skip visualization toggle
- custom fitness weights

`targetFitnessThreshold` controls early stop when high quality candidate is reached.

---

## 9) Example Goal Parameter Presets

`EvolutionGoalPresets` includes recommended presets:

- **Brutal**: higher mutation and trap lethality weighting.
- **Fair**: moderate mutation, rewards survivability + non-excessive trap pressure.
- **Puzzle**: strong path complexity weighting, higher recombination.
- **Balanced**: midline defaults for mixed outcomes.
- **StressTest**: larger population (20), 5 bots, very fast simulation.

---

## Save/Share/Campaign/Sandbox integration notes

- `EvolutionManager.SaveBestDungeon(...)` persists best evolved layout via `DungeonSaveManager`.
- `EvolutionManager.ExportBestShareCode(...)` exports through existing `ShareCodeManager`.
- Since evolved dungeons are stored as normal save data, they can be loaded by existing browser and used in sandbox/campaign workflows.

---

## Performance & Safety

To keep gameplay responsive:

- keep population to 10-20 (default constrained)
- cap bot count to 3-5
- cap simulation depth
- use fast/very fast timescale for lab runs
- use `skipBotVisuals` to reduce UI overhead during evolution

