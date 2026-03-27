# Steps To Achieve Erelia

This document turns the current design direction into an implementation roadmap.

It is based on:
- `Proposition.md`
- `StructureProposition.md`
- `WorldGenerationIdeas.md`
- the current Unity project state

The goal is to give you a realistic order of work, so you do not build features on top of temporary prototype architecture that will need to be thrown away later.

## Core Principles

Before starting the steps, keep these rules in mind:

- Do not expand the current debug world generator into the final generator.
- Do not keep adding game logic into `Context`, scene loaders, or presenter classes.
- Keep authored data separate from runtime run data.
- Prefer `ScriptableObject` references for authored content.
- Keep compact ids only where density matters: voxel cells, save references, generated instances.
- Keep the game playable after each milestone when possible.
- When replacing systems, migrate in layers instead of rewriting everything at once.

## Current Situation

Right now the project is a functional prototype with these characteristics:

- exploration state is split between `Context`, `SystemData`, and `ExplorationState`
- battle state is heavily tied to presenters and scene flow
- chunk generation is local and debug-oriented
- biomes are almost empty
- encounters are driven by `EncounterIds` stamped into chunks
- save/load exists only in partial world form
- items, trainers, progression flags, interiors, and full run persistence are mostly missing

This means the first priority is not content. The first priority is defining a reliable combat foundation, stabilizing the current battle loop on top of it, and only then building the larger architecture around it.

## Recommended Order

The steps below are now ordered in the order I recommend you implement them.

If you skip that order, you will likely rebuild the same systems twice.

## Step 1: Redefine The Creature And Action-Ownership Model Before The Battle Refactor

### Goal

Replace the prototype creature model with the intended long-term species, instance, form, stats, and action-ownership model.

### Why

Right now the creature layer is still too thin and battle is already built directly on top of it.

The current model is mostly:

- species id lookup
- simple additive stats
- direct attack slots on the creature instance
- feat progress

That is not enough for the target architecture, which needs:

- clear separation between species data, creature identity, and battle unit runtime
- forms or variants
- unlocked actions instead of raw fixed slots only
- persistent effects and status sources
- richer stat resolution
- cleaner ownership of what belongs to the creature, the run, and the battle

If you skip this and continue refactoring battle first, you will likely rebuild the battle layer against the wrong creature assumptions.

### What To Do

- redefine `Species` so authored data is not just icon, prefab, display name, and one flat stat block
- redefine creature instances so they own identity, progression, unlocked actions, and persistent state
- decide how forms are authored, selected, and switched
- separate battle-only runtime values from long-term creature state
- replace the current direct `Creature.Attacks` ownership with a cleaner action or learned-move model
- update stat resolution so species, form, growth, equipment or effects, and battle modifiers do not collapse into one simple additive merge

### Files To Refactor

- `Assets/Scripts/Core/Creature/Species.cs`
- `Assets/Scripts/Core/Creature/Stats.cs`
- `Assets/Scripts/Core/Creature/Instance/Model.cs`
- `Assets/Scripts/Battle/Unit/Model.cs`
- `Assets/Scripts/Battle/Unit/LiveStats.cs`
- `Assets/Scripts/Battle/Attack/Definition.cs`

### Done When

- one creature instance has persistent identity, growth, unlocked actions, form data, and persistent state
- battle units read creature data through a clean runtime projection instead of owning the long-term creature model directly
- battle refactor work can continue without relying on prototype creature assumptions

## Step 2: Stabilize The Current Battle Prototype

### Goal

Fix the current battle issues that make actions, targeting, and turns behave incorrectly, using the clarified creature and action foundations from Step 1 but before the deeper battle refactor begins.

This is the first tactical battle-fix step after the combat foundations are clear.

### Why

The battle layer is not only incomplete. Some current behaviors are already wrong for the existing prototype:

- enemy turns currently do not take meaningful actions
- line of sight is authored on attacks but not enforced
- attack preview rules and attack resolution rules do not fully match
- action points can be consumed by attacks that should be rejected or that resolve as no-ops
- the current action validation path is too fragmented to trust during iteration

