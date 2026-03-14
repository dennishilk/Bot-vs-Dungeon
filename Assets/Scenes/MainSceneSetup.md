# Bot vs Dungeon — Visual Polish Scene Setup (Unity 6)

This guide upgrades the prototype into a compact, readable isometric dark-fantasy dungeon arena using only primitives, simple materials, and lightweight visual scripts.

---

## 1) New Folder Structure

Create or align folders like this:

```text
Assets/
  Art/
    Materials/
    Prefabs/
      Environment/
      Traps/
      Bot/
      Markers/
      Props/
  Scenes/
  Scripts/
    Visual/
  UI/
```

If your project already has `Assets/Materials` / `Assets/Prefabs`, keep those and mirror the same subfolder grouping for consistency.

---

## 2) Material List (Low-Poly Readability First)

Use URP/Lit, Standard, or equivalent lit shader. Keep smoothness low (`0.0 - 0.2`) except emissive accents.

| Material | Suggested Color (Hex) | Use |
|---|---|---|
| `M_DungeonFloor` | `#4C515A` | Main walkable stone tiles |
| `M_DungeonFloor_VarA` | `#444953` | Subtle variation every few tiles |
| `M_DungeonWall` | `#666A73` | Raised dungeon walls |
| `M_DungeonWall_Dark` | `#575C66` | Wall variation / pillars |
| `M_PitTrap` | `#0C0D10` | Pit opening / void look |
| `M_SawMetal` | `#8A909A` | Saw blade body |
| `M_SawWarning` | `#A62E2E` | Saw warning stripe/accent |
| `M_BombBody` | `#2D3138` | Bomb trap base |
| `M_BombAccent` | `#D7662D` | Fuse/rune/explosive accent |
| `M_ArcherBody` | `#5A4B3F` | Archer trap base |
| `M_ArcherAccent` | `#B26B3A` | Head/bolt highlight |
| `M_StartMarker` | `#34B7E8` (emission on) | Start tile marker |
| `M_GoalMarker` | `#7DCC4A` or `#C8A84A` (emission on) | Goal tile marker |
| `M_BotBody` | `#AEB8C7` | Bot main material |
| `M_BotAccent` | `#2F6FB5` | Bot highlight/cloak band |
| `M_Projectile` | `#D9B066` | Optional projectile visual |
| `M_Void` | `#06070A` | Outer non-playable void plane |
| `M_TorchFlame` | `#F39A3B` (emission on) | Torch flame sphere |

**Inspector hints:**
- Trap danger accents should be brighter/saturated than floor/walls.
- Start and goal should use emissive color so they remain obvious in low light.

---

## 3) Prefab Assembly from Primitives

All units below assume 1 tile = `1x1`.

### 3.1 `PF_FloorTile` (Environment)
- Root `FloorTile` (Cube)
- Scale: `(1, 0.12, 1)`
- Material: `M_DungeonFloor` or `_VarA`
- Position Y: `0`

### 3.2 `PF_Wall` (Environment)
- Root `Wall` (Cube)
- Scale: `(1, 1.4, 1)`
- Position Y: `0.7`
- Material: `M_DungeonWall`

### 3.3 `PF_PitTrap` (Traps)
- Root `PitTrap` (Empty)
  - Child `PitFrame` (Cube): scale `(1, 0.12, 1)`, material `M_DungeonFloor_VarA`
  - Child `PitVoid` (Cube): scale `(0.82, 0.08, 0.82)`, local position `(0, -0.09, 0)`, material `M_PitTrap`
- Add gameplay script already used by prototype (`PitTrap`) to root.
- Collider can stay on frame and/or root according to existing trap behavior.

### 3.4 `PF_SawTrap` (Traps)
- Root `SawTrap` (Empty)
  - Child `Base` (Cylinder): scale `(0.42, 0.06, 0.42)`, material `M_DungeonFloor_VarA`
  - Child `Blade` (Cylinder): scale `(0.34, 0.03, 0.34)`, local Y `0.08`, material `M_SawMetal`
  - Child `WarningRing` (Cylinder): scale `(0.39, 0.01, 0.39)`, local Y `0.065`, material `M_SawWarning`
- Add trigger collider + existing `SawTrap` trap script.
- Add `SawRotate` script to `Blade`.

### 3.5 `PF_BombTrap` (Traps)
- Root `BombTrap` (Empty)
  - Child `Plate` (Cylinder): scale `(0.35, 0.04, 0.35)`, material `M_BombBody`
  - Child `Charge` (Sphere): scale `(0.28, 0.28, 0.28)`, local Y `0.12`, material `M_BombBody`
  - Child `Fuse` (Cylinder): scale `(0.03, 0.08, 0.03)`, local position `(0.08, 0.24, 0.03)`, material `M_BombAccent`
- Add existing `BombTrap` script to root.

### 3.6 `PF_ArcherTrap` (Traps)
- Root `ArcherTrap` (Empty)
  - Child `Pedestal` (Cube): scale `(0.55, 0.30, 0.55)`, material `M_ArcherBody`
  - Child `Head` (Cube): scale `(0.30, 0.20, 0.40)`, local Y `0.24`, material `M_ArcherBody`
  - Child `Barrel/Direction` (Cylinder): scale `(0.05, 0.18, 0.05)`, rotate Z `90`, local `(0, 0.25, 0.26)`, material `M_ArcherAccent`
- Add existing `ArcherTrap` script to root.
- Ensure root forward (`+Z`) matches firing direction used by script.

