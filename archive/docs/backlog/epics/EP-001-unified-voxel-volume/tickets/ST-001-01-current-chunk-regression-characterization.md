# ST-001-01 — Current Chunk regression characterization

**Status:** Complete

## Intent

Capture stable fixtures around the existing 16³ storage/indexing and Cell semantics **and approve reference images from the current textured Chunk renderer before extraction**. This story has both headless data checks and a required GPU presentation gate using SparkleTestLibrary; see [ARCH-004](../../../architecture/ARCH-004-TEST-STRATEGY.md#textured-chunk-image-regression-gate).

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- The dependency contracts `EP-000, specifically ST-000-05 for the installed SparkleTestLibrary visual-test utilities` used by this story are available; an epic dependency denotes a required contract, not global waterfall completion.
- The test begins from an explicitly constructed, valid state with no pending operation unless the case says otherwise.
- Rendering, authoring data, and display names are not authoritative gameplay state.

## Owned behavior

Capture stable fixtures around the existing 16³ storage/indexing and Cell semantics **and approve reference images from the current textured Chunk renderer before extraction**. This story has both headless data checks and a required GPU presentation gate using SparkleTestLibrary; see [ARCH-004](../../../architecture/ARCH-004-TEST-STRATEGY.md#textured-chunk-image-regression-gate).

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.

## Implementation inputs

### Already defined values/data

- Story intent: Capture stable fixtures around the existing 16³ storage/indexing and Cell semantics **and approve reference images from the current textured Chunk renderer before extraction**. This story has both headless data checks and a required GPU presentation gate using SparkleTestLibrary; see [ARCH-004](../../../architecture/ARCH-004-TEST-STRATEGY.md#textured-chunk-image-regression-gate).
- Required dependency contracts: EP-000, specifically ST-000-05 for the installed SparkleTestLibrary visual-test utilities.
- Test fixture rule: Use the smallest deterministic fixture that exposes the owned behavior. Record exact dimensions, cell coordinates, palette indices, transforms, expected vertices/state, and stable asset IDs in the test source; do not substitute an unspecified representative asset.

### User validation required before implementation

Before implementing behavior controlled by an Open Decision below, ask the user to resolve it and record the approved choice in that OD file. Work that relies only on its fixed constraints may proceed.

## Behavioral contract

The operation validates identity, ownership, bounds, and dependency preconditions before authoritative mutation. Success produces only the state and events owned above. Failure is atomic. Ordering and deterministic inputs are explicit and reproducible.

## Acceptance tests

### Nominal behavior and integration interactions

- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `A 16×16×16 adapter over the current Chunk exposes exactly the same cells and indexing as direct Chunk access.` is observed exactly; no private-state shortcut is used.
- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Scale changes local geometry/bounds but never changes discrete neighbor relationships or cell IDs.` is observed exactly; no private-state shortcut is used.
- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Fixed current-texture Chunk scenes match approved pre-refactor PNG references after volume adaptation; failures expose expected/actual/difference images.` is observed exactly; no private-state shortcut is used.
- [ ] Fixtures cover first/last valid coordinate and representative negative world-to-local conversion through Chunk helpers.
- [ ] Packed Cell fixtures cover empty, oriented and vertically flipped cells.
- [ ] Regression fixture records current semantic output rather than relying on implementation-private addresses.
- [ ] Reviewed pre-refactor PNGs cover current textures/UVs on cube/slab/slope/stair/cross geometry, orientation/flip, partial occlusion and Chunk boundaries, plus a seeded scene.
- [ ] Fixed camera/render state and completed uploads produce repeatable comparisons on the documented Windows/OpenGL runner.
- [ ] References and provenance are committed separately from actual/difference artifacts; missing references fail without automatic regeneration.
- [ ] Unchanged output passes; a controlled texture/UV alteration fails and publishes reviewable image differences.
- [ ] The same references pass after Chunk adaptation; later non-textured migration must visibly fail against them before explicit review/versioning of a new baseline.
- [ ] Invalid input is rejected atomically and leaves the previously valid state unchanged.
- [ ] Identical deterministic inputs produce identical observable results.
- [ ] The exact lower and upper supported boundaries succeed; one-step-outside values are rejected before mutation.
- [ ] A rejected command leaves state, ownership, resources, version counters, scheduled work, and emitted authoritative events byte-for-byte or semantically unchanged.
- [ ] Repeating the same seed, configuration, starting snapshot, and ordered commands produces the same result and event order.
- [ ] Create → use → serialize where applicable → unload → restore/recreate → retry preserves stable IDs and does not duplicate the operation.
- [ ] The nearest upstream and downstream contracts named in prerequisites are exercised together; dependency failure follows the documented fail-closed or rollback behavior.

### Boundaries and invalid/rejected operations

- [ ] Missing IDs, foreign ownership, malformed content, stale versions, and unsupported enum/tag values are rejected with the documented error category.
- [ ] Empty/minimum/maximum fixtures are exercised where the public contract permits them; unsupported empty state is rejected atomically.

### Determinism and lifecycle / retry / persistence

- [ ] Deterministic iteration never depends on pointer values, hash-table accident, render frame rate, or wall-clock timing.
- [ ] A duplicate/retried operation is either idempotent or rejected as already applied, according to the story contract, without duplicating state or events.

### Rendering / golden images

- [ ] Render the deterministic fixtures at `640 × 480` as resolved by OD-024; compare against their reviewed PNGs with the tolerance recorded by the Sparkle TestLibrary fixture. On failure, retain the old expected image and publish actual/difference images for review.
- [ ] Assert semantic geometry/material/transform values independently of the PNG comparison.

## Rendering impact

Yes. The controlled fixture uses a fixed camera, viewport, asset set, lighting, and supported GPU runner. Structural refactors must match the reviewed current baseline; intentional migrations require human review before a new versioned baseline is accepted.

## Open decisions

- [OD-011](../../../open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md) — Resolved after this characterization checkpoint; it did not block capturing the baseline.
- [OD-023](../../../open-decisions/OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) — Resolved: preserve the representation characterized here.
- [OD-024](../../../open-decisions/OD-024-current-chunk-golden-fixtures-and-runner.md) — Resolved: several focused scenes plus one seeded multi-Chunk scene, the installed component's fixed `640 × 480` framebuffer, and GitHub Actions `windows-2025`.

## Completion evidence

- CPU/headless fixtures: `tests/chunk_*_tests.cpp`, `tests/voxel_cell_tests.cpp`, `tests/voxel_shape_transform_tests.cpp`, and `tests/voxel_catalog_tests.cpp`.
- GPU fixtures and controlled UV-sensitivity check: `tests/chunk_golden_tests.cpp` and `tests/library/current_chunk_golden_comparison.cpp`.
- Fixture metadata: `tests/resources/current_chunk_fixtures.md`.
- Approved references: 20 PNGs under `tests/resources/expectedImages/current_chunk/`; ordinary test runs never overwrite them.
- OD-024 records the approved scenes, `640 × 480` capture, and canonical `windows-2025` software-OpenGL runner.
- Merged implementation: Playground commit `0bd12e5053e482fd6a4f1a4f2a843279627f2306`.
- Passing CI: [run 35456326462](https://github.com/EreliaStudio/Playground/actions/runs/35456326462), including successful `CPU/headless tests` and `Windows/OpenGL golden candidates` jobs.
