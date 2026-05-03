# Erelia — Game Backlog

Priority levels:
- **Prioritized** — Required to test the game at all (no correct graphics needed, purely functional systems).
- **Major** — Very important for the game to be complete, but not strictly blocking a first functional test run.
- **Minor** — Polish, visual quality, or quality-of-life; not required to validate core systems.

---

## Battle System — Backend

| Priority | Item | Notes |
|---|---|---|
| **Prioritized** | Stun mechanic on Turn Bar | Stun pauses bar fill from its current value. `BattleTurnRules.AdvanceTurnBars` (called by `IdlePhase.Tick`) needs to skip advancement for stunned units. |
| **Prioritized** | Capture mechanic | Wild encounters: attempt after HP falls below threshold, costs the acting creature's full turn, has a % success chance. Nothing implemented yet. |
| **Major** | Status hook feat events | Status effects applied via `BattleStatusRules.ApplyHook` (e.g. "take damage while shielded", "stun an enemy") can also generate `FeatRequirement.EventBase` events for the affected unit. These are not yet emitted by the hook system; they would complement the effect-level events already wired. |
| **Major** | Status ScriptableObject assets authored | `Status` is a ScriptableObject; the hook logic lives there. The code infrastructure is ready — the missing part is authoring the actual status assets (poison, burn, shield, DoT, HoT, stun, etc.) as project assets. |
| **Major** | AI turn evaluation | `AIBehaviour` / `AIRule` / `AICondition` / `AIDecision` are data classes. `EnemyTurnPhaseController` needs to evaluate the AI rule list top-down and submit a legal `BattleAction`. |
| **Major** | Enemy placement strategy | `TryAutoPlaceUnitsRandomly` is the only placement strategy. The GDD describes fixed / by-line alternatives; a strategy interface would allow per-encounter authoring. |

---

## Battle System — Controllers & HUD

| Priority | Item | Notes |
|---|---|---|
| **Major** | Creature info floating window | `CreatureTeamView` creature cards act as buttons. Clicking one opens a floating, non-modal, draggable window showing the creature's stats, HP/AP/MP bars, and other details. The window must be closable. |
| **Major** | End-of-battle quest progression summary | After `EndPhase`, show which feat nodes progressed or completed during the fight. |
| **Major** | Battle result screen | No victory / defeat screen exists. The player needs clear feedback (win/loss, surviving creatures) before returning to exploration. |
| **Minor** | World-space health bar above creature models | A small health bar floating above each unit's model in the 3D view, complementing the team panel. |
| **Minor** | Damage / heal floating text | Numeric popup feedback on hits and heals. |
| **Minor** | Unit defeat animation trigger | `BattleContext.DefeatUnit` is called but no signal reaches the view layer to play the Death animation. |

---

## Battle System — Board & Camera

| Priority | Item | Notes |
|---|---|---|
| **Prioritized** | Battle camera rig | Exploration camera orbits the player actor. Battle needs a separate fixed/isometric-ish view above the board. No battle camera script exists. |
| **Prioritized** | Board generation from world voxels | `BoardDataBuilder` exists but the pipeline extracting a voxel slice around the player and building `BoardData` from it needs to be completed and connected to the battle-entry flow. |
| **Major** | Camera transition exploration ↔ battle | Smooth blend/cut between the two camera rigs when entering or exiting a fight. |
| **Minor** | Camera shake on impact | Short shake coroutine tied to unit hit events. |

---

## Exploration & World

| Priority | Item | Notes |
|---|---|---|
| **Prioritized** | Battle ↔ Exploration transition wiring | `BattleMode` and `ExplorationMode` both exist. The pipeline (encounter detected → load board → hand off to `BattleMode`) is not connected. |
| **Prioritized** | Encounter emitter wired to player movement | `EncounterEmitter` exists but is not subscribed to `EventCenter.EmitPlayerMoved`. Walking in long grass must trigger the encounter roll. |
| **Prioritized** | Trainer line-of-sight trigger | Trainers should start a battle when the player enters their sight line. No trainer actor or sight-line check exists. |
| **Major** | Procedural biome-based world generation | `MetaWorldGenerator.GenerateChunkMeta` returns `defaultBiome` for every chunk. Perlin/noise-based biome assignment per chunk needs to be implemented. |
| **Major** | Biome → encounter table selection | `BiomeDefinition` has `BiomeEncounterRule`; the connection biome → table → encounter roll needs to be verified and completed in `EncounterResolver`. |
| **Major** | Town / Gym procedural placement | `MetaWorldData` exists but the algorithm that places towns, gyms, and POIs first then connects them via a Voronoi-style road network is not yet present. |
| **Major** | Interior teleport system | Buildings should teleport the player to an interior space. No `InteriorTeleport` or building-entrance mechanic exists. |
| **Major** | Heal point / respawn mechanic | GDD: losing a battle returns the player to the last visited heal point. No heal point entity or respawn flow exists. |
| **Major** | Chunk streaming / unloading | `WorldPresenter` loads chunks but there is no distance-based unload to keep memory bounded during long exploration sessions. |
| **Minor** | Road / path rendering between towns | Roads should be visually present connecting generated locations. |
| **Minor** | Ability-gate structures | Structures unlocked by creature abilities (Cut, Rock Smash, etc.) that hide items or secret areas. |
| **Minor** | Weather / time of day visuals | GDD says no encounter variation by time/weather, so this is purely cosmetic. |

