# ME-001 — Unified Voxel Runtime and Visual Identity

## Goal

Evolve the working terrain voxel implementation into one reusable multi-scale voxel-volume/meshing/material path without regressing current Chunk behavior.

## Epics

- [EP-001 — Unified Voxel Volume and Cell Contract](../epics/EP-001-unified-voxel-volume/EP-001-unified-voxel-volume.md)
- [EP-002 — Voxel Models, Assemblies, Animation, and Mesh Reuse](../epics/EP-002-voxel-model-assemblies/EP-002-voxel-model-assemblies.md)
- [EP-003 — Material Rendering, Mesh Runtime, and Caching](../epics/EP-003-runtime-rendering-mesh-cache/EP-003-runtime-rendering-mesh-cache.md)
- [EP-032 — Unified Voxel Mesher and Occlusion Context](../epics/EP-032-unified-voxel-mesher/EP-032-unified-voxel-mesher.md)
- [EP-033 — Multi-Scale Material Prototype and Visual Validation](../epics/EP-033-material-visual-validation/EP-033-material-visual-validation.md)

## Meta-epic invariants

- A 16³ Chunk and arbitrary VoxelModel expose the same meshing-facing cell/scale contract.
- One VoxelMesher base owns the common model/terrain algorithm and occlusion cache; no terrain/model meshing-loop fork exists.
- Standalone boundaries default to empty through the VoxelMesher hook; `Chunk::Mesher` overrides only outside-volume lookup, and its `Chunk::Collection`/world context stays in EP-004.
- `MaterialResolver` remains an independent input.
- `VoxelMesh` carries a flat integer `paletteElementIndex` into the currently bound Palette SSBO. Chunk and model rendering use the same mesh/shader contract and no GPU palette ID.
- Each Palette is independently bindable; CPU-side PaletteCollection management never forces one global PaletteCollection SSBO.
- Existing cube/slab/slope/stair/cross shape behavior, orientation, vertical flip and cross-chunk occlusion remain covered by regression tests.
- Scale affects geometry coordinates, not logical grid neighborhood.
- Pure VoxelModels contain only volume data. VoxelAssembly owns anchors, part references, animation transforms, and reusable equipment attachments; transform/animation and Palette swaps do not trigger remeshing.
- Material sampling remains stable in the intended coordinate space.
- Headless world/model data and meshing tests require no GPU.