Once Step 1 defines what a creature owns, what battle reads from it, and where actions live, you can safely repair legality, targeting, and turn behavior without introducing more temporary assumptions.

If you do not stabilize this here, the prototype becomes harder to test, harder to trust, and harder to use as a baseline while refactoring toward the target battle model.

### What To Do

- define one source of truth for attack legality
- enforce line of sight in both preview and resolution
- make previewed valid targets match actually resolvable targets
- validate attacks before spending action points
- prevent invalid or empty actions from consuming resources
- add at least a temporary enemy decision pass instead of immediate auto-end-turn behavior
- review whether target filtering belongs on the attack, the effect, or both, then remove contradictions
- add repeatable debug scenarios for:
  - movement
  - single-target attack
  - ally-targeted action
  - area-of-effect action
  - blocked line-of-sight action
  - invalid target or no-op cast

### Files To Fix First

- `Assets/Scripts/Battle/Phase/PlayerTurn/Root.cs`
- `Assets/Scripts/Battle/Phase/ResolveAction/Root.cs`
- `Assets/Scripts/Battle/Phase/EnemyTurn/Root.cs`
- `Assets/Scripts/Battle/Attack/Definition.cs`
- `Assets/Scripts/Battle/Attack/TargetingUtility.cs`
- `Assets/Scripts/Battle/Attack/Effect/Definition.cs`
- `Assets/Scripts/Battle/DecidedAction.cs`

### Done When

- player and enemy units can both complete meaningful turns
- line-of-sight attacks are blocked correctly
- the previewed target/cell set matches the set that resolves successfully
- invalid or no-op actions do not spend AP
- the prototype battle loop is reliable enough to use during the later refactor

## Step 3: Refactor Battle Into Pure Runtime State

### Goal

Separate battle runtime data from scene presenters and prototype shortcuts.

### Why

Current battle data stores runtime presenter references directly, which makes the system harder to save, test, and extend.

### What To Do

- create `Battle.Input`
- create `Battle.State`
- create `Battle.Team`
- create `Battle.Unit`
- move state ownership out of the current `BattleState` holder
- keep `Presenter` classes as view/controller adapters only

### Current Files To Refactor

- `Assets/Scripts/Battle/Data/BattleData.cs`
- `Assets/Scripts/Battle/Unit/Model.cs`
- `Assets/Scripts/Battle/Phase/Orchestrator.cs`
- `Assets/Scripts/Battle/DecidedAction.cs`

### Done When

- battle logic can operate on runtime state without needing `MonoBehaviour` presenters as core data

## Step 4: Replace Prototype Battle Commands With The Target Command Model

### Goal

Support richer battle commands and delayed or structured resolution.

### Why

Current commands are only:

- move
- attack
- end turn

The design target needs:

- move command
- action command
- capture command
- target-selection payloads
- pending actions
- duration state
- applied board and unit effects

### What To Do

- replace `DecidedAction` with command classes
- add target selection models
- add pending action support
- add duration support
- add board effect runtime state

### Files To Review

- `Assets/Scripts/Battle/DecidedAction.cs`
- `Assets/Scripts/Battle/Phase/ResolveAction/Root.cs`
- `Assets/Scripts/Battle/Attack/Definition.cs`
- `Assets/Scripts/Battle/Attack/TargetingUtility.cs`

### Done When

- actions can be immediate, delayed, or recovery-based
- action targeting is explicit and reusable

## Step 5: Implement Real Enemy AI

### Goal

Replace the placeholder enemy phase with actual decision-making.

### Why

Current enemy turn logic only waits, then ends the turn.

### What To Do

- add AI profile assets
- add ordered rules
- add conditions
- add action choice logic
- make AI evaluate available commands

### Files To Replace

- `Assets/Scripts/Battle/Phase/EnemyTurn/Root.cs`

### Done When

- enemies can decide between moving, attacking, waiting, or using support actions

## Step 6: Create The Real Run Root

### Goal

Create the real game root object:

- `GameRun`
- `World.Data`
- `Player.State`
- `Progression.RunProgress`

