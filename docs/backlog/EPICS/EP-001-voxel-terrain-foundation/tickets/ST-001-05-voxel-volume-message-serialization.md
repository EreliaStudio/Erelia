# ST-001-05 — Voxel::Volume Message serialization

**Status:** Ready
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Provide the shared direct `spk::Message << Voxel::Volume` / `>>` serialization contract used by both Server and Client.

## User / system value

EP-001 can move canonical owning Volume data across the real process boundary without duplicating serialization logic or exposing mutable internals.

## Starting state / prerequisites

- ST-001-02 and ST-001-03 are Done and merged.
- DR-017 fixes the friend-operator API and the complete Volume wire/decode contract.
- OQ-037 is resolved for ST-001-05.
- ST-001-03 fixes contiguous Cell storage and Y-fastest, then X, then Z ordering.

## Product ownership

Core owns shared serialization.

## Allowed dependencies

- C++ standard library.
- Sparkle 0.1.3 Core networking/message facilities.
- ST-001-02 / ST-001-03 public Core contracts.

## Forbidden dependencies

Server/Client-specific protocol handlers, graphics facilities, raw serialization of `Voxel::Volume` or `std::vector` object representation, third-party serializers, Volume-specific transport message IDs.

## Owned behavior

- Friend insertion/extraction operators declared on `Voxel::Volume`.
- Definitions as free functions in namespace `Voxel`.
- Sparkle-native logical serialization of dimensions, UnitSize, and contiguous Cell storage.
- Symmetric Volume invariant validation on insertion and extraction.
- Reconstruction of a new owning valid Volume.
- Convenience construction through `explicit Volume(const spk::Message &message)`, delegating to the extraction operator.
- Malformed/truncated payload rejection before an inconsistent Volume becomes observable.

## Explicitly not owned

- Chunk request/response message kinds.
- Server routing.
- Client cache.
- Definition/Shape resource delivery.
- Transport framing internals already owned by Sparkle.
- Platform-independent endian/floating-point wire conversion.

## Public contract

Required API:

```cpp
spk::Message message;
message << volume;
message >> volume;

Voxel::Volume decoded(message);
```

The Message constructor delegates to `operator>>` and therefore has exactly the same validation, ownership, and Message-cursor semantics. The friend declarations remain equivalent to the signatures fixed by DR-017. Callers must not invoke a separate serializer object.

## Exact wire format

The Volume payload is serialized in this exact order:

1. `spk::Vector3UInt dimensions` using Sparkle's native trivially-copyable serialization as one value;
2. `Voxel::Volume::UnitSize unitSize` using Sparkle's native serialization;
3. one contiguous native block of `Voxel::Cell` values.

No explicit Cell-count field is serialized. The decoder derives the Cell count as:

```text
dimensions.x * dimensions.y * dimensions.z
```

using checked multiplication before Cell allocation.

The Cell block uses the existing Volume storage order: Y-fastest, then X, then Z.

The format is intentionally Sparkle-native. Byte identity is deterministic for repeated serialization on the same supported ABI/platform representation; cross-endian or non-equivalent floating-point representation portability is not promised by ST-001-05.

## Volume validity contract

The one valid empty representation is:

```text
dimensions = {0, 0, 0}
unitSize   = 0.0f
cells      = empty
```

A valid non-empty representation requires:

- `dimensions.x > 0`;
- `dimensions.y > 0`;
- `dimensions.z > 0`;
- finite `unitSize > 0.0f`;
- a representable derived Cell count;
- enough remaining Message bytes for exactly the derived contiguous Cell block.

The following are malformed and must throw `spk::Exception`:

- mixed-zero dimensions;
- `{0,0,0}` with non-zero UnitSize;
- non-empty dimensions with zero, negative, NaN, or infinite UnitSize;
- dimension multiplication overflow / unrepresentable Cell count or Cell byte size;
- truncated dimensions metadata;
- truncated UnitSize metadata;
- truncated Cell data.

Both `operator<<` and `operator>>` validate the same Volume invariants. `operator<<` validates contract-invalid source state before appending Volume bytes.

## State transitions and failure behavior

Valid extraction reconstructs a temporary owning Volume and replaces the destination only after the complete Volume has decoded successfully.

If extraction throws, the destination Volume remains unchanged.

The `spk::Message` read cursor intentionally follows Sparkle 0.1.3's normal sequential extraction behavior: earlier successfully-read fields may remain consumed when a later read or validation fails. No arbitrary cursor rollback is added by this ticket.

Before allocating Cell storage, extraction must:

1. validate dimensions and UnitSize;
2. compute Cell count / byte size with checked arithmetic;
3. verify that the Message contains enough remaining bytes for the complete Cell block.

No new arbitrary maximum Volume dimension/count is introduced. Sparkle's network framing separately limits network Message payloads; generic in-memory `Voxel::Volume` remains runtime-sized according to ST-001-03.

Trailing bytes after a decoded Volume are valid and remain available for subsequent fields in a larger protocol message.

## Invariants

