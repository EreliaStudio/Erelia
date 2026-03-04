# Battle.Voxel README

## Purpose
Voxel contains battle-specific voxel extensions, mask types, and overlay mesh generation.
It augments the core voxel system with masks for placement, ranges, and selection.

## Contents
- `Cell`: battle voxel cell with mask support.
- `Type`: mask type enum for overlays.
- `Definition`: battle voxel definition with mask shape data.
- `Data`: placeholder for battle-only voxel metadata.
- `MaskShape`: base class for overlay mask shapes.
- `Mesher`: builds overlay meshes from masks.
- `MesherUtils`: caches transformed cardinal point sets.
- `CardinalPoint` and `CardinalPointSet`: placement entry point helpers.
- `Shape`: concrete mask shapes for voxel geometry.
- `Editor`: custom inspector utilities for battle voxel definitions.

## Adding Or Extending
1. Add new mask types in `Type` and wire sprites in `Battle/Mask`.
2. Add new mask shapes in `Voxel/Shape` and map them in the editor.
3. Extend `Mesher` if overlay visuals need new rules or geometry.
