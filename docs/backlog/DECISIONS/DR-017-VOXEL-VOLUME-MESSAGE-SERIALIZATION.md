# DR-017 — Voxel::Volume serializes directly through spk::Message

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
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

The serializer operates on the logical Volume state:

- `spk::Vector3UInt dimensions`;
- `float voxelSize`;
- the contiguous `Voxel::Cell` storage.

The cell storage should be appended/read as one contiguous block where practical, rather than serializing 4096 terrain cells through 4096 high-level message calls.

The deserializer must reconstruct an owning `Voxel::Volume`; it must never restore `std::vector` object representation from the network.

Exact source-level overload placement and the Volume construction/deserialization helper are implementation-ticket details, but the public ergonomic contract is:

```cpp
spk::Message &operator<<(spk::Message &, const Voxel::Volume &);
const spk::Message &operator>>(const spk::Message &, Voxel::Volume &);
```

or an equivalent API preserving `message << volume` / `message >> volume` usage.

## Consequences

- Chunk response code can remain concise and domain-shaped rather than manually serializing every cell at each call site.
- One 16×16×16 terrain Volume contains 4096 cells, so its raw Cell storage is 16 KiB before Volume metadata.
- `Voxel::Volume` does **not** need to be trivially copyable.
- `Voxel::Cell` retaining a fixed 32-bit packed representation remains useful for compact contiguous transfer.
- Serialization belongs in shared Core code because both Client and Server require it.
- A malformed Volume payload must be rejected before creating inconsistent dimensions/storage.
- The exact byte-order/platform-compatibility policy is not decided by this record and remains part of Q-037 until explicitly resolved.

## Required tests

- Volume round trip preserves dimensions, voxel size, and every Cell exactly.
- empty/default Cell values round trip.
- transformed Cell orientation/flip values round trip.
- 16×16×16 terrain Volume round trip.
- malformed/truncated payload is rejected.
- impossible/unrepresentable dimensions are rejected.
- payload whose cell data cannot satisfy the declared dimensions is rejected.
- serialization never depends on `sizeof(Voxel::Volume)` or the object representation of `std::vector`.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22: prefer sending a complete `Voxel::Volume` through a simple `MyMessage << myVoxelVolume`-style API, with sending just the contiguous cells also considered acceptable internally.

The implementation distinction that `Voxel::Volume` itself is not trivially copyable follows from its owning `std::vector<Voxel::Cell>` member and does not alter the requested call-site API.

## Supersession

None.