- Serialized logical Cell count is derived solely from dimensions.
- Round trip preserves dimensions, UnitSize, and every Cell exactly.
- No serialized bytes depend on `sizeof(Voxel::Volume)` or `std::vector` object layout.
- Cell transfer is one contiguous block, not one high-level Message operation per Cell.
- A malformed payload never changes the destination Volume.
- Decoded storage does not alias the source Message buffer.

## Lifecycle / ownership

Decoded Volume owns its Cell storage independently from the Message buffer and remains valid after the source Message is destroyed.

## Serialization / persistence

This ticket is network-transfer serialization, not persistence.

## Networking / authority

Shared codec only. Successful decode does not make Client data authoritative; Server remains canonical. Both Client and Server validate Volume dimensions/state when serializing and decoding.

## Implementation constraints

- Use the direct friend-operator API from DR-017.
- `volume.hpp` includes `<network/message.hpp>` directly because `spk::Message` is part of the public Volume API.
- Keep definitions in namespace `Voxel`, not `spk`.
- Message extraction reconstructs Volume directly; it must not use `Volume::Builder` or expose Builder internals for networking.
- Serialize `spk::Vector3UInt` as one native Sparkle Message value.
- Serialize Cells as one contiguous native block.
- Use named source-local helpers where decomposition improves clarity; do not introduce Erelia `detail` / `details` namespaces.
- Add compile-time checks for native-layout assumptions relied upon by the contiguous/native format where appropriate.

## Exact test fixtures

The implementation must include:

- direct ADL insertion and extraction compile/use coverage;
- direct `Voxel::Volume(message)` construction and malformed-input validation coverage;
- default/empty Volume round trip;
- a small asymmetric Volume that exposes Y/X/Z Cell ordering;
- non-default packed Cells covering Orientation and FlipOrientation;
- exactly one 16×16×16 Volume (4096 Cells);
- exact dimensions, UnitSize, and packed Cell equality after round trip;
- repeated serialization byte stability on the current supported ABI;
- truncated dimensions metadata;
- truncated UnitSize metadata;
- truncated Cell block;
- mixed-zero dimensions;
- invalid empty UnitSize;
- zero/negative/NaN/infinite UnitSize for non-empty dimensions;
- impossible/overflowing dimensions;
- destination prior-value preservation on every representative extraction failure;
- decoded storage lifetime after source Message destruction.

There is no declared-dimensions/serialized-Cell-count mismatch fixture because the approved format contains no explicit Cell-count field.

## Acceptance tests

### Nominal

- `message << volume` resolves by ADL.
- `message >> volume` resolves by ADL.
- Full logical round trips preserve exact state.

### Boundaries

- canonical empty Volume round trip;
- 16×16×16 terrain Volume round trip;
- asymmetric small Volume proving exact Cell order.

### Invalid / rejected operations

Malformed/truncated inputs listed above throw `spk::Exception`.

### Failure atomicity

The destination Volume is unchanged on decode failure. The Message cursor is not rollback-atomic and follows Sparkle's normal semantics.

### Determinism

The same logical Volume serializes to the same bytes on the same supported ABI/platform representation.

### Lifecycle / ownership

Decoded storage remains valid after the source Message is destroyed.

### Retry / idempotency

Repeated serialization/deserialization preserves semantic equality.

### Concurrency / cancellation

Not applicable: no shared mutable serializer state is intended.

### Authority / trust boundary

Malformed Client/Server payloads cannot create or replace state with an internally invalid Volume.

### Cross-system integration

Later protocol tests must use these operators rather than reimplement Volume encoding.

### Performance

Structural evidence only: terrain Cell storage is transferred as one contiguous block; no timing budget.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-017](../../../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md) — complete serialization/decode contract.
- [OQ-037](../../../OPEN_QUESTIONS/OQ-037-EP001-NETWORK-SERIALIZATION.md) — resolved for this ticket.
- [OQ-035](../../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) — resolved; ST-001-03 implements the required Volume storage order.

## Completion evidence

Definition of Ready is satisfied as of 24 September 2026: public API, exact wire order, native representation policy, validation/failure semantics, destination/cursor post-failure behavior, ownership, determinism scope, malformed cases, and exact tests are explicit.

Implementation is complete on `feat/st-001-05-voxel-volume-message-serialization` / PR #12 at head `9a41b0ecaa5838a7f469f2cb524a64c705cfa679`.

CI run #278 (run ID `35983223882`) passed the complete required matrix on that head:

- clang-format;
- Linux Core+Server Debug and Release build/test;
- Windows Core+Server Debug and Release build/test;
- Windows Client Debug and Release regression build/test.

Focused Core coverage includes direct operators, Message-constructor decoding, canonical empty Volume, asymmetric ordering, non-default packed Cells, 16×16×16/4096-Cell round trip, deterministic same-ABI bytes, malformed/truncated metadata and Cell blocks, overflow protection, destination preservation, Message lifetime independence, repeated round trips, and lazy power-of-two pool reuse.

The ticket remains **Ready**, not Done, pending explicit project-owner review/approval.
