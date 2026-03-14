# Bot vs Dungeon Visual Style Guide (Unity 6)

## 1) Visual Identity
- **Theme**: compact dark fantasy dungeon with readable isometric gameplay.
- **Mood**: moody + warm practical lights, but never too dark to parse trap states.
- **Readability rule**: every gameplay-critical object must read in a 1-second glance from isometric camera.

## 2) Color Palette
- **Stone base**: dark slate (`#2A2F36`, `#343B44`, `#1E232A`).
- **Metal accents**: iron/bronze (`#6B6A63`, `#8C6F43`).
- **Danger**: red/orange (`#B83A2F`, `#E26B2F`).
- **Build/player markers**: cyan/blue (`#4AA8D8`, `#68D5FF`).
- **Goal markers**: green/gold (`#67C97C`, `#D4B04E`).

## 3) Material Usage Rules
- Keep roughness simple and stylized; avoid realistic micro-detail.
- Floor should be medium-value stone for bot/path readability.
- Walls slightly darker than floor with trim edges to separate walkable space.
- Pit material should be near-black with broken rim to imply depth.
- Trap materials should use saturated accent strips for quick identification.

## 4) Trap Readability Rules
- **PitTrap**: black interior + fractured edge ring.
- **SawTrap**: bright metallic blade + red warning base.
- **BombTrap**: spherical dark body + blinking orange emissive notch.
- **ArcherTrap**: directional silhouette (barrel/head) + forward accent line.

## 5) UI Accent Rules
- Panel BG: desaturated charcoal with 80-90% alpha.
- Borders: bronze/iron 1-2 px equivalent.
- Primary buttons: cyan hover, warm-gold selected, red destructive.
- Report/result badges: parchment strip + stamp color by rating.

## 6) Lighting Principles
- Directional light at diagonal key angle (35-50° pitch, 30-50° yaw).
- Ambient: low-intensity cool fill to preserve silhouette separation.
- Warm local points near torches and focal props.
- Goal tile gets gentle pulse/glow (`GoalGlowPulse`).
- Keep contrast high on hazard silhouettes.

## 7) Environment Prefab Recommendations
- Modular floor variants: clean, cracked, rune-etched.
- Wall variants: straight, buttressed, chipped corner.
- Edge trim prefabs: lip stones and iron rail pieces around arena bounds.
- Pit frame prefab: debris rim + depth fog plane.
- Sparse props: torch, pillar, banner, chain cluster, skull pile.

## 8) Camera & Framing
- Isometric framing should keep objective lane centered.
- Reserve screen edges for UI; avoid bright props under HUD.
- Maintain trap and bot readability at all zoom states.
- Optional minimal impact shake via `CameraShakeLight` for bomb events.

## 9) Atmosphere / VFX (Lightweight)
- Subtle dust fields (`AmbientDustSpawner`) in large rooms.
- Torch emissive jitter (`TorchFlicker`) for life.
- Goal pulse (`GoalGlowPulse`) as objective reinforcement.
- Keep particle counts low and avoid dense full-screen fog.

## 10) Inspector Setup Checklist
- Set floor/wall materials to shared palette instances.
- Configure directional light color toward cool-neutral and lower saturation.
- Place warm point lights near torches (range-limited).
- On goal prefab: assign renderer to `GoalGlowPulse.targetRenderer`.
- On menu camera: attach `MenuCameraDrift`.
- On gameplay camera: optional `CameraShakeLight` for event hooks.
