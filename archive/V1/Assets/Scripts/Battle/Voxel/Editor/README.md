# Battle.Voxel.Editor README

## Purpose
This folder no longer owns a separate voxel-definition inspector.
Battle overlay authoring now lives directly on `Core.Voxel.VoxelDefinition`, and the shared inspector is `Core.Voxel.Editor.VoxelDefinitionEditor`.

## Contents
- no runtime code remains here after the voxel-definition merge

## Adding Or Extending
1. Extend `Core.Voxel.Editor.VoxelDefinitionEditor` when new shared voxel authoring fields are introduced.
2. Update the shared shape-to-mask mapping there if new voxel shapes are added.


