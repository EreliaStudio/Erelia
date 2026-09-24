# ARCH-001 — Unified Multi-Scale Voxel Volume

## Decision

Erelia uses **one fundamental voxel-volume representation** for persistent terrain chunks and voxel-authored models. `Chunk` and `VoxelModel` are semantic users of the same cell/grid contract, not separate graphics formats.

As resolved by [OD-011](../open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md), `VoxelVolume` is a concrete vector-owning base. It exposes:

```text
dimensions
voxel cells
coordinate/index access
local bounds
uniform voxelSize
batched edit session + version notification
```

The cell representation remains based on the existing `Voxel::Cell → Voxel::Definition → Voxel::Shape` chain unless measured implementation evidence requires change.

## Scale

A normalized Shape remains approximately inside `[0,1]^3`. Geometry mapping is:

```text
volumeLocalPosition = (voxelCoordinate + shapeLocalVertex) × voxelSize
```

Examples:

```text
Chunk: 16×16×16, voxelSize = 1.0
→ 16×16×16 world-unit local extent

Model: 8×8×16, voxelSize = 0.1
→ 0.8×0.8×1.6 local extent
```

Start with one uniform scalar size. Entity/world transforms may scale the resulting object separately. Independent X/Y/Z cell dimensions are a later decision only if real content requires them.

## Shape and cell reuse

The current Shape system is valuable because normalized cube/slab/slope/stair/cross geometry automatically works at any volume scale. Orientation, vertical flip and material-slot semantics must remain usable for every volume.

Do not introduce `TerrainVoxel` versus `ModelVoxel` merely because ownership differs. A model may use a normal cube Definition most of the time but may also use slab/slope/stair Definitions if useful.

## Chunk specialization

A Chunk retains world-specific responsibilities outside the generic volume contract:

- fixed 16³ dimensions over the storage inherited from `VoxelVolume`;
- chunk coordinate and world/local conversion;
- `Chunk::Collection` membership;
- deterministic generation;
- streaming/request lifecycle;
- cross-chunk neighbor access;
- bake scheduling.

The generic volume must not absorb these responsibilities.

The earlier option to retain a separate fixed `std::array` inside Chunk was superseded by the project owner's OD-011 decision. ST-001-03 must prove that migrating storage ownership does not change Chunk indexing, editing, or rendered output.

## VoxelModel specialization

A VoxelModel uses the same cell semantics with:

- arbitrary runtime dimensions;
- smaller configurable voxel size;
- imported/authored asset source;
- entity/prop/equipment lifetime;
- transform-based placement instead of chunk coordinates.

The model's detailed cells are visual data unless explicit gameplay systems say otherwise.

## One mesher

Current meshing architecture:

```text
VoxelVolume + DefinitionCatalog
              ↓
 VoxelMesher (cached occlusion + exact compatible indexing)
              ↑
 Chunk::Mesher (outside-volume neighbor override only)
              ↓
 TextureMesh3D(position, hard normal, atlas UV)
```

The base mesher is independent of `Chunk::Collection` and treats outside-volume cells as empty. `Chunk::Mesher` maps only out-of-bounds local neighbors through the surrounding collection; it does not duplicate the common algorithm.

The mesher must preserve the current behavior for shape transforms, hidden-face elimination, partial neighbor exposure/occlusion supported by the existing implementation and material assignment.

As resolved by [OD-020](../open-decisions/OD-020-mesher-merging-indexing-and-hard-normal-policy.md), emitted polygons are not merged. Vertices reuse the first deterministically encountered index only when stored position, hard polygon normal, and atlas UV are exactly equal. Palette-element data becomes another compatibility attribute when ST-003-02 introduces it; it is not part of the current textured mesh.

## Articulated characters

A Hero is a VoxelAssembly of independently meshed rigid VoxelModels. Named assembly-anchor transforms animate parts; animation must not edit cells or invoke the mesher. Equipment consists of reusable VoxelModels attached to named assembly anchors. VoxelModel files contain no anchors or hierarchy metadata.

## Editing and cache invalidation

Only voxel-data change invalidates a volume mesh:

```text
edit cells → increment/version or dirty flag → remesh affected volume
```

`VoxelVolume::Editor` is the generic mutation boundary. It retains the current Chunk editor API: `set(coordinate, cell)` records real changes, and `commit()` or RAII destruction publishes at most one volume invalidation for the entire session. A session containing only no-op assignments publishes no invalidation. Chunk-specific scheduling and neighbor invalidation remain outside the volume.

VoxelVolume-owned contract failures use `spk::Exception`. Its `message()` is the stable programmatic diagnostic, while `what()` includes Sparkle's source location and any nested cause. The contract does not expose separate standard exception subtypes.

Entity transform, camera movement, animation transforms, palette changes and ordinary material parameter changes do not remesh geometry.

## Authoritative boundary

Visual voxels are not implicitly simulation units. Chunks feed navigation/world collision through explicit world systems. Entity assemblies use explicit collision/occupancy data. Rendering buffers and Material effects are never authoritative gameplay truth.

## Migration constraint

The first refactor keeps the existing textured/atlas output so old and new meshers can be compared. Material/palette migration occurs only after terrain parity is automated.
