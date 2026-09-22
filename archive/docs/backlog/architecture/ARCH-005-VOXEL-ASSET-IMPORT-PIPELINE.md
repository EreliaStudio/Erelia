# ARCH-005 — External Voxel Asset Import and Articulation Pipeline

## Decision

Do **not** build a custom voxel modeling application or custom animation suite for the first implementation.

Use mature external tools where practical and convert their output into the same runtime voxel-volume representation used by Chunks.

```text
external voxel/3D authoring tool
        ↓ common/standard format
importer / converter
        ↓
VoxelModel volume
        ↓
shared VoxelMesher
        ↓
cached ordinary mesh
```

## Tool/format selection is a spike, not an architectural dependency

The plan must compare candidate authoring tools/formats against these requirements:

- voxel-grid dimensions and palette/color data;
- deterministic coordinate orientation/axis mapping;
- ability to map imported cells to Erelia `Voxel::Definition` IDs;
- optional preservation of names/layers/groups useful for parts;
- stable licensing and format documentation;
- simple headless import suitable for CI;
- no requirement to embed the editor in Sparkle.

Most common voxel formats are cube-centric. That is acceptable for many props/character parts. Existing Erelia slab/slope/stair/cross Definitions remain available in the volume format; richer imported Shape assignments may use sidecar metadata or another mature tool only when art needs prove it useful.

## Articulation

Character assembly data is separate from cell geometry:

```text
part asset + assembly anchor
parent/child transform hierarchy
animation clips over transforms
attachment points
```

Blender or a simple declarative sidecar format may provide anchor hierarchies/animation if useful. The runtime does not need skeletal vertex skinning because each part is a rigid voxel mesh.

## Import is not mesh compilation

The importer produces or populates `VoxelModel` cell volumes. It does not create a separate model-only baked geometry format. The same `VoxelMesher` used for Chunks produces renderable geometry.

Static imported assets may cache serialized mesh results as a build/runtime optimization later, but that cache is derivative data and must be reproducible from the volume + definitions + mesher version.

## Validation

Import must fail before publication on:

- unsupported dimensions or coordinate overflow;
- unknown Definition/material mapping;
- invalid hierarchy references;
- missing required assembly part/anchor metadata;
- format/version errors.

A failed import never replaces the last valid asset revision.