---

## Creature & Feat System

| Priority | Item | Notes |
|---|---|---|
| **Prioritized** | At least one authored `CreatureSpecies` + `FeatBoard` | Without real creature data, nothing playable exists. Minimum: one species with base stats, 2–3 abilities, and a small feat board. |
| **Major** | Feat Board runtime UI | A visual progression tree the player can open to see and interact with a creature's nodes. `FeatBoardEditorWindow` is editor-only; a runtime in-game equivalent is needed. |
| **Minor** | Feat Board respec | GDD mentions it exists but is not a main feature. |

---

## Player Progression & Meta

| Priority | Item | Notes |
|---|---|---|
| **Prioritized** | Save / Load system | `GameSaveData` is the data class. Serialization to disk (JSON or binary) and loading on startup needs to be wired in `GameBootstrapper` / `GameInitializationService`. |
| **Major** | PC storage (creature box) | GDD: active team = 6; extras go to PC box. `PlayerData` likely only holds the team; PC box storage and the swap UI need to be added. |
| **Major** | Gym defeat tracking + encounter tier scaling | `GameSaveData` needs a cleared-gym set; this count feeds `EncounterTier` scaling (0–8 tiers defined by gyms beaten) per the GDD. |
| **Major** | Team management screen | UI to swap creatures between active team and PC box between encounters. |
| **Major** | Gym / Elite Four encounter definitions | Authored `EncounterTable` assets for 8 gyms × 8 tiers + Elite Four. |
| **Minor** | Trainer defeat tracking | Cleared trainers should not re-trigger. `GameSaveData` needs a defeated-trainer set. |

---

## View / Animation System (per modelAnimationProposition.md)

| Priority | Item | Notes |
|---|---|---|
| **Major** | `View.Animation.LogicalPart` enum | Abstract bone-naming layer (root, body, head, front, rear, dominant limb, off limb, weapon, jaw, tail, whole rig). Makes recipes reusable across body types. |
| **Major** | `View.Animation.Rig` MonoBehaviour | Maps `LogicalPart` → concrete `Transform` on the prefab; captures rest pose so recipes can return cleanly. |
| **Major** | `View.Animation.Animator` MonoBehaviour | Executes `Recipe` phases on the rig: one main channel + one optional additive overlay channel. |
| **Major** | `View.Animation.Recipe` ScriptableObject | Sequential phase container; phases run one after another, steps within a phase run in parallel. |
| **Major** | `View.Animation.Set` ScriptableObject | Named dictionary of recipes assigned to a `CreatureForm`; mandatory names: `Idle`, `TakeDamage`, `Death`. |
| **Major** | `View.Animation.Phase` + Step class hierarchy | Concrete steps: `MoveLocalStep`, `RotateLocalStep`, `ScaleStep`, `ShakeStep`, `FlashStep`, `WaitStep`, `SpawnVfxStep`, `PlaySoundStep`. |
| **Major** | Board movement tween for units | Separate from the recipe animator; smoothly moves the creature view tile-to-tile when a `MoveAction` resolves. |
| **Major** | Ability caster animation hook | `Ability` definition should reference an animation name from the caster's `AnimationSet`; the resolution phase triggers it before applying effects. |
| **Minor** | Custom Step inspector | `ManagedReferenceTypePicker` exists; wire it to expose only concrete step subtypes in the Phase inspector. |
| **Minor** | Hit flash overlay channel | Additive overlay for hit-react / charge-up visual cues. |
| **Minor** | VFX spawn step implementation | `SpawnVfxStep` spawning a particle prefab at a `LogicalPart` anchor. |
| **Minor** | Sound step implementation | `PlaySoundStep` playing an `AudioClip` during a recipe phase. |

---

## Audio

| Priority | Item | Notes |
|---|---|---|
| **Major** | Audio manager / service | No audio system exists. Need at minimum a simple singleton for playing SFX and background music. |
| **Major** | Battle music | Background track that starts on battle entry and stops on exit. |
| **Minor** | Ability SFX | Per-ability sounds triggered by `PlaySoundStep`. |
| **Minor** | Footstep SFX | Exploration walking sounds. |
| **Minor** | UI SFX | Button click / menu navigation sounds. |

---

## Content

