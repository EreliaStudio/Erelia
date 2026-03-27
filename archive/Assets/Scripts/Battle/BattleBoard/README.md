# Battle.BattleBoard README

## Purpose
BattleBoard builds and renders the battle grid. It converts exploration world data into a battle board model and
binds that model to rendering and collision meshes.

## Contents
- `Constructor`: exports a battle board from world voxels and encounter data.
- `Model`: stores the 3D cell grid plus origin and center metadata.
- `Presenter`: binds a model to a view and rebuilds render/collision/mask meshes.
- `View`: owns mesh components for board rendering, collisions, and mask overlays.

## Adding Or Extending
1. Add new board metadata to `Model` and update any consumers that read it.
2. Update `Constructor` if the board needs additional data copied from the world.
3. Extend `Presenter` or `View` if you add new visual layers or mesh types.
