# OQ-028 — Is the GDD visual-validation prerequisite the actual first milestone?

**Status:** Resolved
**Decision records:** [DR-009](../DECISIONS/DR-009-FIRST-VOXEL-TERRAIN-MILESTONE.md)
**Affected areas:** Roadmap, voxel terrain, first implementation milestone

## Question

Is the GDD visual-validation prerequisite the actual first milestone?

## Problem / context

The project needs a first implementation slice that validates foundational technology without prematurely designing broad gameplay.

## Known constraints

- Voxel rendering and Server simulation/networking are foundational.
- Production character-control semantics should not block terrain validation.

## Possible solutions

1. Build a broad gameplay vertical slice first.
2. Validate only Client rendering without the Server boundary.
3. Build an end-to-end terrain pipeline through the real dedicated Server/Client boundary.

## Chosen solution

The first implementation milestone is EP-001: Server-generated terrain Chunks, real network delivery, Client meshing/rendering and temporary free-flight visual inspection.
