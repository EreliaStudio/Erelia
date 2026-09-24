# OQ-036 — Does the Client own terrain meshing, and what neighbor policy applies?

**Status:** Partially resolved
**Decision records:** [DR-013](../DECISIONS/DR-013-CLIENT-TERRAIN-MESHING.md)
**Affected areas:** EP-001, Client meshing, Chunk boundaries

## Question

Does the Client own terrain meshing, and what neighbor policy applies?

## Problem / context

The Server must stay headless and send canonical voxel data, but a Client mesher needs a policy for faces at Chunk boundaries when neighboring Chunk data has not arrived yet.

## Known constraints

- Server never sends terrain render meshes.
- Client derives/render meshes from received voxel data.
- Cross-Chunk occlusion must become correct once neighbor data is available.

## Possible solutions

1. Treat absent neighbors as empty, render immediately and remesh affected boundaries when neighbors arrive.
2. Delay a Chunk mesh until all required neighbors are present.
3. Use another explicit boundary representation supplied separately from full neighbor Chunks.

## Remaining ambiguity

Behavior while neighbor data is missing and exact remesh invalidation scope are not chosen yet.

## Chosen solution

Client owns all terrain meshing/rendering; Server sends only voxel/Volume data. The missing-neighbor and remesh policy remains open.