| Priority | Item | Notes |
|---|---|---|
| **Major** | First creature roster | Minimum 3–5 fully specced creatures (stats, abilities, feat board) to allow meaningful test play. |
| **Major** | Ability definitions | Authored `Ability` ScriptableObjects: at least direct damage (physical + magical), a DoT, a heal, a movement debuff, and a buff. |
| **Major** | Voxel tileset | Minimum set of `VoxelDefinition` assets (ground, wall, slope, water, grass) to make the world visually readable. |
| **Minor** | Creature 3D models / voxel prefabs | Prefabs with `View.Animation.Rig`; placeholder cubes are fine for Prioritized testing. |
| **Minor** | Town structures / building voxel templates | Pre-built voxel cell templates for gyms, houses, and POIs. |

---

## Technical / Infrastructure

| Priority | Item | Notes |
|---|---|---|
| **Prioritized** | Bootstrap / scene wiring end-to-end | `GameBootstrapper` and `GameInitializationService` exist; the full startup flow (load save → enter world → spawn player) needs end-to-end validation. |
| **Major** | Random seed determinism | All random calls in `MetaWorldGenerator` and biome/encounter placement must use a seeded `System.Random`, not `UnityEngine.Random`, so the world is reproducible from the same seed. |
| **Major** | Event system completeness | `EventCenter` emits player movement events; verify that encounter emitters, chunk streaming, and trainer sight-lines all subscribe correctly and do not double-subscribe. |
| **Minor** | Debug / cheat console | In-game toggle for forcing encounters, completing feats, warping to locations — accelerates iteration. |
| **Minor** | Voxel mesher performance profiling | `VoxelMesher` generates geometry synchronously; high chunk counts may cause frame spikes that need profiling and possible async offload. |

---

## Confirmed Working

- `BattleContext` owns runtime state, unit collections, board access, placement style, and turn context.
- `TurnContext` models active unit + one pending action cleanly.
- Full phase pipeline wired through `BattleOrchestrator`: Setup → Placement → Idle → PlayerTurn/EnemyTurn → Resolution → End.
- `IdlePhase.Tick` calls `BattleTurnRules.AdvanceTurnBars` and `TrySelectNextReadyUnit`; the stamina time-advancement loop is implemented.
- `IdlePhaseController` transitions correctly to the player/enemy turn phase when a unit is ready.
- `PlacementPhaseController` and placement view are functional.
- `PlayerTurnPhase` exposes thin-controller APIs: `CanMoveTo`, `GetReachableCells`, `TryGetPathTo`, `CanUseAbility`, `GetCastLegality`, `GetValidTargets`, `GetValidTargetCells`, `GetAffectedCells`, `GetAffectedObjects`, `CanTarget`, `CanTargetCell`, `CanCastAtCell`, `CanEndTurn`, `TrySubmitMove`, `TrySubmitAbility`, `TrySubmitEndTurn`.
- `EnemyTurnPhase` chooses a legal action automatically.
- `BattleActionValidator` owns movement legality, pathfinding, cast legality, target legality, end-turn legality.
- `BattleActionResolver` resolves move / ability / end-turn actions and updates stats.
- `BattleTargetingRules` separates `CanCastAtCell`, `GetAffectedCells`, `GetAffectedObjects`.
- `BattleLineOfSightRules` uses a 3D voxel traversal.
- `BattleTurnRules` owns readiness progression, turn begin/end, and deterministic tie-breaking.
- `BattleStatusRules` owns hook dispatch.
- `BattlePlacementRules` computes zones and supports enemy auto-placement.
- `EndPhase` emits `BattleOutcome` through `EventCenter.BattleEnded`.
- `BattleMaskRules` is a front-side bridge for pure cell queries; phases expose overlay-cell queries without mutating masks directly.
- `BoardOverlayState` owns board preview masks outside `BoardData` / `BoardTerrainLayer`; visual board mask mesh refreshes correctly in-game.
- `AbilityShortcutBarView` is bound to the active unit and wired to the player turn HUD.
- `ActionShortcutBarView` end-turn and move-mode are wired to `PlayerTurnPhase`.
- AoE preview overlay works as expected.
- `FeatReward` concrete subclasses exist: `BonusStatsReward`, `AbilityReward`, `PassiveReward`, `ChangeFormReward`.
- `FeatRequirement` concrete subclasses exist and are constructable via the feat board editor window. `CastAbilityCountRequirement` supports an optional `List<Ability>` filter (empty = any ability); `CastDifferentAbilitiesRequirement` was merged into it.
- Evolution branching / sibling-branch blocking logic is implemented in `FeatProgressionService`.
- `DamageTargetEffect` emits `DealDamageRequirement.Event`, `TakeDamageRequirement.Event`, and `SurviveHitRequirement.Event` (when target survives); `HealTargetEffect` emits `HealHealthRequirement.Event` and `HealTargetRequirement.Event`.
- `BattleUnit` accumulates `PendingFeatEvents` during battle via `RecordFeatEvent`; `BattleActionResolver` routes effect-returned events to the source unit.
- `EndPhase` applies accumulated events to `FeatProgressionService.RegisterEvent` for all player units, but only on player victory.
- EditMode test suite: 451 passing tests, split into focused namespaces per requirement type and scope.