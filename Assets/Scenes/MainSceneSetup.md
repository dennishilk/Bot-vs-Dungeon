# Main Scene Setup Guide (Unity 6)

## 1) Scene Core

1. Create `Main` scene.
2. Add `Directional Light`.
3. Add empty `GameManager` object and attach:
   - `GameManager`
   - `SimulationManager`
   - `ArenaManager`
4. Add empty `BuildSystem` object and attach:
   - `BuildModeController`
   - `PlacementSystem`

## 2) Isometric Camera

- Position: `(-10, 18, -10)`
- Rotation: `(45, 45, 0)`
- Projection: Perspective (or Orthographic if preferred)
- Assign camera to `PlacementSystem.mainCamera`

## 3) Placeholder Prefabs (Primitives)

Create these prefabs in `Assets/Prefabs`:

- `PF_Floor`: Cube scaled `(1, 0.1, 1)`, dark gray material
- `PF_Wall`: Cube scaled `(1, 1.5, 1)`
- `PF_Pit`: Cube scaled `(1, 0.1, 1)`, black material, lower Y `-0.4`
- `PF_Saw`: Cylinder scaled `(0.6, 0.1, 0.6)`, add trigger collider + `SawTrap`
- `PF_Bomb`: Sphere scaled `(0.5, 0.5, 0.5)`, add `BombTrap`
- `PF_Archer`: Cube with small forward marker child, add `ArcherTrap`
- `PF_Start`: Blue cube
- `PF_Goal`: Green cube
- `PF_Bot`: Capsule + `BotAgent`, `BotHealth`, `BotPathfinder`, collider

For trap prefabs, set colliders to trigger where needed.

## 4) Hook References

- In `PlacementSystem`, assign all placement prefabs.
- In `SimulationManager`, assign:
  - `ArenaManager`
  - `BuildModeController`
  - `UIController`
  - `botPrefab` (`PF_Bot`)
- In `GameManager`, assign `BuildModeController` and `SimulationManager`.

## 5) Minimal UI

Create a `Canvas` with:

- Top labels: `Mode`, `Selected`, `Status`
- Buttons:
  - Floor (0)
  - Wall (2)
  - Pit (3)
  - Saw (4)
  - Bomb (5)
  - Archer (6)
  - Start (7)
  - Goal (8)
  - Clear
  - Run

Wire buttons:

- Type buttons -> `BuildModeController.SelectTileType(int)`
- Clear -> `BuildModeController.ClearDungeon()`
- Run -> `GameManager.OnRunButtonPressed()`

## 6) Simulation Flow

1. Place floor and walls/traps/start/goal.
2. Press `Run`.
3. Bot spawns with 100 HP and follows weighted path.
4. Result shown as `BOT SURVIVED` or `BOT DIED`.
