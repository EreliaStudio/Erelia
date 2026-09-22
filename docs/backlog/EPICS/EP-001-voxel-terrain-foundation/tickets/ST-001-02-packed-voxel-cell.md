# ST-001-02 — Packed Voxel::Cell value type

**Status:** In Progress
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Introduce the compact shared `Voxel::Cell` value used by terrain Volume storage, networking, generation, and meshing.

## User / system value

Server and Client need one compact, shared semantic voxel value before they can exchange and interpret canonical terrain.

## Starting state / prerequisites

- DR-012 fixes the complete Cell packing/validation direction needed by this ticket.
- OQ-035 remains partially resolved only for later `Voxel::Volume` details; those remaining questions do not block this ticket.

## Product ownership

Core owns the shared value type.

## Allowed dependencies

C++ standard library and headless-safe Sparkle Core.

## Forbidden dependencies

Server, Client, graphics-only Sparkle facilities, and archived code as a requirement.

## Owned behavior

- complete logical representation fits exactly in one `std::uint32_t`;
- trivially copyable;
- `Voxel::Definition::ID` is the semantic Definition identifier type and is an alias of `std::uint32_t`;
- lower 29 bits: Definition ID;
- bits 29-30: horizontal `Voxel::Cell::Orientation`;
- bit 31: `Voxel::Cell::FlipOrientation`;
- `Orientation` exact values are `PositiveX = 0`, `NegativeX = 1`, `PositiveZ = 2`, `NegativeZ = 3`;
- `FlipOrientation` exact values are `PositiveY = 0`, `NegativeY = 1`;
- Definition ID 0 denotes semantic emptiness;
- Orientation/FlipOrientation are still valid and preserved when Definition ID is 0;
- default construction produces packed `0x00000000`;
- `Voxel::Cell::Empty` is the explicit static packed-zero empty value;
- construction from any packed `std::uint32_t` is valid and preserves it exactly;
- construction from logical fields validates their domains;
- packed representation can be retrieved as `std::uint32_t`;
- maximum packed Definition ID is `0x1FFFFFFF`;
- the Cell is immutable after construction.

## Explicitly not owned

- `Voxel::Volume`;
- Definition/Shape catalog semantics;
- network byte order;
- terrain generation or rendering.

## Public contract

`Voxel::Cell` owns one private packed `std::uint32_t`, avoiding C++ bitfields and their implementation-defined physical layout.

The type provides:

- default construction to packed zero;
- explicit construction from a raw packed `std::uint32_t`;
- construction from `Voxel::Definition::ID` + `Orientation` + `FlipOrientation`;
- read-only getters for Definition ID, Orientation, FlipOrientation, and packed value;
- static `Voxel::Cell::Empty`, declared on the type and defined out-of-line in the Cell source file.

Exact getter/constructor spelling may be selected during implementation as long as the complete observable contract above is preserved.

## Invariants

- `sizeof(Voxel::Cell) == sizeof(std::uint32_t)`.
- `std::is_trivially_copyable_v<Voxel::Cell>`.
- Definition ID, Orientation, and FlipOrientation round-trip through the packed value without overlap.
- Definition ID 0 is semantically empty regardless of Orientation/FlipOrientation bits.
- raw packed construction never canonicalizes or rejects a `std::uint32_t`.

## State transitions

The Cell is immutable after construction. There are no mutation operations in this ticket.

## Failure behavior

Raw packed construction cannot fail.

Logical-field construction throws `spk::Exception` when:

- Definition ID is greater than `0x1FFFFFFF`;
- Orientation is outside its four-value domain, including invalid values manufactured through an explicit cast;
- FlipOrientation is outside its two-value domain, including invalid values manufactured through an explicit cast.

Because construction either succeeds or throws before an object becomes observable, there is no mutable partial state to preserve.

## Determinism / ordering

Packing/unpacking is exact and deterministic.

## Lifecycle / ownership

Value type; no external resource ownership.

## Serialization / persistence

The 32-bit packed representation is stable within the approved Cell contract, but wire byte order belongs to ST-001-05/OQ-037.

## Networking / authority

Shared data only; no authority semantics.

## Implementation constraints

