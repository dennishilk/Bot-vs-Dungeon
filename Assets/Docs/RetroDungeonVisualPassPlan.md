# Bot vs Dungeon — Retro Dungeon Visual Pass Plan (Unity 6)

This plan delivers a **nostalgic dark-fantasy presentation pass** inspired by classic 1990s/early-2000s dungeon RPGs while keeping every existing gameplay system intact.

## Scope Guardrails
- No gameplay redesign.
- No new mechanics.
- No complex shaders.
- No photorealistic assets.
- Keep visuals stylized, readable, and lightweight.

## Existing System Compatibility (No Logic Changes)
This pass layers on top of existing systems only:
- build mode, simulation, bot personalities, certification runs
- trap budget, objectives, save/load, share codes, replay viewer
- progression, achievements, dungeon browser, daily challenge
- stress test, adaptive learning, campaign system
- trap ecology, director mode, evolution lab
- classic atmosphere pass, audio identity system

---

## 1) Visual Style Plan
### Mood target
- Ancient stone labyrinths with warm torch pools, deep shadows, and cold ambient fill.
- Strong silhouette readability from isometric camera distance.
- Sparse storytelling props to imply age, danger, and repeated trap usage.

### Pass order
1. **Lighting foundation** (torches, darkness, shadow controls).
2. **Palette + materials normalization** (stone/metal/danger/magic/goal color roles).
3. **Environment storytelling set dressing** (small, sparse props).
4. **Lightweight VFX layer** (dust, sparks, goal/magic pulses).
5. **Camera presentation tuning** (stable retro isometric framing).
6. **UI reskin layer** (stone/bronze/parchment/rune treatment).
7. **Final readability check** (trap silhouettes + objective clarity).

---

## 2) Lighting Setup Recommendations (Retro Dungeon)
### Global lighting
- Keep one very soft cool directional moon-fill only.
- Ambient intensity should remain low (target dark baseline).
- Use soft ambient color in deep blue-grey range.

### Torch lighting
- Place warm point lights at junctions, corners, room entries, and trap clusters.
- Torch range should be small to create **light pools**, not full-room illumination.
- Use subtle flicker on intensity/range/color.

### Shadows
- Enable shadows on nearby hazards and major silhouette-defining geometry.
- Prefer medium shadow distance for performance.
- Ensure trap trigger surfaces remain readable in darkness.

### Contrast rules
- Target ratio: bright torch pool / dark surrounding space.
- Avoid evenly lit floors.
- Keep goals and critical hazards in controlled highlight zones.

---

## 3) Material Palette (Retro, Low-Noise)
### Stone family
- Dark slate grey: `#252B33`
- Deep blue-grey: `#2F3742`
- Highlight stone edge: `#434C58`

### Metal family
- Iron: `#686A6F`
- Aged bronze: `#8A6A3F`

### Gameplay accent family
- Danger red: `#B33A33`
- Danger orange: `#D77A2E`
- Magic blue: `#4C6FC4`
- Magic purple: `#6E54B8`
- Goal gold: `#C7A04A`
- Goal green glow: `#65B971`

### Material style rules
- Flat/semi-flat roughness response (no glossy modern look).
- Subtle tiling only; avoid high-frequency details.
- Let value contrast, silhouette, and color coding do most readability work.

---

## 4) Environment Prop Ideas (Sparse Storytelling)
Use low-count placement to prevent visual clutter:
- skull clusters near old kill-zones
- broken pillar bases in corners
- chain hangers on wall spans
- rune etchings near teleport/magic logic points
- debris strips near wall edges
- soot/burn decals around fire or explosive traps
- scratch decals near saw trap lanes
- simple torch holders and iron brackets

Placement rules:
- Never block traversal or trap telegraph readability.
- Keep center gameplay lanes mostly clean.
- Concentrate props at boundaries and room transitions.

---

## 5) Retro VFX Suggestions (Lightweight)
### Keep effects simple and cheap
- **Dust motes:** low-rate particles in large spaces.
- **Torch sparks:** tiny intermittent ember bursts.
- **Trap sparks:** brief bursts for metal trap activation.
- **Goal glow pulse:** slow emissive pulse + optional small halo sprite.
- **Magic teleport swirl:** low-particle circular drift with blue/purple tint.

### VFX limits
- Low particle counts.
- Minimal overdraw.
- No long-lived smoke blankets over gameplay lanes.

---

## 6) Retro Camera Configuration
### Gameplay camera baseline
- Isometric yaw: `45`
- Pitch: `40-44` (slightly higher than modern action framing)
- Distance: fixed/near-fixed for stable readability
- Zoom: minimal (small clamp window only)

### Behavior rules
- Favor stability over cinematic motion.
- Keep arena and trap relationships legible at all times.
- Avoid dramatic rotation during normal simulation.

### Optional event polish
- Small goal-reached zoom-in (short duration, low amplitude).
- Return quickly to baseline framing.

---

## 7) UI Visual Theme Instructions (Retro Fantasy Admin)
### Surface treatment
- Dark parchment backgrounds for panels.
- Stone frame motifs for major windows.
- Bronze borders and engraved corner ornaments.

### Highlight language
- Rune glow accents for selected controls.
- Gold/green success accents for objective states.
- Red/orange warnings for danger, fail, or critical states.

### Typography + readability
- Prioritize high-contrast text over decorative excess.
- Keep icon silhouettes simple and old-RPG-like.
- Avoid modern ultra-flat UI treatment and bright gradients.

---

## 8) Internal Style Guide Snapshot (for Future Consistency)
### Lighting rules
1. Warm local torches + cool global fill.
2. Maintain deep shadows without obscuring gameplay-critical cues.
3. Always preserve clear trap telegraphs.

### Color rules
1. Stone + metal are neutral base.
2. Danger = red/orange only.
3. Magic = blue/purple only.
4. Goal = gold + green glow.

### Material rules
1. Semi-flat, low-noise surfaces.
2. Restrained texture contrast.
3. Strong silhouettes over micro-detail.

### UI frame rules
1. Stone/parchment foundations.
2. Bronze trim + subtle rune highlights.
3. State colors must remain consistent everywhere.

### Trap silhouette rules
1. Distinct shape language per trap type.
2. Distinct accent color where possible.
3. Trigger/readiness cues visible in <1 second.

---

## 9) Acceptance Checklist (Quality Bar)
- Dungeon reads as nostalgic dark fantasy at first glance.
- Lighting is dramatic but gameplay-readable.
- Palette is disciplined and consistent.
- Props add history without clutter.
- VFX support mood without visual noise.
- Camera remains clear and stable.
- UI feels like a dungeon-era fantasy interface.

