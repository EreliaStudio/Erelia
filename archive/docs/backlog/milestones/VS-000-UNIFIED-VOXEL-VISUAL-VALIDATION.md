# VS-000 — Unified Voxel Visual Validation

## Purpose

Validate the selected multi-scale voxel direction **before** large gameplay/content investment. This milestone is successful only if the working terrain behavior survives the refactor and small-scale voxel assets are visually/technically convincing without a custom modeling application.

## Required scene

One runtime scene contains simultaneously:

- generated 16×16×16 terrain Chunks at voxelSize 1.0;
- visible cube, slab, slope and stair Shape usage;
- palette Material terrain prototype;
- one externally authored/imported small-scale barrel VoxelModel;
- one articulated Hero assembled from multiple small-scale VoxelModels;
- one attached voxel weapon;
- lighting/camera sufficient to judge the art direction.

All voxel geometry must pass through the same fundamental path:

```text
VoxelVolume + OcclusionResolver + MaterialResolver → VoxelMesher → VoxelMesh → same voxel shader + bound Palette SSBO
```

## Entry gates

- Sparkle PR #1 is merged and its exported optional `Sparkle::TestLibrary` is available. Playground has consumed the installed component, configured project-owned reference/result roots, wired the supported GPU job, and retained failure artifacts.
- Current Chunk mesh semantic regression fixtures and reviewed pre-refactor textured PNG references exist.
- EP-001/EP-032 pass both semantic and image parity before material changes; any palette/non-textured image-baseline transition follows the explicit review/versioning gate in [ARCH-004](../architecture/ARCH-004-TEST-STRATEGY.md#later-non-textured-rendering-migration).
- EP-001 volume contract can adapt current Chunk and own runtime-sized model storage.
- EP-032 has old/new Chunk parity on representative fixtures.
- EP-030 can import at least one external model source without custom authoring software.

## System acceptance cases

1. **Terrain parity:** current representative Chunk fixtures retain cell/Shape/orientation/flip/occlusion semantics after old Baker path is replaced.
2. **Cross-Chunk neighbor:** boundary occlusion uses adjacent Chunk through resolver/context rather than VoxelMesher coupling to `Chunk::Collection`.
3. **Scale:** terrain 16³@1.0 and small models @<1.0 produce correct geometric extents while using identical grid-neighbor logic.
4. **Shared mesher:** telemetry/test instrumentation demonstrates terrain, barrel, Hero parts and weapon all use EP-032.
5. **Material terrain stability:** intended world-space palette variation is stable under camera movement and seamless across Chunk boundary fixture.
6. **Material object stability:** barrel/weapon object-local variation moves with object and does not swim.
7. **Articulation:** Hero idle/walk pose updates transforms with zero remesh after initial mesh availability.
8. **Equipment:** weapon attachment follows hand/part transform and can be swapped without body remesh.
9. **Edit invalidation:** changing a test barrel cell causes barrel remesh; moving it does not.
10. **Headless parity:** volume/import/meshing tests run without GPU; presentation scene does not change authoritative state hash.
11. **Performance capture:** record meshing time, mesh triangle/vertex counts, cache hits, draw calls and frame cost for the scene.
12. **Decision record:** document whether the visual result is accepted, what Material sampling rule is chosen, and any measured blocker before H1 content production.
13. **Palette contract:** the Chunk scene binds one shared WorldPalette, model instances bind their selected smaller Palettes, the shader consumes flat `paletteElementIndex`, and Palette swaps alter no geometry or mesher count.
14. **Strict visual gate:** semantic parity alone cannot satisfy VS-000. The textured refactor image remains identical; the later palette change is reviewed through old/actual/difference images before a new versioned baseline is accepted.

## Explicit non-goals

- custom voxel editor;
- custom animation editor;
- GPU meshing;
- voxel LOD/SVO;
- per-voxel physics/networking;
- full PBR/material graph;
- automatic object fracturing.
