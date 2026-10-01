# DR-017 — Voxel::Volume Sparkle Message serialization

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Last clarified:** 2026-10-01 (Sparkle Message Writer/Reader migration)
**Applies to:** Core voxel representation, EP-001 payloads, Client/Server serialization

## Context

`Voxel::Cell` is exactly 32 bits and trivially copyable. `Voxel::Volume` owns its Cells through pooled storage and is not trivially copyable, so the C++ object itself must never be serialized by copying `sizeof(Voxel::Volume)` bytes.

ST-001-05 originally delivered a concise friend insertion/extraction API against the then-current mutable Sparkle Message surface. Sparkle Version-0.1.3 has since changed its Message model:

- finalized `spk::Message` is immutable;
- construction/serialization uses `spk::Message::Writer`;
- decoding uses independent `spk::Message::Reader` values;
- Readers retain shared access to the immutable pooled Message storage.

The logical Volume wire format and validation contract remain unchanged.

## Decision

`Voxel::Volume` exposes Sparkle Writer/Reader serialization operators:

```cpp
namespace Voxel
{
    class Volume
    {
        friend spk::Message::Writer &operator<<(
            spk::Message::Writer &writer,
            const Volume &volume);

        friend const spk::Message::Reader &operator>>(
            const spk::Message::Reader &reader,
            Volume &volume);
    };
}
```

Definitions belong to namespace `Voxel` so ADL resolves:

```cpp
spk::Message::Writer writer;
writer << volume;

spk::Message message = std::move(writer).build();
auto reader = message.reader();
reader >> volume;
```

`Voxel::Volume` additionally exposes:

```cpp
explicit Volume(const spk::Message &message);
```

The constructor creates a Reader from the Message and delegates to the same extraction contract. It is only a convenience form.

No Volume-specific transport Message type or separate serializer object is introduced.

### Exact wire order

A serialized Volume contains, in this exact order:

1. `spk::Vector3UInt dimensions` as one Sparkle-native trivially-copyable value;
2. `Voxel::Volume::UnitSize unitSize`;
3. one contiguous native block of `Voxel::Cell` values in Y-fastest, then X, then Z storage order.

No explicit Cell count is serialized. The decoder derives it from dimensions using checked multiplication.

The format intentionally uses Sparkle-native representation. Cross-endian/platform-independent representation is not part of this contract.

### Valid and invalid states

The only valid serialized empty Volume is:

```text
dimensions = {0, 0, 0}
unitSize   = 0.0f
cells      = empty
```

For a non-empty Volume:

- all three dimensions are strictly positive;
- `unitSize` is finite and strictly positive;
- derived Cell count/byte size is representable;
- the Reader contains enough remaining bytes for the complete Cell block.

Mixed-zero dimensions, invalid unit sizes, arithmetic overflow, or truncated metadata/Cells throw `spk::Exception`.

Both Writer insertion and Reader extraction validate the Volume contract.

### Extraction state and ownership

Extraction reconstructs a temporary valid Volume and replaces the destination only after complete decode succeeds. Decode failure leaves the destination unchanged.

Reader cursor semantics are local to that Reader. Successfully-read fields remain consumed in the Reader if a later extraction step fails; no cursor rollback is required.

A caller needing transaction-like parsing of a larger message creates/owns its Reader and commits higher-level state only after its complete parse succeeds.

Before allocating Cell storage, extraction validates dimensions/unit size, checked byte counts, and remaining Reader bytes.

For each non-empty decode, extraction obtains fresh mutable pooled Volume storage, copies the Cell block, constructs immutable Volume content, and only then replaces the destination. It never overwrites existing shared immutable backing in place.

A decoded Volume owns its Cell storage independently from the source Message/Reader lifetime. Trailing bytes remain valid because Volume is an embeddable payload value.

## Consequences

- Core protocol codecs can serialize Volume values through the same Writer/Reader model used by other domain values.
- Independent Message Readers make serialization compatible with ST-001-11 parallel response-section parsing.
- `Voxel::Volume` does not need to be trivially copyable.
- one 16×16×16 Cell block is 4096 Cells / 16 KiB before metadata;
- DR-019 shared immutable backing remains authoritative;
- dedicated fixed-size Chunk serialization may omit redundant Volume metadata.

## Required tests

- Writer insertion resolves through ADL.
- Reader extraction resolves through ADL.
- `Voxel::Volume(message)` uses the same decode contract.
- canonical empty round trip.
- asymmetric Volume proves Y/X/Z order.
- packed Cell variants round trip.
- 16×16×16 Volume round trip.
- same-ABI repeated serialization produces equal bytes.
- malformed/truncated dimensions, unit size, and Cell block are rejected.
- invalid dimension/unit-size combinations are rejected.
- overflow is rejected before allocation.
- failed decode leaves destination unchanged.
- decoded storage survives source Message destruction.
- independent Readers over the same Message may decode without shared cursor state.
- no implementation depends on raw `Voxel::Volume` or `std::vector` object representation.

## Resolution provenance

The logical format was resolved with the project owner on 2026-09-22/24 and delivered by ST-001-05.

The 2026-10-01 clarification updates only the Sparkle call-site/API model after the merged Version-0.1.3 Message redesign: mutable `spk::Message << / >>` is superseded by `Message::Writer <<` and `Message::Reader >>`.

## Supersession

DR-019 supersedes the former successful-decode optimization that reused/overwrote the destination's same-size-class Buffer. Shared immutable backing requires fresh decode storage before replacement.

ST-001-08 later introduced a dedicated fixed-size Chunk codec. That does not remove this generic runtime-sized Volume codec.
