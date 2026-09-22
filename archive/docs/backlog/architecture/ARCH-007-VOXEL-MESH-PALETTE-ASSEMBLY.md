# ARCH-007 — Voxel Meshing, Palette Binding, and Assembly Contract

## Fixed pipeline

```text
VoxelVolume + DefinitionCatalog
              ↓
 VoxelMesher (cached occlusion + default empty boundary)
              ↑
 Chunk::Mesher (external-neighbor override only)
              ↓
 exact position/normal/UV first-seen indexing
              ↓
 TextureMesh3D + current atlas rendering
```

- `VoxelVolume` owns one runtime-sized vector of cells, generic versioning, and a batched RAII editor. It exposes dimensions, uniform voxel scale, bounds, checked coordinate access, and a read-only span. One changed edit session publishes one invalidation; it owns no world lookup, palette, animation, or rendering behavior.
- `VoxelMesher` owns the common iteration, Shape expansion/transform, visibility evaluation, mesh emission, and reusable occlusion-result cache. In-bounds neighbors come from the common volume read contract; its protected outside-volume hook returns `Voxel::Cell{}` by default.
- `Chunk::Mesher` is the EP-004 world-aware subclass. It overrides only outside-volume neighbor lookup through Chunk world/collection state so a voxel in Chunk B can occlude a boundary polygon in Chunk A.
- Base `VoxelMesher` never depends directly on `Chunk::Collection`; `Chunk::Mesher` does not duplicate the generic algorithm or cache.
- OD-020 preserves emitted polygons and hard flat normals. The common output path reuses a vertex only for exact stored position/normal/UV equality and assigns its index at the first deterministic encounter.
- `MaterialResolver`, palette-element vertex data, Palette validation/binding, and the common palette shader are the next ST-003-02 migration, not part of ST-032-04.

## Epic ownership

| Epic | Owns | Does not own |
|---|---|---|
| EP-001 | Concrete owning volume contract; dimensions/cell access/uniform scale/bounds; batched editing/versioning; fixed-dimension 16³ Chunk adaptation; semantic runtime-sized VoxelModel type | World lookup, streaming, external occlusion, meshing, rendering |
| EP-032 | Generic VoxelMesher; cached occlusion; default outside-is-empty hook; Shape expansion; visibility; deterministic exact-compatible TextureMesh3D indexing | Chunk::Collection, Palette resources/data/binding, shader migration, streaming, GPU upload |
| EP-004 | Chunk::Mesher outside-neighbor override; world/local lookup; Chunk integration; bake scheduling; dirty/version and neighbor invalidation; streaming/generation integration | A second meshing algorithm or duplicate occlusion cache |

The structural order is EP-001 → EP-032 → EP-004 at the required-contract level. EP-001 plus EP-029 feeds EP-030. EP-001, EP-030, and EP-032 feed EP-002; EP-002 then feeds EP-031.

## Pure VoxelModel and separate VoxelAssembly

A VoxelModel asset is reusable pure volume data: dimensions, voxel scale, occupancy/Shape-or-cell data, and an authored palette element index. It contains no anchors, pivots, parents, skeleton, animation, equipment slots, or world transform.

A VoxelAssembly separately defines named anchors, optional parent anchors, local transforms, and parts that reference reusable VoxelModels. Animation clips target anchor transforms. Hierarchy evaluation produces per-part model matrices. It never edits voxel geometry or invokes `VoxelMesher`; cached part meshes remain valid.

Equipment uses the same named-anchor mechanism and reusable model parts. The exact anchor vocabulary and serialized model/assembly/animation syntax require user-approved sample assets before they are frozen. EP-002 owns runtime representation and execution. EP-031 owns external authoring/import and production fixtures that consume EP-002 contracts.

## Palette and mesh contract

The following is the target contract owned by ST-003-02 and later Palette tickets. It is not current ST-032-04 behavior.

A CPU-side `PaletteCollection` may manage Palette resources, but it is not uploaded as one global GPU buffer. Each `Palette` is independently associated with its own SSBO. A render command binds the program, mesh vertex/index buffers, the Palette required by that renderable, transform data, and then draws.

As resolved by [OD-026](../open-decisions/OD-026-initial-palette-element-resolver-and-command-lifetime.md), `Palette::Data` initially contains one RGBA color. `Palette` is default-constructible. `resize(count)` defines the number of `Palette::Data` elements and exposes them through `data(index)` for CPU-side editing. Resizing does not implicitly validate/upload the resource; after construction, resize, or data modification, the caller explicitly invokes `validate()` when the Palette is ready for GPU use. `MaterialResolver` receives immutable volume, Definition, transformed-polygon, coordinate, and Cell context and returns only the element index. The render command stores a Palette value copy, which shares and retains Sparkle's underlying GPU resource. [OD-027](../open-decisions/OD-027-render-command-palette-trust-boundary.md) makes compatibility a caller precondition: the command accepts `const Palette&` and does not rescan mesh vertices.

`VoxelMesh` vertices semantically carry at least `position`, `normal`, and integer `paletteElementIndex`. The index is flat/non-interpolated and addresses the currently bound Palette SSBO. It is not a Voxel ID, and no GPU `paletteId` exists in the initial design.

Palette selection belongs to the renderable/part instance, not the pure model asset. Consequently one cached arm mesh can render with Human, Orc, or Undead palettes without remeshing. All referenced indices must exist in the bound Palette; submitting a compatible mesh/Palette pair is the caller's responsibility.

Chunks normally share one relatively large `WorldPalette`. Chunk material resolution may use both the Shape polygon's semantic `materialSlot` and spatial `outerSide`; these concepts remain distinct. A dedicated content-schema ticket defines precedence among default, slot, side, and exact slot+side overrides after a user-approved example. Top, side, bottom, and individual sides can resolve to different elements of the same WorldPalette.

Chunks and VoxelModels use the same VoxelMesh format and voxel shader. Changing the bound Palette changes material output but neither mesh geometry nor mesher invocation count.

## Visual-regression sequence

1. Capture and manually approve current textured Chunk images.
2. Introduce VoxelVolume, the generic cached VoxelMesher/default boundary hook, and the Chunk::Mesher override.
3. Prove semantic mesh parity and unchanged textured images.
4. Introduce palette-based rendering as a separate intentional migration.
5. Review expected, produced, and difference images manually.
6. Only after approval, add a new versioned expected baseline.

Required semantic fixtures cover authored model indices, Chunk slot/side mapping, top/side/bottom and per-side variation, identical geometry across Palette swaps, no remesh on Palette swap, consistent polygon vertex indices, internal-face occlusion, multiple Shapes, and deterministic output. Golden fixtures cover the corresponding visible cases and render one model mesh with two compatible Palettes plus a Chunk through WorldPalette.
