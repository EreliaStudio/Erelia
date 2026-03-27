# Exploration README

## Purpose
Exploration contains the runtime systems for the overworld:
- Player movement and camera control
- World chunk streaming and rendering
- Encounter triggering

## Core Flow
1. `Erelia.Core.GameContext` holds an `ExplorationState` instance.
2. `Exploration.Loader` binds the world and player state objects to presenters.
3. `ExplorationState` stores the exploration player state and the loaded world state.
4. Player movement emits `PlayerMotion` and `PlayerChunkMotion` events.
5. World presenter streams chunks around the player.
6. Encounter trigger checks the chunk encounter grid and requests battle scenes.

## Player
### Player
- `Exploration.Player.Presenter` handles movement input.
- `Exploration.Player.View` exposes the linked camera transform.
- `Exploration.Player.PlayerMotionEmitter` emits movement events.
- `Exploration.Player.ChunkMotionEmitter` emits chunk-change events.

### Camera:
- `Exploration.Player.Camera.Presenter` handles orbit and zoom.
- `Exploration.Player.Camera.OrbitCameraSettings` stores sensitivity and speed config.

## World
### World
- `Exploration.World.WorldState` stores chunks and handles chunk I/O.
- `Exploration.World.Presenter` streams chunks based on view radius.
- `Exploration.World.View` spawns chunk views.
- `Exploration.World.VoxelCatalog` loads the voxel registry from Resources.

### Chunks:
- `Exploration.World.Chunk.ChunkData` stores voxel cells and encounter ids.
- `Exploration.World.Chunk.Presenter` rebuilds render/collision meshes.
- `Exploration.World.Chunk.View` holds the mesh components.
- `Exploration.World.Chunk.Generation.IGenerator` populates new chunks.

## Encounters
- `Exploration.Player.EncounterTriggerEmitter` checks the chunk encounter grid.
- If the player is sitting on a cell that contain an encounter ID, check if the encounter should trigger.
- If an encounter triggers:
  - Picks the enemy team from the encounter table.
  - Builds a battle board from the world.
  - Emits `BattleSceneDataRequest` with the board and enemy team.

## Serialization
- World metadata is saved as JSON (`WorldState.Save`).
- Chunk data is saved as binary via `ChunkData.ToFile`.
- Generator state is saved/loaded via `IGenerator.Save/Load`.

## Authoring Workflow
1. Assign a chunk generator in the world presenter.
2. Ensure voxel registry exists in `Resources/Voxel/VoxelRegistry`.
3. (Optional) Configure biomes and encounter registry.
