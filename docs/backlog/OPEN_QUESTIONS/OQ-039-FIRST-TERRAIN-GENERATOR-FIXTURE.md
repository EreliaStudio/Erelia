# OQ-039 — What exact basic terrain generator should be the first deterministic fixture?

**Status:** Partially resolved
**Decision records:** [DR-015](../DECISIONS/DR-015-FIRST-TERRAIN-VALIDATION-SCENE.md)
**Affected areas:** EP-001, Server terrain generation, visual validation

## Question

What exact basic terrain generator should be the first deterministic fixture?

## Problem / context

The first generator should be simple enough to debug networking/meshing while exercising horizontal, vertical, underside and transformed voxel Shapes.

## Known constraints

- The result is a deterministic technical validation scene, not production world generation.
- It must exercise adjacent Chunks and Shape Orientation/Flip.

## Possible solutions

1. Flat plane only.
2. Flat baseline plus axis-aligned walls and elevated Shape fixtures.
3. Seeded noise/height-field terrain immediately.

## Remaining ambiguity

Exact authored coordinates, wall extents/heights, Definition IDs/material choices, and the complete slab/slope/stair Orientation/Flip fixture table remain open.

## Chosen solution

Generate a flat baseline, walls around the X=0 and Z=0 world planes, and elevated stairs/slabs/slopes around Y≈3 with varied Orientation/Flip so they can be inspected from above and below. Exact fixture coordinates/Definitions remain to be fixed.
