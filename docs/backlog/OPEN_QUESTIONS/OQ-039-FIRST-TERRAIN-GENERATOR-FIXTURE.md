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

ST-001-04 / DR-018 now define the shared Shape/Definition/Catalog representation, the canonical +X Shape orientation, exact Orientation/Flip transforms, and the first cube/slab/slope/stair Shape resources.

What remains open here is specific to the later deterministic terrain-generator scene: exact authored world coordinates, wall extents/heights, exact scene Definition IDs, material choices, and the complete placement table for the validation fixture. These remaining details block ST-001-06 and later scene validation, but no longer block ST-001-04.

## Chosen solution

Generate a flat baseline, walls around the X=0 and Z=0 world planes, and elevated stairs/slabs/slopes around Y≈3 with varied Orientation/Flip so they can be inspected from above and below. Exact fixture coordinates/Definitions remain to be fixed.
