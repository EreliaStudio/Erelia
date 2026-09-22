# ST-001-02 — Read-only VoxelVolume contract

**Status:** Complete

## Intent

Introduce the smallest common owning volume and read contract required by later meshing, plus the generic controlled edit transaction clarified by the project owner, following resolved [OD-011](../../../open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md).

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, schemas, ownership, error policy, architecture, or technology not defined here or by a resolved linked decision.
- Treat a missing observable choice as an Open Decision and pause the affected work.
- Preserve the existing `Voxel::Cell` representation exactly as resolved by OD-023.

## Starting state / prerequisites

- ST-001-01 is complete: the current Chunk has headless characterization and 20 approved textured references.
- Playground consumes the installed `Sparkle::TestLibrary` from the pinned prebuilt Sparkle package.
- OD-011 and OD-023 are resolved.

## Owned behavior

- A concrete `VoxelVolume` owns a runtime-sized `std::vector<Voxel::Cell>`.
- Construction takes runtime `spk::Vector3UInt` dimensions and a uniform voxel size.
- Every dimension and the finite voxel size are strictly positive.
- Cells default to the existing empty `Voxel::Cell` value.
- Meshing-facing access is read-only through checked `at(spk::Vector3Int)` and `std::span<const Voxel::Cell>`.
- Storage order preserves current Chunk indexing: X fastest, then Z, then Y.
- Local bounds are `[0, dimensions × voxelSize]`.
- `VoxelVolume` owns generic `VersionedTrait` behavior and exposes `edit()`.
- Nested `VoxelVolume::Editor` preserves the current Chunk editor API: move-only RAII lifetime, `bool set(spk::Vector3Int, Voxel::Cell)`, and `void commit()`.
- Any number of real changes in one edit session publish exactly one version invalidation at commit/destruction; no-op sessions publish none.
- VoxelVolume-owned contract failures use `spk::Exception`; stable classification uses `message()`, while `what()` includes the Sparkle throw-site diagnostic.

## Explicitly not owned

- Adapting or modifying `Chunk`; ST-001-03 owns that migration.
- Introducing the `VoxelModel` semantic type; ST-001-04 owns it.
- World/chunk lookup, streaming, generation, coordinate conversion, meshing, rendering, Palette ownership, animation, or GPU behavior.
- Any change to `Voxel::Cell`, Definition, Shape, texture, UV, or approved golden references.

## Behavioral acceptance

- [x] Runtime dimensions and uniform voxel size are returned exactly.
- [x] A valid first, interior, and last coordinate returns the expected `Voxel::Cell`.
- [x] Every negative and one-step-past coordinate is rejected with `spk::Exception` and cannot alias valid storage.
- [x] The read-only span contains exactly `x × y × z` cells in the documented order.
- [x] Zero in any dimension and an unrepresentable cell count are rejected with `spk::Exception`.
- [x] Zero, negative, infinite, and NaN voxel sizes are rejected with `spk::Exception`.
- [x] Local minimum is zero and local maximum equals `dimensions × voxelSize`.
- [x] Changing voxel size changes physical bounds without changing dimensions, topology, or packed cell IDs.
- [x] Multiple changed cells in one editor session publish exactly one version notification.
- [x] Assigning an unchanged value publishes no version notification.
- [x] Out-of-range mutation is rejected without aliasing valid storage or publishing a notification.
- [x] Explicit commit is idempotent and closes the editor against further mutation.
- [x] Sparkle exception evidence includes the stable message, a non-empty source file, a nonzero source line, and the expected empty cause for direct validation failures.
- [x] All focused tests run without creating a Window or OpenGL context.
- [x] Existing CPU/headless tests remain green.
- [x] Existing current-Chunk golden tests remain green on the supported runner; no reference or tolerance is changed.

## Rendering impact

None. This ticket adds headless data behavior only. The existing GPU suite is a no-change regression gate, not a source of new references.

## Open decisions

- [OD-011](../../../open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md) — Resolved: concrete vector-owning base, strict non-empty dimensions, finite positive scale, checked read access, and generic batched editor/versioning.
- [OD-023](../../../open-decisions/OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) — Resolved: preserve the current packed Cell representation byte-for-byte.

## Completion evidence

- `tests/voxel_volume_metadata_tests.cpp`: dimensions, scale, default empty storage, bounds, and invalid construction.
- `tests/voxel_volume_access_tests.cpp`: first/interior/last access, span order, out-of-range rejection, and scale-independent topology/IDs.
- `tests/voxel_volume_editor_tests.cpp`: changed/no-op edit transactions, one-notification batching, rejected coordinates, explicit commit, and closed-editor rejection.
- Prior local focused run after editor refinement: 10/10 GoogleTests passed without Window/OpenGL initialization.
- Local focused run after exception-policy refinement: 12/12 GoogleTests passed without Window/OpenGL initialization.
- Remote implementation commit: `d00e467c29ac907c1ef2e8f2cf41aaf081454624`.
- [CI run 35460049379](https://github.com/EreliaStudio/Playground/actions/runs/35460049379): `CPU/headless tests` and `Windows/OpenGL golden candidates` both passed.
- Editor/version refinement commit: `9effce86dee4d3304e9a61024e97afcd9894edf4`.
- [CI run 35461166765](https://github.com/EreliaStudio/Playground/actions/runs/35461166765): the complete `CPU/headless tests` and unchanged `Windows/OpenGL golden candidates` lanes both passed after moving editor/version ownership into VoxelVolume.
- Sparkle-exception refinement commit: `7b04ab21725be179afa7adf1054f51250715ba58`.
- [CI run 35463284473](https://github.com/EreliaStudio/Playground/actions/runs/35463284473): the complete `CPU/headless tests` and unchanged `Windows/OpenGL golden candidates` lanes both passed with the selected exception policy.
- No PNG reference, comparison tolerance, texture, UV, Chunk implementation, or rendering code changed.
- OD-011/OD-023 provenance and current project status were updated with the implementation.
