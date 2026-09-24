# ARCH-003 — Sparkle Integration Boundary

Sparkle provides generic engine/runtime capabilities; Erelia owns game-domain semantics.

## Strong Sparkle candidates after proof in Playground

- generic read-only voxel-volume/grid access contract with runtime dimensions and uniform cell scale;
- reusable voxel mesher operating on `Voxel::Cell`-like Definition/Shape resolution only if generalized beyond Erelia cleanly;
- generalized neighbor-access callback/context independent of world streaming;
- generic Material/render-parameter abstraction;
- mesh/GPU resources, caching and render passes;
- math, JSON/content primitives and stable IDs;
- thread-safe snapshot/FIFO primitives;
- profiling/statistics;
- platform/window/surface/render runtimes.

## Keep in Playground/Erelia until proven reusable

- `Chunk` generation/streaming semantics;
- Erelia VoxelDefinition gameplay fields;
- external-content import mappings and game content manifests;
- articulated Hero part naming and equipment attachment conventions;
- world generation, navigation, encounters, combat, economy and dungeons;
- Erelia-specific Material palettes/effects.

## Integration rule

The unified voxel work is first proven in Playground. Move a component into Sparkle only once its API no longer depends on Erelia gameplay, content paths, `Chunk::Collection`, or one specific imported asset format.

Do not turn the Sparkle integration into a prerequisite for the visual validation milestone.
