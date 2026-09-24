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

Use a compact `Voxel::Cell` whose complete logical representation fits in one `std::uint32_t`.

The approved packed concepts are:

- runtime voxel Definition ID;
- horizontal Orientation;
- vertical Flip;
- ID 0 represents empty;
- packed representation is directly retrievable as a `std::uint32_t`;
- the type must remain exactly 32 bits and trivially copyable.

The archived bit allocation is the approved starting layout:

- upper 2 bits below the sign/high bit: Orientation;
- highest bit: Flip;
- remaining lower 29 bits: Definition ID.

This provides a maximum packed Definition ID of `0x1FFFFFFF` (536,870,911).

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
- The exact storage order, empty-cell canonicalization, editor/versioning behavior, and wire byte-order remain explicit follow-up contracts and are not inferred from the archive.
- `spk::Message << Voxel::Volume` / `>>` is the approved ergonomic serialization direction; see DR-017.

## Required tests

Once the remaining exact contracts are resolved:

- `sizeof(Voxel::Cell) == sizeof(std::uint32_t)`;
- `std::is_trivially_copyable_v<Voxel::Cell>`;
- exact ID/orientation/flip packing fixtures;
- ID-capacity boundary and overflow rejection;
- default Cell is empty;
- packed round trip;
- Volume construction/access/bounds/storage-order tests;
- invalid dimension/coordinate/voxel-size tests;
- mutation/versioning tests if versioning is retained.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-035 and providing the archived prototype as the preferred design basis.

## Supersession

None.
