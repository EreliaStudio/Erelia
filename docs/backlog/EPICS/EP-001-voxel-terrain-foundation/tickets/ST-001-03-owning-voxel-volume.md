# ST-001-03 — Owning Voxel::Volume

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Introduce the reusable owning `Voxel::Volume` container consumed by terrain generation, networking, and Client meshing.

## User / system value

Core needs one shared runtime-sized voxel container rather than separate Server/Client or terrain/model storage formats.

## Starting state / prerequisites

- Depends on ST-001-02.
- DR-012 fixes the high-level Volume direction.
- OQ-035 still leaves storage/index/editor/validation behavior open.

## Product ownership

Core owns the generic container.

## Allowed dependencies

C++ standard library; headless-safe Sparkle Core.

## Forbidden dependencies

Server authority code, Client presentation code, graphics-only dependencies, terrain streaming logic.

## Owned behavior

Known approved responsibilities:

- runtime dimensions;
- uniform voxel size;
- contiguous owning `std::vector<Voxel::Cell>` storage;
- checked coordinate lookup;
- read-only contiguous cell access;
- local bounds derived from dimensions and voxel size;
- controlled mutation rather than unrestricted writable storage.

## Explicitly not owned

- world/Chunk coordinate identity;
- generation;
- caching/streaming;
- Definition/Shape catalog semantics;
- rendering resources;
- network framing.

## Public contract

Logical state is `dimensions + voxelSize + contiguous Cells`. DR-017 later requires friend Message operators, but their encoding belongs to ST-001-05.

## Invariants

- Cell count exactly matches the approved dimensions product.
- Storage is contiguous and owning.
- Checked access never returns an unrelated cell for an invalid coordinate.
- Voxel size is uniform for the complete Volume.

## State transitions

Construction establishes a valid Volume. Controlled edits may change cells according to the eventual editor/versioning contract.

## Failure behavior

**Blocked by OQ-035:** invalid dimensions, invalid voxel size, coordinate failure behavior, storage order, mutation/editor/versioning behavior, and canonical empty initialization are not fully fixed.

## Determinism / ordering

Storage/index order must be explicit before Ready because it affects serialization and meshing. OQ-035 currently leaves it unresolved.

## Lifecycle / ownership

Volume owns its Cell storage and does not expose a mutable external buffer whose lifetime can outlive/invalidate the Volume unnoticed.

## Serialization / persistence

Not implemented by this ticket. ST-001-05 serializes the logical Volume after this contract is fixed.

## Networking / authority

Not applicable beyond shared representation.

## Implementation constraints

- Use domain-scoped name `Voxel::Volume`.
- Do not raw-copy `std::vector` implementation state.
- Keep Core headless-safe.

## Exact test fixtures

Blocked until OQ-035 fixes storage order and validation. At minimum, the final Ready version must include small asymmetric dimensions that expose index order, checked in/out-of-bounds coordinates, valid/invalid voxel sizes, empty/default Cell initialization, and local bounds.

## Acceptance tests

### Nominal

Construction/access/cell-span/bounds tests once exact fixtures are fixed.

### Boundaries

Zero/one/max practical dimensions as explicitly approved; currently blocked.

### Invalid / rejected operations

Blocked by OQ-035.

### Failure atomicity

Any rejected edit/construction path must not expose partially inconsistent dimensions/storage; exact paths are blocked.

### Determinism

Index mapping/storage order must be tested once chosen.

### Lifecycle / ownership

Returned read-only contiguous views remain valid only under the final documented invalidation rules; those rules are blocked.

### Serialization / persistence

Not applicable here.

### Retry / idempotency

Not applicable.

### Concurrency / cancellation

No concurrent mutation contract is approved.

### Authority / trust boundary

Not applicable.

### Dependency failure

Not applicable.

### Cross-system integration

Deferred.

### Performance

Structural requirement only: contiguous Cell storage; no timing threshold.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-012](../../../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [OQ-035](../../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) — blocking.

## Completion evidence

Ticket becomes Ready only after OQ-035 supplies the missing storage/index/validation/editor/lifetime fixtures.
