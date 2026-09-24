# OD-025 — VoxelMesher Chunk specialization and occlusion cache ownership

## Status

Resolved

## Problem

Chunk and runtime-sized VoxelModel data require the same cell iteration, Shape expansion, transform, visibility, and mesh-output algorithm, but only Chunk meshing may resolve an out-of-volume neighbor through adjacent world Chunks. The architecture must preserve that additional behavior without duplicating the meshing algorithm or losing the effective occlusion-result cache.

## Affected epics/tickets

[EP-032](../epics/EP-032-unified-voxel-mesher/EP-032-unified-voxel-mesher.md), consolidated [ST-032-02](../epics/EP-032-unified-voxel-mesher/tickets/ST-032-02-generic-cell-iteration-and-scale-aware-shape-transform.md), and the later [ST-004-01](../epics/EP-004-chunk-world-generation/tickets/ST-004-01-chunk-mesher-integration.md) consumer transition. ST-032-03 and ST-004-02 remain superseded historical links.

## Known constraints

- Chunk and VoxelModel must share one generic meshing implementation.
- VoxelMesher must remain usable for a standalone VoxelVolume without Chunk/world dependencies.
- A missing or unavailable external neighbor is treated as the existing empty `Voxel::Cell{}` representation (runtime ID `0`).
- The cache for transformed polygon/occluder results is performance-relevant and must remain shared generic behavior.
- OD-020 still owns later merging, final indexing, and hard-normal policy.

## Options considered

- Inject independent `OcclusionResolver` and `ChunkOcclusionResolver` strategy objects.
- Put Chunk lookup directly into one mesher and branch on the concrete volume type.
- Keep one inheritable `VoxelMesher` algorithm with a narrow overridable outside-volume neighbor hook, then derive `Chunk::Mesher` only for Chunk lookup.

## Decision

- `VoxelMesher` owns deterministic volume iteration, in-bounds cell lookup, Shape expansion/transform, visibility/occlusion evaluation, mesh emission, and the reusable occlusion-result cache.
- Neighbor coordinates inside the volume are resolved directly by `VoxelMesher` through the common read contract.
- Only an out-of-volume coordinate reaches a protected virtual neighbor hook. The base implementation returns `Voxel::Cell{}`, so standalone VoxelVolume and VoxelModel boundaries are empty.
- `Chunk::Mesher` derives from `VoxelMesher` and overrides only that hook. It maps the out-of-volume coordinate through the source Chunk coordinate and `Chunk::Collection` to query an adjacent available Chunk; missing Chunk data returns `Voxel::Cell{}`.
- `Chunk::Mesher` must not duplicate the iteration, Shape, occlusion, cache, or mesh-emission algorithm.
- The exact protected method name is an implementation detail; its call boundary and semantics are the contract.

## Rationale

Inheritance expresses the one intentional behavioral specialization while keeping the expensive and correctness-sensitive algorithm in one place. Models and standalone volumes get the simplest safe boundary rule. Chunk meshing adds world lookup without teaching the generic mesher about Chunk collection, streaming, or concrete volume type tests. Keeping the cache in the base ensures every meshing path benefits and preserves the current optimization.

## Consequences

- Consolidated ST-032-02 introduces the common VoxelMesher, default outside-is-empty hook, and concrete Chunk::Mesher override together.
- EP-004/ST-004-01 later redirects production Chunk consumers from `Chunk::Baker` to the already validated `Chunk::Mesher`.
- The earlier standalone `OcclusionResolver`/`ChunkOcclusionResolver` strategy-object plan is superseded.
- Future specializations may override only external neighbor resolution unless another user-approved decision expands the inheritance contract.

## Validation / evidence

- ST-032-01 canonical fixtures remain the parity oracle for positions, normals, atlas UVs, winding, visible topology, and Chunk-boundary outcomes.
- Consolidated ST-032-02 proves standalone outside-is-empty behavior, cache reuse, adjacent/missing Chunk lookup, all six boundary directions, and unchanged cross-Chunk occlusion.

## Resolution provenance

Resolved by the project owner during ST-032-01 execution on 20 September 2026: one generic VoxelMesher base, a Chunk::Mesher subclass that overrides external-neighbor lookup, and retention of the shared occlusion-result cache.
