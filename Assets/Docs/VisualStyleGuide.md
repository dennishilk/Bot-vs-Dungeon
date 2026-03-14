# Bot vs Dungeon Visual Style Guide (Unity 6)

For the complete production checklist, use `Assets/Docs/ClassicDungeonAtmospherePass.md`.

## Core Mood Pillars
- Dark fantasy stone dungeon.
- Warm torch pools against cool global fill.
- Stylized readability over realism.

## Palette Anchors
- Stone: `#1E232A`, `#2A2F36`, `#343B44`
- Torch: `#FFAD66`, `#FFC073`
- Danger: `#B83A2F`, `#E26B2F`
- Arcane: `#4E6CD3`, `#7A55C3`
- Goal: `#67C97C`, `#D4B04E`

## Readability Rules
- Gameplay-critical elements should parse in <1 second from camera default.
- Avoid placing bright props on key traversal lanes.
- Traps must preserve unique silhouette + accent identity.

## Lighting Rules
- Keep fog subtle and depth-oriented, never obscuring trap telegraphs.
- Prefer point-light torches with small range and animated flicker.
- Use high contrast around goals and trap clusters.

## UI Rules
- Dark stone/iron base, bronze accents, muted parchment support.
- Preserve color-coded state clarity (selected/safe/danger/success).
- Decorative rune detail should remain subtle and lightweight.
