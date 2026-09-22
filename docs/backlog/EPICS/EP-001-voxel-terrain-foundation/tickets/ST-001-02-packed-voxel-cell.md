# ST-001-02 — Packed Voxel::Cell value type

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Introduce the compact shared `Voxel::Cell` value used by terrain Volume storage, networking, generation, and meshing.

## User / system value

Server and Client need one compact, shared semantic voxel value before they can exchange and interpret canonical terrain.

## Starting state / prerequisites

- DR-012 fixes the 32-bit packing direction.
- OQ-035 is still partially resolved.

## Product ownership

Core owns the shared value type.

## Allowed dependencies

C++ standard library and headless-safe Sparkle Core where useful.

## Forbidden dependencies

Server, Client, graphics-only Sparkle facilities, and archived code as a requirement.

## Owned behavior

Known approved behavior:

- complete logical representation fits exactly in one `std::uint32_t`;
- trivially copyable;
- lower 29 bits: Definition ID;
- next 2 bits: horizontal Orientation;
- highest bit: vertical Flip;
- Definition ID 0 denotes empty;
- packed representation can be retrieved as `std::uint32_t`;
- maximum packed Definition ID is `0x1FFFFFFF`.

## Explicitly not owned

- `Voxel::Volume`;
- Definition/Shape catalog semantics;
- network byte order;
- terrain generation or rendering.

## Public contract

The exact final Cell construction/mutation API must preserve the approved packed semantics. Exact source-level signatures are not fixed here.

## Invariants

- `sizeof(Voxel::Cell) == sizeof(std::uint32_t)`.
- `std::is_trivially_copyable_v<Voxel::Cell>`.
- Definition ID, Orientation, and Flip round-trip through the packed value without overlap.
- ID 0 is semantically empty.

## State transitions

Construction or controlled mutation changes the logical packed fields atomically.

## Failure behavior

**Blocked:** OQ-035 must define canonical empty representation and remaining validation behavior, including how invalid/capacity-exceeding values are rejected and whether non-ID bits are canonicalized when Definition ID is 0.

## Determinism / ordering

Packing/unpacking is exact and deterministic.

## Lifecycle / ownership

Value type; no external resource ownership.

## Serialization / persistence

The 32-bit packed representation is stable within the approved Cell contract, but wire byte order belongs to ST-001-05/OQ-037.

## Networking / authority

Shared data only; no authority semantics.

## Implementation constraints

Do not widen the Cell beyond 32 bits. Do not copy archived APIs blindly.

## Exact test fixtures

Known fixtures can include:

- default Cell is semantically empty;
- ID `1`, each Orientation value, each Flip value;
- maximum ID `0x1FFFFFFF`;
- packed round-trip for combinations of ID/Orientation/Flip.

Exact empty packed bits and invalid-value rejection fixtures remain blocked by OQ-035.

## Acceptance tests

### Nominal

Exact field packing/unpacking tests once OQ-035 closes the remaining contract.

### Boundaries

Minimum/non-empty ID and maximum ID.

### Invalid / rejected operations

Blocked by OQ-035.

### Failure atomicity

Rejected field updates, if the chosen API permits mutation, must leave the prior Cell unchanged; exact cases are blocked by OQ-035.

### Determinism

Same logical fields produce the same packed value.

### Lifecycle / ownership

Trivial value semantics.

### Serialization / persistence

Packed value round-trip only; wire encoding is not owned here.

### Retry / idempotency

Not applicable.

### Concurrency / cancellation

Not applicable.

### Authority / trust boundary

Not applicable.

### Dependency failure

Not applicable.

### Cross-system integration

Deferred to Volume/protocol/mesher tickets.

### Performance

Compile-time size/trivial-copyability assertions are required; no timing threshold.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-012](../../../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [OQ-035](../../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) — blocking.

## Completion evidence

Ticket may become Ready only after OQ-035 fixes the remaining Cell validation/canonical-empty contract and exact tests are updated accordingly.