### Why

Your current runtime state is spread across:

- `Assets/Scripts/Core/Context.cs`
- `Assets/Scripts/Core/SystemData.cs`
- `Assets/Scripts/Exploration/ExplorationData.cs`
- `Assets/Scripts/Battle/Data/BattleData.cs`

That is enough for a prototype, but not enough for a run-based game with saves, progression, generated worlds, and interiors.

### What To Do

- Create a new runtime namespace and root model for the active run.
- Move player team ownership out of `SystemData`.
- Move exploration position and respawn/safe position into `Player.State`.
- Move world ownership into `World.Data`.
- Move progression flags into `RunProgress`.
- Keep `Battle.State` transient and separate from the saved run unless some battle results must persist.

### Files To Refactor First

- `Assets/Scripts/Core/Context.cs`
- `Assets/Scripts/Core/SystemData.cs`
- `Assets/Scripts/Exploration/ExplorationData.cs`
- `Assets/Scripts/Exploration/Player/Model.cs`

### Done When

- one object clearly represents the current run
- the player team is owned by player state, not system bootstrap code
- respawn location is part of player state
- the world is part of the run, not a loose exploration field

## Step 7: Implement Full Save/Load

### Goal

Make the game able to save and restore one full run.

### Why

Your `Saving` folder is empty, and the current save support only covers chunks and generator metadata in `Exploration.World.WorldState`.

That is not enough for the target game.

### What To Do

- Create `SaveData`
- Store `GameRun`
- store save slot metadata
- write a save service
- write a load service
- make the loading scene restore a run instead of only creating default context

### Files Likely To Touch

- `Assets/Scripts/Loading/Loader.cs`
- `Assets/Scripts/Core/Utils/JsonIO.cs`
- `Assets/Scripts/Exploration/World/Model.cs`
- new files under `Assets/Scripts/Saving`

### Done When

- you can start a run, move, save, quit, load, and resume in the same place
- player team, progression, and world generator state survive reload

## Step 8: Split Authored Data From Runtime Data Cleanly

### Goal

Make your data model match the authored/runtime split described in the design docs.

### Why

A lot of current systems mix authoring assumptions with runtime assumptions.

Examples:

- species use registry ids instead of direct references
- encounters use JSON and registries rather than authored assets
- battle board setup is mixed into encounter tables

### What To Do

- keep authored data as `ScriptableObject` assets
- keep runtime data serializable and run-owned
- decide which current registry patterns stay as convenience only
- reduce dependence on ids for normal authored references

### Good Early Targets

- `Species`
- `Biome`
- `Encounter`
- `RoadProfile`
- `Structure.Template`
- `Badge`
- `Trainer.Definition`

### Done When

- authored content can be linked in the inspector directly
- runtime state stores references or compact ids only where justified

## Step 9: Refactor The World Model Into Maps + Generator

### Goal

Replace the current single-world chunk cache model with:

- `World.Data`
- `World.MapData`
- `World.Generator`
- named maps and interiors

### Why

Current world ownership is centered on:

- `Assets/Scripts/Exploration/World/Model.cs`
- `Assets/Scripts/Exploration/World/Chunk/Model.cs`

That model assumes one streamable chunk world and does not yet support:

- named maps
- generated interiors
- town or gym interiors
- tunnels
- map transitions

### What To Do

- create a dictionary from world/map name to `MapData`
- move chunk ownership under `MapData`
- keep generator state separate from concrete loaded chunks
- introduce `MapLocation`

### Done When

- the active position is a map name plus local position
- the world can contain overworld plus interiors

## Step 10: Build The Biome System Properly

### Goal

Turn the biome system into real authored gameplay data.

### Why

Current biome support is minimal:

- `BiomeType` only has `Unknown`
- `Biome` only stores `EncounterId`

That is far below the target described in the docs.

### What To Do

- replace enum-driven biome data with biome assets
- add terrain generation parameters
- add ground voxel palette
- add scenery rules
- add location generation rules
- add wild encounter definitions

### Files To Replace Or Heavily Refactor

