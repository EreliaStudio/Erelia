# DR-013 — Terrain meshes are Client-owned

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Server terrain runtime, network Chunk payloads, Client meshing/rendering

## Context

EP-001 needs a clean authority/data boundary between headless terrain generation and graphical rendering.

## Decision

The Server **never emits terrain render meshes**.

The Server owns/canonicalizes voxel terrain data and sends terrain Chunks as cell/volume data.

The Client owns:

- receiving Chunk voxel data;
- storing the Client-side Chunk/Volume representation;
- converting voxel data into render meshes;
- rendering those meshes;
- invalidating/remeshing Client meshes when relevant voxel/neighbor data changes.

Mesh data is a Client-side derived artifact, not authoritative world state.

## Consequences

- Server remains headless and has no terrain GPU-mesh dependency.
- Wire contracts carry voxel data, not vertices/indices/render commands.
- Core may own generic voxel/Shape algorithms needed by both sides, but rendering resources remain Client-owned.
- Missing-neighbor behavior at Chunk boundaries still requires an explicit policy before meshing tickets become Ready.

## Required tests

- Server terrain generation tests require no graphics context.
- protocol tests prove Chunk responses contain canonical voxel data rather than render meshes.
- Client mesher tests derive expected geometry from exact voxel fixtures.
- cross-Chunk boundary tests are required once missing-neighbor behavior is resolved.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-036.

## Supersession

None.
