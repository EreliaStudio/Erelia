# ME-000 — Erelia Implementation

## Goal

Coordinate the complete implementation while preserving headless authority, deterministic behavior, testability and the unified multi-scale voxel architecture.

## Capability Meta-Epics

- [ME-001 — Unified Voxel Runtime and Visual Identity](me-001-unified-voxel-runtime-and-visual-identity.md)
- [ME-002 — World Exploration and Navigation](me-002-world-exploration-and-navigation.md)
- [ME-003 — Tactical Encounter and Combat](me-003-tactical-encounter-and-combat.md)
- [ME-004 — Heroes, Items, Crafting, and Economy](me-004-heroes,-items,-crafting,-and-economy.md)
- [ME-005 — Persistent Frontier and Dungeons](me-005-persistent-frontier-and-dungeons.md)
- [ME-006 — Authoritative Online Runtime](me-006-authoritative-online-runtime.md)
- [ME-007 — Quality, Observability, and Content Contracts](me-007-quality,-observability,-and-content-contracts.md)
- [ME-008 — Voxel Asset Import and Character Assembly](me-008-voxel-asset-import-and-character-assembly.md)

## Root system invariants

1. Same ServerSeed + WorldID + generation/content version produces the same immutable base World.
2. Dynamic server state may differ across servers sharing that seed.
3. Ordinary gameplay does not permanently mutate base terrain geometry.
4. One item has exactly one authoritative owner/location.
5. One participant belongs to at most one encounter.
6. One entity belongs to exactly one authoritative time domain.
7. Pausing one encounter does not pause Region Clock/other encounters.
8. Presentation/animation/material state cannot create authoritative gameplay outcomes.
9. Failed authoritative transactions cannot leave impossible half-state.
10. Replay of an accepted deterministic command stream reaches the same authoritative terminal state.
11. Gameplay suite runs headlessly without Window/Surface/GPU.
12. Persistent long-lived data uses versioned IDs/schemas.
13. Chunk and VoxelModel cell data share the same voxel-volume/Definition/Shape representation at different scales.
14. One generalized VoxelMesher serves terrain and voxel-authored models unless measured evidence later justifies a split.
