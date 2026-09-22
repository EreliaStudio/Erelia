# ST-001-01 — Shared terrain coordinate conversion

**Status:** Done
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Provide the shared terrain coordinate conversion contract used by Server generation, Chunk addressing, Client requests, and Client rendering.

## User / system value

Every EP-001 layer can address the same terrain cell and Chunk identically, including across negative coordinates and Chunk boundaries.

## Starting state / prerequisites

- Core already links headless-safe Sparkle Core.
- DR-011 defines the complete coordinate convention.
- No other ST-001 ticket is required.

## Product ownership

Core owns the reusable coordinate conversion helpers and fixed terrain-Chunk coordinate constants. Server and Client consume them.

## Allowed dependencies

- C++ standard library.
- Sparkle Core, including `spk::Vector3Int`.

## Forbidden dependencies

- Server or Client targets.
- Sparkle graphics/presentation facilities.
- Archived code as a runtime dependency.

## Owned behavior

- The fixed terrain Chunk extent of 16 cells on X/Y/Z.
- One terrain cell equals one world unit as the terrain convention; concrete per-Volume unit-size instance state is deferred to ST-001-03.
- Global-cell -> Chunk coordinate conversion.
- Global-cell -> local-Chunk coordinate conversion.
- Correct mathematical floor behavior for positive and negative coordinates.

## Explicitly not owned

- `Voxel::Cell` packing or runtime Cell state beyond the coordinate type shell.
- `Voxel::Volume` dimensions, storage, validation, access, or mutation beyond the local-coordinate type shell.
- World-position floating-point conversion.
- Chunk generation, networking, caching, meshing, or rendering.

## Public contract

The Core contract uses semantic domain types backed by `spk::Vector3Int`:

- `Voxel::Cell::Coordinate` for a global terrain-cell coordinate;
- `Chunk::Coordinate` for a Chunk-grid coordinate;
- `Voxel::Volume::LocalCoordinate` for a coordinate local to a voxel Volume.

ST-001-01 introduces `Voxel::Cell` and `Voxel::Volume` only as structural shells for these nested coordinate aliases. Their packed Cell state and owning Volume behavior remain explicitly deferred to ST-001-02 and ST-001-03.

The coordinate behavior is owned by the `Chunk` structure:

```cpp
namespace Voxel
{
    struct Cell
    {
        using Coordinate = spk::Vector3Int;
    };

    struct Volume
    {
        using LocalCoordinate = spk::Vector3Int;
        using UnitSize = float;
    };
}

struct Chunk : public Voxel::Volume
{
    using Coordinate = spk::Vector3Int;

    inline static constexpr std::int32_t Extent = 16;

    [[nodiscard]] static Coordinate toCoordinate(
        const Voxel::Cell::Coordinate &globalCell) noexcept;

    [[nodiscard]] static Voxel::Volume::LocalCoordinate toLocalCoordinate(
        const Voxel::Cell::Coordinate &globalCell) noexcept;
};
```

The semantic type organization is an explicit project-owner direction. Observable conversion behavior remains fixed by DR-011.

## Invariants

- Chunk extent is exactly 16 on every axis.
- Local coordinates are always in `[0, 15]` on every axis.
- Mathematically, `chunk * 16 + local == global` per axis.
- X/Y/Z are converted independently.
- +Y is world up; no Erelia-specific axis remapping is introduced.

## State transitions

Not applicable: conversion is stateless.

## Failure behavior

Every representable integer terrain-cell coordinate is a valid input. Conversion must not rely on C++ truncating division for negative values and must not invoke undefined signed-overflow behavior internally.

## Determinism / ordering

Pure deterministic conversion: identical input yields identical Chunk/local coordinates on every supported platform.

## Lifecycle / ownership

Not applicable: no owned runtime resource or mutable state.

## Serialization / persistence

Not applicable: this ticket defines coordinate semantics only.

## Networking / authority

Not applicable directly. The shared semantics are later consumed by network contracts; this ticket does not send messages or grant authority.

## Implementation constraints

- Use mathematical floor division/modulo semantics from DR-011.
- `Chunk::Coordinate` is a semantic alias of `spk::Vector3Int`; do not restore the archived wrapper implementation.
- Keep the implementation headless-safe in Core.

## Exact test fixtures

For each 3D fixture below, expected Chunk/local coordinates are exact:

