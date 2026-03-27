# Exploration.World README

## Purpose
World contains the exploration world model, chunk streaming presenter, and registries
for voxels and biomes. It manages chunk loading, generation, and rendering.

## Contents
- `WorldState`: owns chunk cache, chunk I/O, and generator persistence.
- `Presenter`: streams chunks around the player and builds chunk presenters.
- `View`: spawns chunk views and defines view radius.
- `VoxelCatalog`: lazy loader for the voxel registry asset in Resources.
- `BiomeRegistry`, `Biome`, `BiomeType`: biome lookup and encounter configuration.

## Adding Or Extending
1. Add new world-level state to `WorldState` and keep save/load consistent.
2. Add new registries here if world generation needs additional lookups.
3. Keep chunk creation logic in `Presenter` and chunk data in `World/Chunk`.


