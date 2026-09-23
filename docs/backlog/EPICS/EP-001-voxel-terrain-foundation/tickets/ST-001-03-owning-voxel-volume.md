# ST-001-03 — Owning Voxel::Volume

**Status:** In Progress
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Introduce the reusable immutable built `Voxel::Volume` representation and its mutable `Voxel::Volume::Builder`, with pooled Cell-buffer reuse suitable for both fixed terrain Chunks and later runtime-sized voxel models.

## Starting state / prerequisites

- ST-001-02 is Done and merged.
- OQ-035 is Resolved.
- DR-012 fixes the packed Cell and current Volume/Builder direction.
- Sparkle Version-0.1.3 now provides the reusable `spk::Pool<TElement>` used by the Builder.
- DR-017 fixes the later `spk::Message` friend-operator direction; encoding remains ST-001-05.

## Product ownership

Core owns the generic representation.

## Public contract

### Voxel::Volume

Semantic aliases:

- `Voxel::Volume::LocalCoordinate = spk::Vector3Int`;
- `Voxel::Volume::UnitSize = float`.

Required API:

- default construction;
- cheap copy/move construction and assignment;
- read-only `dimensions()`;
- read-only `unitSize()`;
- `contains(LocalCoordinate)`;
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

Storage order is **Y fastest, then X, then Z**:

```text
index = y + sizeY * (x + sizeX * z)
```

The Builder owns exact logical Cell count. Pool reuse may provide a vector whose retained capacity is larger than its current logical size.

## Immutable sharing and destructive rebuild

A built Volume owns a shared backing Content object.

- Volume copy construction/assignment shares that immutable Content and does not duplicate Cells.
- Volume move transfers Content and leaves the source default-empty.
- `Builder(std::move(volume))` consumes the source Volume.
- If the consumed Content is uniquely owned, Builder reuses that same Content and Cell-buffer lease.
- If other Volume copies share the Content, Builder obtains another pooled buffer and copies Cells before mutation.
- This guarantees that rebuilding one moved Volume never mutates another immutable copy.

## Pooled Cell buffers

Backing Content owns a `spk::Pool<std::vector<Voxel::Cell>>::Lease`.

Pool objects are source-file implementation details in `core/src/voxel/volume_builder.cpp`.

### Chunk pool

Exact `16×16×16` dimensions use one dedicated process-lifetime Chunk Cell-buffer pool.

Another Volume shape with the same total Cell count does **not** use the Chunk pool.

### General pool registry

Other dimensions use:

```cpp
std::map<std::size_t, spk::Pool<std::vector<Voxel::Cell>>>
```

The key is the pool's intended capacity size class.

Selection uses `lower_bound(requestedCellCount)`:

1. use an exact existing class when present;
2. otherwise use the smallest existing higher class;
3. when no equal-or-higher class exists, create a new pool at the requested size.

Each pool factory creates a vector and reserves its size-class capacity. The per-obtain callback resets or copy-fills the vector logical contents while retaining reusable capacity.

The Pool implementation is intentionally single-threaded; this ticket introduces no concurrent Builder/pool access contract.

## Checked access and failure behavior

- `contains()` is false for every coordinate on the default-empty Volume;
- Volume `at()` / `operator[]` outside dimensions throw `spk::Exception`;
- Builder `set()` outside dimensions throws `spk::Exception`;
- invalid Builder dimensions or unit size throw `spk::Exception`;
- Cell-count overflow throws `spk::Exception`;
- failed checked operations do not alias another Cell;
- Pool preparation failures follow the Sparkle Pool exception contract.

## Copy / move behavior

### Volume copy

Copies share immutable Content, including the same Cell data address.

### Volume move

Transfers Content and leaves the source default-empty.

### Builder move

Builder is move-only.

### Builder from Volume

Consumes the source Volume and applies the unique/shared Content rules above.

## Explicitly removed from the prior review implementation

- `spk::VersionedTrait` inheritance;
- `Voxel::Volume::Editor`;
- `volume_editor.hpp`;
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
- checked Volume and Builder coordinates;
- invalid dimensions, unit sizes, and Cell-count overflow;
- read-only contiguous Cell view;
- cheap shared immutable Volume copies;
- move-to-empty Volume behavior;
- unique Content reuse when constructing Builder from a moved Volume;
- shared Content copy-before-mutation behavior;
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
- private `core/src/voxel/volume_content.hpp`;
- removal of `volume_editor.hpp`;
- `core/CMakeLists.txt`.

Acceptance coverage is in `core/tests/voxel_volume_test.cpp`.

The revised implementation remains under project-owner review. It must remain **In Progress** until explicit approval is recorded.