| Global cell | Chunk | Local |
| --- | --- | --- |
| `(0,0,0)` | `(0,0,0)` | `(0,0,0)` |
| `(15,15,15)` | `(0,0,0)` | `(15,15,15)` |
| `(16,16,16)` | `(1,1,1)` | `(0,0,0)` |
| `(-1,-1,-1)` | `(-1,-1,-1)` | `(15,15,15)` |
| `(-16,-16,-16)` | `(-1,-1,-1)` | `(0,0,0)` |
| `(-17,-17,-17)` | `(-2,-2,-2)` | `(15,15,15)` |

Also test each scalar value `-17, -16, -15, -1, 0, 1, 15, 16, 17` independently on X, Y, and Z while the other axes are zero.

## Acceptance tests

### Nominal

- Exact DR-011 fixtures above pass.
- Mixed-sign coordinates convert each axis independently.

### Boundaries

- Every listed value around -16, 0, and +16 boundaries produces the expected floor-division/floor-modulo result.
- Every tested local component remains in `[0,15]`.

### Invalid / rejected operations

Not applicable: every representable integer cell coordinate is valid.

### Failure atomicity

Not applicable: pure stateless calculation.

### Determinism

- Repeat every fixture and obtain exactly identical results.
- The reconstruction invariant holds for every fixture.

### Lifecycle / ownership

Not applicable.

### Serialization / persistence

Not applicable.

### Retry / idempotency

Not applicable beyond pure deterministic repeatability.

### Concurrency / cancellation

Not applicable: no mutable shared state.

### Authority / trust boundary

Not applicable.

### Dependency failure

Not applicable: no runtime external dependency.

### Cross-system integration

Not part of this ticket. Later Server/Client tickets must use this Core contract rather than reimplement coordinate math.

### Performance

No timing budget. The helper must not allocate dynamically for one conversion.

### Client-visible / golden-image validation

Not applicable: headless Core behavior.

## Decisions / unresolved questions

- [DR-011](../../../DECISIONS/DR-011-VOXEL-COORDINATES.md)
- [ARCH-001](../../../ARCHITECTURE/ARCH-001-PRODUCT-BOUNDARIES.md)

No unresolved question blocks this ticket.

## Completion evidence

- Initial production implementation commit: `e67032414ece0c7c00018ee29db03bc1ad842cd4` on `feat/st-001-01-shared-terrain-coordinate-conversion`.
- Namespace follow-up: the project-owned API does not use a redundant top-level `erelia::` namespace, per explicit project-owner direction.
- Domain-structure follow-up: coordinate aliases live on `Voxel::Cell`, `Voxel::Volume`, and `Chunk`; `Voxel::Volume` also owns the `UnitSize` scalar alias. `Chunk` inherits `Voxel::Volume` and owns the conversion helpers/constants. No per-instance Volume unit-size storage/accessor is introduced by this ticket; that remains part of the blocked Volume implementation contract.
- PR #7 was merged into `backlog/ep-001-implementation-tickets` on 22 September 2026 as merge commit `22a599a281ea435444bacdc1912cd3125f96a817`.
- GitHub Actions CI run `35775258869` / run #32:
  - `clang-format`: passed;
  - Ubuntu 24.04 headless Debug: Erelia build passed; `EreliaCoreTestSuite`, `EreliaServerTestSuite`, and `EreliaServerSmoke` all passed (3/3);
  - Ubuntu 24.04 headless Release: Erelia build passed; the same 3/3 tests passed;
  - Windows Server 2022 headless Debug: Erelia build passed; the same 3/3 tests passed;
  - Windows Server 2022 headless Release: Erelia build passed; the same 3/3 tests passed.
- `terrain_coordinate_test.cpp` covers the exact DR-011 3D fixtures, every required scalar boundary on X/Y/Z, mixed signs, deterministic repeatability, local range, reconstruction, and representable `std::int32_t` extremes.
- Core still depends only on the standard library plus `sparkle::core`; no Server, Client, graphics, networking, voxel-storage, generation, meshing, or rendering dependency was introduced.
- The conversion implementation is scalar arithmetic only and performs no dynamic allocation.
- Human completion approval is recorded by the project owner's merge of PR #7 on 22 September 2026. With implementation, automated validation, and human approval satisfied, the ticket is **Done**.
