# OD-011 — C++ ownership and view design for VoxelVolume

## Status

Resolved

## Problem

The project requires an explicit choice for c++ ownership and view design for voxelvolume. Leaving it implicit would force an implementation ticket to invent design, balance, content, or technology policy.

## Affected epics/tickets

[EP-001](../epics/EP-001-unified-voxel-volume/EP-001-unified-voxel-volume.md). Each affected ticket links back to this record.

## Known constraints

- Do not contradict the current GDD or the fixed contracts stated in affected tickets.
- Work independent of this choice may proceed behind configuration or an interface.
- No arbitrary value or technology is selected merely to unblock implementation.

## Options considered

- Abstract polymorphic interface: stable non-template mesher boundary and storage freedom, with runtime dispatch for cell access.
- Non-owning contiguous view: no inheritance and a stable non-template mesher boundary, but caller-managed view lifetime and separate owning types.
- C++23 concept/template contract: static dispatch and no view lifetime, but template/header coupling in the mesher.
- Concrete owning volume: one vector-backed implementation and access contract shared by semantic derived types, at the cost of replacing Chunk's fixed array.

## Decision

Use one concrete, vector-owning `VoxelVolume` base.

- `VoxelVolume` owns validated immutable dimensions, uniform voxel size, and a zero-initialized runtime-sized `std::vector<Voxel::Cell>`.
- All dimensions must be strictly positive.
- `voxelSize` must be finite and strictly positive.
- Local bounds are origin-based and derived as `[0, dimensions × voxelSize]`.
- Public read access is checked `at(spk::Vector3Int)` plus a `std::span<const Voxel::Cell>` over the full row-major storage.
- X is the fastest-changing coordinate, followed by Z and then Y, preserving current Chunk indexing semantics.
- The meshing-facing access contract remains read-only. Controlled mutation is exposed only through `edit()` and a nested RAII `VoxelVolume::Editor` with the same `set(coordinate, cell)` and `commit()` API as the current `Chunk::Editor`.
- `VoxelVolume` owns versioning. One edit session that changes one or more cells invalidates the volume exactly once when committed or destroyed; a no-op session does not invalidate it.
- All VoxelVolume-owned contract violations use `spk::Exception`, including invalid construction, unrepresentable cell count, invalid coordinates, and committed-editor misuse. Tests and callers use the stable `message()` for classification while `what()` retains source-location diagnostics.
- `VoxelVolume` owns cell lifetime. Consumers use `const VoxelVolume&` and do not acquire ownership.
- This decision explicitly supersedes the earlier fixed-`std::array` optimization constraint: ST-001-03 keeps Chunk dimensions fixed at 16³ but migrates its storage to the vector owned by `VoxelVolume`.

## Rationale

The project owner preferred one natural owning volume whose data can be inspected either by coordinate or as a span. Chunk and VoxelModel then differ in semantic constraints rather than storage mechanics. This keeps `VoxelMesher` non-template, removes view-lifetime hazards, and centralizes indexing and validation. The vector allocation and loss of Chunk's inline fixed array are accepted explicitly; performance must be measured before adding a specialization.

## Consequences

- ST-001-02 introduces the concrete owning storage/read contract and headless contract tests.
- ST-001-03 derives/adapts Chunk to that base, removes its duplicate array, direct `VersionedTrait` inheritance, and nested editor implementation, and must prove identical fixed 16³ indexing, editing, notification, and rendered output.
- ST-001-04 introduces the semantic runtime-sized VoxelModel type; generic vector storage, versioning, and controlled editing already come from VoxelVolume.
- VoxelMesher consumes `const VoxelVolume&`; it neither owns the volume nor depends on templates for concrete volume types.
- Tests use the public checked `at()`, `cells()`, and `edit()` paths; no test-only mutation back door is required.
- VoxelVolume callers catch `spk::Exception` (or `std::exception`) rather than relying on standard exception subtypes; this trades subtype classification for Sparkle throw-site and cause diagnostics.

## Validation / evidence

- Existing Chunk indexing is row-major and contiguous, so it can migrate without changing coordinate-to-index results.
- The future VoxelModel requirement explicitly needs runtime dimensions and owning storage.
- ST-001-02 focused tests cover dimensions, cell access, rejected coordinates, scale validation, derived bounds, scale-independent topology, and one-notification edit transactions.

## Resolution provenance

Resolved by the project owner during ST-001-02 execution on 19 September 2026. After comparing an abstract interface, non-owning span view, concept, and concrete owner, the owner selected the concrete vector-owning base and strict non-empty validation policy. The owner explicitly approved replacing Chunk's fixed array in ST-001-03 after the conflict with the earlier optimization constraint was identified. The owner then clarified that the current Chunk `Editor` API and its single-notification transaction behavior belong to `VoxelVolume`; ST-001-03 will remove the duplicate Chunk implementation when adopting the base. After reviewing Sparkle's source-location and cause-aware exception implementation, the owner selected `spk::Exception` consistently for VoxelVolume-owned failures instead of standard semantic subtypes.
