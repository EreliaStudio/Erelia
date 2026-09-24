# ST-004-01 — Transition Chunk consumers from Baker to Chunk::Mesher

## Intent

After consolidated ST-032-02 is complete, replace runtime use of `Chunk::Baker` with the validated `Chunk::Mesher` without changing Chunk output, scheduling, invalidation, or rendering behavior.

## Execution decision policy

- Do not invent unspecified behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology.
- Treat every unspecified observable choice as an Open Decision and ask the project owner before implementing it.
- Deliberate failures originating in Playground throw `spk::Exception`.

## Starting state / prerequisites

- Consolidated ST-032-02 provides the reviewed `VoxelMesher` and `Chunk::Mesher` implementations plus semantic/numerical/golden evidence.
- `Chunk::Baker` remains the current runtime implementation and independent parity oracle.
- Existing `Chunk::BakeScheduler`, views, application setup, and current textured references are passing.

## Owned behavior

- Redirect `Chunk::BakeScheduler`, application setup, and other production Chunk-mesh consumers to one appropriately owned `Chunk::Mesher`.
- Preserve scheduler contracts, version/dirty behavior, neighbor rebuild scheduling, Chunk transforms, atlas use, and render-command behavior.
- Prove complete semantic and approved textured-image parity before removing obsolete `Chunk::Baker` implementation files.
- Remove `Chunk::Baker` only after no production or test consumer requires it and the parity evidence remains independently meaningful in retained fixtures.

## Explicitly not owned

- Changes to the generic meshing algorithm, outside-neighbor policy, cache, Shape transforms, or fixture baselines.
- OD-020 optimization/indexing choices or Palette/material migration.
- Chunk generation, streaming, editing, collision, or gameplay changes.

## Acceptance tests

- [x] `Chunk::BakeScheduler` produces the same completion events and meshes through `Chunk::Mesher`.
- [x] Editing a Chunk still schedules itself and every available face-neighbor exactly once under the existing contract.
- [x] Application and view wiring use `Chunk::Mesher` without a second adapter or meshing loop.
- [x] All ST-032-01 semantic snapshots and consolidated ST-032-02 numerical/semantic tests remain unchanged.
- [x] All 20 current textured Chunk references and all approved consolidated-mesher references remain unchanged.
- [x] Seeded generation output and deterministic replay remain unchanged.
- [x] No production reference to `Chunk::Baker` remains before its implementation is removed.
- [x] Invalid input uses `spk::Exception` and does not poison subsequent scheduling or baking.

## Rendering impact

Structural only. The approved `640 × 480` OD-024 baselines must remain unchanged; any visual difference blocks completion rather than creating a replacement reference.

## Open decisions

None identified. OD-020 remains outside this transition.

## Completion evidence

- Implementation commit [`7fd8029`](https://github.com/EreliaStudio/Playground/commit/7fd8029b849b5244324ef5c4fafd91e344e13689) transitions all consumers and removes the obsolete Baker implementation.
- Refinement commit [`d0edb2a`](https://github.com/EreliaStudio/Playground/commit/d0edb2af4aacf4fc9e01b953a8a2143889a89f38) directly nests the specialization as `Chunk::Mesher` without an alias.
- Search evidence shows no production or test source references `Chunk::Baker`; historical fixture documentation remains intentionally named for its captured baseline.
- [CI run 35514737157](https://github.com/EreliaStudio/Playground/actions/runs/35514737157) passes both CPU/headless and Windows/OpenGL lanes with all 32 approved `640 × 480` references unchanged.
