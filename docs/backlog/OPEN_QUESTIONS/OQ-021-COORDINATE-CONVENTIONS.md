# OQ-021 — What coordinate conventions become architectural contracts?

**Status:** Resolved
**Decision records:** [DR-011](../DECISIONS/DR-011-VOXEL-COORDINATES.md)
**Affected areas:** Voxel coordinates, Chunk addressing, rendering

## Question

What coordinate conventions become architectural contracts?

## Problem / context

Generation, streaming, meshing and rendering need one convention, especially around negative coordinates and Chunk boundaries.

## Known constraints

- Sparkle uses +Y as up and -Z as forward.
- Terrain Chunks are 16×16×16 cells at one world unit per cell.

## Possible solutions

1. Introduce an Erelia-specific axis remapping layer.
2. Follow Sparkle coordinates directly and define floor-division/modulo Chunk conversion.
3. Use truncating integer division for simplicity.

## Chosen solution

Use +Y up and -Z forward. Terrain cells and Chunk coordinates use integer 3D coordinates; global-to-Chunk conversion uses mathematical floor division by 16 and floor modulo, including negative coordinates.
