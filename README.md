# Bot vs Dungeon

Build a dungeon. Launch a bot. Watch it fail.

Bot vs Dungeon is a small Unity + C# prototype where you create deadly dungeon layouts and test them against an AI-controlled bot trying to reach the exit.

The project focuses on a simple idea:

You are not the hero.
You are the dungeon architect.

Place traps, walls, and hazards.
Then start the simulation and observe whether the bot survives.

Current prototype features:

- isometric dungeon arena
- build mode
- simulation mode
- basic AI pathfinding
- traps and hazards
- bot HP system
- success / death states

Tech stack:

Unity
C#

Future ideas:

multiple bots
heatmaps
difficulty scoring
simulation logs
dungeon sharing

## Project Structure

```text
Assets/
  Scenes/
  Scripts/
    Core/
    Build/
    Bot/
    Traps/
    UI/
    Data/
  Prefabs/
  Materials/
  Models/
  Textures/
  UI/
```

## Quick Setup (Unity 6)

1. Create/open this folder as a Unity 6 3D project.
2. Create scene `Assets/Scenes/Main.unity`.
3. Add empty objects:
   - `GameManager` (attach `GameManager`, `SimulationManager`, `ArenaManager`)
   - `BuildModeController` (attach `BuildModeController`, `PlacementSystem`)
   - `UIController` (attach `UIController`)
4. Create a `Canvas` with buttons for `Floor`, `Wall`, `Pit`, `Saw`, `Bomb`, `Archer`, `Start`, `Goal`, `Clear`, `Run`.
5. Link each button to `BuildModeController.SelectTileType(...)` or `BuildModeController.ClearDungeon()` / `SimulationManager.StartSimulation()`.
6. Add a directional light.
7. Position camera for isometric view (example: X=35, Y=45, Z=-35, Rotation X=45, Y=45, Z=0).
8. Create prefabs using primitives:
   - Floor: flat cube, dark stone material
   - Wall: tall cube
   - Pit: black quad/cube depression
   - Saw: gray cylinder or disc
   - Bomb: red sphere
   - Archer: brown cube with forward marker
   - Bot: capsule (with `NavMeshAgent`, `BotAgent`, `BotHealth`, `BotPathfinder`)
   - Goal: glowing green cube
9. Assign prefabs in `PlacementSystem` and bot/goal references in `SimulationManager`.
10. Bake a NavMesh for floor tiles and test build/simulate loop.

## Controls

- Left mouse: place selected tile/object
- UI buttons: choose placement type
- Clear button: reset dungeon
- Run button: start simulation

## Prototype Notes

- `PitTrap` and walls are marked impassable.
- Danger tiles (saw/bomb/archer) apply additional path cost.
- Bot starts at `Start` tile, seeks `Goal`, and can die at 0 HP.
- Result text prints `BOT SURVIVED` or `BOT DIED`.

## UI Upgrade (Dark Fantasy Presentation Layer)

A focused UI/HUD pass is provided under `Assets/Scripts/UI` and `Assets/UI/Docs/UISetupGuide.md` with:

- main menu flow (start, quit, settings placeholder)
- in-game HUD for mode, selected build tool, simulation status
- build toolbar button highlight behavior
- bot HP/state panel hooks
- result banner controller for survive/died feedback

This upgrade intentionally avoids adding new gameplay systems and only improves readability, menu flow, and thematic presentation.


## Developer Debug Toolkit (AI Testing Lab)

A built-in debugging toolkit is now available for in-game testing without terminal logs:

- debug HUD overlay for live bot telemetry
- simulation controls (spawn/run/reset/clear/pause/resume/slow-mo/step)
- scrollable event log with clear + max count cap
- gizmo visualizations for path, danger, goal, and current target
- personality dropdown (`Careful`, `Balanced`, `Reckless`) for danger-cost testing
- `TestArena` scene workflow and optional prebuilt layouts

See `Assets/Docs/DebugToolkitSetup.md` for full setup and inspector wiring.

## Bureau Campaign + Visual Polish Additions

The project now includes lightweight meta-progression and style-pass guidance without changing core gameplay systems:

- dungeon certification bureau campaign framework (tiers, assignments, rank, promotions)
- bureau score and career profile persistence
- campaign/profile/promotion UI controller scripts
- visual style guide + inspector setup recommendations
- lightweight atmosphere helpers (`GoalGlowPulse`, `AmbientDustSpawner`, `MenuCameraDrift`, `CameraShakeLight`)

See:
- `Assets/Docs/CampaignBureauSetup.md`
- `Assets/Docs/VisualStyleGuide.md`
