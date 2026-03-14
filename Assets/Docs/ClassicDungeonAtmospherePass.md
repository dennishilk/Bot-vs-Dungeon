# Bot vs Dungeon — Classic Dungeon Atmosphere Pass (Unity 6)

This document is a production-ready atmosphere pass that **preserves existing gameplay systems** and focuses only on mood, presentation, audio, and polish.

---

## 1) Classic Dungeon Atmosphere Design Plan

### Pass goals
- Dark fantasy dungeon mood with warm practical torch light.
- Readable isometric gameplay first, atmosphere second.
- Stylized, low-cost presentation inspired by old-school dungeon/action RPG feel.

### Implementation phases
1. **Lighting + Fog foundation**
   - Set global fog and base directional/ambient balance.
   - Add torch prefab + flicker script and place along key walls/corners.
2. **Material language pass**
   - Standardize floor/walls/trim/pits to dark slate stone.
   - Set clear trap accents (danger warm colors / magic cool colors).
3. **Atmosphere VFX pass**
   - Add pit mist, rune pulses, subtle ambient dust.
   - Improve goal portal pulse + ambient magical particles.
4. **Audio pass**
   - Layer wind/drips/chains/hum ambience.
   - Add simple menu/gameplay/results music routing.
5. **Camera + transitions + UI skin polish**
   - Lock readable isometric framing.
   - Optional light camera shake for high-impact trap beats.
   - Use dark iron/bronze UI framing and subtle transition overlays.

---

## 2) Torch Lighting System Setup

### Torch prefab (`TorchPrefab`) composition
- Root: `TorchPrefab`
  - `TorchMesh` (simple bracket/sconce mesh).
  - `Flame` (particle or quad flame visual).
  - `TorchLight` (point light).
  - `TorchAudio` (optional low-volume fire loop).

### Torch light baseline
- Type: Point Light
- Color: warm orange (`#FFAD66` to `#FFC073`)
- Intensity: `0.75 - 1.2`
- Range: `4.0 - 5.25`
- Shadows: off by default (enable only in hero/menu spaces)

### Placement rules
- One torch every 5–8 floor tiles on major walls.
- Always place near corners, junctions, and decision points.
- Keep center play lanes uncluttered.

### Script hookup
- Attach `TorchFlicker` to torch root.
- Assign `targetLight` = `TorchLight`, `flameVisual` = `Flame` transform.

---

## 3) Fog and Mist Setup Instructions

### Global fog (recommended)
Use `RenderSettings`:
- Fog: enabled
- Fog mode: ExponentialSquared
- Fog color: deep cool gray-blue (`#1A1C22`)
- Fog density: `0.012 - 0.02`

### Local mist usage
- Place `PitMistController` prefabs in pits only (or large void-adjacent edges).
- Keep alpha low so trap readability stays intact.
- Avoid any full-screen volumetric effects.

---

## 4) Updated Material / Style Recommendations

### Material targets
- **Floor**: medium-dark worn slate; slight rough variation.
- **Walls**: darker than floor with edge trim to separate walkables.
- **Edge trim**: chipped stone + occasional iron edge pieces.
- **Pits**: near-black depth gradient with cracked rim.
- **Metal traps**: iron/bronze rough look, no clean sci-fi specular.
- **Magic runes/teleports**: cool blue-violet emission pulse.
- **Goal marker**: green/gold emission and soft bloom-like glow (if cheap).

### Suggested palette
- Stone: `#1E232A`, `#2A2F36`, `#343B44`
- Torch warmth: `#FFAD66`, `#FFC073`
- Danger accents: `#B83A2F`, `#E26B2F`
- Metal: `#6B6A63`, `#8C6F43`
- Arcane: `#4E6CD3`, `#7A55C3`
- Goal: `#67C97C`, `#D4B04E`

---

## 5) Environment Prop Recommendations

Use lightweight props at boundaries and non-walkable edges:
- wall torches
- short stone pillars
- chain hangers
- rubble piles
- cracked floor decals
- skull clusters
- rune markings near traps/goals

Rules:
- never block bot routes
- avoid placing bright props under UI overlays
- maintain clear silhouettes of traps and pressure plates

---

## 6) Ambient Audio Setup

### Layer design (subtle)
- Loop A: distant wind bed
- Loop B: stone drips
- Loop C: chain rattle occasional layer
- Loop D: low ominous dungeon hum

### Routing
- Add `AmbientAudioController` to a persistent scene object.
- Assign four ambient `AudioSource`s and one `musicSource`.
- Keep ambience around ~35-45% master relative to core SFX.

### Mix rule
- UI click/feedback and trap telegraph SFX must always read above ambience.

---

## 7) Music Logic / Track Usage Recommendations

Use simple scene-state music, no adaptive complexity:
- **Menu**: low-intensity dark fantasy pad/drone loop.
- **Gameplay**: slightly tenser but restrained ambient loop.
- **Results**: short stinger (success or somber fail tone).

