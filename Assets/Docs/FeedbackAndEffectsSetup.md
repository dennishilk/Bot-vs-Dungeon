# Bot vs Dungeon - Audio/VFX Feedback Setup

## 1) Folder structure

- `Assets/Audio/SFX/` (drop all one-shot and loop clips here)
- `Assets/VFX/Particles/` (particle templates and materials)
- `Assets/Prefabs/VFX/` (particle prefabs)
- `Assets/Scripts/Audio/` (`AudioManager.cs`)
- `Assets/Scripts/VFX/` (`ExplosionEffect.cs`, `HitFlashEffect.cs`, `GoalFeedback.cs`)

## 2) Required lightweight particle prefabs

Create these prefabs in `Assets/Prefabs/VFX/`:

### ExplosionParticle
- Duration: `0.8`
- Looping: Off
- Start Lifetime: `0.35-0.55`
- Start Speed: `3-5`
- Start Size: `0.15-0.35`
- Max Particles: `40`
- Emission Burst: `20`
- Color over Lifetime: orange -> gray fade
- Add sub-emitter for smoke (`8` particles, low speed)

### HitParticle
- Duration: `0.25`
- Looping: Off
- Start Lifetime: `0.08-0.15`
- Start Speed: `1.2-2`
- Start Size: `0.05-0.12`
- Max Particles: `12`
- Emission Burst: `8`
- Shape: cone, small angle (`10`)

### GoalSparkle (optional third, used by goal feedback)
- Duration: `1.2`
- Looping: On
- Start Lifetime: `0.5-0.8`
- Start Speed: `0.15-0.4`
- Start Size: `0.03-0.08`
- Max Particles: `24`
- Emission Rate over Time: `8`

## 3) Sound trigger logic map

- Saw trap
  - loop starts on enable (`SawTrap` + `AudioManager.PlaySawLoop`)
- Bomb trap
  - arm cue before delay (`BombArm`)
  - explosion cue on detonation (`BombExplosion`)
- Archer trap
  - fire cue per shot (`ArcherFire`)
- Bot
  - hurt cue on damage (`BotHurt`)
  - death cue when hp reaches 0 (`BotDeath`)
  - success cue on goal (`BotSuccess`)
- UI
  - click cue on all toolbar/control buttons (`UIButtonClick`)
  - start cue when simulation begins (`SimulationStart`)
- Result
  - success/failure cues from simulation stop (`ResultVictory` / `ResultFail`)

## 4) Minimal scripts added

- `AudioManager.cs`
- `ExplosionEffect.cs`
- `HitFlashEffect.cs`
- `GoalFeedback.cs`
- `ArrowProjectile.cs`

## 5) Inspector setup instructions

1. Add `AudioManager` to a scene singleton object (e.g., `Managers`).
2. Create and assign 3 AudioSources on that object:
   - `uiSource` (2D)
   - `trapSource` (2D)
   - `resultSource` (2D)
3. Assign all clips to `AudioManager` serialized fields.
4. Saw trap prefab:
   - Ensure `AudioSource` exists with `Spatial Blend = 1`.
   - Assign optional `contactSpark` particle.
5. Bomb trap prefab:
   - Assign `ExplosionEffect` reference.
   - Assign `blinkRenderer` for pre-explosion blink.
6. Archer trap prefab:
   - Assign `arrowProjectilePrefab` and `muzzlePoint`.
   - Optional `archerVisual` for recoil.
7. Bot prefab:
   - Add `HitFlashEffect` and wire to `BotHealth.hitFlashEffect`.
8. Goal tile object:
   - Add `GoalFeedback` with optional light + sparkle particle.
9. Result banner root:
   - Add/assign `CanvasGroup` to `ResultBannerController`.

## 6) Example AudioManager usage

```csharp
AudioManager.Instance?.PlayUISound(SoundCue.UIButtonClick);
AudioManager.Instance?.PlayTrapSound(SoundCue.BombExplosion);
AudioManager.Instance?.PlayResultSound(SoundCue.ResultVictory);
```

## 7) Recommended audio volumes

- Saw loop: `0.45`
- Bomb arm: `0.6`
- Bomb explosion: `0.9`
- Archer fire: `0.65`
- Bot hurt: `0.55`
- Bot death: `0.75`
- Bot success: `0.75`
- UI click: `0.5`
- Simulation start: `0.7`
- Result fail/victory: `0.9`

## 8) Optional lighting feedback

- Explosion light:
  - Intensity `6`
  - Range `4`
  - Flash duration `0.12s`
- Goal light pulse:
  - Base intensity `1`
  - Pulse amplitude `0.25`
  - Pulse speed `2`
- Torch flicker:
  - Keep subtle (`±0.15` intensity range)