- `Assets/Scripts/Exploration/World/BiomeType.cs`
- `Assets/Scripts/Exploration/World/Biome.cs`
- `Assets/Scripts/Exploration/World/BiomeRegistry.cs`

### Done When

- one biome asset contains terrain, encounter, and placement rules
- chunk generation can ask the biome what to generate here

## Step 11: Build The Macro World Plan

### Goal

Implement the high-level finite world generation plan from `WorldGenerationIdeas.md`.

### Why

This is the most important architectural change.

Your final generator should not discover the world only from local chunk noise.

It should generate first:

- world bounds
- landmass
- height/rivers
- major cities
- biome anchors
- villages
- roads
- bridges
- ports
- sea routes
- tunnels

Then chunk generation should realize that plan locally.

### What To Do

- create a world-generation profile asset
- create a macro plan runtime model
- generate the continent and water mask
- generate a biome field
- place settlements
- generate a transport graph
- classify graph links into road, bridge, sea, or tunnel

### Important Rule

Do not attempt to make `SimpleDebugChunkGenerator` evolve into this.
Create a separate generator layer.

### Done When

- one seed produces one stable world plan
- chunk loading no longer decides global structure by itself

## Step 12: Make Chunk Generation Read The Macro Plan

### Goal

Make chunks a realization layer, not a planning layer.

### Why

The final world should feel coherent globally while remaining cheap locally.

### What To Do

- replace `SimpleDebugChunkGenerator`
- make chunk generation query:
  - biome at coordinate
  - height at coordinate
  - river presence
  - road or bridge stamp
  - structure presence
  - town or port footprint
- keep chunks as dense voxel containers

### Files Likely To Replace

- `Assets/Scripts/Exploration/World/Chunk/Generation/SimpleDebugChunkGenerator.cs`
- `Assets/Scripts/Exploration/World/Chunk/Generation/IGenerator.cs`

### Done When

- chunk generation is deterministic from the macro plan
- different chunks agree on the same rivers, roads, and structures

## Step 13: Introduce Structures, Interiors, And Map Transitions

### Goal

Support towns, buildings, gyms, caves, ports, tunnels, and interior maps.

### Why

Your docs describe a world with placed structures, interactive doors, tunnel entrances, and interior spaces.
None of that exists as a proper system yet.

### What To Do

- add structure template assets
- add placed-structure runtime data
- add interactive objects
- add interior prefabs and interior map data
- create transition handling between overworld and interiors

### This Unlocks

- gym entrances
- buildings
- cave or tunnel interiors
- ports and travel hubs
- handcrafted interior encounters

### Done When

- the player can enter a generated or authored structure and load another map

## Step 14: Refactor Encounters Around World Rules

### Goal

Stop using chunk `EncounterIds` as the main encounter system.

### Why

Current encounter flow is:

- chunk stores `EncounterIds`
- player steps on a cell
- id resolves to one encounter table

That is too rigid for the target game.

### What To Do

- remove encounter ownership from `ChunkData`
- let encounters come from:
  - biome wild encounter rules
  - interior wild encounter rules
  - trainer definitions
  - interaction-triggered events
  - voxel interaction tags if needed
- separate:
  - encounter trigger logic
  - enemy team selection
  - battle board configuration

### Files To Refactor

- `Assets/Scripts/Exploration/Player/EncounterTriggerEmitter.cs`
- `Assets/Scripts/Core/Encounter/EncounterTable.cs`
- `Assets/Scripts/Core/Encounter/EncounterTableRegistry.cs`
- `Assets/Scripts/Exploration/World/Chunk/Model.cs`
- `Assets/Scripts/Battle/BattleBoard/BattleBoardConstructor.cs`

### Done When

- stepping in grass, entering a cave, talking to a trainer, and triggering a special interaction can all start battles through the same high-level encounter pipeline

## Step 15: Merge The Voxel Gameplay Model Properly

### Goal

Move toward the target model where one voxel definition drives both exploration and battle.

### Why

Right now you still have a split between:

- `Core.Voxel.VoxelDefinition`
- `Battle.Voxel.Definition`

