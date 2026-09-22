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

- `Voxel::Volume` is a generic owning container and derives from `spk::VersionedTrait`;
- `Voxel::Volume::LocalCoordinate` aliases `spk::Vector3Int`;
- dimensions are represented by `spk::Vector3UInt`;
- `Voxel::Volume::UnitSize` aliases `float`;
- default construction is the sole valid empty Volume: dimensions `{0,0,0}`, unit size `0.0f`, and zero Cells;
- explicit construction requires every dimension to be strictly greater than zero and unit size to be finite and strictly greater than zero;
- invalid explicit dimensions, cell-count overflow, invalid unit size, invalid checked access, and invalid editor use throw `spk::Exception`;
- there is no project-defined maximum dimension beyond the requirement that `x * y * z` is representable by `std::size_t`; a representable allocation failure is allowed to propagate the standard allocation exception;
- explicit construction default-constructs every owned Cell, therefore every initial Cell is `Voxel::Cell::Empty`;
- storage is an owning contiguous `std::vector<Voxel::Cell>`;
- storage order is **Y fastest, then X, then Z**, with
  `index = y + sizeY * (x + sizeX * z)`;
- checked `at(LocalCoordinate)` returns a `Voxel::Cell` copy;
- read-only contiguous access is exposed as `std::span<const Voxel::Cell>`;
- the span remains valid across ordinary Editor mutations because storage is not resized; it is invalidated by destruction or any copy/move assignment that replaces the Volume state, and a span obtained from a source before move construction is invalid after the move;
- mutable storage is not exposed directly;
- mutation uses nested `Voxel::Volume::Editor` objects returned by `edit()`;
- `Editor::set()` returns `true` only when the Cell actually changes;
- one Editor batches all effective changes into exactly one `VersionedTrait::invalidate()` on commit;
- an Editor containing only no-op writes does not invalidate;
- destroying an uncommitted Editor commits it;
- explicit `commit()` is idempotent;
- using an Editor after commit throws `spk::Exception`;
- rejected Editor operations do not change Cells or publish a version;
- Volume copy construction copies dimensions, unit size, and Cells into an independent Volume with a fresh version state starting at 0 and no copied subscribers;
- copy assignment preserves the destination's subscriptions, replaces its logical state atomically, and invalidates the destination exactly once; self-assignment is a no-op;
- move construction transfers the logical state into an independent destination with a fresh version state, resets the source to the valid default-empty state, and invalidates/notifies the source exactly once;
- move assignment preserves the destination's subscriptions, transfers the logical state, invalidates the destination exactly once, resets the source to the valid default-empty state, and invalidates/notifies the source exactly once; self-move-assignment is a no-op;
- local-bounds API is deliberately not part of ST-001-03.

`Voxel::Volume` later declares/uses the friend `spk::Message` insertion/extraction contract required by DR-017. The wire encoding itself belongs to ST-001-05 and is not decided here.

## Resolution provenance

The Cell portion was resolved by the project owner on 22 September 2026. The remaining Volume storage, validation, editor/versioning, contiguous-view, copy/move, and failure contracts were resolved by the project owner on 23 September 2026.

## Remaining ambiguity

None within OQ-035. Wire byte order/platform portability remains owned by OQ-037 and ST-001-05 rather than this question.
