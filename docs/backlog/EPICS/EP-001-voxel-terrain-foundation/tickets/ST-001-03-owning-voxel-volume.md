# ST-001-03 — Owning Voxel::Volume

**Status:** Ready
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Introduce the reusable owning `Voxel::Volume` container consumed by terrain generation, networking, and Client meshing.

## User / system value

Core needs one shared runtime-sized voxel container rather than separate Server/Client or terrain/model storage formats.

## Starting state / prerequisites

- ST-001-02 is Done and merged through PR #8.
- OQ-035 is Resolved.
- DR-012 fixes the Volume representation/editor/versioning contract.
- DR-017 fixes the later `spk::Message` friend-operator direction; encoding remains ST-001-05.

## Product ownership

Core owns the generic container.

## Allowed dependencies

C++ standard library; headless-safe Sparkle Core.

## Forbidden dependencies

Server authority code, Client presentation code, graphics-only dependencies, terrain streaming logic.

## Public contract

`Voxel::Volume` derives from `spk::VersionedTrait`.

Semantic aliases:

- `Voxel::Volume::LocalCoordinate = spk::Vector3Int`;
- `Voxel::Volume::UnitSize = float`.

Required API shape:

- default constructor;
- explicit logical constructor from `spk::Vector3UInt dimensions` and `UnitSize unitSize`;
- copy constructor and copy assignment;
- move constructor and move assignment;
- read-only `dimensions()`;
- read-only `unitSize()`;
- `contains(LocalCoordinate)`;
- checked `at(LocalCoordinate)` returning a `Voxel::Cell` copy;
- read-only contiguous `cells()` returning `std::span<const Voxel::Cell>`;
- `edit()` returning a nested `Voxel::Volume::Editor`;
- Editor `set(LocalCoordinate, Voxel::Cell)` returning whether the Cell changed;
- Editor `commit()`.

No local-bounds API is introduced by this ticket.

## Owned state and invariants

Logical state is `dimensions + unitSize + contiguous Cells`.

- default construction is the sole valid empty Volume: dimensions `{0,0,0}`, unit size `0.0f`, zero Cells;
- explicit construction requires `x > 0`, `y > 0`, and `z > 0`;
- explicit unit size must be finite and strictly positive;
- explicit Cell count is exactly `x * y * z` and must be representable by `std::size_t`;
- there is no other project-defined maximum;
- all explicitly constructed Cells are default-constructed and therefore equal to `Voxel::Cell::Empty`;
- storage is owning, contiguous `std::vector<Voxel::Cell>`;
- storage size never changes during ordinary Editor mutation;
- unit size is uniform for the whole Volume.

## Determinism / storage order

Storage order is **Y fastest, then X, then Z**:

```text
index = y + sizeY * (x + sizeX * z)
```

For dimensions `{2,3,4}`:

| Coordinate | Index |
| --- | ---: |
| `{0,0,0}` | 0 |
| `{0,1,0}` | 1 |
| `{0,2,0}` | 2 |
| `{1,0,0}` | 3 |
| `{1,2,0}` | 5 |
| `{0,0,1}` | 6 |
| `{1,2,3}` | 23 |

This exact mapping is part of the public representation contract.

## Checked access and failure behavior

- `contains()` returns false for every coordinate on a default-empty Volume;
- `at()` outside the Volume throws `spk::Exception`;
- Editor `set()` outside the Volume throws `spk::Exception`;
- explicit construction with any zero dimension throws `spk::Exception`;
- explicit construction with zero, negative, NaN, positive infinity, or negative infinity unit size throws `spk::Exception`;
- dimension products not representable by `std::size_t` throw `spk::Exception`;
- representable allocation failure is not translated and may propagate the standard allocation exception;
- rejected construction exposes no partially constructed Volume;
- rejected Editor operations do not mutate Cells and do not publish a version.

## Editor / version behavior

`Voxel::Volume` uses the inherited `spk::VersionedTrait` provider for edition subscriptions.

- `edit()` creates one Editor batch;
- `Editor::set()` returns `true` only when the stored packed Cell value actually changes;
- multiple effective writes in one Editor publish exactly one `invalidate()` when the Editor commits;
- a no-op Editor batch publishes no invalidation;
- Editor destruction automatically commits;
- explicit `commit()` is idempotent;
- `set()` after commit throws `spk::Exception`;
- an invalid `set()` does not alias another Cell, does not publish a version, and leaves the Editor usable for later valid operations unless it was already committed.

## Copy / move behavior

### Copy construction

Copies dimensions, unit size, and all Cells into independent storage.

The new object has a fresh `VersionedTrait` state:

- version starts at 0;
- subscriptions are not copied.

The source is unchanged.

### Copy assignment

Self-assignment is a no-op.

Otherwise:

- prepare the replacement logical state before changing the destination;
- on successful replacement, preserve destination subscriptions;
- replace dimensions, unit size, and Cells;
- invalidate/notify the destination exactly once;
- source state/version/subscriptions are unchanged.

If preparation fails, the destination remains unchanged and publishes no invalidation.

### Move construction

Self-move construction is not applicable.

- transfer dimensions, unit size, and Cell ownership to the new object;
- the destination has a fresh version state starting at 0 and no source subscriptions;
- reset the source to the valid default-empty state;
- invalidate/notify the source exactly once;
- any Cell span obtained from the source before the move is invalid after the move.