- Do not widen the Cell beyond 32 bits.
- Do not use C++ bitfields for the stored representation.
- Store one private `std::uint32_t` and use masks/shifts for field extraction/packing.
- Use `spk::Exception` for logical-construction validation failures.
- Keep the Cell immutable after construction.
- Declare `Voxel::Cell::Empty` in the header and define it in the Cell source file.
- Do not copy archived APIs blindly.

## Exact test fixtures

Required fixtures:

- default Cell: packed `0x00000000`, Definition ID `0`, `PositiveX`, `PositiveY`;
- `Voxel::Cell::Empty`: same packed/getter values as the default Cell;
- Definition ID `1` with each Orientation value;
- Definition ID `1` with each FlipOrientation value;
- raw `0x60000000`: Definition ID `0`, `NegativeZ`, `PositiveY`, packed value preserved exactly;
- raw `0xFFFFFFFF`: maximum Definition ID, `NegativeZ`, `NegativeY`, packed value preserved exactly;
- maximum logical Definition ID `0x1FFFFFFF`;
- logical Definition ID `0x20000000`: rejected with `spk::Exception`;
- `static_cast<Orientation>(4)`: rejected with `spk::Exception`;
- `static_cast<FlipOrientation>(2)`: rejected with `spk::Exception`;
- packed round-trip for representative Definition ID/Orientation/FlipOrientation combinations.

## Acceptance tests

### Nominal

Exact field packing/unpacking for the fixtures above, including the approved enum-to-bit mapping.

### Boundaries

Definition ID `0`, ID `1`, and maximum ID `0x1FFFFFFF`; all four Orientation values; both FlipOrientation values; raw `0x00000000` and `0xFFFFFFFF`.

### Invalid / rejected operations

Logical construction rejects Definition ID overflow and invalid enum-domain values with `spk::Exception`.

### Failure atomicity

Not applicable to mutation because the type is immutable. Failed construction does not expose a partially constructed Cell.

### Determinism

Same logical fields produce the same packed value; raw packed construction preserves the raw value exactly.

### Lifecycle / ownership

Compile-time size and trivial-copyability assertions; ordinary value copy semantics.

### Serialization / persistence

Packed value round-trip only; wire encoding is not owned here.

### Retry / idempotency

Not applicable.

### Concurrency / cancellation

Not applicable; immutable value semantics introduce no mutation/concurrency contract.

### Authority / trust boundary

Not applicable.

### Dependency failure

Not applicable.

### Cross-system integration

Deferred to Volume/protocol/mesher tickets.

### Performance

Compile-time size/trivial-copyability assertions are required. Getter implementations are constant-time integer mask/shift operations; no timing threshold is required.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-012](../../../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [OQ-035](../../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) — Cell portion resolved; OQ remains partially resolved for later Volume details.

## Definition-of-Ready review

Ready: another implementation agent can write the required acceptance tests before production code without selecting any remaining Cell behavior. The unresolved Volume portions of OQ-035 belong to ST-001-03 and later work.

## Completion evidence

Implementation branch: `feat/st-001-02-packed-voxel-cell`.

Implemented so far:

- acceptance tests added first in `core/tests/voxel_cell_test.cpp`;
- `Voxel::Cell` implemented as one private packed `std::uint32_t`;
- `Voxel::Cell::Empty` declared in the header and defined in `core/src/voxel/cell.cpp`;
- exact Orientation / FlipOrientation mappings and mask/shift getters implemented in `cell.cpp`;
- raw packed construction preserves every `std::uint32_t` and is implemented in `cell.cpp`;
- logical construction takes `Voxel::Definition::ID` and rejects capacity/enum-domain violations with `spk::Exception`;
- Core CMake/test registration updated.

Validation evidence:

- draft PR #8 targets `backlog/ep-001-implementation-tickets`;
- CI run #55 / run ID `35788589950` passed `clang-format`;
- Linux headless Core/Server Debug passed, including configure/build/CTest;
- Linux headless Core/Server Release passed, including configure/build/CTest;
- Windows headless Core/Server Debug passed, including configure/build/CTest;
- Windows headless Core/Server Release passed, including configure/build/CTest.

Required human approval is still outstanding. The ticket remains In Progress and must not be marked Done until the project owner records that approval.
