# Technical / Infrastructure

See [global.md](global.md) for the epic summary.

| Priority | Pts | Ticket | Notes |
|----------|-----|--------|-------|
| 195 | 2 | **[TI-01] Validate full startup flow end-to-end** | Trace: launch → `GameBootstrapper.Start` → `GameInitializationService` → load or create save → generate/load world → spawn player at saved position. Fix any broken links. Document the verified call chain. |
| 195 | 2 | **[TI-02] New game initialization — world seed + initial `GameSaveData`** | On first launch (no save file), generate a random world seed, write it to `GameSaveData`, call `MetaWorldGenerator` with that seed, and place the player at a starting position near a town. |
| 140 | 2 | **[TI-03] Audit `EventCenter` subscriptions for double-subscribe bugs** | Grep all `EventCenter` subscribe calls. For each, verify the unsubscribe call exists in the corresponding `Exit` / `OnDisable` / `OnDestroy`. Log or assert on any subscription that would survive a mode switch. |
| 140 | 1 | **[TI-04] Add subscription guards on `Mode.Enter` / `Mode.Exit`** | `ExplorationMode` and `BattleMode` must subscribe all `EventCenter` listeners in `Enter` and unsubscribe all in `Exit`. Add a base-class helper or checklist comment to enforce this pattern for future modes. |
| 130 | 2 | **[TI-05] Replace `UnityEngine.Random` with seeded `System.Random` in `MetaWorldGenerator`** | Grep `MetaWorldGenerator` (and any called utilities) for `UnityEngine.Random`. Replace every call with the generator's own `System.Random` instance, seeded from `GameSaveData.WorldSeed`. Ensures reproducible world generation. |
| 130 | 1 | **[TI-06] Store world seed in `GameSaveData` and restore on load** | `GameSaveData.WorldSeed` (int or long). Written once on new game (TI-02), read back on load and passed to `MetaWorldGenerator`. Never regenerated for an existing save. |
| 90 | 1 | **[TI-07] `DebugConsole` MonoBehaviour — toggle with key** | Toggled by a configurable key (default: backtick). Shows an input field when open. Stays on top of all UI via a high sort-order `Canvas`. Only compiled in debug / editor builds (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`). |
| 90 | 3 | **[TI-08] Debug commands: force encounter, complete feat node, warp** | Three console commands: `encounter [tableName]` — emit `BattleStartRequested` with a named encounter table. `feat [creatureIndex] [nodeId]` — mark a `FeatNode` as completed on the given creature. `warp [x] [z]` — teleport player to world coordinates. |
| 70 | 1 | **[TI-09] Profile `VoxelMesher` in a high chunk-count scenario** | Use Unity's Profiler to measure `VoxelMesher` time per frame when 20+ chunks are loaded. Record the baseline spike duration and memory footprint. Decision point: if spikes exceed 16 ms, proceed to TI-10. |
| 70 | 2 | **[TI-10] Offload `VoxelMesher` geometry generation to async** | If TI-09 reveals unacceptable spikes, move mesh data computation (vertex/index arrays) to a `Task.Run` thread. Marshal the resulting `Mesh` creation back to the main thread via a queue polled in `Update`. |
