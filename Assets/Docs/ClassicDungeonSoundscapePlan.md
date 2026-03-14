# Bot vs Dungeon — Classic Dungeon Soundscape Pass (Unity 6)

This pass expands the existing audio stack without changing gameplay systems.
All clips referenced below are placeholders/royalty-free descriptors.

## Audio Architecture

1. **Ambient Dungeon Layer** (`AmbientAudioController`)
   - low wind bed
   - deep cavern rumble
   - ancient mechanical hum
   - stone resonance loop
2. **Environment Detail Layer** (`AmbientAudioController` random scheduler)
   - water drips, chain movement, dust falls, stone cracks, metal creaks
   - randomized from world emitters at long intervals
3. **Trap Sound Layer** (`TrapAudioController`)
   - per-trap event map (idle/activate/arm/impact/etc.)
   - spatial one-shots + optional loop anchors
4. **Bot Reaction Layer** (`BotAudioController`)
   - spawn, hurt, death, success, fall
   - subtle personality-weighted loudness
5. **UI Layer** (`UIAudioController`)
   - metallic click, panel open/close, achievement/promotion, simulation start
6. **Rare Music Layer** (`AmbientAudioController`)
   - menu, results, promotion, special stress events
   - gameplay track intentionally low-intensity
7. **Announcer Layer** (`AnnouncerSystem`)
   - dungeon bureau subtitles with optional voice clips

## Sound Layer Definitions

### Ambient Dungeon Layer
- Always-on, very low-level looping bed.
- Priority: mood, not foreground.
- Suggested placeholder assets:
  - `amb_distant_wind_loop_01`
  - `amb_cavern_rumble_loop_01`
  - `amb_mechanical_drone_loop_01`
  - `amb_stone_resonance_loop_01`

### Environment Detail Layer
- Randomized one-shots with long delays.
- Uses 3D emitter positions to make the dungeon feel active.
- Suggested placeholders:
  - `env_water_drip_01..03`
  - `env_chain_creak_01..02`
  - `env_dust_fall_01`
  - `env_stone_crack_01..02`
  - `env_metal_creak_01..02`

### Trap Layer
- Every trap has a distinct sonic signature and readable timing.
- Short transients for telegraph clarity.

### Bot Layer
- Subtle feedback tied to bot lifecycle and damage moments.
- Avoids noisy overlap by keeping level restrained.

### UI Layer
- Classic RPG/Bureau style tactile sounds.
- Slight pitch variation keeps repeated clicks from sounding robotic.

### Music Layer
- Minimal atmospheric score.
- Gameplay track is intentionally sparse and quiet.

## Trap Sound Mapping

| Trap | Primary Cues | Notes |
|---|---|---|
| SawTrap | `IdleLoop` grinding + `Damage` contact grind | Persistent mechanical danger read |
| BombTrap | `Arm` ticking/arming + `Impact` explosion + `Secondary` debris | Distinct anticipation then payoff |
| ArcherTrap | `Activate` bow twang + `Flight` whoosh + `Impact` hit | Directional warning and impact read |
| SpikeTrap | `Activate` metal rise + `Impact` pierce | Fast, sharp, recognizable |
| FlameJetTrap | `Activate` ignition burst + `IdleLoop` burn | Heat hazard identity |
| TeleportTrap | `Activate` arcane pulse + `Secondary` swirl tail | Magical contrast vs mechanical set |
| PressurePlate | `Activate` stone/metal click | Mechanical trigger hint |
| FakeFloor | `Activate` crack + `Impact` collapse | Collapse warning and consequence |

## Ambient System Plan

- Keep ambient loops quiet (ambient-weighted under SFX/UI).
- Emit environment details at long random intervals to avoid fatigue.
- Use spatial 3D falloff for environmental one-shots.
- Keep stress tension as a separate optional layer for special modes.

## Music Usage Plan

- **Main Menu**: dark ambient pad/drone (moderate level)
- **Gameplay**: very low, minimal melodic content
- **Result Screen**: short restrained cue
- **Promotion Screen**: ceremonial minimal fanfare
- **Special Events**: stress tension overlays and stingers only when needed

## UI Sound Plan

- Button press: metallic click
- Panel open/close: short parchment/metal transitions
- Achievement unlocked: brighter but short confirmation tone
- Promotion earned: ceremonial bureau tone
- Result appear: neutral reveal cue

## Announcer System Plan

Supports both voice and subtitle-first operation:
- "Certification sequence started."
- "Bot fatality recorded."
- "Survivability threshold achieved."
- "Compliance rating approved."

If voice clips are missing, subtitles still display via fallback defaults.

## Audio Settings Integration

Settings menu should expose and persist:
- Master volume
- Music volume
- Ambience volume
- SFX volume
- UI volume

Persistence keys:
- `audio_master`
- `audio_music`
- `audio_ambient`
- `audio_sfx`
- `audio_ui`

## Implementation Notes

- No gameplay rules, AI behavior, or trap logic changed.
- This pass only changes presentation routing/mix behavior and documentation.
