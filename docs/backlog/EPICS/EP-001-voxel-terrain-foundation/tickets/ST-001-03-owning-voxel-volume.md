# ST-001-03 — Owning Voxel::Volume

**Status:** Done
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Introduce the reusable immutable built `Voxel::Volume` representation and its mutable `Voxel::Volume::Builder`, with pooled Cell-buffer reuse suitable for both fixed terrain Chunks and later runtime-sized voxel models.

## Starting state / prerequisites

- ST-001-02 is Done and merged.
- OQ-035 is Resolved.
- DR-012 fixes the packed Cell and current Volume/Builder direction.
- Sparkle Version-0.1.3 provides the reusable `spk::Pool<TElement>` used by Volume buffers.
- DR-017 fixes the later `spk::Message` friend-operator direction; encoding remains ST-001-05.

## Product ownership

Core owns the generic representation.

## Public contract

### Voxel::Volume

Semantic aliases and nested types:

- `Voxel::Volume::LocalCoordinate = spk::Vector3Int`;
- `Voxel::Volume::UnitSize = float`;
- `Voxel::Volume::Buffer` derives from `std::vector<Voxel::Cell>`;
- `Voxel::Volume::Buffer::Pool = spk::Pool<Buffer>`;
- `Voxel::Volume::Buffer::Lease = Buffer::Pool::Lease`.

Required API:

- default construction;
- deep-copy construction and assignment;
- move construction and assignment;
- read-only `dimensions()`;
- read-only `unitSize()`;
- `contains(LocalCoordinate)` for a pure bounds query;
- `tryGet(LocalCoordinate)` returning `std::optional<Voxel::Cell>` without throwing for an out-of-range coordinate;
- checked `at(LocalCoordinate)` returning a `Voxel::Cell` copy;
- checked `operator[](LocalCoordinate)` with the same copy semantics;
- read-only contiguous `cells()` returning `std::span<const Voxel::Cell>`.

Default construction is the valid empty Volume: dimensions `{0,0,0}`, unit size `0.0f`, and zero Cells.

A non-empty Volume is immutable after build.

### Voxel::Volume::Builder

The complete nested Builder declaration lives in `volume_builder.hpp`.

Required API:

- `Builder(spk::Vector3UInt dimensions, UnitSize unitSize)`;
- `Builder(Volume &&volume)`;
- move-only Builder semantics;
- checked `set(LocalCoordinate, Cell)`, returning true only when the packed Cell value changes;
- rvalue-qualified `std::move(builder).build()` producing a Volume.

Explicit Builder construction requires every dimension to be positive, finite positive unit size, and a Cell-count product representable by `std::size_t`.

## Storage and determinism

A Volume or Builder directly owns:

- dimensions;
- unit size;
- one `Voxel::Volume::Buffer::Lease`.

There is no shared backing Content object.

Storage order is **Y fastest, then X, then Z**:

```text
index = y + sizeY * (x + sizeX * z)
```

The Builder owns the exact logical Cell count. Pool reuse may provide a Buffer whose retained capacity is larger than its current logical size.

## Copy / move and destructive rebuild

### Volume copy

Copy construction and copy assignment are deep value copies.

`Buffer::Lease` copy semantics obtain another Buffer from the source Lease's Pool and copy-assign the Buffer contents. The copied Volume therefore has independent Cell storage.

Copy assignment prepares the replacement Volume before moving it into the destination, so a failed deep copy does not partially replace the destination.

### Volume move

Move construction and assignment transfer the existing Buffer Lease and leave the source Volume in the valid default-empty state.

### Builder move

Builder is move-only.

### Builder from Volume

`Builder(std::move(volume))` consumes the source Volume and directly transfers its dimensions, unit size, and existing Buffer Lease into the Builder.

Because Volume copies already own independent Buffers, this path never needs shared-ownership detection or copy-on-write logic.

### Build

`std::move(builder).build()` transfers the Builder dimensions, unit size, and existing Buffer Lease directly into the immutable Volume.

## Pooled Cell buffers

