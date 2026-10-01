# ST-001-05 — Voxel::Volume Message serialization

**Status:** Done
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Provide one shared Sparkle Message serialization contract for runtime-sized `Voxel::Volume` values used by both Server and Client.

## Starting state / decisions

- ST-001-02 and ST-001-03 are Done.
- DR-017 fixes the Volume wire/decode contract.
- OQ-037 is Resolved.
- DR-019 later supersedes destination-buffer reuse with shared immutable Volume backing.
- The merged Sparkle Version-0.1.3 Message redesign supersedes the original mutable `spk::Message << / >>` call shape with `spk::Message::Writer` insertion and `spk::Message::Reader` extraction.

## Public contract

Current API:

```cpp
spk::Message::Writer writer;
writer << volume;

spk::Message message = std::move(writer).build();

auto reader = message.reader();
reader >> volume;

Voxel::Volume decoded(message);
```

`Voxel::Volume` declares friend Writer/Reader operators in namespace `Voxel`:

```cpp
friend spk::Message::Writer &operator<<(
    spk::Message::Writer &writer,
    const Volume &volume);

friend const spk::Message::Reader &operator>>(
    const spk::Message::Reader &reader,
    Volume &volume);
```

`Volume(const spk::Message&)` creates a Reader and delegates to the same decode contract.

No separate serializer object or Volume-specific transport MessageID exists.

## Exact wire format

The Volume payload is serialized in this exact order:

1. `spk::Vector3UInt dimensions`;
2. `Voxel::Volume::UnitSize unitSize`;
3. one contiguous native block of `Voxel::Cell` values.

No explicit Cell-count field is serialized. The decoder derives:

```text
dimensions.x * dimensions.y * dimensions.z
```

with checked multiplication before allocation.

Cells remain Y-fastest, then X, then Z.

The representation is intentionally Sparkle-native. Cross-endian/platform-independent scalar encoding is not part of ST-001-05.

## Validity

The only valid empty Volume representation is:

```text
dimensions = {0, 0, 0}
unitSize   = 0.0f
cells      = empty
```

A valid non-empty representation requires:

- strictly positive dimensions;
- finite strictly positive unit size;
- representable Cell count/byte size;
- enough remaining Reader bytes for the complete Cell block.

Malformed dimensions, invalid unit size, arithmetic overflow, or truncated input throw `spk::Exception`.

Writer insertion validates the source Volume before serialization. Reader extraction validates metadata and available bytes before constructing replacement state.

## Decode atomicity and ownership

Reader extraction constructs a temporary valid Volume and replaces the destination only after complete decode succeeds. Failure leaves the destination unchanged.

Reader cursor advancement is local to that Reader. Earlier fields may be consumed in that Reader when a later step throws; there is no cursor rollback.

Independent Readers over the same immutable Message do not share cursor state.

For non-empty decode, fresh mutable pooled storage is obtained, filled, then published as immutable Volume backing. Existing shared Volume copies continue observing their old backing content.

Decoded Volume storage is independent from the source Message lifetime. Trailing bytes are allowed because Volume is an embeddable payload value.

## Invariants

- logical Cell count is derived only from dimensions;
- round trip preserves dimensions, unit size, and every packed Cell;
- no bytes depend on `sizeof(Voxel::Volume)` or `std::vector` object layout;
- Cells are transferred as one contiguous block;
- malformed input never replaces the destination;
- decoded backing does not alias mutable Message construction storage;
- Sparkle Message construction occurs through Writer and finalized Messages are immutable.

## Implementation constraints

- keep operators in namespace `Voxel`;
- `volume.hpp` includes `<network/message.hpp>`;
- networking decode does not use `Volume::Builder`;
- serialization implementation remains in `core/src/voxel/volume_networking.cpp`;
- use compile-time checks for native-layout assumptions;
- do not overwrite/reuse an already-published shared immutable destination buffer in place.

## Required tests

- Writer insertion through ADL;
- Reader extraction through ADL;
- `Volume(message)` convenience construction;
- empty round trip;
- asymmetric Volume proving Y/X/Z order;
- non-default packed Cell values;
- 16×16×16 / 4096-Cell round trip;
- same-ABI repeated byte stability;
- truncated dimensions/unit-size/Cell payload rejection;
- mixed-zero dimensions rejection;
- invalid unit-size rejection;
- overflow rejection before allocation;
- destination preservation after decode failure;
- decoded lifetime after source Message destruction;
- independent Reader cursor behavior;
- no raw `Voxel::Volume` / `std::vector` object serialization.

## Completion evidence

ST-001-05 was implemented on `feat/st-001-05-voxel-volume-message-serialization` and merged through PR #12 on 24 September 2026 after project-owner approval.

Final code-bearing head `0baf9d27698c67855c12abf021125a3575fc364d` passed CI run #289 (run ID `35986211668`) across the required matrix.

The later DR-019 change superseded only successful-decode in-place destination-buffer reuse.

The 1 October 2026 ST-001-11 planning branch migrates the completed codec and focused tests to Sparkle's immutable Message + Writer/Reader API without changing the logical ST-001-05 wire bytes or validation semantics.

## Decisions

- [DR-017](../../../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
- [OQ-037](../../../OPEN_QUESTIONS/OQ-037-EP001-NETWORK-SERIALIZATION.md)