### Move assignment

Self-move-assignment is a no-op.

Otherwise:

- preserve destination subscriptions;
- transfer dimensions, unit size, and Cell ownership;
- invalidate/notify the destination exactly once;
- reset the source to the valid default-empty state;
- invalidate/notify the source exactly once.

## Read-only contiguous view lifetime

`cells()` returns `std::span<const Voxel::Cell>`.

- callers can iterate/read but cannot mutate Cells through the view;
- ordinary Editor mutation does not resize storage, so an existing span remains valid and observes subsequent committed or uncommitted Cell writes;
- destruction invalidates every span;
- copy assignment and move assignment are state-replacing operations and invalidate spans previously obtained from the destination;
- move construction invalidates spans previously obtained from the source;
- no API promises view validity after one of those invalidating operations.

## Explicitly not owned

- world/Chunk coordinate identity;
- generation;
- caching/streaming;
- Definition/Shape catalog semantics;
- rendering resources;
- network framing;
- Volume wire encoding;
- local bounds.

## Serialization / persistence

Not implemented by this ticket. ST-001-05 implements the DR-017 `spk::Message` logical serialization contract after this representation exists.

## Networking / authority

No authoritative behavior is owned here. Volume is shared Core representation.

## Implementation constraints

- use domain-scoped name `Voxel::Volume`;
- keep declarations in headers and move implementation into source files where practical;
- do not expose mutable storage;
- keep Core headless-safe;
- do not copy archived APIs beyond the explicitly approved contract;
- do not implement ST-001-04 or ST-001-05 behavior.

## Exact test fixtures

Use explicit constructor fixture `dimensions = {2,3,4}`, `unitSize = 0.25f`.

Storage-order fixture writes distinct Cells so the exact Y-X-Z indices above are observable. At minimum use distinct packed values at:

- `{0,0,0}` -> index 0;
- `{0,1,0}` -> index 1;
- `{1,0,0}` -> index 3;
- `{0,0,1}` -> index 6;
- `{1,2,3}` -> index 23.

Boundary fixture `{1,1,1}` with unit size `1.0f`.

Invalid dimensions include `{0,1,1}`, `{1,0,1}`, `{1,1,0}`, and a dimension triple whose product exceeds `std::size_t` capacity.

Invalid unit sizes include `0.0f`, a negative finite value, `NaN`, `+infinity`, and `-infinity`.

Invalid coordinates include negative coordinates and coordinates exactly equal to a dimension.

## Acceptance tests

### Nominal

- default Volume has zero dimensions, zero unit size, zero Cells, and version 0;
- explicit `{2,3,4}` construction stores dimensions/unit size and owns 24 default-empty Cells;
- `contains()`, `at()`, and `cells()` agree on the exact Y-X-Z storage fixture;
- `at()` returns a Cell copy;
- Cell span is read-only and contiguous.

### Boundaries

- `{1,1,1}` is valid;
- maximum representable Cell-count policy is enforced through overflow detection rather than an arbitrary project limit;
- zero dimensions are valid only through default construction.

### Invalid / rejected operations

- every zero explicit dimension is rejected with `spk::Exception`;
- invalid unit-size fixtures are rejected with `spk::Exception`;
- negative/out-of-range access is rejected with `spk::Exception`;
- Editor use after commit is rejected with `spk::Exception`.

### Failure atomicity

- invalid Editor writes leave all Cells/version unchanged;
- failed copy-assignment preparation leaves destination state/version unchanged;
- invalid explicit construction never exposes a partially valid object.

### Determinism / ordering

- exact `{2,3,4}` Y-X-Z index fixtures pass.

### Lifecycle / ownership

- ordinary Editor writes preserve an existing span and the span observes the updated Cells;
- copied Volumes own independent Cell storage;
- copy construction starts at version 0 without copied subscriptions;
- copy assignment preserves destination subscriptions and emits one destination invalidation;
- move construction leaves destination with transferred state/version 0, resets source to default-empty, and emits one source invalidation;
- move assignment preserves destination subscriptions, emits one destination invalidation, resets source to default-empty, and emits one source invalidation;
- self-copy and self-move assignment are no-ops.

### Serialization / persistence

Not applicable: ST-001-05.

### Retry / idempotency

Editor `commit()` is explicitly idempotent; no network retry contract applies.

### Concurrency / cancellation

Not applicable: no concurrent mutation contract is provided by this container.

### Authority / trust boundary

Not applicable: this is shared Core representation.

### Dependency failure

Only allocation failure is relevant; representable allocation failure propagates the standard allocation exception.

### Cross-system integration

Deferred to later generator/network/mesher tickets.

### Performance

Structural invariant only: owned Cell storage is contiguous. No timing budget is introduced.

### Client-visible / golden-image validation

Not applicable.

## Decisions

- [DR-012](../../../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [DR-017](../../../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
- [OQ-035](../../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) — Resolved.

## Definition of Ready review

**Pass.** Another coding agent can write the failing acceptance tests above before touching production code without deciding observable behavior.

## Completion evidence

Before Done, record:

- production files changed;
- exact acceptance-test files;
- local/CI build and CTest commands/results;
- required headless regression matrix;
- confirmation that no forbidden dependency was introduced;
- human project-owner approval.

Until human approval is recorded after implementation review, the ticket must remain In Progress rather than Done.
