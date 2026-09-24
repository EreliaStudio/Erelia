# DR-015 — First deterministic terrain generator is a visual validation scene

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Last clarified:** 2026-09-24
**Applies to:** EP-001 Server Chunk generation, Client terrain rendering, golden/manual validation

## Context

The first terrain implementation must be simple enough to debug coordinates, networking, Chunk boundaries, Shape transforms and later meshing while exercising more geometry than a featureless plane.

It is deliberately temporary technical terrain, not the production world-generation algorithm.

## Decision

The first Server implementation is a deterministic validation scene with an infinite horizontal baseline, two axis walls, and three dedicated Shape-fixture Chunks.

### Definitions

Definition ID 0 remains Air/Empty.

The four authored validation Definitions are exactly:

| Definition ID | Shape |
| ---: | --- |
| 1 | `cube` |
| 2 | `slope` |
| 3 | `stair` |
| 4 | `slab` |

Material binding uses the exact string rule:

```text
<shape-id>-<slot-id>
```

Therefore:

- cube: `cube-side`, `cube-top`, `cube-bottom`;
- slope: `slope-side`, `slope-top`, `slope-bottom`;
- stair: `stair-side`, `stair-top`, `stair-bottom`;
- slab: `slab-side`, `slab-top`, `slab-bottom`.

These are semantic `Voxel::Material::ID` strings only. ST-001-06 does not add a rendered material system.

### Baseline

Every world Cell satisfying:

```text
Y = 0
```

is Definition 1 (`cube`) for every X/Z coordinate.

This is an infinite X/Z validation baseline. A later rendering fixture chooses only a finite set of loaded Chunks; the generator itself does not have a special finite-world edge.

### Axis walls

On top of the baseline, Definition 1 cubes occupy:

```text
X = 0, any Z, Y = 1..3
Z = 0, any X, Y = 1..3
```

The X=0/Z=0 intersection is one Cell, not duplicated state.

### Dedicated Shape-fixture Chunks

The non-cube Shape fixtures live in these exact Chunk coordinates:

| Shape | Definition | Chunk coordinate |
| --- | ---: | --- |
| slope | 2 | `(1, 0, 1)` |
| stair | 3 | `(2, 0, 1)` |
| slab | 4 | `(1, 0, 2)` |

All three use the **same local placement matrix**; only their Definition ID differs.

#### Ground transform matrix

All ground Shape cells are at local/world-within-that-Chunk `Y = 1`:

| Local coordinate | Orientation | FlipOrientation |
| --- | --- | --- |
| `(4, 1, 4)` | PositiveX | PositiveY |
| `(5, 1, 4)` | NegativeZ | PositiveY |
| `(6, 1, 4)` | NegativeX | PositiveY |
| `(7, 1, 4)` | PositiveZ | PositiveY |
| `(4, 1, 5)` | PositiveX | NegativeY |
| `(5, 1, 5)` | NegativeZ | NegativeY |
| `(6, 1, 5)` | NegativeX | NegativeY |
| `(7, 1, 5)` | PositiveZ | NegativeY |

This compact 4x2 group deliberately creates horizontal adjacency.

#### Elevated transform/contact matrix

The elevated group starts at world/local `Y = 4`, leaving empty space beneath it, and uses two vertically stacked rows:

| Local coordinate | Orientation | FlipOrientation |
| --- | --- | --- |
| `(4, 4, 10)` | PositiveX | PositiveY |
| `(5, 4, 10)` | NegativeZ | PositiveY |
| `(6, 4, 10)` | NegativeX | PositiveY |
| `(7, 4, 10)` | PositiveZ | PositiveY |
| `(4, 5, 10)` | PositiveX | NegativeY |
| `(5, 5, 10)` | NegativeZ | NegativeY |
| `(6, 5, 10)` | NegativeX | NegativeY |
| `(7, 5, 10)` | PositiveZ | NegativeY |

This deliberately exercises:

- all 8 Orientation x FlipOrientation combinations;
- horizontal adjacency;
- four direct vertical Shape-on-Shape contacts;
- empty space below elevated geometry for later visual inspection and meshing validation.

### Empty world rule

Every Cell not occupied by the baseline, axis walls, or the three explicit Shape-fixture groups is `Voxel::Cell::Empty` / Definition 0.

In particular:

- world space below `Y = 0` is empty;
- there is no underground fill;
- Chunks outside the authored special fixture Chunks still follow the infinite baseline/wall rules where applicable and are otherwise empty.

### Exact Chunk set already approved for generator validation

The project owner approved these Chunk coordinates as the canonical positive/negative fixture set:

```text
(0, 0, 0)
(1, 0, 1)
(2, 0, 1)
(1, 0, 2)
(-1, 0, 0)
(0, 0, -1)
(-1, 0, -1)
(0, -1, 0)
```

Their intended roles are:

- origin floor + both axis walls;
- all three dedicated Shape fixtures;
- negative-X and negative-Z wall/baseline cases;
- negative-X/Z baseline-only case;
- a completely below-baseline Chunk proving no underground terrain.

## Resource authoring

DR-018 defines the mixed aggregate/single-element JSON loading contract used by the first resources.

ST-001-06 should keep cube+slab together and slope/stair separate for both Shapes and Definitions so both catalog load forms remain exercised.

## Consequences

- the fixture is deterministic and semantically exact without introducing production noise/biomes;
- later tests/rendering can select a finite Chunk set while the generator itself remains spatially defined by the rules above;
- transformed slope/stair/slab geometry is available for later occlusion/meshing/golden validation;
- the fixture intentionally creates contacts that later mesher tests can reduce to exact expected topology/vertex/index counts;
- generation itself produces only voxel/Chunk data and no image.

## Required validation

The fixture contract above is exact.

DR-007 still requires repeated generation to be semantically deterministic. ST-001-06 must also validate the fixed Chunk invariants and the approved positive/negative Chunk set.

The project owner has **not yet fixed the exact assertion depth** for the temporary prototype provider (for example, whether the Server test compares complete Cell arrays or a smaller exact set of representative occupied/empty cells plus repeat equality). That test-policy choice remains a ST-001-06 readiness item; it does not make the scene geometry itself ambiguous.

## Resolution provenance

Initial direction was approved directly by the project owner on 22 September 2026.

The exact Definition mapping, material strings, baseline/wall rules, Shape-fixture Chunks, ground/elevated placement tables, all-eight transform matrix, default-empty rule, and positive/negative validation Chunk set were approved during ST-001-06 specification on 24 September 2026.

## Supersession

None.
