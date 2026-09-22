# ST-001-01 — Shared terrain coordinate conversion

**Status:** In Progress
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
- One terrain cell equals one world unit.
- Global-cell -> Chunk coordinate conversion.
- Global-cell -> local-Chunk coordinate conversion.
- Correct mathematical floor behavior for positive and negative coordinates.

## Explicitly not owned

- `Voxel::Cell` packing.
- `Voxel::Volume` storage.
- World-position floating-point conversion.
- Chunk generation, networking, caching, meshing, or rendering.

## Public contract

The Core contract accepts a global terrain-cell `spk::Vector3Int` and exposes both:

- the containing Chunk coordinate as `spk::Vector3Int`;
- the local cell coordinate as `spk::Vector3Int`.

The implemented public API lives in `core/include/erelia/core/terrain/coordinate.hpp` under `core::terrain`:

```cpp
inline constexpr std::int32_t chunkExtent = 16;
inline constexpr float cellWorldExtent = 1.0F;

[[nodiscard]] spk::Vector3Int toChunkCoordinate(const spk::Vector3Int &globalCell) noexcept;
[[nodiscard]] spk::Vector3Int toLocalCoordinate(const spk::Vector3Int &globalCell) noexcept;
```

These names are implementation-level choices; observable behavior remains fixed by DR-011.

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
- Do not restore the archived `Chunk::Coordinate` wrapper merely because it existed historically.
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
- Namespace follow-up: the project-owned API uses `core::terrain` rather than a redundant top-level `erelia::` namespace, per explicit project-owner direction.
- Draft validation PR: #7, targeting `backlog/ep-001-implementation-tickets`.
- GitHub Actions CI run `35775258869` / run #32:
  - `clang-format`: passed;
  - Ubuntu 24.04 headless Debug: Erelia build passed; `EreliaCoreTestSuite`, `EreliaServerTestSuite`, and `EreliaServerSmoke` all passed (3/3);
  - Ubuntu 24.04 headless Release: Erelia build passed; the same 3/3 tests passed;
  - Windows Server 2022 headless Debug: Erelia build passed; the same 3/3 tests passed;
  - Windows Server 2022 headless Release: Erelia build passed; the same 3/3 tests passed.
- `terrain_coordinate_test.cpp` covers the exact DR-011 3D fixtures, every required scalar boundary on X/Y/Z, mixed signs, deterministic repeatability, local range, reconstruction, and representable `std::int32_t` extremes.
- Core still depends only on the standard library plus `sparkle::core`; no Server, Client, graphics, networking, voxel-storage, generation, meshing, or rendering dependency was introduced.
- The conversion implementation is scalar arithmetic only and performs no dynamic allocation.
- Human completion approval/review has not yet been recorded. The ticket therefore remains **In Progress**, not Done.
