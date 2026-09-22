# OQ-035 — What is the first terrain voxel / cell representation?

**Status:** Partially resolved
**Decision records:** [DR-012](../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md), [DR-017](../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
**Affected areas:** EP-001, Core voxel data, networking

## Question

What is the first terrain voxel / cell representation?

## Problem / context

EP-001 needs a compact shared cell value and a reusable owning Volume that both Server and Client understand. The archived prototype is a useful reference but must not be copied blindly.

## Known constraints

- `Voxel::Cell` should fit in one 32-bit value.
- ID 0 denotes empty.
- `Voxel::Volume` owns contiguous Cell storage plus dimensions and voxel size.
- Volume serialization uses friend `spk::Message` operators.

## Possible solutions

1. Keep the archived-style 32-bit packed Cell and generic owning Volume, while re-deciding questionable details.
2. Store Shape/material directly in every terrain cell.
3. Use a terrain-specific Chunk array without a generic Volume abstraction.

## Remaining ambiguity

The Cell contract is now sufficiently resolved for ST-001-02. Remaining OQ-035 ambiguity belongs to the later `Voxel::Volume` ticket: exact contiguous storage/index order, invalid dimensions/unit-size behavior, controlled editor/versioning behavior, coordinate-failure behavior, and contiguous-view lifetime/invalidation.

## Chosen solution

Use a 32-bit packed `Voxel::Cell` carrying Definition ID + Orientation + FlipOrientation, and a generic `Voxel::Volume` owning dimensions, voxel size and contiguous cells.

For `Voxel::Cell` specifically:

- the complete instance state is one private `std::uint32_t`;
- lower 29 bits store Definition ID;
- bits 29-30 store `Voxel::Cell::Orientation` with exact values `PositiveX = 0`, `NegativeX = 1`, `PositiveZ = 2`, `NegativeZ = 3`;
- bit 31 stores `Voxel::Cell::FlipOrientation` with exact values `PositiveY = 0`, `NegativeY = 1`;
- default construction produces packed value `0x00000000`;
- `Voxel::Cell::Empty` is the explicit static empty value and is also packed `0x00000000`;
- semantic emptiness depends only on Definition ID being 0. Orientation/FlipOrientation bits are still valid for an ID-0 Cell, so values such as `0x60000000` are accepted and preserved rather than canonicalized;
- construction from any packed `std::uint32_t` is valid and preserves that value exactly;
- construction from logical fields validates the 29-bit Definition capacity and the enum domains, throwing `spk::Exception` for invalid logical input;
- the Cell is immutable after construction and exposes read-only getters for Definition ID, Orientation, FlipOrientation, plus the packed `std::uint32_t` representation;
- bit extraction uses masks/shifts against the stored integer rather than C++ bitfields, avoiding implementation-defined bitfield layout while keeping the object exactly 32 bits and trivially copyable.

`Voxel::Volume` declares friend `spk::Message` insertion/extraction operators for direct `message << volume` / `message >> volume` use. Its remaining representation details stay open for ST-001-03, so OQ-035 remains Partially resolved.
