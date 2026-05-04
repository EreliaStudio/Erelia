# Erelia — Game Backlog

## Backlog Planning Fields

The backlog now uses a numeric gameplay-value priority.

### Priority

Priority is now a value from 0 to 200.

- 200 = absolutely mandatory for a playable end-to-end prototype.
- 150–199 = core loop / prototype-critical gameplay systems.
- 100–149 = important game-completeness systems.
- 50–99 = polish, feedback, iteration speed, and quality-of-life.
- 0–49 = optional, cosmetic, or low-impact improvements.

Higher values should be handled earlier.

### Pts

Empty by design. Use this column later for agile estimation once you start planning sprints or implementation batches.

---
## Battle System — Backend

| Priority	| Pts	| Item										| Notes |
| 200		| 		| Capture mechanic							| Wild encounters: attempt after HP falls below threshold, costs the acting creature's full turn, has a % success chance. Nothing implemented yet. |
| 185		| 		| Stun mechanic on Turn Bar					| Stun pauses bar fill from its current value. `BattleTurnRules.AdvanceTurnBars` called by `IdlePhase.Tick` needs to skip advancement for stunned units. |
| 155		| 		| AI turn evaluation						| `AIBehaviour` / `AIRule` / `AICondition` / `AIDecision` are data classes. `EnemyTurnPhaseController` needs to evaluate the AI rule list top-down and submit a legal `BattleAction`. |
| 140		| 		| Status ScriptableObject assets authored	| `Status` is a ScriptableObject; the hook logic lives there. The code infrastructure is ready — the missing part is authoring the actual status assets poison, burn, shield, DoT, HoT, stun, etc. as project assets. |
| 125		| 		| Enemy placement strategy					| `TryAutoPlaceUnitsRandomly` is the only placement strategy. The GDD describes fixed / by-line alternatives; a strategy interface would allow per-encounter authoring. 		|
| 115		| 		| Status hook feat events					| Status effects applied via `BattleStatusRules.ApplyHook`, such as take damage while shielded or stun an enemy, can also generate `FeatRequirement.EventBase` events for the affected unit. These are not yet emitted by the hook system; they would complement the effect-level events already wired. |

---

## Battle System — Controllers & HUD

| Priority	| Pts	| Item											| Notes |
| 145		| 		| Battle result screen							| No victory / defeat screen exists. The player needs clear feedback win/loss, surviving creatures before returning to exploration. |
| 130		| 		| End-of-battle quest progression summary		| After `EndPhase`, show which feat nodes progressed or completed during the fight. |
| 110		| 		| Creature info floating window					| `CreatureTeamView` creature cards act as buttons. Clicking one opens a floating, non-modal, draggable window showing the creature's stats, HP/AP/MP bars, and other details. The window must be closable. |
| 85		| 		| Damage / heal floating text					| Numeric popup feedback on hits and heals. |
| 80		| 		| World-space health bar above creature models	| A small health bar floating above each unit's model in the 3D view, complementing the team panel. |
| 70		| 		| Unit defeat animation trigger					| `BattleContext.DefeatUnit` is called but no signal reaches the view layer to play the Death animation. |

---

## Battle System — Board & Camera

| Priority	| Pts	| Item									| Notes |
| 190		| 		| Board generation from world voxels	| `BoardDataBuilder` exists but the pipeline extracting a voxel slice around the player and building `BoardData` from it needs to be completed and connected to the battle-entry flow. |
| 175		| 		| Battle camera rig						| Exploration camera orbits the player actor. Battle needs a separate fixed/isometric-ish view above the board. No battle camera script exists. |
| 120		| 		| Camera transition exploration ↔ battle| Smooth blend/cut between the two camera rigs when entering or exiting a fight. |
| 65		| 		| Camera shake on impact				| Short shake coroutine tied to unit hit events. |

---

## Exploration & World

