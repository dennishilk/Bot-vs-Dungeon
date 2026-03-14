# Bot vs Dungeon — Visual Style Guide (Retro Dungeon)

For implementation sequencing and inspector recommendations, see:
- `Assets/Docs/ClassicDungeonAtmospherePass.md`
- `Assets/Docs/RetroDungeonVisualPassPlan.md`

## 1) Color Palette (Use Consistently)
- Stone dark slate: `#252B33`
- Stone blue-grey: `#2F3742`
- Stone edge highlight: `#434C58`
- Iron: `#686A6F`
- Bronze: `#8A6A3F`
- Danger red: `#B33A33`
- Danger orange: `#D77A2E`
- Magic blue: `#4C6FC4`
- Magic purple: `#6E54B8`
- Goal gold: `#C7A04A`
- Goal green glow: `#65B971`

## 2) Lighting Rules
- Warm torch pools in cool, dark spaces.
- Surrounding areas should stay dim to preserve contrast.
- Shadow casting is required around trap clusters and key architecture.
- Ambient fill stays soft and low; avoid over-bright scenes.

## 3) Material Style Rules
- Semi-flat, stylized, low-noise materials.
- Subtle texture tiling only.
- Avoid photoreal detail density.
- Preserve strong silhouette readability from isometric view.

## 4) UI Frame Style
- Core panel language: dark parchment + stone framing.
- Border language: bronze trim with engraved corners.
- Active/selected state: subtle rune glow accents.
- Keep state colors consistent: danger red/orange, success gold/green, magic blue/purple.

## 5) Trap Silhouette Rules
- Every trap type must be identifiable by shape at a glance.
- Add restrained color accents for quick recognition.
- Do not bury trap silhouettes under props, fog, or particles.
- Trigger/readiness cues should be readable in under one second.
