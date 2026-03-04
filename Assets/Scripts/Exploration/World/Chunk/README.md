# Exploration.World.Chunk README

## Purpose
Chunk contains the world chunk data model, coordinates, and rendering helpers.
It stores voxel cells and encounter ids and handles chunk serialization.

## Contents
- `Model`: voxel and encounter grids with binary save/load.
- `Coordinates`: chunk coordinate math and world conversions.
- `Presenter`: rebuilds chunk meshes when the model validates.
- `View`: owns render and collision meshes for a chunk.

## Adding Or Extending
1. Add new per-chunk data to `Model` and update `ToFile` and `FromFile`.
2. Trigger `Model.Validate` when data changes to rebuild meshes.
3. Add visual updates in `Presenter` or `View` if rendering changes.
