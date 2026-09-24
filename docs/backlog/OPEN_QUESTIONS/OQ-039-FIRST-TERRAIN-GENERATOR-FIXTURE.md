# OQ-039 — What exact basic terrain generator should be the first deterministic fixture?

**Status:** Resolved
**Decision records:** [DR-015](../DECISIONS/DR-015-FIRST-TERRAIN-VALIDATION-SCENE.md)
**Affected areas:** EP-001, Server terrain generation, visual validation

## Question

What exact basic terrain generator should be the first deterministic fixture?

## Problem / context

The first generator should be simple enough to debug networking/meshing while exercising horizontal, vertical, underside, adjacency and transformed voxel Shapes.

## Considered directions

1. Flat plane only.
2. Flat baseline plus axis-aligned walls and deliberately authored Shape fixtures.
3. Seeded noise/height-field terrain immediately.

## Chosen solution

Use option 2.

The exact approved fixture is recorded in DR-015:

- infinite X/Z cube baseline at world Y=0;
- cube walls on X=0 and Z=0 for Y=1..3;
- Definition IDs 1 cube, 2 slope, 3 stair, 4 slab;
- exact `<shape-id>-<slot-id>` material strings;
- dedicated slope Chunk `(1,0,1)`, stair Chunk `(2,0,1)`, slab Chunk `(1,0,2)`;
- identical ground/elevated local placement matrices for all three Shapes;
- all eight Orientation/Flip combinations in each matrix;
- horizontal adjacency and vertical stacking/contact;
- elevated geometry beginning at Y=4;
- every non-authored Cell is Air/Empty, including all world space below Y=0;
- exact approved positive/negative Chunk validation set.

The generator remains an artificial technical fixture rather than a production terrain algorithm.

## Remaining ambiguity

None in the **scene/fixture definition** itself.

ST-001-06 still has independent readiness questions about the general Provider/Collection API edge semantics and the desired depth of prototype-output assertions. Those are implementation-contract questions, not unresolved OQ-039 scene geometry.

## Resolution provenance

Partially resolved on 22 September 2026 when the project owner selected the baseline/walls/elevated-Shape direction.

Fully resolved on 24 September 2026 when the project owner approved the exact Definition/material mapping, infinite baseline/wall extents, Shape fixture Chunk coordinates, ground/elevated placement matrices, complete Orientation/Flip table, default-empty rule and positive/negative validation Chunk set.
