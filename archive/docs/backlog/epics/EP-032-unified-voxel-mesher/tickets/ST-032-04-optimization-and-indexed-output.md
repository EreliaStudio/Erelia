# ST-032-04 — Deterministic compatible-vertex reuse

## Intent

Reuse exactly compatible vertices in the common `VoxelMesher` output path without changing emitted polygons, triangle semantics, hard normals, atlas UVs, or rendered output.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- Merged PR #7 removed `Chunk::Baker` and moved all production consumers to the nested `Chunk::Mesher` specialization over the common `VoxelMesher` algorithm.
- The pre-change `VoxelMesher` at merged main commit `67d5471208d7da4188bb1034bc69d32a69eb1441`, the retained ST-032-01 semantic snapshots, and the 32 approved PNG references are the comparison baseline.
- ST-032-02 supplies generic `VoxelVolume`/`VoxelModel` iteration, scale-aware Shape transforms, cached occlusion/clipping, and the `Chunk::Mesher` external-neighbor override.
- [OD-020](../../../open-decisions/OD-020-mesher-merging-indexing-and-hard-normal-policy.md) resolves exact compatible-vertex reuse, deterministic first-seen indices, no polygon merging, and hard flat normals.
- [OD-024](../../../open-decisions/OD-024-current-chunk-golden-fixtures-and-runner.md) fixes graphical validation at `640 × 480` on the canonical Windows/OpenGL lane.

## Owned behavior

- Add one bake-local compatible-vertex lookup to the shared `VoxelMesher` mesh-emission path.
- Reuse a prior index only when position, stored polygon normal, and atlas UV are exactly equal.
- Assign indices on first encounter while preserving established cell, polygon, polygon-vertex, and triangle traversal order.
- Preserve every polygon currently emitted by authored and occlusion-clipped Shapes, including triangle topology/winding and hard flat normals.
- Apply the same implementation to generic `VoxelVolume`/`VoxelModel` and inherited `Chunk::Mesher` bakes.

## Explicitly not owned

- Greedy, face, or adjacent-polygon merging and any invented optimization threshold.
- Normal averaging, interpolation, recalculation, or smoothing.
- Palette resources, palette-element vertex data, Palette binding/range validation, voxel shader changes, or material migration; ST-003-02 owns those contracts.
- New fixture content or changed UV semantics merely to force a vertex reduction.
- Mesh caching, Chunk scheduling/generation/editing/streaming/gameplay changes, or a Chunk-specific indexing loop.
- Expected-PNG, comparison-tolerance, camera, atlas, or shader changes.

## Implementation inputs

### Already defined values/data

- Use the smallest existing production fixture that genuinely exposes exact cross-polygon compatible sharing.
- If atlas seams prevent every existing production fixture from sharing across polygons, record that evidence and ask the project owner before adding content or changing UV semantics.
- Deliberate failures originating in Playground continue to use `spk::Exception`.

## Behavioral contract

`VoxelMesher::bake` validates all referenced Definitions before creating observable output. Its vertex lookup is temporary per bake. A failed bake cannot retain partial index state or affect a later valid bake. Lookup may use a hash table, but table iteration never emits vertices or indices.

## Acceptance tests

### Nominal behavior and integration interactions

- [x] Exact position, normal, and UV equality reuses the first-seen index; changing any participating attribute prevents sharing.
- [x] Different normals retain distinct vertices at hard polygon boundaries.
- [x] Atlas UV seams retain distinct vertices even when position and normal match.
- [x] Repeated identical inputs produce byte-identical ordered vertex and index buffers.
- [x] Existing orientation, vertical flip, uniform scaling, clipping, and cross-Chunk occlusion results remain valid.
- [x] Generic volume/model and Chunk bakes exercise the same indexing implementation; `Chunk::Mesher` contains no second emission or lookup loop.
- [x] Invalid Definition input throws `spk::Exception`, leaves no persistent indexing state, and cannot poison a later valid bake.
- [x] ST-032-01 semantic snapshots remain unchanged.
- [x] ST-032-02 fixtures remain semantically valid; only exact compatible-reuse vertex expectations changed, while index counts remain unchanged.
- [x] Before/after vertex and index counts are recorded for deterministic representative fixtures.
- [x] Hosted CI is not a reliable timing environment, so no timing assertion or unsupported performance claim was added.

### Boundaries and invalid/rejected operations

- [x] Empty and minimum valid volumes continue to bake through the existing contract.
- [x] Unknown runtime Definition IDs retain the established `spk::Exception` failure and recovery behavior.

### Determinism and lifecycle / retry / persistence

- [x] Deterministic output never depends on pointer values, hash-table iteration, render frame rate, or wall-clock timing.
- [x] The first deterministic encounter owns each emitted index, including after an earlier rejected bake.

### Rendering / golden images

- [x] All 20 original Chunk and all 12 consolidated-mesher `640 × 480` references pass unchanged with existing tolerances.
- [x] No comparison failed; no expected image, tolerance, camera, atlas, or shader changed.
- [x] Semantic geometry/material/transform assertions pass independently of PNG comparison.

## Rendering impact

Yes. The controlled fixture uses a fixed camera, viewport, asset set, lighting, and supported GPU runner. Structural refactors must match the reviewed current baseline; intentional migrations require human review before a new versioned baseline is accepted.

## Open decisions

- [OD-020](../../../open-decisions/OD-020-mesher-merging-indexing-and-hard-normal-policy.md) — Resolved by the project owner: no polygon merging; exact position/normal/UV reuse; deterministic first-seen indices; hard flat normals.
- [OD-024](../../../open-decisions/OD-024-current-chunk-golden-fixtures-and-runner.md) — Resolved: preserve the approved `640 × 480` canonical visual baseline.

## Completion evidence

- `VoxelMesherIndexingTest` proves existing stair-compatible reuse, hard-normal separation, atlas-UV seam separation, deterministic byte output, identical generic/Chunk output, and recovery after `spk::Exception` rejection.
- Existing `VoxelMesherTest`, `VoxelMesherSemanticGoldenTest`, `ChunkMesherTest`, and `ChunkMesherTransformTest` retain scale, orientation, vertical flip, clipping, cache, six-direction neighbor, cross-Chunk occlusion, winding, and seven snapshot evidence.

| Deterministic fixture | Vertices before | Vertices after | Indices before | Indices after |
|---|---:|---:|---:|---:|
| One existing `debug_stair` cell | 40 | 38 | 60 | 60 |
| JSON/procedural cross-statue fixture | 290 | 288 | 432 | 432 |
| Two adjacent stone cubes with atlas UV seams | 40 | 40 | 60 | 60 |
| One stone cube | 24 | 24 | 36 | 36 |

- The stair is the smallest existing production fixture with cross-polygon exact-compatible vertices: two first-seen indices are reused. No fixture content or UV semantics changed.
- Implementation commit [`1120e1b`](https://github.com/EreliaStudio/Playground/commit/1120e1bd462f44ff128b59dad528c84e9988a123) on [PR #8](https://github.com/EreliaStudio/Playground/pull/8).
- [CI run 35521204431](https://github.com/EreliaStudio/Playground/actions/runs/35521204431) passes both complete CPU/headless and Windows/OpenGL lanes. All 32 approved expected PNG files remained unchanged and passed at OD-024's `640 × 480` configuration.
- No representative timing is reported: the available hosted runner is not a controlled reliable benchmark environment, and vertex-count reduction alone is not claimed as a performance improvement.
- ST-003-02 is the next implementation checkpoint.
