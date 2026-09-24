# ST-001-12 — Client terrain mesher

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Client
**Test suite(s):** EreliaClientTestSuite

## Intent

Convert canonical cached terrain Volume data plus shared Definition/Shape semantics into Client-owned terrain mesh data.

## User / system value

Received canonical terrain becomes renderable while keeping GPU/presentation artifacts entirely Client-owned.

## Starting state / prerequisites

- Depends on ST-001-03 and ST-001-04.
- DR-013 fixes Client ownership of terrain meshes.
- OQ-036 still leaves missing-neighbor behavior and remesh invalidation scope unresolved.
- ST-001-04 is Ready with its Definition/Shape geometry semantics fixed by DR-018; this ticket still waits for ST-001-04 implementation plus OQ-036.

## Product ownership

Client owns meshing and derived mesh data.

## Allowed dependencies

EreliaClientLibrary, EreliaCore shared voxel/Shape contracts, Sparkle graphics/data facilities appropriate to Client, standard library.

## Forbidden dependencies

Server mesh generation, Server graphics dependencies, sending mesh data over the network.

## Owned behavior

The final ticket derives mesh geometry for the approved first terrain Shapes, applies Orientation/Flip, removes/keeps faces according to exact local/neighbor occupancy, and reports mesh data suitable for later rendering integration.

## Explicitly not owned

GPU scene lifetime/render loop, Chunk request/cache policy, Server generation, production LOD/greedy meshing unless explicitly approved.

## Public contract

Blocked until:

- ST-001-04 fixes Definition/Shape normalized geometry and lookup;
- OQ-036 fixes how absent neighbors are treated and exact remesh invalidation scope;
- exact semantic mesh output representation/ordering needed by tests is specified.

## Invariants

- Meshing is derived Client state only.
- Server canonical voxel data is not mutated by meshing.
- Orientation/Flip semantics match shared Core contract.
- Cross-Chunk occlusion becomes correct under the eventual OQ-036 policy.

## State transitions

Canonical Chunk data + relevant neighbor state -> derived mesh. Neighbor/cache changes may invalidate/remesh according to OQ-036.

## Failure behavior

Invalid/missing Definition and invalid Volume behavior must follow ST-001-04/ST-001-03. Missing-neighbor behavior is blocked by OQ-036.

## Determinism / ordering

For identical canonical input/neighbor state, semantic mesh output must be identical. Vertex/index ordering must be fixed if golden semantic snapshots depend on it.

## Lifecycle / ownership

Mesh is Client-owned derived state. Exact invalidation when source Chunk/neighbor is replaced/evicted must match ST-001-11 and OQ-036.

## Serialization / persistence

Not applicable.

## Networking / authority

Mesher never sends or receives authoritative state; it consumes Server-derived cached data.

## Implementation constraints

- Server never emits terrain meshes.
- Do not import archived meshing policy as a requirement.
- Keep production optimization/LOD out unless needed for exact EP-001 acceptance.

## Exact test fixtures

Final Ready tests require minimal exact fixtures for:

- one cube Cell;
- adjacent same-definition cells with internal face occlusion;
- slab, slope, stair;
- Orientation/Flip variants;
- exact multi-Shape occlusion/contact fixtures, including vertically opposed/flipped Shapes (for example two slopes stacked against each other), with expected semantic mesh topology and exact emitted vertex/index counts;
- boundary Cell with present neighbor;
- boundary Cell with absent neighbor under chosen OQ-036 policy;
- neighbor arrival/removal/remesh invalidation.

Expected semantic vertices/indices/normals/material references must be exact.

## Acceptance tests

### Nominal

Exact semantic mesh output for each approved Shape fixture. Include compact multi-Shape contact/occlusion cases where topology counts are asserted directly; for example, two vertically opposed/flipped slopes should have an explicitly derived expected mesh and exact vertex/index counts so internal-face/partial-occlusion behavior is validated structurally without relying on rendered images.

### Boundaries

Chunk-edge faces with neighbor present/absent.

### Invalid / rejected operations

Invalid Definition/Volume per prerequisite contracts.

### Failure atomicity

Failed remesh leaves prior valid mesh state unchanged or removes it according to the final explicit policy.

### Determinism

Repeat fixtures produce exact semantic mesh output.

### Lifecycle / ownership

Source replacement/neighbor change invalidates/remeshes exactly as specified.

### Serialization / persistence

Not applicable.

### Retry / idempotency

Remeshing unchanged input is semantically idempotent.

### Concurrency / cancellation

No async meshing is approved; if introduced, cancellation/lifetime rules must be specified first.

### Authority / trust boundary

Mesh is never authoritative and never sent by Server.

### Dependency failure

Missing Definition/neighbor behavior per final contracts.

### Cross-system integration

Consumes ST-001-11 cached data later; renderer consumes this mesh in ST-001-13.

### Performance

No hard budget until OQ-031; structural mesh counts may be recorded later.

### Client-visible / golden-image validation

Golden rendering belongs to ST-001-16; this ticket uses semantic mesh assertions.

## Decisions / unresolved questions

- [DR-013](../../../DECISIONS/DR-013-CLIENT-TERRAIN-MESHING.md)
- [OQ-036](../../../OPEN_QUESTIONS/OQ-036-TERRAIN-MESHING-NEIGHBOR-POLICY.md) — blocking.
- ST-001-04 implementation remains a prerequisite for the exact Shape fixtures.

## Completion evidence

Promote to Ready only after neighbor/remesh and exact Definition/Shape/mesh fixture contracts are fixed.
