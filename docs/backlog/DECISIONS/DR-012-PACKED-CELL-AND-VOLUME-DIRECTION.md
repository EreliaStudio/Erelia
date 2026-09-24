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

Introduce a reusable immutable built voxel container named **`Voxel::Volume`** with a nested mutable **`Voxel::Volume::Builder`**.

The approved Volume contract is:

- `LocalCoordinate` is `spk::Vector3Int`;
- dimensions are `spk::Vector3UInt`;
- `UnitSize` is `float`;
- default Volume construction creates the valid empty value: zero dimensions, zero unit size, and zero Cells;
- non-empty Volumes are produced through `Voxel::Volume::Builder(dimensions, unitSize)`;
- Builder construction requires all dimensions > 0 and a finite unit size > 0;
- Cell count must be representable by `std::size_t`;
- `Voxel::Volume::Buffer` is the semantic Cell-buffer type, derives from `std::vector<Voxel::Cell>`, and exposes nested `Buffer::Pool` and `Buffer::Lease` aliases;
- each built Volume directly owns dimensions, unit size, and one `Buffer::Lease`;
- storage order is Y-fastest, then X, then Z: `index = y + sizeY * (x + sizeX * z)`;
- `Builder::set()` is checked and returns true only when the packed Cell value changes;
- `std::move(builder).build()` transfers the Builder's dimensions, unit size, and pooled Buffer lease into an immutable Volume;
- built Volume exposes only read operations: dimensions, unit size, `contains()`, non-throwing `tryGet()` returning `std::optional<Cell>`, checked `at()` / `operator[]` returning Cell copies, and read-only contiguous `cells()`;
- Volume copy construction/assignment performs a deep Cell copy through `Buffer::Lease` copy semantics, producing independent pooled storage;
- Volume move transfers the existing Buffer lease and leaves the source default-empty;
- constructing a Builder from `std::move(volume)` consumes the source and directly transfers/reuses its existing Buffer lease without copying;
- destroying/replacing the final Lease returns the Buffer to its originating Pool;
- Pool instances are source-file implementation details of the Builder;
- exact 16×16×16 dimensions use a dedicated Chunk `Buffer::Pool`;
- all other dimensions use an ordered `std::map<std::size_t, Buffer::Pool>` keyed by size class;
- general selection uses `lower_bound(requestedCellCount)`: exact class when present, otherwise the smallest higher class, otherwise a newly-created class at the requested size;
- pool factories reserve their size-class capacity; per-obtain preparation resets the Buffer logical contents while preserving reusable capacity;
- `Voxel::Volume` no longer derives from `spk::VersionedTrait` and no Editor API is part of the contract;
- no local-bounds API is part of this first Volume contract.

## Consequences

- Terrain network payloads can represent cells compactly as 32-bit packed values.
- `Voxel::Volume` owns pooled dynamic Cell storage and therefore is not itself trivially copyable; direct network use is provided by explicit logical serialization, not raw object copying. See DR-017.
- Volume copies are value copies with independent Cell storage; moves and the Volume-to-Builder path transfer pooled storage without copying.
- Server and Client share the same Cell semantics in Core.
- `Voxel::Volume` is not inherently a world Chunk: world position/Chunk coordinate remains separate semantic information.
- EP-001 network responses may therefore naturally contain `{chunkCoordinate, volumeData}`.
- The Volume storage/indexing, Builder, pooled-buffer, deep-copy, contiguous-view, and copy/move contracts are fixed by this record; wire byte-order remains a separate follow-up contract.
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
- Volume default construction, Builder construction, Y-X-Z storage-order, checked-access, `tryGet()`, and span tests;
- invalid dimension/product-overflow/coordinate/unit-size tests;
- Builder mutation, checked-rejection, build, and moved-Volume reconstruction tests;
- deep-copy Volume construction/assignment tests proving independent Cell-buffer addresses;
- Volume move and Volume-to-Builder tests proving pooled Buffer transfer/reuse;
- dedicated Chunk-pool and ordered general size-class reuse tests.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 and refined during ST-001-03 review on 2026-09-23 after introducing the reusable Sparkle Pool.

## Supersession

None.
