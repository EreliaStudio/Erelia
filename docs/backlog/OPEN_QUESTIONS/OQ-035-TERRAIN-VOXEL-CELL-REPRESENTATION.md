# OQ-035 — What is the first terrain voxel / cell representation?

**Status:** Resolved
**Decision records:** [DR-012](../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md), [DR-017](../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
**Affected areas:** EP-001, Core voxel data, networking

## Question

What is the first terrain voxel / cell representation?

## Problem / context

EP-001 needs a compact shared cell value and a reusable owning Volume that both Server and Client understand. The archived prototype is a useful reference but is not authoritative.

## Chosen solution

Use a 32-bit packed `Voxel::Cell` carrying Definition ID + Orientation + FlipOrientation, and a generic owning `Voxel::Volume`.

### Voxel::Cell

- the complete instance state is one private `Voxel::Cell::PackedType`, aliasing `std::uint32_t`;
- lower 29 bits store `Voxel::Definition::ID`;
- bits 29-30 store `Voxel::Cell::Orientation` with exact values `PositiveX = 0`, `NegativeX = 1`, `PositiveZ = 2`, `NegativeZ = 3`;
- bit 31 stores `Voxel::Cell::FlipOrientation` with exact values `PositiveY = 0`, `NegativeY = 1`;
- default construction and `Voxel::Cell::Empty` are packed zero;
- Definition ID 0 alone determines semantic emptiness; non-zero orientation/flip bits on ID 0 remain valid and are preserved;
- every raw `PackedType` is valid and preserved exactly;
- logical-field construction validates the 29-bit Definition capacity and enum domains and throws `spk::Exception` when invalid;
- the Cell is immutable after construction, exactly one `PackedType`, and trivially copyable.

### Voxel::Volume

- `Voxel::Volume` is a generic immutable built container;
- `Voxel::Volume::LocalCoordinate` aliases `spk::Vector3Int`;
- dimensions are `spk::Vector3UInt`;
- `Voxel::Volume::UnitSize` aliases `float`;
- default construction is the valid empty Volume: dimensions `{0,0,0}`, unit size `0.0f`, and zero Cells;
- non-empty construction goes through nested `Voxel::Volume::Builder`;
- Builder requires positive dimensions and finite positive unit size and rejects Cell-count overflow with `spk::Exception`;
- `Voxel::Volume::Buffer` is a semantic wrapper over `std::vector<Voxel::Cell>` and exposes `Buffer::Pool` and `Buffer::Lease`;
- Builder owns dimensions, unit size, and a pooled `Buffer::Lease`, and exposes checked `set()`;
- storage order is Y fastest, then X, then Z: `y + sizeY * (x + sizeX * z)`;
- `std::move(builder).build()` transfers the Builder state and pooled Buffer into the immutable Volume;
- built Volumes expose `contains()`, non-throwing `tryGet()` returning `std::optional<Cell>`, checked copy access through `at()` / `operator[]`, and a read-only contiguous `std::span<const Voxel::Cell>`;
- Volume copies/assignments deep-copy their Cells through the Pool Lease copy semantics and therefore own independent pooled Buffers;
- Volume moves transfer their Buffer lease and leave the source default-empty;
- `Builder(std::move(volume))` consumes the source Volume and directly reuses/transfers its existing pooled Buffer without copying;
- the pool registry is private to `volume_builder.cpp`;
- exact 16×16×16 dimensions use a dedicated Chunk `Buffer::Pool`;
- other dimensions use an ordered `std::map<std::size_t, Buffer::Pool>`, selecting with `lower_bound(requestedSize)` and creating a new exact size class only when no equal-or-larger class exists;
- Buffers are reset through the Pool per-obtain callback without intentionally discarding retained capacity;
- `VersionedTrait` and the previous Editor mutation model are no longer part of Volume.

`Voxel::Volume` later declares/uses the friend `spk::Message` insertion/extraction contract required by DR-017. The wire encoding itself belongs to ST-001-05 and is not decided here.

## Resolution provenance

The Cell portion was resolved by the project owner on 22 September 2026. The Volume storage, validation, immutable Builder model, pooled-buffer reuse, contiguous-view, and copy/move contracts were finalized by the project owner on 23 September 2026.

## Remaining ambiguity

None within OQ-035. Wire byte order/platform portability remains owned by OQ-037 and ST-001-05 rather than this question.
