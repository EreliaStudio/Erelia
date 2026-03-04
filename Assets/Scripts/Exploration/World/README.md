# Exploration.World README

## Purpose
World contains the exploration world model, chunk streaming presenter, and registries
for voxels and biomes. It manages chunk loading, generation, and rendering.

## Contents
- `Model`: owns chunk cache, chunk I/O, and generator persistence.
- `Presenter`: streams chunks around the player and builds chunk presenters.
- `View`: spawns chunk views and defines view radius.
- `VoxelRegistry`: lazy loader for the voxel registry asset in Resources.
- `BiomeRegistry`, `BiomeData`, `BiomeType`: biome lookup and encounter configuration.

## Adding Or Extending
1. Add new world-level data to `Model` and keep save/load consistent.
2. Add new registries here if world generation needs additional lookups.
3. Keep chunk creation logic in `Presenter` and chunk data in `World/Chunk`.
