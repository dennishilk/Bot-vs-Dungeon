# Persistence, Share Codes, and Replay Viewer Setup (Unity 6 + C#)

## What was added

### 1) Serializable save data classes
- `DungeonSaveData`, `PlacedObjectData`, and `SerializableVector2Int` in `Assets/Scripts/Data/DungeonSaveData.cs`.
- Replay data classes `ReplayFrameData`, `RunReplayData`, `CertificationReplayData` in `Assets/Scripts/Data/ReplayData.cs`.

### 2) JSON save/load logic
- `DungeonSerializer` captures runtime arena tile data to `DungeonSaveData` and applies it back to the scene.
- Uses `JsonUtility` for deterministic JSON serialization/deserialization.

### 3) Local file storage logic
- `DungeonSaveManager` writes one JSON file per save under:
  - `Application.persistentDataPath/DungeonSaves`
- Supports save, load, overwrite, delete, and list.

### 4) Share code export/import system
- `ShareCodeManager` exports current dungeon to:
  - JSON -> GZip -> Base64
  - prefixed with `BVD1:` for versioning
- Imports by validating prefix, decoding payload, parsing JSON, then applying layout.

### 5) Replay recording data system
- `ReplayRecorder` subscribes to `SimulationManager` run lifecycle events.
- Captures frame data at a configurable interval.
- Stores a bounded history of runs and grouped certification sessions.

### 6) Replay playback logic
- `ReplayViewer` can:
  - select replay
  - play/pause/stop
  - step forward
  - adjust speed
- Uses a separate replay bot and optional line/death marker visualization.

### 7) UI panel setup instructions

Create three panels in Canvas:

#### SaveLoadPanel
- Components:
  - `TMP_InputField` for save name
  - `TMP_Dropdown` for save list
  - `TMP_Text` for status
  - Buttons: Save, Load, Delete, Overwrite, Refresh
- Add `SaveLoadPanel` script to panel root.
- Wire:
  - Save -> `SaveLoadPanel.SaveClicked`
  - Load -> `SaveLoadPanel.LoadClicked`
  - Delete -> `SaveLoadPanel.DeleteClicked`
  - Overwrite -> `SaveLoadPanel.OverwriteClicked`
  - Refresh -> `SaveLoadPanel.RefreshSaveList`

#### SharePanel
- Components:
  - multiline `TMP_InputField` for share code
  - `TMP_Text` for status
  - Buttons: Export Code, Import Code, Copy, Paste
- Add `SharePanelController` script and wire button methods accordingly.

#### ReplayViewerPanel
- Components:
  - `TMP_Dropdown` replay list
  - `TMP_Text` details/status
  - `Slider` replay speed
  - Buttons: Refresh, Select Replay, Play, Pause, Stop, Step Forward
- Add `ReplayPanelController` script and wire methods.

Also add top-level menu buttons for opening/closing each panel by calling `SetVisible(bool)` on each panel controller.

### 8) Inspector setup instructions

#### DungeonSerializer
- Assign `ArenaManager`, `TrapBudgetManager`
- Populate `tilePrefabs` for each `TileType` (Floor, Wall, Pit, Saw, Bomb, Archer, Start, Goal)

#### DungeonSaveManager
- Assign `DungeonSerializer`, `SimulationManager`
- Configure `saveFolderName` (default `DungeonSaves`)

#### ShareCodeManager
- Assign `DungeonSerializer`
- Configure version prefix (default `BVD1:`)

#### ReplayRecorder
- Assign `SimulationManager`
- Configure:
  - `recordFrameInterval`
  - `maxReplayHistoryCount`

#### ReplayViewer
- Assign:
  - `ReplayRecorder`
  - `replayBotPrefab`
  - `replayRoot`
  - optional `LineRenderer`
  - optional death marker prefab
- Configure `defaultReplaySpeed`

#### CertificationManager
- Assign `ReplayRecorder` to new field so certification sessions get grouped replay data.

### 9) Example save file format

```json
{
  "version": "BVD_SAVE_V1",
  "saveName": "bomb_chamber_01",
  "width": 12,
  "height": 10,
  "startPosition": { "x": 1, "y": 1 },
  "goalPosition": { "x": 10, "y": 8 },
  "trapBudgetUsed": 9,
  "createdUnixTime": 1731000000,
  "placedObjects": [
    { "objectType": 7, "gridPosition": { "x": 1, "y": 1 }, "rotationY": 0.0 },
    { "objectType": 8, "gridPosition": { "x": 10, "y": 8 }, "rotationY": 0.0 },
    { "objectType": 5, "gridPosition": { "x": 5, "y": 6 }, "rotationY": 90.0 }
  ]
}
```

### 10) Example share code workflow
1. Build your dungeon in Build Mode.
2. Open Share panel.
3. Click **Export Code**.
4. Click **Copy** and send code to another player.
5. Receiver opens Share panel, pastes code, and clicks **Import Code**.
6. Imported layout is applied into the current scene (existing layout replaced).

---

## Error handling included
- invalid/empty share code
- unsupported share code version prefix
- corrupted JSON/share payload
- missing save file
- invalid runtime state references during load/apply

## Notes
- This implementation stays local-only (no backend/cloud/multiplayer).
- Existing architecture is extended by adding manager/controller scripts and lifecycle hooks rather than replacing systems.