With `AmbientAudioController`:
- call `SetMusicState(Menu)` for menu scenes
- call `SetMusicState(Gameplay)` on simulation start
- call `SetMusicState(Results)` on result reveal

---

## 8) Camera Polish Instructions

### Gameplay camera
- Attach `ClassicCameraController` to gameplay camera.
- Target arena center or active bot.
- Suggested values:
  - Yaw `45`
  - Pitch `38`
  - Distance `16`
  - Height `13`

### Menu camera
- Use `MenuCameraDrift` for idle motion.
- Keep amplitude low enough to avoid UI discomfort.

### Event impact
- Add `LightCameraShake` to gameplay camera.
- Trigger for bomb explosions or major trap chains only.

---

## 9) UI Skinning Recommendations

Apply a fantasy administration/bureau style:
- panel backgrounds: deep charcoal or muted parchment overlays
- panel borders: iron/bronze trim with subtle corner rune motifs
- button hierarchy:
  - primary: warm gold highlight
  - neutral: slate
  - destructive: muted red
- typography: readable high-contrast serif-like or fantasy-clean font

Per-screen direction:
- build toolbar: engraved slot framing, active rune highlight
- campaign/profile panels: ledger/parchment + metal frame
- reports/results: stamp/seal motif with clear win/fail contrast

---

## 10) Transition / Screen FX Recommendations

Keep transitions lightweight:
- fade-to-black baseline between scene loads
- optional rune-overlay alpha pulse for major panel opens
- warm flash on goal success
- subtle red vignette pulse on bot death

Implementation note:
- continue using `SceneTransitionController` with dark fades;
- add optional UI overlay images (rune/parchment) if desired.

---

## 11) Lightweight Helper Scripts Added

- `TorchFlicker.cs`: torch light intensity/range/color drift + flame sway.
- `PitMistController.cs`: local pit mist bob/alpha animation.
- `RunePulse.cs`: emissive pulse utility for runes/teleports.
- `ClassicCameraController.cs`: smooth isometric framing controller.
- `LightCameraShake.cs`: optional lightweight impact shake.
- `AmbientAudioController.cs`: ambient layers + menu/gameplay/results music switching.

These scripts are presentation-only and do not add gameplay logic.

---

## 12) Inspector Setup Instructions

### Torch prefab (`TorchFlicker`)
- `targetLight`: Torch point light
- `flameVisual`: flame child transform
- `minIntensity`: 0.75
- `maxIntensity`: 1.2
- `minRange`: 4
- `maxRange`: 5.25

### Pit mist (`PitMistController`)
- `mistParticles`: assigned pit mist particle system
- `bobAmplitude`: 0.03-0.05
- `alphaMin/alphaMax`: 0.2-0.45

### Rune pulse (`RunePulse`)
- `targetRenderer`: rune mesh renderer
- `emissionProperty`: `_EmissionColor`
- `pulseSpeed`: 1.2-1.8

### Camera (`ClassicCameraController`)
- `focusTarget`: arena center anchor or bot pivot
- `moveSmooth`: 6
- `rotateSmooth`: 9

### Camera shake (`LightCameraShake`)
- attach to gameplay camera root
- set default duration `0.15`, magnitude `0.08`
- assign in `BombTrap.cameraShake` if desired

### Ambient audio (`AmbientAudioController`)
- assign all ambience loops with `loop` clips
- assign menu/gameplay/result clips
- set `ambientMaster` ~0.45 and `musicVolume` ~0.35

---

## 13) Mini Style Guide (Visual + Audio Consistency)

### Visual mood principles
1. Warm practical lights in cool stone spaces.
2. Darkness frames danger, never hides gameplay.
3. Keep silhouettes clean from isometric distance.

### Material language
- stone = aged, matte, chipped
- metal = heavy iron/bronze, restrained highlights
- magic = soft emissive cool pulses, never neon overload

### Trap readability rules
- Each trap must have a unique silhouette and accent color.
- Trigger indicators should be visible in <1 second glance.
- Use accents on edges/faces visible from camera angle.

### Lighting rules
- Global cool fill + local warm torches.
- Avoid uniformly lit rooms.
- Keep brightness highest at objectives and active hazards.

### UI rules
- dark frames, metallic accents, high-contrast text
- avoid flat modern gradients and hyper-saturated fills
- preserve clear state color coding (danger/success/selection)

### Atmosphere audio rules
- ambience should be felt, not foregrounded
- musical intensity stays low in simulation
- stingers are short and informative, not cinematic

---

## Suggested Folder Extensions

```text
Assets/
  Art/
  Environment/
  Lighting/
  Audio/
    Ambience/
    Music/
  VFX/
  UI/
  Themes/
  Materials/
  Prefabs/
    Atmosphere/
```

Use these folders gradually as assets are authored/imported.
