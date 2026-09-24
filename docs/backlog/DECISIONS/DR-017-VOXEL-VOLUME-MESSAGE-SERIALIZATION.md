# DR-017 — Voxel::Volume serializes directly through spk::Message

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Last clarified:** 2026-09-24
**Applies to:** Core voxel representation, EP-001 Chunk payloads, Client/Server serialization

## Context

The EP-001 Chunk response naturally contains a Chunk coordinate and a `Voxel::Volume`.

The desired call-site API is intentionally simple and direct:

```cpp
message << volume;
message >> volume;
```

`Voxel::Cell` is exactly 32 bits and trivially copyable. `Voxel::Volume`, however, owns its cells through `std::vector<Voxel::Cell>`, so the C++ object itself is **not** trivially copyable and must not be transmitted by copying `sizeof(Voxel::Volume)` bytes.

Doing so would serialize the vector's implementation state (pointer/size/capacity), not the owned cells.

## Decision

Provide explicit Erelia/Sparkle message serialization support for `Voxel::Volume` so callers can write/read a complete logical Volume directly using `spk::Message`.

The operators are part of the `Voxel::Volume` public contract and are declared as friends directly on the class:

```cpp
namespace Voxel
{
    class Volume
    {
        // ...

        friend spk::Message &operator<<(
            spk::Message &message,
            const Volume &volume);

        friend const spk::Message &operator>>(
            const spk::Message &message,
            Volume &volume);
    };
}
```

The corresponding free-function definitions belong to namespace `Voxel`, allowing argument-dependent lookup to resolve:

```cpp
message << volume;
message >> volume;
```

`Voxel::Volume` additionally exposes:

```cpp
explicit Volume(const spk::Message &message);
```

The constructor delegates to `message >> *this`, so it is only a convenience construction form over the same extraction contract and validation rules.

This friend-operator form remains the core EP-001 serialization API; an alternative `volume.serialize(message)`, serializer object, or Volume-specific transport Message type does not replace it. `Voxel::Volume::Builder` is not part of networking reconstruction.

### Exact wire order

A serialized Volume contains, in this exact order:

1. `spk::Vector3UInt dimensions` as one Sparkle-native trivially-copyable value;
2. `Voxel::Volume::UnitSize unitSize` as one Sparkle-native value;
3. one contiguous native block of `Voxel::Cell` values in the Volume's existing Y-fastest, then X, then Z storage order.

No explicit Cell-count field is present. The decoder derives the expected count from the dimensions using checked multiplication.

The format intentionally uses Sparkle-native representation rather than defining an Erelia-specific fixed endian or floating-point wire encoding. Consequently deterministic byte equality is guaranteed for repeated serialization under the same supported ABI/platform representation, not as a cross-endian/cross-floating-representation portability guarantee.

The implementation must assert the native-layout assumptions it relies on where practical; in particular, `Voxel::Cell` remains exactly 32 bits and trivially copyable, and `spk::Vector3UInt` is treated as the native Sparkle value requested by the project owner.

### Valid and invalid states

The only serialized empty Volume is:

```text
dimensions = {0, 0, 0}
unitSize   = 0.0f
cells      = empty
```

For a non-empty Volume:

- all three dimensions are strictly positive;
- `unitSize` is finite and strictly positive;
- the derived Cell count is representable;
- the payload contains exactly enough Cell bytes for that derived count before the Cell block is read.

Mixed-zero dimensions, `{0,0,0}` with non-zero unit size, non-empty dimensions with zero/negative/NaN/infinite unit size, arithmetic overflow, and truncated metadata/Cell data are malformed and throw `spk::Exception`.

Both `operator<<` and `operator>>` validate these invariants. Insertion validates before appending Volume bytes for contract-invalid source state.

### Extraction state and ownership

Extraction reconstructs a temporary valid owning `Voxel::Volume` and assigns/moves it into the destination only after the complete decode succeeds. Therefore a failed extraction leaves the destination Volume unchanged.

The `spk::Message` read cursor follows Sparkle Version-0.1.3's normal sequential extraction semantics: successfully-read earlier fields remain consumed if a later read/validation step throws. This is intentional; callers that need transaction-like replacement of larger application state should decode into temporary domain objects and commit them only after the containing message has fully validated.

Before allocating the Cell buffer, extraction must validate the dimension product and verify that the Message has enough remaining bytes for the derived contiguous Cell block. No additional arbitrary Erelia Volume dimension cap is introduced by this ticket.

For a non-empty decoded Volume, extraction obtains a pooled `Buffer::Lease` directly from the shared capacity-based Volume buffer-pool implementation, pulls the contiguous Cell block into it, and constructs the temporary Volume. An empty decode keeps the Lease default-constructed/null. Networking does not construct a `Volume::Builder`.

The decoded Volume owns its Cell storage independently of the source Message lifetime. Trailing Message bytes are permitted because Volume is an embeddable payload value rather than a complete transport message.

## Consequences

- Chunk/protocol code can remain concise and domain-shaped rather than manually serializing every Cell at each call site.
- One 16×16×16 terrain Volume contains 4096 Cells, so its Cell block is 16 KiB before Volume metadata.
- `Voxel::Volume` does **not** need to be trivially copyable.
- Compact contiguous transfer uses the already-approved packed `Voxel::Cell` representation.
- Serialization belongs in shared Core code because both Client and Server require it.
- Higher-level protocol message IDs remain owned by protocol tickets such as ST-001-08 rather than `Voxel::Volume`.
- The format inherits Sparkle's native representation assumptions; platform-independent wire encoding is not part of ST-001-05.

## Required tests

- `spk::Message message; message << volume;` resolves through ADL.
- `message >> volume;` resolves through ADL.
- `Voxel::Volume(message)` delegates to the same extraction/validation contract.
- default/empty Volume round trip.
- asymmetric Volume proves exact Y/X/Z Cell order.
- transformed/non-default packed Cell values round trip exactly.
- 16×16×16 Volume round trip.
- repeated serialization of the same logical Volume yields the same bytes on the current supported ABI.
- truncated dimensions metadata is rejected.
- truncated unit-size metadata is rejected.
- truncated Cell block is rejected.
- mixed-zero dimensions are rejected.
- invalid empty/non-empty unit-size combinations are rejected.
- impossible/overflowing dimension products are rejected before Cell allocation.
- decode failure leaves the destination Volume unchanged.
- decoded Cell storage remains valid after the source Message is destroyed.
- no test or implementation depends on `sizeof(Voxel::Volume)` or raw `std::vector` object representation.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22: prefer sending a complete `Voxel::Volume` through a simple `MyMessage << myVoxelVolume`-style API.

Clarified with the project owner on 24 September 2026 for ST-001-05 readiness:

- retain the direct friend operator API;
- use Sparkle-native scalar representation;
- serialize `spk::Vector3UInt` as one native Message value;
- derive Cell count from dimensions rather than serializing a redundant count;
- transfer Cells as one contiguous native block;
- validate the same Volume invariants on insertion and extraction;
- preserve the destination Volume on failed extraction;
- retain Sparkle's normal partial read-cursor advancement behavior on failure;
- expose an explicit Message constructor as a convenience over `operator>>`;
- keep networking reconstruction independent from `Volume::Builder`.

## Supersession

None.
