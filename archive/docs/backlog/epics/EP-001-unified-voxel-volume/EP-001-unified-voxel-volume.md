# EP-001 — Unified Voxel Volume and Cell Contract

**Dependencies:** EP-000, specifically ST-000-05 for the installed SparkleTestLibrary visual-test utilities  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Extract one meshing-facing volume/grid contract from the current Chunk data model while preserving the existing compact Voxel::Cell and Definition/Shape chain. OD-011 explicitly accepts migrating Chunk's cell storage from its fixed array to the common vector-owning base while retaining fixed 16³ Chunk semantics.

## Starting state

- Contracts supplied by `EP-000, specifically ST-000-05 for the installed SparkleTestLibrary visual-test utilities` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- OD-011 resolves one concrete vector-owning `VoxelVolume` base with read-only meshing access plus a controlled batched editor and version notification.
- Runtime dimensions are data for imported models; do not encode arbitrary model dimensions as C++ template types.
- `Chunk` remains fixed at 16³ and migrates its cells to the storage owned by `VoxelVolume` in ST-001-03.
- Voxel scale is a uniform scalar initially and belongs to the volume contract.
- Do not widen/change packed `Voxel::Cell` orientation/flip during this refactor unless a separate requirement proves necessary.
- Current texture/UV fields remain valid until the later Material migration.
- Before structural changes, approve current textured Chunk PNG references and require image comparison to keep passing through this epic; follow [ARCH-004](../../architecture/ARCH-004-TEST-STRATEGY.md#textured-chunk-image-regression-gate).
- Generic volume code owns no streaming, generation, world coordinate or entity gameplay semantics.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- VoxelVolume dimensions, cell access, uniform scale, bounds, and batched editing/versioning
- Fixed-dimension 16×16×16 Chunk adaptation to inherited VoxelVolume storage
- Runtime-sized VoxelModel semantic type over VoxelVolume storage
- Provisional sparse `spk::JSON::Reader` constructor for early VoxelModel fixtures
- No world lookup, streaming, meshing, palettes, or animation

## Out of scope

- Responsibilities explicitly assigned to neighboring epics.
- Final choices tracked by unresolved Open Decisions.
- Unapproved balance values, content, or technology selections.
- Stable VoxelModel asset serialization, file loading, importer behavior, and Definition-name/catalog mapping beyond the explicitly approved prototype Reader schema.

## Architecture / behavioral overview

- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- OD-011 resolves one concrete vector-owning `VoxelVolume` base with read-only meshing access plus a controlled batched editor and version notification.
- Runtime dimensions are data for imported models; do not encode arbitrary model dimensions as C++ template types.
- `Chunk` remains fixed at 16³ and migrates its cells to the storage owned by `VoxelVolume` in ST-001-03.
- Voxel scale is a uniform scalar initially and belongs to the volume contract.
- Do not widen/change packed `Voxel::Cell` orientation/flip during this refactor unless a separate requirement proves necessary.
- Current texture/UV fields remain valid until the later Material migration.
- Before structural changes, approve current textured Chunk PNG references and require image comparison to keep passing through this epic; follow [ARCH-004](../../architecture/ARCH-004-TEST-STRATEGY.md#textured-chunk-image-regression-gate).
- Generic volume code owns no streaming, generation, world coordinate or entity gameplay semantics.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Current Chunk semantic and textured-render regression baseline | [ST-001-01](tickets/ST-001-01-current-chunk-regression-characterization.md) | [ST-001-01](tickets/ST-001-01-current-chunk-regression-characterization.md) |
| VoxelVolume dimensions, owning cell storage, checked read access, uniform scale, bounds, and batched editing/versioning | [ST-001-02](tickets/ST-001-02-read-only-voxelvolume-contract.md) | [ST-001-02](tickets/ST-001-02-read-only-voxelvolume-contract.md) |
| Fixed 16×16×16 Chunk adaptation and unchanged behavior/rendering | [ST-001-03](tickets/ST-001-03-chunk-adapter-specialization.md) | [ST-001-03](tickets/ST-001-03-chunk-adapter-specialization.md) |
| Runtime-sized VoxelModel semantic type and provisional sparse Reader loader over common storage | [ST-001-04](tickets/ST-001-04-runtime-sized-voxelmodel-storage.md) | [ST-001-04](tickets/ST-001-04-runtime-sized-voxelmodel-storage.md) |
| No world lookup, streaming, meshing, palettes, or animation in the volume contract | [ST-001-02](tickets/ST-001-02-read-only-voxelvolume-contract.md) | [ST-001-02](tickets/ST-001-02-read-only-voxelvolume-contract.md) |

## Ticket index

- [ST-001-01 — Current Chunk regression characterization](tickets/ST-001-01-current-chunk-regression-characterization.md) — Capture stable fixtures around the existing 16³ storage/indexing and Cell semantics **and approve reference images from the current textured Chunk renderer before extraction**. This story has both headless data checks and a required GPU presentation gate using SparkleTestLibrary; see [ARCH-004](../../architecture/ARCH-004-TEST-STRATEGY.md#textured-chunk-image-regression-gate).
- [ST-001-02 — Read-only VoxelVolume contract](tickets/ST-001-02-read-only-voxelvolume-contract.md) — Introduce common dimensions/cell/scale/bounds access plus the generic batched editor and version notification.
- [ST-001-03 — Chunk adapter/specialization](tickets/ST-001-03-chunk-adapter-specialization.md) — Make current Chunk inherit the common storage/editor/version contract without moving Chunk-specific responsibilities into it.
- [ST-001-04 — Runtime-sized VoxelModel storage](tickets/ST-001-04-runtime-sized-voxelmodel-storage.md) — Add the semantic VoxelModel type over the owning storage and controlled editor introduced by ST-001-02.

## Epic integration acceptance tests

1. A 16×16×16 adapter over the current Chunk exposes exactly the same cells and indexing as direct Chunk access.
2. A runtime-sized 8×8×16 volume stores/retrieves cells correctly with no template type dedicated to that dimension.
3. A 16³ volume at scale 1.0 reports 16×16×16 local extent.
4. An 8×8×16 volume at scale 0.1 reports 0.8×0.8×1.6 local extent within defined floating tolerance.
5. Scale changes local geometry/bounds but never changes discrete neighbor relationships or cell IDs.
6. Empty/minimum dimensions and invalid coordinates follow the chosen explicit validation policy without aliasing valid storage.
7. Existing packed Cell ID/orientation/flip round-trips byte-for-byte.
8. Volume access and scale/bounds calculations run headlessly and without Definition catalog/GPU ownership.
9. Fixed current-texture Chunk scenes match approved pre-refactor PNG references after volume adaptation; failures expose expected/actual/difference images.
10. A controlled texture/UV change fails the visual comparison; missing references or an unavailable GPU run cannot count as visual parity.
11. A provisional sparse JSON model fixture loads dimensions, voxel size, numeric values, named orientations and optional vertical flip while rejecting duplicate/out-of-range entries with exact source paths.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-011](../../open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md) — Resolved during ST-001-02: use one concrete vector-owning VoxelVolume base; migrate Chunk storage in ST-001-03.
- [OD-023](../../open-decisions/OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) — Resolved: preserve the existing packed Cell orientation/flip representation.

## Rendering validation

Graphical tickets use deterministic `512 × 512` Sparkle TestLibrary comparisons unless a resolved fixture decision records a component limitation. [OD-024](../../open-decisions/OD-024-current-chunk-golden-fixtures-and-runner.md) fixes ST-001-01 at the installed context's `640 × 480`. Structural refactors preserve reviewed baselines. Intentional visual migrations retain old references until actual/difference images receive human approval, then create a versioned baseline.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [x] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [x] Every coverage-matrix row has implementation and test evidence.
- [x] Applicable decisions are resolved in their persistent OD files, with provenance.
- [x] No graphical baseline was replaced solely because a comparison failed.