Pool objects are source-file implementation details in `core/src/voxel/volume_builder.cpp`.

### Chunk pool

Exact `16×16×16` dimensions use one dedicated process-lifetime `Voxel::Volume::Buffer::Pool`.

Another Volume shape with the same total Cell count does **not** use the Chunk pool.

### General pool registry

Other dimensions use:

```cpp
std::map<std::size_t, Voxel::Volume::Buffer::Pool>
```

The key is the pool's intended capacity size class.

Selection uses `lower_bound(requestedCellCount)`:

1. use an exact existing class when present;
2. otherwise use the smallest existing higher class;
3. when no equal-or-higher class exists, create a new pool at the requested size.

Each pool factory creates a Buffer and reserves its size-class capacity. Per-obtain preparation clears/resizes the Buffer to the requested logical Cell count while retaining reusable capacity.

The Pool implementation is intentionally single-threaded; this ticket introduces no concurrent Builder/pool access contract.

## Checked access and failure behavior

- `contains()` is false for every coordinate on the default-empty Volume;
- `tryGet()` returns `std::nullopt` for coordinates outside the Volume;
- Volume `at()` / `operator[]` outside dimensions throw `spk::Exception`;
- Builder `set()` outside dimensions throws `spk::Exception`;
- invalid Builder dimensions or unit size throw `spk::Exception`;
- Cell-count overflow throws `spk::Exception`;
- failed checked operations do not alias another Cell;
- Pool preparation/copy failures follow the Sparkle Pool exception contract.

## Explicitly removed from the prior review implementation

- `spk::VersionedTrait` inheritance;
- `Voxel::Volume::Editor`;
- `volume_editor.hpp`;
- shared backing Content;
- copy-on-write/shared-ownership checks;
- mutation/version subscriptions on built Volumes.

## Explicitly not owned

- world/Chunk coordinate identity;
- terrain generation;
- streaming/cache policy;
- Definition/Shape catalog semantics;
- graphics resources;
- network framing or Volume wire encoding;
- local bounds.

## Acceptance coverage

Core tests cover:

- default empty Volume;
- Builder construction and default-empty Cells;
- exact Y-X-Z storage order;
- effective/no-op Builder writes;
- `contains()` bounds queries;
- `tryGet()` success and out-of-range `std::nullopt` behavior;
- checked Volume and Builder coordinates;
- invalid dimensions, unit sizes, and Cell-count overflow;
- read-only contiguous Cell view;
- deep-copy construction and assignment with independent Buffer addresses;
- move-to-empty Volume behavior;
- direct Buffer reuse when constructing Builder from a moved Volume;
- copied Volumes remaining independent when one copy is moved into a Builder and modified;
- dedicated 16×16×16 Chunk pool reuse and isolation from another 4096-cell shape;
- ordered general pool reuse where a smaller request consumes the smallest available higher size class.

## Serialization / networking

Not implemented here. ST-001-05 owns DR-017 logical `spk::Message` serialization.

## Decisions

- [DR-012](../../../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [DR-017](../../../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
- [OQ-035](../../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md)

## Completion evidence

Implementation branch: `feat/st-001-03-owning-voxel-volume`.

Review PR: #9 — `ST-001-03 — Owning Voxel::Volume`.

Production changes include:

- `core/include/erelia/core/voxel/volume.hpp`;
- `core/include/erelia/core/voxel/volume_builder.hpp`;
- `core/src/voxel/volume.cpp`;
- `core/src/voxel/volume_builder.cpp`;
- removal of the superseded `volume_editor.hpp` and shared `volume_content.hpp`;
- `core/CMakeLists.txt`.

Acceptance coverage is in `core/tests/voxel_volume_test.cpp`.

Project-owner approval was explicitly recorded on 23 September 2026. CI run #105 passed clang-format, Linux/Windows Core+Server Debug/Release builds and CTest, plus Windows Client Debug/Release regression builds and CTest. The ticket is **Done**; PR #9 remains open only for the final merge into the planning baseline.