| Priority	| Pts	| Item										| Notes |
| 200		| 		| Battle ↔ Exploration transition wiring	| `BattleMode` and `ExplorationMode` both exist. The pipeline encounter detected → load board → hand off to `BattleMode` is not connected. |
| 190		| 		| Encounter emitter wired to player movement| `EncounterEmitter` exists but is not subscribed to `EventCenter.EmitPlayerMoved`. Walking in long grass must trigger the encounter roll. |
| 170		| 		| Trainer line-of-sight trigger				| Trainers should start a battle when the player enters their sight line. No trainer actor or sight-line check exists. |
| 145		| 		| Biome → encounter table selection			| `BiomeDefinition` has `BiomeEncounterRule`; the connection biome → table → encounter roll needs to be verified and completed in `EncounterResolver`. |
| 135		| 		| Procedural biome-based world generation	| `MetaWorldGenerator.GenerateChunkMeta` returns `defaultBiome` for every chunk. Perlin/noise-based biome assignment per chunk needs to be implemented. |
| 125		| 		| Heal point / respawn mechanic				| GDD: losing a battle returns the player to the last visited heal point. No heal point entity or respawn flow exists. |
| 115		| 		| Chunk streaming / unloading				| `WorldPresenter` loads chunks but there is no distance-based unload to keep memory bounded during long exploration sessions. |
| 105		| 		| Town / Gym procedural placement			| `MetaWorldData` exists but the algorithm that places towns, gyms, and POIs first then connects them via a Voronoi-style road network is not yet present. |
| 95		| 		| Interior teleport system					| Buildings should teleport the player to an interior space. No `InteriorTeleport` or building-entrance mechanic exists. |
| 75		| 		| Road / path rendering between towns		| Roads should be visually present connecting generated locations. |
| 65		| 		| Ability-gate structures					| Structures unlocked by creature abilities Cut, Rock Smash, etc. that hide items or secret areas. |
| 45		| 		| Weather / time of day visuals				| GDD says no encounter variation by time/weather, so this is purely cosmetic. |

---

## Creature & Feat System

| Priority	| Pts	| Item													| Notes |
| 200		| 		| At least one authored `CreatureSpecies` + `FeatBoard`	| Without real creature data, nothing playable exists. Minimum: one species with base stats, 2–3 abilities, and a small feat board. |
| 135		| 		| Feat Board runtime UI									| A visual progression tree the player can open to see and interact with a creature's nodes. `FeatBoardEditorWindow` is editor-only; a runtime in-game equivalent is needed. |
| 55		| 		| Feat Board respec										| GDD mentions it exists but is not a main feature. |

---

## Player Progression & Meta

| Priority	| Pts	| Item											| Notes |
| 195		| 		| Save / Load system							| `GameSaveData` is the data class. Serialization to disk JSON or binary and loading on startup needs to be wired in `GameBootstrapper` / `GameInitializationService`. 		|
| 135		| 		| Gym defeat tracking + encounter tier scaling	| `GameSaveData` needs a cleared-gym set; this count feeds `EncounterTier` scaling 0–8 tiers defined by gyms beaten per the GDD. |
| 125		| 		| Team management screen						| UI to swap creatures between active team and PC box between encounters. |
| 115		| 		| PC storage creature box						| GDD: active team = 6; extras go to PC box. `PlayerData` likely only holds the team; PC box storage and the swap UI need to be added. |
| 105		| 		| Gym / Elite Four encounter definitions		| Authored `EncounterTable` assets for 8 gyms × 8 tiers + Elite Four. |
| 75		| 		| Trainer defeat tracking						| Cleared trainers should not re-trigger. `GameSaveData` needs a defeated-trainer set. |

---

## View / Animation System

| Priority	| Pts	| Item											| Notes |
| 130		| 		| Board movement tween for units				| Separate from the recipe animator; smoothly moves the creature view tile-to-tile when a `MoveAction` resolves. |
| 120		| 		| `View.Animation.Set` ScriptableObject			| Named dictionary of recipes assigned to a `CreatureForm`; mandatory names: `Idle`, `TakeDamage`, `Death`. |
| 115		| 		| `View.Animation.Rig` MonoBehaviour			| Maps `LogicalPart` → concrete `Transform` on the prefab; captures rest pose so recipes can return cleanly. |
| 110		| 		| `View.Animation.Animator` MonoBehaviour		| Executes `Recipe` phases on the rig: one main channel + one optional additive overlay channel. |
| 105		| 		| Ability caster animation hook					| `Ability` definition should reference an animation name from the caster's `AnimationSet`; the resolution phase triggers it before applying effects. |
| 100		| 		| `View.Animation.Recipe` ScriptableObject		| Sequential phase container; phases run one after another, steps within a phase run in parallel. |
| 95		| 		| `View.Animation.Phase` + Step class hierarchy	| Concrete steps: `MoveLocalStep`, `RotateLocalStep`, `ScaleStep`, `ShakeStep`, `FlashStep`, `WaitStep`, `SpawnVfxStep`, `PlaySoundStep`. |
| 90		| 		| `View.Animation.LogicalPart` enum				| Abstract bone-naming layer root, body, head, front, rear, dominant limb, off limb, weapon, jaw, tail, whole rig. Makes recipes reusable across body types. |
| 75		| 		| VFX spawn step implementation					| `SpawnVfxStep` spawning a particle prefab at a `LogicalPart` anchor. |
| 70		| 		| Sound step implementation						| `PlaySoundStep` playing an `AudioClip` during a recipe phase. |
| 65		| 		| Hit flash overlay channel						| Additive overlay for hit-react / charge-up visual cues. |
| 60		| 		| Custom Step inspector							| `ManagedReferenceTypePicker` exists; wire it to expose only concrete step subtypes in the Phase inspector. |

