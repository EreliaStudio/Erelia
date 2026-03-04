# Battle.Voxel.Shape README

## Purpose
Shape contains concrete `MaskShape` implementations for battle voxels.
These shapes define the overlay faces and cardinal points used by mask rendering.

## Contents
- `Cube`: mask faces and cardinal points for cube voxels.
- `Slab`: mask faces and cardinal points for slab voxels.
- `Slope`: mask faces and cardinal points for slope voxels.
- `Stair`: mask faces and cardinal points for stair voxels.
- `CrossPlane`: mask faces and cardinal points for cross-plane voxels.

## Adding Or Extending
1. Create a new class that derives from `MaskShape`.
2. Implement `ConstructMaskFaces` and any custom cardinal points.
3. Map the new shape in `BattleVoxelDefinitionEditor`.
