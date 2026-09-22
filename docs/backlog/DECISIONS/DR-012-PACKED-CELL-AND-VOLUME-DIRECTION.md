# DR-012 — Packed Voxel::Cell and generic Voxel::Volume direction

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Core voxel representation, Chunk payloads, Server generation, Client meshing

## Context

EP-001 needs a compact cell representation and a reusable owning container for groups of voxel cells.

The project owner explicitly identified the archived packed Cell prototype and generic volume prototype as the desired design direction, while requiring the greenfield implementation to review rather than blindly copy archived code.

## Decision

### Cell

Use a compact `Voxel::Cell` whose complete logical representation fits in one `Voxel::Cell::PackedType`, with `PackedType` aliasing `std::uint32_t`.

The approved packed concepts are:

- runtime voxel Definition ID;
- horizontal Orientation;
- vertical Flip;
- ID 0 represents empty;
- packed representation is directly retrievable as `Voxel::Cell::PackedType`;
- the type must remain exactly 32 bits and trivially copyable.

`Voxel::Definition::ID` is the semantic identifier type for voxel definitions and aliases `std::uint32_t`.

The exact packed layout is:

- lower 29 bits: `Voxel::Definition::ID`;
- bits 29-30: `Voxel::Cell::Orientation`;
- bit 31: `Voxel::Cell::FlipOrientation`.

The exact enum mapping is:

- `Orientation::PositiveX = 0`;
- `Orientation::NegativeX = 1`;
- `Orientation::PositiveZ = 2`;
- `Orientation::NegativeZ = 3`;
- `FlipOrientation::PositiveY = 0`;
- `FlipOrientation::NegativeY = 1`.

This provides a maximum packed Definition ID of `0x1FFFFFFF` (536,870,911).

A Cell stores one private `Voxel::Cell::PackedType` rather than C++ bitfields. The value is immutable after construction and logical fields are retrieved through read-only mask/shift getters. Default construction and the explicit static `Voxel::Cell::Empty` value both use packed `0x00000000`.

Definition ID 0 alone determines semantic emptiness. Orientation and FlipOrientation remain valid when the Definition ID is 0; for example `0x60000000` is a valid semantically empty Cell and is preserved exactly.

Every raw `Voxel::Cell::PackedType` is a valid packed Cell representation. Logical-field construction validates Definition ID capacity and enum domains and throws `spk::Exception` for values outside those domains.

### Volume

Introduce a reusable owning voxel container concept named **`Voxel::Volume`**, rather than the prefixed `VoxelVolume` naming style.

Its intended responsibilities are:

- runtime dimensions;
- uniform voxel size;
- contiguous owning storage of `Voxel::Cell`;
- checked coordinate lookup;
- read-only contiguous cell access;
- local bounds derived from dimensions and voxel size;
- controlled mutation rather than unrestricted external writable storage.

This abstraction is intended to represent groups of voxel cells generically, including fixed-size terrain Chunk payloads and later runtime-sized voxel models where applicable.

## Consequences

- Terrain network payloads can represent cells compactly as 32-bit packed values.
- `Voxel::Volume` is an owning type containing `std::vector<Voxel::Cell>` and therefore is not itself trivially copyable; direct network use is provided by explicit logical serialization, not raw object copying. See DR-017.
- Server and Client share the same Cell semantics in Core.
- `Voxel::Volume` is not inherently a world Chunk: world position/Chunk coordinate remains separate semantic information.
- EP-001 network responses may therefore naturally contain `{chunkCoordinate, volumeData}`.
- The exact Volume storage order and editor/versioning behavior, plus wire byte-order, remain explicit follow-up contracts and are not inferred from the archive.
- `spk::Message << Voxel::Volume` / `>>` is the approved ergonomic serialization direction; see DR-017.

## Required tests

Once the remaining exact contracts are resolved:

- `std::is_same_v<Voxel::Cell::PackedType, std::uint32_t>`;
- `sizeof(Voxel::Cell) == sizeof(Voxel::Cell::PackedType)`;
- `std::is_trivially_copyable_v<Voxel::Cell>`;
- exact ID/orientation/flip packing fixtures using the approved enum mapping;
- ID-capacity boundary and invalid logical enum rejection through `spk::Exception`;
- default Cell and `Voxel::Cell::Empty` are packed zero;
- ID-0 Cells with non-zero orientation/flip bits remain semantically empty and preserve their packed value;
- every raw `Voxel::Cell::PackedType` value round-trips exactly;
- Volume construction/access/bounds/storage-order tests;
- invalid dimension/coordinate/voxel-size tests;
- mutation/versioning tests if versioning is retained.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-035 and providing the archived prototype as the preferred design basis.

## Supersession

None.
