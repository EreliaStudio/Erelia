# Revision Summary — Unified Multi-Scale Voxel Architecture

This is a revision of the existing backlog, not a second implementation plan.

## Architectural replacement

Previous direction:

```text
MicroVoxel source model
→ custom modeling program
→ model-specific mesh compiler
→ BakedModel
→ model runtime

Chunk
→ Chunk::Baker
→ terrain runtime
```

Revised direction:

```text
Voxel::Cell → Definition → normalized Shape
                     ↓
                 VoxelVolume
              ↙              ↘
           Chunk            VoxelModel
              ↘              ↙
              OcclusionResolver
                     ↓
                VoxelMesher
                     ↓
             Mesh + Materials
```

A custom modeling application is no longer part of the required implementation. Imported assets are converted from mature external tools into the same VoxelVolume representation.

## Materially rewritten items

| Previous item | Revised item / decision |
|---|---|
| `ARCH-001 — Micro-Voxel Model Architecture` | `ARCH-001 — Unified Multi-Scale Voxel Volume` |
| dedicated `ModelSource`/MicroVoxel source representation | runtime-sized `VoxelModel` using the same `Voxel::Cell` representation as Chunk |
| model-specific offline compiler/BakedModel as authoritative model representation | one `VoxelMesher`; optional serialized mesh is only derivative cache |
| `ARCH-005 — Asset Pipeline` centered on custom modeling application | `ARCH-005 — External Voxel Asset Import and Articulation Pipeline` |
| `ME-001 — Voxel Runtime and Visual Identity` | `ME-001 — Unified Voxel Runtime and Visual Identity` |
| `ME-008 — Voxel Modeling Toolchain and Mesh Compilation` | `ME-008 — Voxel Asset Import and Character Assembly` |
| `EP-001 — Micro-Voxel Primitive and Material Runtime` | `EP-001 — Unified Voxel Volume and Cell Contract` |
| `EP-002 — Baked Models, Definitions, Animation, and Attachments` | `EP-002 — Voxel Models, Assemblies, Animation, and Mesh Reuse` |
| `EP-003 — Runtime Rendering, Material Effects, and Mesh Caching` | `EP-003 — Material Rendering, Mesh Runtime, and Caching` |
| `EP-030 — Voxel Modeling Program Foundation` | `EP-030 — External Voxel Asset Import and Volume Conversion` |
| `EP-031 — Micro-Voxel Authoring Tools` | `EP-031 — Articulated Voxel Character Assembly and Transform Animation` |
| `EP-032 — Mesh Compiler / Baked Format` | `EP-032 — Unified Voxel Mesher and Occlusion Context` |
| `EP-033 — Model Preview / Animation / Export` | `EP-033 — Multi-Scale Material Prototype and Visual Validation` |
| first playable required custom editor/compiler | new `VS-000` validates unified voxel visuals before `VS-001` gameplay slice |

## Preserved working behavior

The migration explicitly protects:

- fixed 16×16×16 Chunk semantics and coordinate/index order over shared VoxelVolume storage;
- compact packed `Voxel::Cell` representation;
- current normalized Shape JSON geometry;
- cube, slab, slope, stair and crossed-plane content;
- horizontal orientation and vertical flip;
- Definition lookup and Shape material slots;
- current atlas/UV rendering during the structural mesher refactor;
- hidden-face/neighbor occlusion behavior;
- cross-Chunk neighbor lookup through `Chunk::Collection` semantics;
- deterministic generation and headless world data.

Material/palette rendering is introduced only after generic-mesher terrain parity is automated.

## First validating milestone

[`VS-000 — Unified Voxel Visual Validation`](milestones/VS-000-UNIFIED-VOXEL-VISUAL-VALIDATION.md) is now the first visual architecture gate. It requires one scene containing generated terrain Chunks, cube/slab/slope/stair Shapes, palette Materials, an imported barrel, an articulated Hero and an attached weapon, all using the same `VoxelVolume → VoxelMesher → Material rendering` path.
