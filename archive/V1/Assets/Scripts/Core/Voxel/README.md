# Voxel README

## Purpose
Voxel is a small voxel pipeline used by Erelia to define voxel types, store voxel grids, and generate render/collision meshes.

Core flow:
1. A `Cell` stores voxel data for a grid location (id, orientation, flip).
2. The `VoxelRegistry` maps a cell id to a `VoxelDefinition` asset.
3. Each `VoxelDefinition` owns a `Shape` that provides geometry faces.
4. The `Mesher` builds meshes from a `Cell[,,]` grid and a `VoxelRegistry` to produce a `UnityEngine.Mesh`.

## Coordinate System
- Unit cube space for shapes: each voxel lives in [0,1] on X, Y, Z.
- Y is up.
- `AxisPlane` names match the unit cube planes (PosX = x=1, NegX = x=0, PosY = y=1, NegY = y=0, PosZ = z=1, NegZ = z=0).
- `Orientation` is a rotation around Y in 90 degree steps.
- `FlipOrientation` mirrors along Y.

## Cells
- A `Cell` is a lightweight container for `Id`, `Orientation`, `FlipOrientation`.
- By convention, `Id < 0` means "empty voxel".
- `Cell.WriteTo` and `Cell.ReadFrom` provide a fixed binary layout used by chunk serialization.

## Registry and Definitions
- `VoxelRegistry` is a ScriptableObject that maps integer ids to `VoxelDefinition` assets.
- `VoxelDefinition` groups `VoxelProperties` (gameplay flags) and a `Shape` (geometry).
- The registry is rebuilt on `OnEnable` and `OnValidate`.
- At runtime, `VoxelRegistry.TryGet(id, out VoxelDefinition)` resolves a cell id.

## Shapes and Faces
- `Shape` is the geometry source for a voxel type.
- Shapes produce two channels:
  - Render faces (for visuals).
  - Collision faces (for physics).
- Faces are split into:
  - Outer shell: faces on axis planes, used for neighbor occlusion.
  - Inner faces: everything else, not tied to a plane.
- All shapes are authored in a canonical orientation:
  - Facing +X, no flip.
  - Orientation and flip are applied later by the mesher.

## Mesher
- `Mesher.BuildRenderMesh` creates the visual mesh.
- `Mesher.BuildCollisionMesh` creates the physics mesh.
- Both take:
  - `Cell[,,]` grid
  - `VoxelRegistry`
  - Optional predicate to filter voxels (ex: only obstacles)

Important behavior:
- Outer faces are culled if fully occluded by neighbors.
- Inner faces are emitted only if the voxel is exposed.
- Some operations are cached (see `MesherUtils`).

## Traversal and Gameplay Data
- `VoxelProperties.Traversal` describes movement rules (Obstacle, Walkable).
- The mesher exposes predicates:
  - `OnlyObstacleVoxelPredicate`
  - `OnlyWalkableVoxelPredicate`

## Serialization
- Chunk data is serialized with a fixed binary layout:
  - `Id` (int), `Orientation` (byte), `FlipOrientation` (byte)
- If you change this layout, old chunks become incompatible unless you add versioning.

## Adding a New Voxel Shape
1. Create a new class that inherits from `Shape`.
2. Implement `ConstructRenderFaces`.
3. Optionally override `ConstructCollisionFaces`.
4. Add the shape type to the custom editor if needed.
5. Create a new `VoxelDefinition` asset and select the shape.
6. Register the definition in the `VoxelRegistry`.

Guidelines:
- Always build faces in the canonical orientation (PositiveX, not flipped).
- Ensure outer faces lie exactly on axis planes when applicable.
- Ensure polygon winding is consistent for normals.

## Troubleshooting
- Missing faces: check `OuterShell` planes and orientation mapping.
- Faces flicker or disappear: check face winding or occlusion logic.
- Collision looks wrong: verify `ConstructCollisionFaces`.
- A voxel does not appear: check `VoxelRegistry` id mapping and `Id < 0` rules.

