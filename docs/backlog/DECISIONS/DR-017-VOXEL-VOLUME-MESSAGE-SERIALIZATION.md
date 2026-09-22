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

The operators are part of the `Voxel::Volume` public contract and are declared as friends directly on the class so they may access the owning Volume state without exposing mutable serialization-only accessors:

```cpp
namespace Voxel
{
	class Volume : public spk::VersionedTrait
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

This friend-operator form is the required public API for EP-001; an alternative `volume.serialize(message)`-style interface does not satisfy this decision.

The exact internal deserialization construction helper remains an implementation-ticket detail.

## Consequences

- Chunk response code can remain concise and domain-shaped rather than manually serializing every cell at each call site.
- One 16×16×16 terrain Volume contains 4096 cells, so its raw Cell storage is 16 KiB before Volume metadata.
- `Voxel::Volume` does **not** need to be trivially copyable.
- `Voxel::Cell` retaining a fixed 32-bit packed representation remains useful for compact contiguous transfer.
- Serialization belongs in shared Core code because both Client and Server require it.
- Serialization operators are friends of `Voxel::Volume`; Volume does not expose mutable internals merely to support networking.
- Operator definitions live in namespace `Voxel`, not namespace `spk`.
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
- serialization never depends on `sizeof(Voxel::Volume)` or the object representation of `std::vector`;
- compile/use test proves `spk::Message message; message << volume;` resolves without explicit serializer calls;
- deserialize-use test proves `message >> volume;` resolves through the friend operator contract.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22: prefer sending a complete `Voxel::Volume` through a simple `MyMessage << myVoxelVolume`-style API, with sending just the contiguous cells also considered acceptable internally.

The implementation distinction that `Voxel::Volume` itself is not trivially copyable follows from its owning `std::vector<Voxel::Cell>` member and does not alter the requested call-site API.

The project owner subsequently clarified that the exact desired API is the friend-operator form declared directly inside `Voxel::Volume`; this clarification is incorporated into the resolved decision.

## Supersession

None.
