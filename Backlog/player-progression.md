# Player Progression & Meta

See [global.md](global.md) for the epic summary.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 195 | 2 | **[PP-01] JSON serialization for `GameSaveData` to disk** | `JsonUtility` or `Newtonsoft.Json` serialization of `GameSaveData` to `Application.persistentDataPath/save.json`. Include error handling for corrupt files. |
| 195 | 2 | **[PP-02] Load `GameSaveData` from disk on startup in `GameBootstrapper`** | On launch, attempt to read and deserialize the save file. If absent or corrupt, call `GameInitializationService.TryInitializeNewGameSave()`. Set loaded data on `GameContext`. |
| 195 | 1 | **[PP-03] Auto-save on mode transitions** | Serialize and write `GameSaveData` to disk in `ExplorationMode.Enter` (after returning from battle) and when the player uses a heal point. Avoids requiring an explicit save action. |
| 135 | 1 | **[PP-04] Add `HashSet<string> DefeatedGyms` to `GameSaveData`** | String key = gym ID or asset GUID. Persists across sessions. `EndPhaseController` writes to this set after a gym battle victory. |
| 135 | 2 | **[PP-05] Wire gym defeat count to `EncounterTier` scaling in `EncounterResolver`** | `EncounterResolver.Resolve` reads `GameSaveData.DefeatedGyms.Count` and selects the matching `EncounterTier` index (0–8). Clamp to max tier if more than 8 gyms cleared. |
| 125 | 1 | **[PP-06] `TeamManagementScreen` MonoBehaviour — open / close** | Panel activated from the exploration HUD. Blocked during battle. Shows the active 6-slot team on the left and the PC box grid on the right. |
| 125 | 3 | **[PP-07] Drag-and-drop creature slot reordering in `TeamManagementScreen`** | Player can drag a team slot card onto another to swap positions. Implement via `IBeginDragHandler` / `IDropHandler`. Update `PlayerData.Team` order on drop. |
| 125 | 2 | **[PP-08] Swap creature between team and PC box** | Dragging a team slot card onto a PC box slot (or vice versa) swaps the creatures. Calls `PlayerData.TryAddCreature` / removal logic. Prevents swapping if the team would drop below 1 creature. |
| 115 | 2 | **[PP-09] `PCBoxView` — scrollable grid of creature slots** | Grid layout with `GameRule.MaxPCSlots` cells. Each cell shows portrait + name if occupied, or an empty state. Instantiated from a slot prefab. Scrollable via `ScrollRect`. |
| 115 | 1 | **[PP-10] Bind `PCBoxView` to `PlayerData.PCBox` observable list** | Subscribe to `PlayerData.PCBox` change events (add / remove). On change, rebind the affected slot cells. Do not rebuild the full grid on every update. |
| 105 | 3 | **[PP-11] Author Gym 1–4 `EncounterTable` assets (8 tiers each)** | Each gym gets one `EncounterTable` SO with 8 `EncounterTier` rows. Minimum 2–3 team compositions per tier. Balance is rough prototype tuning. |
| 105 | 3 | **[PP-12] Author Gym 5–8 `EncounterTable` assets (8 tiers each)** | Same structure as PP-11. Higher base stats in team compositions to reflect progression. |
| 105 | 2 | **[PP-13] Author Elite Four `EncounterTable` assets** | Four tables (one per Elite Four member), each with a single tier (post-game difficulty). |
| 75 | 1 | **[PP-14] Add `HashSet<string> DefeatedTrainers` to `GameSaveData`** | String key = trainer ID. Written by `EndPhaseController` after a trainer battle victory. Persists across sessions. |
| 75 | 1 | **[PP-15] Write defeated trainer ID to `GameSaveData` in `EndPhaseController`** | After a non-wild, non-gym battle victory, look up the active trainer's ID (from encounter data) and add it to `DefeatedTrainers`. |
| 75 | 1 | **[PP-16] `TrainerActor` skips sight check if already defeated** | At the start of `TrainerActor.Tick`, check `GameSaveData.DefeatedTrainers.Contains(trainerID)`. If true, return early without performing the sight-line evaluation. |