---

## Audio

| Priority	| Pts	| Item						| Notes |
| 110		| 		| Audio manager / service	| No audio system exists. Need at minimum a simple singleton for playing SFX and background music. |
| 85		| 		| Battle music				| Background track that starts on battle entry and stops on exit. |
| 65		| 		| Ability SFX				| Per-ability sounds triggered by `PlaySoundStep`. |
| 50		| 		| UI SFX					| Button click / menu navigation sounds. |
| 45		| 		| Footstep SFX				| Exploration walking sounds. |

---

## Content

| Priority	| Pts	| Item											| Notes |
| 185		| 		| First creature roster							| Minimum 3–5 fully specced creatures stats, abilities, feat board to allow meaningful test play. |
| 175		| 		| Ability definitions							| Authored `Ability` ScriptableObjects: at least direct damage physical + magical, a DoT, a heal, a movement debuff, and a buff. |
| 160		| 		| Voxel tileset									| Minimum set of `VoxelDefinition` assets ground, wall, slope, water, grass to make the world visually readable. |
| 80		| 		| Creature 3D models / voxel prefabs			| Prefabs with `View.Animation.Rig`; placeholder cubes are fine for prototype testing. |
| 70		| 		| Town structures / building voxel templates	| Pre-built voxel cell templates for gyms, houses, and POIs. |

---

## Technical / Infrastructure

| Priority	| Pts	| Item									| Notes |
| 195		| 		| Bootstrap / scene wiring end-to-end	| `GameBootstrapper` and `GameInitializationService` exist; the full startup flow load save → enter world → spawn player needs end-to-end validation. |
| 140		| 		| Event system completeness				| `EventCenter` emits player movement events; verify that encounter emitters, chunk streaming, and trainer sight-lines all subscribe correctly and do not double-subscribe. |
| 130		| 		| Random seed determinism				| All random calls in `MetaWorldGenerator` and biome/encounter placement must use a seeded `System.Random`, not `UnityEngine.Random`, so the world is reproducible from the same seed. |
| 90		| 		| Debug / cheat console					| In-game toggle for forcing encounters, completing feats, warping to locations — accelerates iteration. |
| 70		| 		| Voxel mesher performance profiling	| `VoxelMesher` generates geometry synchronously; high chunk counts may cause frame spikes that need profiling and possible async offload. |

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
- `FeatRequirement` concrete subclasses exist and are constructable via the feat board editor window. `CastAbilityCountRequirement` supports an optional `List<Ability>` filter empty = any ability; `CastDifferentAbilitiesRequirement` was merged into it.
- Evolution branching / sibling-branch blocking logic is implemented in `FeatProgressionService`.
- `DamageTargetEffect` emits `DealDamageRequirement.Event`, `TakeDamageRequirement.Event`, and `SurviveHitRequirement.Event` when target survives; `HealTargetEffect` emits `HealHealthRequirement.Event` and `HealTargetRequirement.Event`.
- `BattleUnit` accumulates `PendingFeatEvents` during battle via `RecordFeatEvent`; `BattleActionResolver` routes effect-returned events to the source unit.
- `EndPhase` applies accumulated events to `FeatProgressionService.RegisterEvent` for all player units, but only on player victory.
- EditMode test suite: 451 passing tests, split into focused namespaces per requirement type and scope.
