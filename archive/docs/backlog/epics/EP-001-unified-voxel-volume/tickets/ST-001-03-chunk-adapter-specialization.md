# ST-001-03 — Chunk adapter/specialization

## Intent

Make current Chunk derive from/use the resolved concrete `VoxelVolume` contract while preserving fixed 16³ Chunk semantics and all observable behavior.

## Execution decision policy

- Do not invent unspecified observable behavior; record a missing choice as an Open Decision.
- Follow resolved OD-011 even where it supersedes the earlier fixed-array optimization option.
- Preserve `Voxel::Cell` byte-for-byte under resolved OD-023.

## Starting state / prerequisites

- ST-001-01 current-Chunk semantic and textured golden baseline is complete.
- ST-001-02 owning VoxelVolume/read contract is complete.

## Owned behavior

- Construct the Chunk volume as exactly `16 × 16 × 16` at voxel size `1.0`.
- Remove duplicate Chunk cell ownership and use the vector storage owned by `VoxelVolume`.
- Remove Chunk's direct `VersionedTrait` inheritance and duplicate nested editor implementation; inherit `VoxelVolume` versioning and `VoxelVolume::Editor` while preserving the existing edit-call API and observable transaction behavior.
- Preserve coordinate-to-index order and all existing mutation/version notifications.
- Inherited VoxelVolume construction/access/editor failures use the `spk::Exception` policy recorded in OD-011; Chunk-only APIs retain their current failure policy unless this ticket necessarily replaces them with the generic path.
- Preserve Chunk coordinate/world conversion, Collection, generator, scheduler, and renderer call sites.

## Explicitly not owned

- Runtime-sized VoxelModel behavior.
- World lookup or cross-Chunk access inside `VoxelVolume`.
- Mesher extraction, Palette migration, new rendering behavior, or changed golden images.

## Behavioral acceptance

- [x] Chunk reports dimensions `16 × 16 × 16`, voxel size `1.0`, and local bounds `[0,16]³` through `VoxelVolume`.
- [x] Chunk's static coordinate/index helpers retain the existing X-then-Z-then-Y order and rejection policy.
- [x] No Chunk-owned storage, access, editor, versioning, or diagnostic behavior remains; those inherited contracts are tested once by ST-001-02 rather than duplicated in Chunk tests.
- [x] A changed Chunk queues itself and every available face-neighbor for rebuild through the Chunk-owned BakeScheduler behavior.
- [x] Chunk world/local conversion, Collection, generation, baking, and scheduling CPU tests remain green.
- [x] All 20 approved current textured Chunk golden comparisons remain green on the supported runner.
- [x] No approved reference, comparison tolerance, texture, or UV is changed.

## Rendering impact

No intentional visual impact. The existing 20-reference suite is mandatory regression evidence for the storage migration.

## Open decisions

- [OD-011](../../../open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md) — Resolved: Chunk keeps fixed dimensions but migrates from its fixed array to the vector owned by VoxelVolume.
- [OD-023](../../../open-decisions/OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) — Resolved: preserve current packing.

## Completion evidence

- Focused Chunk specialization/index tests in `tests/chunk_storage_tests.cpp`, with inherited behavior covered by the VoxelVolume test families.
- Chunk-owned neighbor rebuild coverage in `tests/chunk_bake_scheduler_tests.cpp`.
- Implementation commit [`27c095f`](https://github.com/EreliaStudio/Playground/commit/27c095f467cdb4bd1525aa5be81bccfab889b5fb).
- Test-ownership refinement commit [`f3f855b`](https://github.com/EreliaStudio/Playground/commit/f3f855b71c67bfb96100bb09a3e52fbedd14683b).
- PR #4 [CI run 35467461734](https://github.com/EreliaStudio/Playground/actions/runs/35467461734) passed the complete CPU/headless and supported Windows/OpenGL golden suites without reference or tolerance changes.
