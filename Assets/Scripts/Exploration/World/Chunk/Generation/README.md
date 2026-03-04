# Exploration.World.Chunk.Generation README

## Purpose
Generation contains the chunk generation contract and implementations.
Generators populate chunk voxel data and handle their own save/load state.

## Contents
- `IGenerator`: abstract base class for chunk generation.
- `SimpleDebugChunkGenerator`: sample generator for quick testing.

## Adding Or Extending
1. Create a new `ScriptableObject` that derives from `IGenerator`.
2. Implement `Generate`, `Save`, and `Load`.
3. Assign the generator in `World.Presenter` or `World.Model`.
