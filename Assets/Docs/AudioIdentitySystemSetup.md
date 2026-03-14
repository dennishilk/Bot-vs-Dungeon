# Bot vs Dungeon — Audio Identity System (Unity 6)

This pass delivers a modular, lightweight audio architecture with independent layer control while preserving existing gameplay systems.

## 1) Complete Audio Architecture

### Layer stack
1. Ambient Dungeon Layer (continuous low-intensity loops)
2. Environmental Detail Layer (randomized spatial one-shots)
3. Trap Sound Layer (trap-specific signatures)
4. Bot Reaction Layer (spawn, hurt, death, success)
5. UI Interaction Layer (mechanical menu feedback)
6. Music Layer (minimal atmospheric score)

### Runtime components
- `AudioManager`: central routing, volume mix, dynamic events, compatibility bridge.
- `AmbientAudioController`: ambience loops, environmental detail scheduler, music fades.
- `TrapAudioController`: trap clip map, spatial one-shots, optional trap loops.
- `BotAudioController`: personality-aware bot cue volume and spatial playback.
- `UIAudioController`: non-spatial UI feedback routing.
- `AnnouncerSystem`: text + optional voice lines with cooldown.
- `AudioSettingsPanel`: slider integration for saved preferences.

## 2) AudioManager Implementation Plan

`AudioManager` responsibilities:
- volume ownership for Master/Music/Ambient/SFX/UI
- `PlayerPrefs` load/save for per-layer settings
- central methods:
  - `PlayTrap(...)`
  - `PlayBotEvent(...)`
  - `PlayUI(...)`
  - `PlayMusicTrack(...)`
  - `QueueAnnouncement(...)`
- dynamic audio events:
  - fatality streak sting
  - objective resonance
  - stress-test tension activation
- backwards compatibility with existing `SoundCue` calls

## 3) Ambient System Setup

### Ambient dungeon layer loops
- wind bed
- low cavern rumble
- ancient mechanical hum
- stone resonance

All are looped at low level and scaled by Ambient + Master sliders.

### Environmental detail layer
`AmbientAudioController` coroutine randomly triggers one-shots:
- water drip
- chain movement
- stone crack
- dust fall
- metal creak

Use emitter transforms to spatialize events around the dungeon.

## 4) Trap Sound Mapping

Recommended trap map in `TrapAudioController` inspector:
- SawTrap: `IdleLoop` (metal rotation), `Damage` (grind contact)
- BombTrap: `Arm` (arming click/rumble), `Impact` (blast), `Secondary` (debris)
- ArcherTrap: `Activate` (release), `Flight` (arrow whoosh), `Impact` (arrow hit)
- SpikeTrap: `Activate` (rise), `Impact` (pierce hit)
- FlameJetTrap: `Activate` (ignite), `IdleLoop` (flame burn)
- TeleportTrap: `Activate` (arcane pulse), `Secondary` (swirl)
- PressurePlate: `Activate` (mechanical click)
- FakeFloor: `Activate` (stone crack), `Impact` (collapse)

## 5) Bot Reaction Mapping

`BotAudioController` event map:
- Spawn
- Hurt
- Death
- Success
- Fall

Personality multipliers:
- Careful: quieter
- Balanced: neutral
- Reckless: louder
- Panic: slightly elevated

## 6) UI Feedback System

Use `UIAudioController` events:
- Button click
- Panel open
- Panel close
- Achievement unlocked
- Promotion earned
- Result appear
- Simulation start

Theme direction: metallic/mechanical bureau tones.

## 7) Announcer Event System

`AnnouncerSystem` supports:
- text lines in TMP label
- optional voice clip playback
- global cooldown between announcements

Configured event lines:
- Certification sequence initiated
- Bot fatality recorded
- Survivability threshold achieved
- Compliance rating approved

## 8) Audio Settings Integration

`AudioSettingsPanel` can be wired to settings menu sliders:
- Master Volume
- Music Volume
- Ambient Volume
- SFX Volume
- UI Volume

Volumes persist through `PlayerPrefs` keys:
- `audio_master`
- `audio_music`
- `audio_ambient`
- `audio_sfx`
- `audio_ui`

## 9) Folder Structure Recommendation

Create and keep this hierarchy for source assets:

```text
Assets/
  Audio/
    Ambience/
    Environment/
    Traps/
    Bots/
    UI/
    Music/
    Announcer/
```

Use naming like:
- `amb_dungeon_wind_loop_01`
- `env_chain_creak_02`
- `trap_bomb_explosion_01`
- `bot_hurt_reckless_01`
- `ui_panel_open_01`
- `mus_gameplay_drone_a`
- `ann_certification_initiated`

## 10) Inspector Setup Instructions

### AudioManager
- Assign all category controllers.
- Set default slider values (Master/Music/Ambient/SFX/UI).
- Assign dynamic event stings.
- Configure fatality window and threshold.

### AmbientAudioController
- Assign four ambient loop sources.
- Assign one environment one-shot source and emitter transforms.
- Set random interval range (~5 to 11 sec).
- Set music clips (menu/gameplay/results/fanfare/stress).
- Tune fade duration (~1.2 sec).

### TrapAudioController
- Assign one-shot source and loop prefab source.
- Fill trap map entries by trap type and event slot.
- Keep distances moderate for readability.

### BotAudioController
- Assign spatial one-shot source.
- Fill spawn/hurt/death/success/fall clips.
- Tune personality multipliers.

### UIAudioController
- Assign non-spatial source.
- Fill all UI clip fields.

### AnnouncerSystem
- Assign TMP text target.
- Optionally assign voice source.
- Set cooldown and text duration.
- Fill event lines + optional clips.

### AudioSettingsPanel
- Assign sliders.
- Bind slider `OnValueChanged` events to panel methods.
- Call `SyncFromAudioManager` when menu opens.

---

All additions are presentation-only and preserve existing gameplay logic.
