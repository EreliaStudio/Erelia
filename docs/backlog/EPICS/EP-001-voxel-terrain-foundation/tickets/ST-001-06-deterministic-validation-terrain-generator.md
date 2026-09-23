# ST-001-06 — Deterministic validation terrain generator

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Server
**Test suite(s):** EreliaServerTestSuite

## Intent

Implement the first Server-owned deterministic technical terrain generator for exact Chunk coordinates.

## User / system value

The network/render pipeline receives stable canonical terrain that deliberately exposes Chunk boundaries, vertical faces, undersides, and transformed voxel Shapes.

## Starting state / prerequisites

- Depends on ST-001-01, ST-001-02, ST-001-03, and ST-001-04.
- DR-015 fixes the scene direction.
- OQ-039 still lacks exact authored fixture coordinates, Definition IDs/material choices, Orientation/Flip table, and complete generator fixture.

## Product ownership

Server owns generation and the canonical generated result. Core owns only shared data/contracts.

## Allowed dependencies

Server library, EreliaCore, standard library, Sparkle Core.

## Forbidden dependencies

Client/graphics code, production world-generation scope, archived generator code as a requirement.

## Owned behavior

The final ticket generates requested 16×16×16 canonical terrain Volumes containing:

- flat baseline;
- walls aligned around world X=0 and Z=0 planes;
- elevated stairs/slabs/slopes around Y≈3;
- approved Orientation/Flip fixtures;
- deterministic results across positive and negative Chunk coordinates.

## Explicitly not owned

Networking, cache/streaming, Client meshing/rendering, production biomes/noise/world generation, persistence of dynamic world edits.

## Public contract

Input must identify the requested Chunk coordinate and any explicitly approved generator identity/seed/version required by the final fixture. Output is the canonical terrain `Voxel::Volume` for that Chunk.

Exact generator seed/version input is not yet fully specified.

## Invariants

- Output terrain Chunk dimensions are exactly 16×16×16.
- Terrain voxel size is exactly 1 world unit.
- World-to-Chunk placement uses ST-001-01 coordinate semantics.
- Identical approved inputs produce semantically identical cells.

## State transitions

Stateless generation from approved deterministic inputs.

## Failure behavior

Blocked until OQ-039 states complete valid input/fixture semantics and any unavailable/invalid-coordinate behavior required by this technical generator.

## Determinism / ordering

Semantic determinism is required by DR-007. Exact Cell values at exact fixture coordinates must repeat.

## Lifecycle / ownership

Returned/generated Volume is an owning value independent of generator temporary state.

## Serialization / persistence

Not owned. Generated output is consumed by later message serialization/protocol tickets.

## Networking / authority

Server is the authoritative producer. Client must never substitute local generation as canonical terrain.

## Implementation constraints

- Keep generator deliberately artificial/simple.
- Do not introduce production noise/biomes.
- No graphics dependency.
- Archived scenes are inspiration only.

## Exact test fixtures

**Blocked by OQ-039.** The Ready ticket must enumerate all authored world-cell coordinates/regions, expected exact Cell values, selected empty-space probes, exact Definition IDs/material choices, Orientation/Flip combinations, and any seed/version constant.

## Acceptance tests

### Nominal

Exact authored cells for baseline, walls, slabs, slopes, and stairs.

### Boundaries

Fixtures crossing X/Y/Z Chunk boundaries, including negative Chunk coordinates.

### Invalid / rejected operations

Exact cases blocked by OQ-039.

### Failure atomicity

Not applicable for stateless value generation unless generation can fail after output mutation; final API must make behavior explicit.

### Determinism

Repeated generation for each fixture Chunk yields identical semantic Cell content.

### Lifecycle / ownership

Returned Volume remains valid independently.

### Serialization / persistence

Not applicable.

### Retry / idempotency

Repeated calls with identical inputs are semantically identical.

### Concurrency / cancellation

No shared mutable generator state should be required; if introduced, concurrency semantics must be specified before Ready.

### Authority / trust boundary

Server result is canonical; no Client-authored cells are accepted.

### Dependency failure

Not applicable unless Definition lookup/resource loading becomes fallible; then exact failure must be specified.

### Cross-system integration

Later Server protocol handler must return this generator output without converting it to meshes.

### Performance

No numeric budget until OQ-031 is resolved.

### Client-visible / golden-image validation

Later tickets own visual evidence.

## Decisions / unresolved questions

- [DR-007](../../../DECISIONS/DR-007-SEMANTIC-DETERMINISM.md)
- [DR-015](../../../DECISIONS/DR-015-FIRST-TERRAIN-VALIDATION-SCENE.md)
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) — blocking.

## Completion evidence

Exact generator fixtures pass headlessly in EreliaServerTestSuite and repeat identically for all approved positive/negative Chunk cases after OQ-039 is resolved.
