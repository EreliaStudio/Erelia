# ST-032-01 — Current Baker semantic golden fixtures

## Intent

Capture current mesh semantics before code extraction and build comparison helpers tolerant of harmless vertex ordering differences.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- The dependency contracts `EP-000, EP-001` used by this story are available; an epic dependency denotes a required contract, not global waterfall completion.
- The test begins from an explicitly constructed, valid state with no pending operation unless the case says otherwise.
- Rendering, authoring data, and display names are not authoritative gameplay state.

## Owned behavior

Capture current mesh semantics before code extraction and build comparison helpers tolerant of harmless vertex ordering differences.

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.

## Implementation inputs

### Already defined values/data

- Story intent: Capture current mesh semantics before code extraction and build comparison helpers tolerant of harmless vertex ordering differences.
- Required dependency contracts: EP-000, EP-001.
- Test fixture rule: Use the smallest deterministic fixture that exposes the owned behavior. Record exact dimensions, cell coordinates, palette indices, transforms, expected vertices/state, and stable asset IDs in the test source; do not substitute an unspecified representative asset.

### User validation required before implementation

Before implementing behavior controlled by an Open Decision below, ask the user to resolve it and record the approved choice in that OD file. Work that relies only on its fixed constraints may proceed.

## Behavioral contract

The operation validates identity, ownership, bounds, and dependency preconditions before authoritative mutation. Success produces only the state and events owned above. Failure is atomic. Ordering and deterministic inputs are explicit and reproducible.

## Acceptance tests

### Semantic golden fixtures

- [x] Minimal deterministic fixtures cover cube, slab, slope, stair, cross, an unloaded adjacent Chunk, and an available occluding adjacent Chunk.
- [x] Every fixture records exact Chunk/cell coordinates, stable Definition name/runtime ID, orientation, flip, topology, and atlas-slot expectations.
- [x] Canonical snapshots compare triangle positions, normals, atlas UVs, winding, and visible topology.
- [x] Canonicalization ignores triangle emission order, vertex-buffer indices, and cyclic starting vertices while preserving winding.
- [x] Missing adjacent Chunk data is characterized as empty; the boundary cell keeps all 12 cube triangles.
- [x] An available solid neighbor across local X boundaries `15 → 0` removes exactly the shared face, leaving 10 triangles.
- [x] The old `Chunk::Baker` remains the exercised production implementation and test oracle; no `VoxelMesher` extraction begins in this ticket.

### Boundaries, rejection, and determinism

- [x] The Chunk upper/lower boundary coordinates used for cross-Chunk lookup are exercised explicitly.
- [x] An unknown runtime Definition ID throws `spk::Exception`; the same Baker still produces the unchanged valid cube snapshot afterward.
- [x] Repeating identical inputs produces byte-identical canonical semantic output.
- [x] Tests are CPU/headless and do not depend on pointers, wall-clock time, render timing, or unordered triangle emission.
- [x] Serialization, ownership transfer, version mutation, and retry effects are not operations owned by this read-only characterization ticket.

### Rendering / golden images

- [x] Semantic geometry/material/transform assertions run independently of PNG comparison.
- [x] The existing 20 approved textured Chunk references remain the graphical parity gate and pass unchanged.
- [x] No expected image, comparison tolerance, camera, atlas, shader, or canonical runner changed.
- [x] OD-024's approved `640 × 480` framebuffer remains authoritative; its resolved decision supersedes the generic `512 × 512` template wording.

## Rendering impact

Yes. The controlled fixture uses a fixed camera, viewport, asset set, lighting, and supported GPU runner. Structural refactors must match the reviewed current baseline; intentional migrations require human review before a new versioned baseline is accepted.

## Open decisions

- [OD-020](../../../open-decisions/OD-020-mesher-merging-indexing-and-hard-normal-policy.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-024](../../../open-decisions/OD-024-current-chunk-golden-fixtures-and-runner.md) — Resolved: preserve the approved `640 × 480`, `windows-2025` textured baseline.
- [OD-025](../../../open-decisions/OD-025-voxelmesher-chunk-specialization-and-occlusion-cache.md) — Resolved during delivery for the next extraction: one generic cached VoxelMesher plus a Chunk::Mesher outside-neighbor override.

## Completion evidence

- `CurrentBakerSemanticGoldenTest` covers all seven fixed semantic snapshots, deterministic replay, and rejection recovery.
- `SemanticMeshSnapshotTest` proves the comparison ignores harmless ordering and still detects reversed visible winding.
- `tests/resources/current_baker_semantics.md` records the exact fixtures; seven `.mesh` resources store their canonical expected values.
- Implementation commits: [`f11db3f`](https://github.com/EreliaStudio/Playground/commit/f11db3ff439ded33321937c1fc084a0a2b9165d4) plus cross-platform line-ending fix [`aa1b9c2`](https://github.com/EreliaStudio/Playground/commit/aa1b9c216a86476ebef0bd46000bf38b9c82e8a8), delivered by [PR #6](https://github.com/EreliaStudio/Playground/pull/6).
- PR #6 [CI run 35501490841](https://github.com/EreliaStudio/Playground/actions/runs/35501490841) passed the CPU/headless semantic fixtures and unchanged Windows/OpenGL golden suite.