That split exists because the prototype added battle overlays on top of the exploration voxel model.

### What To Do

- decide on one main voxel definition asset
- keep traversal and shape data there
- keep overlay mask data there if needed
- unify lookups so battle and exploration resolve the same voxel meaning

### Files To Review

- `Assets/Scripts/Core/Voxel/VoxelDefinition.cs`
- `Assets/Scripts/Battle/Voxel/Definition.cs`
- `Assets/Scripts/Core/Voxel/VoxelRegistry.cs`
- `Assets/Scripts/Battle/Voxel/Mesher.cs`

### Done When

- exploration and battle use the same source voxel gameplay definition

## Step 16: Add Trainers, Progression Sources, Items, And Rewards

### Goal

Implement the meta-game systems needed for a complete run.

### Why

These systems are mostly absent, but your design assumes them heavily.

### What To Do

- implement trainer definitions
- implement trainer rewards
- implement badges
- implement milestone flags
- implement progression source states
- implement inventory and item definitions
- implement world-object persistence such as cleared trainers or removed obstacles

### Done When

- defeating a trainer can grant money, items, badges, or milestones
- world progression persists between visits and after save/load

## Step 17: Add The Animation Authoring Layer

### Goal

Add the fake-animation recipe system after the battle and creature foundations are stable.

### Why

This is useful, but it should not come before core gameplay architecture.

### What To Do

- create animation set assets
- create animation recipe assets
- create logical-part rigs
- create runtime animation player

### Done When

- creatures can play reusable authored action and reaction animations without hardcoded prefab-specific animation logic

## Step 18: Build Content Pipelines And Tooling

### Goal

Make content production practical after systems are stable.

### What To Do

- custom inspectors for polymorphic action effects
- custom inspectors for AI conditions
- custom inspectors for structure templates
- generation debug views
- save/load debug commands
- world-plan visualization tools

### Done When

- adding biomes, trainers, encounters, and structures does not require editing code every time

## Recommended Milestones

If you want a practical milestone breakdown, use this:

### Milestone A: Combat And Creature Foundation

- Step 1
- Step 2
- Step 3
- Step 4
- Step 5

Outcome:
- the creature and action foundation is defined first, the current battle loop is stabilized second, and then battle is rebuilt on the proper runtime and command model

### Milestone B: Run Foundation

- Step 6
- Step 7
- Step 8

Outcome:
- the game has a real run model and save or load

### Milestone C: World Backbone

- Step 9
- Step 10
- Step 11
- Step 12

Outcome:
- the world is generated from a macro plan and realized in chunks

### Milestone D: Traversal And Encounters

- Step 13
- Step 14
- Step 15

Outcome:
- the world supports structures, interiors, and proper encounter sources

### Milestone E: Meta Progression

- Step 16

Outcome:
- trainers, rewards, items, badges, and world progression align with the intended game loop

### Milestone F: Polish And Production

- Step 17
- Step 18

Outcome:
- animation, tooling, and content production become efficient

## First Concrete Sprint I Recommend

If you want the best next move right now, do this first:

1. Lock the minimal creature foundation battle depends on: creature identity, stat layers, and action ownership
2. Update `Battle.Unit.Model` and `LiveStats` so battle reads that foundation cleanly
3. Fix attack legality so preview and resolution use the same validation rules
4. Enforce line of sight and prevent invalid or no-op actions from consuming AP
5. Replace the current enemy auto-end-turn behavior with at least a temporary action-selection pass
6. Then move into Step 3 so battle state and commands stop depending on presenter-heavy prototype flow

Do not start with:

- more debug chunk generation
- more encounter id stamping
- more world-generation complexity
- more one-off battle content built on broken action logic

Those would deepen prototype debt before the foundation is ready.

## Final Rule

Whenever you hesitate between adding a feature quickly to the current prototype or building the target architecture first, prefer the architecture first if the feature depends on:

- persistence
- world generation
- map transitions
- trainer progression
- encounter rules
- battle extensibility

Those are the systems most likely to force expensive rewrites if you delay the structural work.




