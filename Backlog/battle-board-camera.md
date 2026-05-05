# Battle System — Board & Camera

See [global.md](global.md) for the epic summary.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 190 | 3 | **[BC-01] Extract voxel slice around player in `BoardDataBuilder`** | Sample a rectangular region of the `VoxelGrid` centered on the player's world position. Region size comes from encounter data (board dimensions). Output: flat list of `VoxelCell` with local board coordinates. |
| 190 | 2 | **[BC-02] Build `BoardData` from voxel slice** | Feed the slice into `BoardDataBuilder` to populate `BoardTerrainLayer` and `BoardNavigationLayer` (walkable graph, A\* nodes). Reuses the existing `VoxelTraversalGraph` builder. |
| 190 | 2 | **[BC-03] Wire `BoardDataBuilder` output into `BattleMode.Enter`** | `BattleMode.Enter(EncounterData)` must call the builder, await the result, then pass the finished `BoardData` to `BattleOrchestrator.Initialize`. Gate battle start on builder completion. |
| 175 | 2 | **[BC-04] `BattleCameraRig` MonoBehaviour — fixed isometric view** | New script. On `Activate`, positions and orients the camera to an isometric-ish fixed angle above the board. Camera is not the player-following exploration camera; it is a separate `Camera` component on a dedicated GameObject. |
| 175 | 2 | **[BC-05] Auto-frame the board bounds on `BattleCameraRig` setup** | Reads `BoardData` bounds (min/max corners) and adjusts camera position / orthographic size or FOV so the full board fits within the viewport with padding. |
| 120 | 3 | **[BC-06] `CameraTransitionService` — blend between exploration and battle cameras** | On `BattleStarted`: disable exploration camera, enable battle camera with a short crossfade (fade to black → switch → fade in). On `BattleEnded`: reverse. Duration configurable. |
| 120 | 1 | **[BC-07] Trigger transition on `BattleStarted` / `BattleEnded` events** | `CameraTransitionService` subscribes to `EventCenter.BattleStarted` and `EventCenter.BattleEnded`. No direct call from mode controllers — decouple via events. |
| 65 | 1 | **[BC-08] `CameraShakeEffect` coroutine on `BattleCameraRig`** | Short (~0.25 s) positional shake. Amplitude and duration configurable. Uses a sine-based or random offset applied to the camera's local position, reset cleanly on completion. |
| 65 | 1 | **[BC-09] Subscribe `BattleCameraRig` shake to unit hit events** | `BattleCameraRig` subscribes to `EventCenter.UnitHit` (or the damage resolution event). On receipt, call the shake coroutine if the hit unit is on the board currently being viewed. |
