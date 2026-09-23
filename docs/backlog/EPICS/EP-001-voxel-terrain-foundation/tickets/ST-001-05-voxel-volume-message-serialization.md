# ST-001-05 — Voxel::Volume Message serialization

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Provide the shared direct `spk::Message << Voxel::Volume` / `>>` serialization contract used by both Server and Client.

## User / system value

EP-001 can move canonical owning Volume data across the real process boundary without duplicating serialization logic or exposing mutable internals.

## Starting state / prerequisites

- Depends on ST-001-02 and ST-001-03.
- DR-017 fixes the friend-operator API and logical fields.
- OQ-037 still leaves scalar byte-order/platform portability unresolved.
- ST-001-03 still leaves storage/index details unresolved through OQ-035.

## Product ownership

Core owns shared serialization.

## Allowed dependencies

- C++ standard library.
- Sparkle Core networking/message facilities.
- ST-001-02 / ST-001-03 public Core contracts.

## Forbidden dependencies

Server/Client-specific protocol handlers, graphics facilities, raw serialization of `std::vector` object representation.

## Owned behavior

- Friend insertion/extraction operators declared on `Voxel::Volume`.
- Definitions as free functions in namespace `Voxel`.
- Logical serialization of dimensions, voxel size, and contiguous Cell storage.
- Reconstruction of a new owning valid Volume.
- Malformed/truncated payload rejection before an inconsistent Volume becomes observable.

## Explicitly not owned

- Chunk request/response message kinds.
- Server routing.
- Client cache.
- Definition/Shape resource delivery.
- Transport framing internals already owned by Sparkle.

## Public contract

Required API:

```cpp
message << volume;
message >> volume;
```

with friend declarations equivalent to the signatures fixed by DR-017. Callers must not invoke a separate serializer object.

## Invariants

- Serialized logical cell count matches dimensions.
- Round trip preserves dimensions, voxel size, and every Cell.
- No serialized bytes depend on `sizeof(Voxel::Volume)` or `std::vector` object layout.
- A malformed payload never leaves the destination Volume internally inconsistent.

## State transitions

Valid payload extraction replaces/reconstructs the destination with the decoded valid logical Volume. Failed extraction leaves no partially decoded inconsistent state; exact prior-value preservation semantics must be fixed before Ready.

## Failure behavior

**Blocked:** exact scalar wire representation/endianness is unresolved by OQ-037. The extraction failure signaling mechanism and destination-state guarantee must also be explicit before Ready.

## Determinism / ordering

Field and cell serialization order must be fixed and stable. Cell order depends on the final ST-001-03 storage order.

## Lifecycle / ownership

Decoded Volume owns its Cell storage independently from the Message buffer.

## Serialization / persistence

This ticket is the serialization contract. It is network transfer serialization, not persistence.

## Networking / authority

Shared codec only. Successful decode does not make Client data authoritative; Server remains canonical.

## Implementation constraints

- Use the direct friend-operator API from DR-017.
- Prefer contiguous Cell-block transfer rather than 4096 high-level Cell operations where Sparkle permits.
- Do not add a third-party serializer.
- Keep implementation in namespace `Voxel`, not `spk`.

## Exact test fixtures

Final Ready tests must include:

- a small asymmetric Volume exposing cell order;
- default/empty Cells;
- transformed Cells with Orientation/Flip;
- exactly one 16×16×16 Volume (4096 Cells);
- truncated metadata;
- truncated Cell block;
- declared dimensions/cell count mismatch;
- impossible/unrepresentable dimensions.

Exact byte fixture(s) remain blocked by OQ-037.

## Acceptance tests

### Nominal

- `message << volume` compiles/resolves by ADL.
- `message >> volume` compiles/resolves by ADL.
- Full logical round trips preserve exact state.

### Boundaries

16×16×16 terrain Volume round trip and approved small/empty boundaries.

### Invalid / rejected operations

Malformed/truncated/inconsistent payloads are rejected according to the final error contract.

### Failure atomicity

Destination state guarantee on decode failure must be tested once fixed.

### Determinism

The same logical Volume serializes to the same approved byte representation under the chosen wire policy.

### Lifecycle / ownership

Decoded storage remains valid after the source Message is destroyed.

### Serialization / persistence

All owned behavior is covered here.

### Retry / idempotency

Serializing/deserializing repeatedly preserves semantic equality.

### Concurrency / cancellation

Not applicable: no shared mutable serializer state is intended.

### Authority / trust boundary

Malformed Client/Server payloads cannot create an internally invalid Volume.

### Dependency failure

Not applicable beyond malformed Message input.

### Cross-system integration

Later protocol tests must use these operators rather than reimplement Volume encoding.

### Performance

Structural evidence only: terrain Cell storage is transferred as a contiguous block where supported; no timing budget.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-017](../../../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
- [OQ-037](../../../OPEN_QUESTIONS/OQ-037-EP001-NETWORK-SERIALIZATION.md) — blocking.
- [OQ-035](../../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) — blocks ST-001-03/storage order.

## Completion evidence

Promote to Ready only after wire scalar policy and decode-failure/destination-state semantics are explicit and prerequisite Volume behavior is Ready.
