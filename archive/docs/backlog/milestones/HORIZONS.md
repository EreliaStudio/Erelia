# Planning Horizons

## H0 — Foundations and visual validation

Resolve expensive-to-change contracts while protecting the working voxel world:

- EP-000 test harness/CI and regression fixtures;
- EP-001 unified volume/cell/scale contract;
- EP-032 generic neighbor context + VoxelMesher extracted from current Chunk::Baker;
- EP-004/005 deterministic world/traversal parity;
- EP-029 versioned voxel/model/material content contracts;
- EP-030 one external voxel import path;
- EP-002/031 rigid-part model/character assembly;
- EP-003 minimal Material/render/cache path;
- EP-033 multi-scale visual prototype;
- EP-006 simulation clocks and EP-027 early profiling where useful.

H0 culminates in [`VS-000 — Unified Voxel Visual Validation`](VS-000-UNIFIED-VOXEL-VISUAL-VALIDATION.md). Do not start broad Hero/enemy/equipment art production before this scene proves the direction.

## H1 — First playable

Cut vertically across gameplay epics to prove the Erelia loop using the architecture validated by VS-000. Content breadth remains deliberately narrow.

## H2 — Onlineization and scale

Separate authority into dedicated hosting, add replication/interest/reconciliation and harden persistence/operations.

## H3 — Economy/content depth

Expand marketplace procurement robustness, content catalog, Material effects and scale/performance based on profiling.

The plan is intentionally not `EP-000 → EP-033` waterfall order. Dependencies identify contracts; milestones select the smallest coherent vertical slice.