### 3.7 `PF_StartMarker` (Markers)
- Root `StartMarker` (Empty)
  - Child `Plate` (Cube): scale `(0.92, 0.03, 0.92)`, material `M_StartMarker`
  - Child `Rune` (Cylinder): scale `(0.25, 0.01, 0.25)`, local Y `0.03`, material `M_StartMarker`

### 3.8 `PF_GoalMarker` (Markers)
- Root `GoalMarker` (Empty)
  - Child `Plate` (Cube): scale `(0.92, 0.03, 0.92)`, material `M_GoalMarker`
  - Child `RuneRing` (Cylinder): scale `(0.30, 0.01, 0.30)`, local Y `0.03`, material `M_GoalMarker`
  - Optional Child `GlowOrb` (Sphere): scale `(0.16, 0.16, 0.16)`, local Y `0.25`, material `M_GoalMarker`

### 3.9 `PF_Bot` (Bot)
- Root `Bot` (Capsule): scale `(0.75, 0.9, 0.75)`, material `M_BotBody`
  - Child `HeadAccent` (Sphere): scale `(0.35, 0.18, 0.35)`, local Y `0.8`, material `M_BotAccent`
  - Child `FacingMarker` (Cube): scale `(0.10, 0.08, 0.20)`, local `(0, 0.35, 0.35)`, material `M_BotAccent`
- Keep existing gameplay components on root (`BotAgent`, `BotHealth`, `BotPathfinder`, collider/agent as already configured).

### Optional `PF_Torch` (Props)
- Root `Torch`
  - Child `Pole` (Cylinder): `(0.04, 0.45, 0.04)`, `M_DungeonWall_Dark`
  - Child `Brazier` (Sphere): `(0.15, 0.08, 0.15)`, `M_DungeonWall`
  - Child `Flame` (Sphere): `(0.10, 0.16, 0.10)`, `M_TorchFlame`
  - Child `Point Light`
- Add `TorchFlicker` to root and assign point light + flame.

---

## 4) Visual-Only Scripts

Use these scripts only for look/feedback, no gameplay changes:

- `Assets/Scripts/Visual/SawRotate.cs`
  - Rotates saw blade continuously for immediate hazard readability.
- `Assets/Scripts/Visual/TorchFlicker.cs` (optional)
  - Adds subtle warm light variation and flame jitter to torches.

---

## 5) Exact Camera Setup (Static Isometric)

Create camera object `Main Camera` and use:

- **Projection:** Orthographic
- **Position:** `(12, 16, -12)`
- **Rotation:** `(35, 45, 0)`
- **Orthographic Size:** `9.5` (tune `8.5 - 11` by arena bounds)
- **Near Clip:** `0.3`
- **Far Clip:** `100`
- **Follow behavior:** none (static framing)
- **Clear Flags:** Solid Color
- **Background:** `#0A0C11`

Result: full arena visibility, low distortion, clear trap placement read.

---

## 6) Exact Lighting Setup

### Main Directional Light
- Rotation: `(50, -35, 0)`
- Color: slightly cool neutral (`#D7DCE6`)
- Intensity: `1.15`
- Shadow Strength: `0.65`
- Shadow Bias: default/slight increase to avoid acne on tiles

### Ambient / Environment
- Environment Lighting Source: Color
- Ambient Color: `#2A2F3A`
- Ambient Intensity: `0.75`

### Optional Accent Lights
- Place 2-4 torches near walls/corners and 1 near goal.
- Point light color: warm (`#FFB164`)
- Range: `3.0 - 4.5`
- Intensity: `0.8 - 1.2`
- Keep radius local so the arena stays readable and not blown out.

---

## 7) Recommended Scene Hierarchy

```text
Main
  GameManagers
    GameManager (+ SimulationManager + ArenaManager)
  Environment
    FloorRoot
    WallRoot
    DecorationRoot
    VoidPlane
  TrapsRoot
  MarkersRoot
    StartRoot
    GoalRoot
  BotRoot
  Lighting
    Directional Light
    AccentLights(optional)
  Camera
    Main Camera
  UI
    Canvas
      Mode/Status labels
      Build + Run/Clear buttons
```

---

## 8) Key Inspector Settings

- **Trap prefabs:** keep colliders/trigger behavior consistent with existing trap scripts.
- **ArcherTrap root orientation:** ensure local forward points to intended shot direction.
- **Start/Goal markers:** emissive enabled (`Emission Intensity ~1.5 - 2.5`).
- **Saw blade:** attach `SawRotate`; speed around `220 - 320` deg/sec.
- **Torches:** attach `TorchFlicker`; min/max intensity around `0.75 / 1.15`.
- **Camera:** orthographic + fixed transform; no follow script.
- **VoidPlane:** large plane under arena (e.g., scale `6,1,6`) using `M_Void`.

---

## 9) Why This Style Improves Gameplay Readability

- **Strong silhouette language:** each trap shape is unique (flat void, spinning disc, compact bomb, directional turret).
- **Color coding with restraint:** neutral dungeon palette + saturated hazard/start/goal accents draws attention to gameplay-critical elements.
- **Isometric orthographic framing:** ensures tactical overview without perspective distortion.
- **Layered lighting:** moody dungeon atmosphere while preserving enough contrast to instantly parse walkable space and threats.
- **Simple modular prefabs:** low-poly primitives are easy to edit, performant, and consistent with prototype scope.
