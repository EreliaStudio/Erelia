# Erelia Implementation Status

**Last updated:** 21 September 2026

**Implementation base:** `main` at `3cfae1c79e9a9dfeb81e4cd96891ff4fb3510cfd`

**Delivery:** [PR #9](https://github.com/EreliaStudio/Playground/pull/9) from `feature/st-003-02-palette-binding`

**Current horizon:** H0 — Foundations and visual validation

**Current checkpoint:** ST-003-02 is complete, including owner-approved Palette golden images. ST-033-01 is next but remains at its material-mapping decision gate.

This is the living answer to:

> Where are we now, what has actually been completed, and what should be implemented next?

The word **complete** below means implementation and test evidence exist. A written ticket is only specified.

## Status legend

| Marker | Meaning |
|---|---|
| ✅ Complete | Finished with identified evidence. |
| 🟡 Current | The one active implementation checkpoint. |
| ⬜ Next | Ordered follow-up work whose prerequisites are known. |
| ⛔ Decision gate | Work must pause for user clarification or a recorded Open Decision. |
| 🔭 Later | Deliberately outside the current checkpoint/horizon. |

## Where we are now

```text
Working textured Chunk renderer                    ✅
Sparkle::TestLibrary export                        ✅
Playground TestLibrary consumption                 ✅
Prebuilt Sparkle package consumption               ✅
Current Chunk semantic + golden baseline           ✅
VoxelVolume contract + batched editor/versioning   ✅
Chunk inheritance / storage/editor migration       ✅
Runtime-sized VoxelModel                           ✅
Current Baker semantic fixtures                    ✅
VoxelMesher + Chunk::Mesher implementation         ✅
Chunk::Mesher consumer transition                  ✅
Mesher compatible-vertex reuse (ST-032-04)         ✅
Palette rendering contract (ST-003-02)             ✅
Palette candidate visual review                    ✅
ST-033-01 Palette terrain prototype                ⛔ DECISION
VS-000 unified voxel visual validation             ⬜
VS-001 first playable                              🔭
```

## Completed

### Current implementation baseline

- ✅ Data-driven voxel Shapes, Definitions, compact packed `Voxel::Cell`, fixed 16³ Chunk behavior, Collection/world lookup, and the unified `VoxelMesher`/`Chunk::Mesher` path exist.
- ✅ The duplicated `Chunk::Baker` implementation is retired; its approved semantic snapshots remain as independent historical parity fixtures.
- ✅ The existing textured Chunk renderer is the protected visual baseline for structural work.
- ✅ Deliberate failures originating in Playground C++ code use `spk::Exception`, including validation, state, catalog lookup, and test-library diagnostics.

### Installed test/package infrastructure

- ✅ Sparkle exports the optional installed `Sparkle::TestLibrary` component.
- ✅ Playground resolves `sparkle CONFIG REQUIRED COMPONENTS TestLibrary` and links `Sparkle::TestLibrary` through `PlaygroundTestLibrary`.
- ✅ CI downloads the checksum-pinned prebuilt Sparkle `0.1.2` package at `65c1091fcc3d3387f7d019486e3538c7df5572d6`; Playground does not rebuild Sparkle source.
- ✅ CPU/headless and Windows/OpenGL jobs are separate and retain diagnostics/artifacts.
- ✅ Architecture planning used the older Sparkle baseline `9784377d41509234d43e4adec16505382eef178f`; it is not presented as the currently consumed package.

### ST-001-01 — Current Chunk characterization

- ✅ Semantic tests cover storage/indexing, coordinates, Collection, Editor, scheduler, meshing, transforms, catalog, Shape, and packed Cell behavior.
- ✅ The Windows/OpenGL harness covers five scene families from four fixed views each.
- ✅ All 20 approved current textured Chunk PNG references are checked in.
- ✅ The controlled UV-regression sensitivity path proves an altered UV fails comparison without changing tolerances or references.
- ✅ OD-024 records the approved fixtures, `640 × 480` capture, and canonical `windows-2025` software-OpenGL runner.
- ✅ Main-branch [CI run 35456326462](https://github.com/EreliaStudio/Playground/actions/runs/35456326462) passed both `CPU/headless tests` and `Windows/OpenGL golden candidates` for commit `0bd12e5`.

### Planning and decisions

- ✅ The backlog contains 34 epics, 137 tickets, and 27 persistent decision records.
- ✅ Seven decisions are resolved: OD-011, OD-020, OD-023, OD-024, OD-025, OD-026, and OD-027. Twenty remain open.
- ✅ OD-011 selects one concrete vector-owning `VoxelVolume` base and explicitly accepts replacing Chunk's fixed array during ST-001-03.
- ✅ OD-023 preserves the existing packed Cell orientation/vertical-flip representation until evidence justifies a separate expansion decision.

### ST-001-02 — VoxelVolume contract and generic editor

- ✅ `VoxelVolume` owns runtime-sized, zero-initialized `std::vector<Voxel::Cell>` storage.
- ✅ Runtime dimensions, finite positive uniform voxel size, checked coordinate access, read-only span access, and origin-based local bounds are implemented.
- ✅ Meshing-facing access is read-only; controlled mutation uses the generic nested `VoxelVolume::Editor`.
- ✅ `VoxelVolume` owns versioning, and a changed editor session publishes exactly one invalidation while a no-op session publishes none.
- ✅ VoxelVolume-owned failures use the selected `spk::Exception` policy with stable messages and source-location diagnostics.
- ✅ Twelve focused headless GoogleTests cover metadata/validation, access/topology, editor/version transactions, and Sparkle exception diagnostics.
- ✅ Branch [CI run 35460049379](https://github.com/EreliaStudio/Playground/actions/runs/35460049379) passed both complete lanes for remote commit `d00e467`.
- ✅ Editor/version refinement [CI run 35461166765](https://github.com/EreliaStudio/Playground/actions/runs/35461166765) passed both complete lanes for remote commit `9effce8`.
- ✅ Sparkle-exception refinement [CI run 35463284473](https://github.com/EreliaStudio/Playground/actions/runs/35463284473) passed both complete lanes for remote commit `7b04ab2`.
- ✅ No approved golden reference, tolerance, texture, UV, Chunk implementation, or rendering output changed.

### ST-001-03 — Chunk adapter/specialization

- ✅ `Chunk` now derives from `VoxelVolume` and constructs the shared storage as exactly `16×16×16` at voxel size `1.0`.
- ✅ Chunk's duplicate fixed array, direct `VersionedTrait` inheritance, and duplicate editor implementation were removed.
- ✅ `Chunk::edit()` resolves to the inherited `VoxelVolume::Editor`; changed and no-op sessions retain the established one-notification transaction behavior.
- ✅ Chunk keeps its coordinate, world conversion, Collection, generation, baking, scheduling, and rendering responsibilities.
- ✅ Focused Chunk tests cover the fixed constructor specialization and Chunk-owned coordinate/index behavior; inherited storage, access, editor, versioning, and diagnostics remain covered once by the VoxelVolume suites.
- ✅ BakeScheduler coverage proves an edited Chunk queues itself and every available face-neighbor for rebuild.
- ✅ Implementation commit: [`27c095f`](https://github.com/EreliaStudio/Playground/commit/27c095f467cdb4bd1525aa5be81bccfab889b5fb).
- ✅ Test-ownership refinement commit: [`f3f855b`](https://github.com/EreliaStudio/Playground/commit/f3f855b71c67bfb96100bb09a3e52fbedd14683b).
- ✅ PR #4 [CI run 35467461734](https://github.com/EreliaStudio/Playground/actions/runs/35467461734) passed both `CPU/headless tests` and `Windows/OpenGL golden candidates` for commit `f3f855b`.
- ✅ No approved reference, comparison tolerance, texture, UV, or intended rendering output changed.

### ST-001-04 — Runtime-sized VoxelModel storage

- ✅ `VoxelModel` is one final semantic specialization over `VoxelVolume`; runtime dimensions and uniform voxel scale remain constructor data rather than template parameters.
- ✅ Representative `10×10×14`, `12×7×14`, and `32×32×22` models use the same C++ type.
- ✅ An `8×8×16` model at scale `0.1` reports local bounds `0.8×0.8×1.6`.
- ✅ `VoxelModel` inherits the common packed-cell read contract, batched editor, version notification, and `spk::Exception` validation policy without adding a model-specific mutation API.
- ✅ A provisional `spk::JSON::Reader` constructor accepts runtime dimensions, voxel size, and sparse `{position, value, orientation, flip}` entries without changing packed Cell storage.
- ✅ Sparse-loader duplicate detection uses `spk::Vector3Int` directly as an ordered key through Sparkle `0.1.2`; the temporary tuple adapter is removed.
- ✅ Orientation and flip use reviewed named strings; `orientation` is required and omitted `flip` defaults to `positive-y`.
- ✅ Duplicate or out-of-range positions, unknown fields/enum names, and values beyond packed Cell capacity fail with JSON file/path diagnostics.
- ✅ Nine focused headless GoogleTests cover runtime storage plus the provisional sparse JSON loader and its rejection behavior.
- ✅ No stable asset format, file-loading API, catalog mapping, rendering, Palette, GPU, transform, anchor, animation, or Chunk-world behavior was added.
- ✅ Implementation head: [`affc850`](https://github.com/EreliaStudio/Playground/commit/affc850945e233928acfa813e7add7d7a8a6cf69).
- ✅ PR #5 [CI run 35498639959](https://github.com/EreliaStudio/Playground/actions/runs/35498639959) passed both `CPU/headless tests` and `Windows/OpenGL golden candidates`.

### ST-032-01 — Current Baker semantic golden fixtures

- ✅ Seven minimal fixtures lock the current cube, slab, slope, stair, cross, unloaded-neighbor, and cross-Chunk occlusion semantics.
- ✅ Canonical snapshots compare positions, normals, atlas UVs, winding, and visible topology while ignoring triangle emission order, vertex-buffer indices, and cyclic starting vertices.
- ✅ Exact Chunk/cell coordinates, stable Definition names/runtime IDs, orientations, flips, topology, and atlas expectations are recorded with the fixtures.
- ✅ Boundary coverage proves an absent adjacent Chunk is empty and an available solid neighbor removes only the shared face across local X coordinates `15 → 0`.
- ✅ Repeated inputs produce byte-identical semantic output; an unknown Definition ID throws `spk::Exception` without poisoning subsequent valid bakes.
- ✅ At this checkpoint, `Chunk::Baker` remained intact as the later `VoxelMesher` parity oracle; ST-004-01 subsequently retired its implementation after parity passed.
- ✅ No approved textured reference, comparison tolerance, camera, atlas, shader, or canonical runner changed.
- ✅ OD-025 records the approved one-base-mesher design: shared cached occlusion in VoxelMesher and only external-neighbor lookup overridden by Chunk::Mesher.
- ✅ Semantic-fixture implementation: [`aa1b9c2`](https://github.com/EreliaStudio/Playground/commit/aa1b9c216a86476ebef0bd46000bf38b9c82e8a8).
- ✅ Playground-wide Sparkle-exception refinement: [`f0db728`](https://github.com/EreliaStudio/Playground/commit/f0db728d49360e78e9d7e44ff16d2fe83d715ff8).
- ✅ PR #6 [CI run 35502696771](https://github.com/EreliaStudio/Playground/actions/runs/35502696771) passed both `CPU/headless tests` and `Windows/OpenGL golden candidates` after the exception-policy refinement.

### ST-032-02 — Unified VoxelMesher and Chunk::Mesher implementation

- one base `VoxelMesher` owns deterministic iteration, scale-aware Shape transforms, textured mesh emission, visibility, and the reusable occlusion cache;
- base outside-volume lookup returns `Voxel::Cell{}`, while derived `Chunk::Mesher` overrides only that hook through `Chunk::Collection`;
- an `8 × 8 × 16`, scale-`0.1` JSON cross statue and identical procedural raw volume emit the same `290` vertices/`432` indices and pixel-identical four-view captures;
- filled adjacent Chunk sections prove `144 → 120 + 120` indices, hidden-neighbor occlusion, both-rendered output, and correct lookup in all six directions;
- all seven ST-032-01 snapshots match, rejected IDs use `spk::Exception`, repeated bakes reuse the cache, and the existing Baker remained untouched at this checkpoint as the parity oracle;
- candidate artifact `10606095311` from CI run `35510619947`, SHA-256 `91bb25488ec4a2c0f5c4c47184559ae1f6e9b44922e82e4a1f7abff36ec447d4`, was explicitly approved by the project owner;
- commit [`ae1deac`](https://github.com/EreliaStudio/Playground/commit/ae1deac7a3d1ac7dbf2e8d75f08780b863845c35) checks in those exact twelve `640 × 480` PNGs while preserving the existing 20 references;
- comparison-lifecycle fix [`6e0054f`](https://github.com/EreliaStudio/Playground/commit/6e0054f098a241193cec0781090ddf17d67f44c0) protects JSON/procedural parity inputs from successful-comparison cleanup;
- CI run [35513348937](https://github.com/EreliaStudio/Playground/actions/runs/35513348937) passes both the complete CPU/headless and Windows/OpenGL lanes with all 32 approved references.

ST-032-03 and ST-004-02 are superseded by this approved consolidation.

### ST-004-01 — Transition Chunk consumers from Baker to Chunk::Mesher

- ✅ `Chunk::Mesher` is a directly declared nested Chunk specialization in `chunk_mesher.hpp`; no compatibility alias or prefixed top-level class was introduced.
- ✅ The application owns one `Chunk::Mesher`, and `Chunk::BakeScheduler` uses that instance without a second adapter or meshing loop.
- ✅ Scheduler tests preserve completion delivery, empty/solid mesh index counts, version invalidation, and exactly-once rebuild scheduling for the edited Chunk plus every available face-neighbor.
- ✅ Invalid Definition data throws `spk::Exception`; repairing it requeues and successfully bakes both the source and available neighbor.
- ✅ Seeded generation is unchanged in the approved rendered fixture and now has explicit replay coverage across different request orders.
- ✅ The retained ST-032-01 snapshots independently protect old Baker semantics while all consolidated VoxelMesher numerical/semantic tests remain unchanged.
- ✅ The obsolete Baker declaration, implementation, private occlusion cache, and Baker-only tests were removed; no production or test source references `Chunk::Baker`.
- ✅ All 20 current textured Chunk references and all 12 consolidated-mesher references remain byte-for-byte unchanged.
- ✅ Consumer-transition implementation: [`7fd8029`](https://github.com/EreliaStudio/Playground/commit/7fd8029b849b5244324ef5c4fafd91e344e13689).
- ✅ Nested-type refinement: [`d0edb2a`](https://github.com/EreliaStudio/Playground/commit/d0edb2af4aacf4fc9e01b953a8a2143889a89f38).
- ✅ PR #7 [CI run 35514737157](https://github.com/EreliaStudio/Playground/actions/runs/35514737157) passed both complete CPU/headless and Windows/OpenGL lanes with all 32 approved references.

### ST-032-04 — Deterministic compatible-vertex reuse

- ✅ OD-020 records the project-owner-approved policy: no polygon merging, exact position/normal/UV compatibility, deterministic first-seen indices, and preserved hard flat normals.
- ✅ One bake-local lookup in `VoxelMesher` serves `VoxelVolume`, `VoxelModel`, and inherited `Chunk::Mesher`; no Chunk-specific indexing loop exists.
- ✅ The smallest existing sharing fixture is one `debug_stair`: `40 → 38` vertices while indices remain `60`. The JSON/procedural fixture changes `290 → 288` vertices while retaining `432` indices.
- ✅ A two-cube atlas-seam fixture remains `40` vertices/`60` indices, and a cube corner retains three distinct hard-normal vertices.
- ✅ Focused tests prove exact compatible reuse, normal and UV separation, byte-identical repeated output, identical generic/Chunk output, and clean recovery after an invalid Definition throws `spk::Exception`.
- ✅ Existing scale, orientation, vertical flip, clipped-polygon, six-direction Chunk-neighbor, cross-Chunk occlusion, winding, cache, and all seven ST-032-01 semantic snapshot results remain valid.
- ✅ Implementation commit [`1120e1b`](https://github.com/EreliaStudio/Playground/commit/1120e1bd462f44ff128b59dad528c84e9988a123) is delivered by [PR #8](https://github.com/EreliaStudio/Playground/pull/8).
- ✅ [CI run 35521204431](https://github.com/EreliaStudio/Playground/actions/runs/35521204431) passes both complete lanes; all 20 original Chunk and 12 consolidated-mesher `640 × 480` references pass unchanged.
- ✅ No timing claim is made because the available hosted runner is not a controlled benchmark environment; no unstable timing assertion was added.

### ST-003-02 — Palette resource binding and unified voxel shader contract

- ✅ `Palette` owns one independently bindable Sparkle SSBO by composition and keeps a typed `ShaderStorageBuffer::View<void, Palette::Data>` over its RGBA entries; reference edits require explicit `validate()`.
- ✅ `VoxelMesh` carries position, hard flat normal, and integer `paletteElementIndex`; the attribute and both shader stages preserve its integer, flat/non-interpolated contract.
- ✅ `MaterialResolver` receives immutable volume, Definition, transformed emitted polygon, coordinate, and Cell context and returns only the currently bound Palette element index.
- ✅ Generic `VoxelMesher` and `Chunk::Mesher` emit the same deterministic Palette mesh representation. Palette element identity participates in compatible-vertex reuse, so material seams cannot collapse while existing polygons and hard normals remain intact.
- ✅ `VoxelRenderCommand` accepts a required Palette reference, retains a shared-resource Palette value copy, and binds program → mesh → Palette → camera/model data → draw. Per OD-027 it trusts prepared mesh/Palette compatibility and does not rescan vertices.
- ✅ Palette/transform swaps reuse identical vertex/index bytes and do not invoke the mesher again. Sequential Human/Orc commands bind different resources; Chunk and VoxelModel use the same mesh/shader path.
- ✅ The existing TextureMesh3D atlas path, all 32 approved textured references, tolerances, and ST-032-04 no-merge policy remain unchanged.
- ✅ OD-026 records the approved RGBA payload, full resolver context, value-copy lifetime, SSBO composition/View, and explicit validation policy.
- ✅ OD-027 records the owner-selected render-command trust boundary: no empty/out-of-range compatibility check in the command, mesher, or shader; compatible inputs are a caller precondition.
- ✅ [Sparkle issue #5](https://github.com/EreliaStudio/Sparkle/issues/5) tracks a possible later inheritance migration; ST-003-02 uses the pinned Sparkle `0.1.2` package unchanged.
- ✅ [PR #9 CI run 35538271204](https://github.com/EreliaStudio/Playground/actions/runs/35538271204) passes both complete lanes for commit [`b5cbcdc`](https://github.com/EreliaStudio/Playground/commit/b5cbcdc814a608cf493b0cefe4eb9b2592997bf1).
- ✅ [PR #9 CI run 35572632309](https://github.com/EreliaStudio/Playground/actions/runs/35572632309) passes both `CPU/headless tests` and `Windows/OpenGL golden tests` after OD-027 removes render-command validation and the approved Palette references become enforced comparisons.
- ✅ The project owner approved the exact Human, Orc, sequential Human+Orc, and WorldPalette Chunk candidates from final CI run `35538640066` on 21 September 2026. Those four `640 × 480` images are checked in as Palette golden references; the existing textured references remain unchanged.
- ✅ Final-run artifact `10613866642` preserves the rendered actuals and textured-to-Palette difference; artifact `10614355465` preserves the reviewed proposed baselines.

## Current implementation checkpoint

### ⛔ ST-033-01 — Palette Material terrain prototype decision gate

ST-003-02 is complete after owner approval and promotion of its four reviewed Palette references. Before ST-033-01 implementation can freeze content behavior, resolve the exact `WorldPalette` precedence among default, `materialSlot`, `outerSide`, and exact overrides. OD-019 remains a separate gate for procedural Palette variation; direct element lookup does not require it.

## Next implementation sequence

| Order | Status | Work | Why it comes here |
|---:|---|---|---|
| 1 | ✅ | [ST-001-02](epics/EP-001-unified-voxel-volume/tickets/ST-001-02-read-only-voxelvolume-contract.md) — owning VoxelVolume/read/edit contract | Generic storage, reads, editing/versioning, Sparkle diagnostics, and both complete CI lanes are passing. |
| 2 | ✅ | [ST-001-03](epics/EP-001-unified-voxel-volume/tickets/ST-001-03-chunk-adapter-specialization.md) — Chunk inheritance/storage/editor migration | Chunk now uses inherited 16³/1.0 storage, reads, editing, and versioning while retaining Chunk-only behavior and unchanged rendering. |
| 3 | ✅ | [ST-001-04](epics/EP-001-unified-voxel-volume/tickets/ST-001-04-runtime-sized-voxelmodel-storage.md) — runtime-sized VoxelModel semantic type | One semantic model type now forwards runtime dimensions and scale to shared storage and inherits editing/versioning unchanged. |
| 4 | ✅ | [ST-032-01](epics/EP-032-unified-voxel-mesher/tickets/ST-032-01-current-baker-semantic-golden-fixtures.md) — current Baker semantic fixtures | Seven canonical fixtures now protect positions, normals, atlas UVs, winding, visible topology, and both Chunk-boundary outcomes. |
| 5 | ✅ | [ST-032-02](epics/EP-032-unified-voxel-mesher/tickets/ST-032-02-generic-cell-iteration-and-scale-aware-shape-transform.md) — unified VoxelMesher + Chunk::Mesher | Shared algorithm/cache, Chunk specialization, numerical/semantic evidence, and all twelve approved references pass both CI lanes. |
| 6 | ✅ | [ST-004-01](epics/EP-004-chunk-world-generation/tickets/ST-004-01-chunk-mesher-integration.md) — transition Chunk consumers | Application, scheduler, tests, and golden renderer use the nested `Chunk::Mesher`; obsolete Baker code is removed. |
| 7 | ✅ | [ST-032-04](epics/EP-032-unified-voxel-mesher/tickets/ST-032-04-optimization-and-indexed-output.md) — deterministic compatible-vertex reuse | OD-020 is resolved; exact first-seen position/normal/UV reuse preserves polygons, hard normals, semantics, and all 32 references. |
| 8 | ✅ | Prove unchanged semantic mesh and textured golden-image parity after the consumer transition | Retained semantic fixtures and all 32 unchanged image references pass CI run 35514737157. |
| 9 | ✅ | [ST-003-02](epics/EP-003-runtime-rendering-mesh-cache/tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) implementation | Per-Palette SSBO composition, common mesh/shader contract, caller-owned compatibility, value-copy lifetime, semantic tests, and both CI lanes pass. |
| 10 | ✅ | ST-003-02 Palette golden images | The owner approved the four final-run candidates; exact checked-in references now guard Human, Orc, sequential dual-Palette, and WorldPalette rendering. |
| 11 | ⛔ DECISION | [ST-033-01](epics/EP-033-material-visual-validation/tickets/ST-033-01-palette-material-terrain-prototype.md) — Palette Material terrain prototype | Next smallest backlog step; exact WorldPalette default/materialSlot/outerSide/exact-override precedence must be approved before implementation. OD-019 separately gates any procedural variation. |
| 12 | ⬜ | EP-030 → EP-002 → EP-031 asset import, runtime assembly, and authored character workflow | Builds model/assembly content on validated runtime contracts. |
| 13 | ⬜ | [VS-000](milestones/VS-000-UNIFIED-VOXEL-VISUAL-VALIDATION.md) | Validates terrain, model, assembly, equipment, Palette, and performance together. |
| 14 | 🔭 | [VS-001](milestones/VS-001-FIRST-PLAYABLE.md) | Starts the gameplay vertical slice after the visual/runtime foundation. |

## Known user gates

- OD-020 is resolved: do not merge polygons; share only exact position/normal/UV matches in deterministic first-seen order and preserve hard flat normals.
- OD-025 is resolved: VoxelMesher owns the common cached algorithm; Chunk::Mesher overrides only outside-volume neighbor lookup.
- OD-026 is resolved: Palette uses RGBA data, SSBO composition/View, explicit validation, the full immutable resolver context, and shared-resource value-copy command lifetime.
- OD-027 supersedes OD-026's command-input validation only: `VoxelRenderCommand` accepts `const Palette&` and trusts the caller's mesh/Palette compatibility without rescanning vertices.
- The four ST-003-02 Palette candidates from CI run `35538640066` were explicitly approved by the project owner on 21 September 2026 and promoted without modifying the pre-existing textured references.
- ST-033-01 must not invent WorldPalette default/materialSlot/outerSide/exact-override precedence. Resolve that mapping from an owner-approved example before freezing the content schema.
- OD-019 remains open for any procedural Palette variation; direct element lookup is implemented, but variation must not be invented.
- The proposed broader `Material`/`MaterialCollection` layer is explicitly deferred and is not part of PR #9.
- Sparkle SSBO inheritance is deferred to [Sparkle issue #5](https://github.com/EreliaStudio/Sparkle/issues/5); current Palette composition is intentional.
- Approval of any model, assembly, animation, equipment, import, or material-mapping schema before it is frozen.
- Review of expected, produced, and difference images before accepting the later Palette migration baseline.

Additional ambiguity discovered during implementation becomes an Open Decision. The executing agent must not improvise it.

## How to update this file

1. Move a ticket to **Complete** only when implementation, automated tests, applicable CI, decision provenance, and visual evidence exist.
2. Keep exactly one immediate checkpoint **Current** unless independent active work truly exists.
3. Record the next smallest executable ticket, not an entire epic.
4. Update this file in the same branch as the code that changes project state.
5. Preserve completed history and distinguish architecture baselines from currently consumed dependency revisions.

## Quick links

- [Backlog overview](README.md)
- [Epic index](EPIC-INDEX.md)
- [Open Decisions](open-decisions/README.md)
- [Planning horizons](milestones/HORIZONS.md)
- [VS-000](milestones/VS-000-UNIFIED-VOXEL-VISUAL-VALIDATION.md)
- [VS-001](milestones/VS-001-FIRST-PLAYABLE.md)
- [Ticket execution prompt](templates/TICKET-EXECUTION-PROMPT.md)
