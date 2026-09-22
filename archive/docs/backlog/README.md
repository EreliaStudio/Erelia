# Erelia Implementation Backlog — Unified Multi-Scale Voxel Revision

> **Current position:** The installed test harness, reviewed textured Chunk baseline, complete EP-001 `VoxelVolume`/Chunk/`VoxelModel` storage contract, unified `VoxelMesher`, nested `Chunk::Mesher`, production consumer transition, and deterministic compatible-vertex reuse are complete. OD-020 is resolved and all 32 image references still pass unchanged. ST-003-02 Palette binding and the unified voxel shader contract is next; see [Erelia Implementation Status](CURRENT-STATUS.md).

This folder is the **same progressively elaborated Erelia implementation backlog**, revised around the unified multi-scale voxel architecture. Unrelated gameplay, authority, economy, dungeon and networking epics are retained; the voxel/model/tooling assumptions have been rewritten rather than replaced with a second plan.

See [GDD alignment review](traceability/GDD-ALIGNMENT-REVIEW.md) for the repository integration and design changes.

## Verified baseline

- GDD: [Erelia GDD](../gdd/Erelia_GDD_Current.md)
- Original source/baseline GDD: **17 September 2026**, Playground commit `ed75feb5e9e9a748c8bb4df95e341a19648d0091`
- Authoritative integrated GDD: **18 September 2026**, integration commit `d0e8bf6`
- Sparkle architecture-planning baseline: `Version0.1.1` at `9784377d41509234d43e4adec16505382eef178f`
- Current prebuilt Sparkle package: `0.1.2` at `65c1091fcc3d3387f7d019486e3538c7df5572d6`

The original Playground migration baseline provided normalized data-driven `Voxel::Shape` polygons, material-slot names, UVs, `Voxel::Definition`, a compact 32-bit `Voxel::Cell` with orientation/vertical flip, headless 16³ `Chunk` storage, `Chunk::Collection::worldCell`, and `Chunk::Baker`. Those semantics are now preserved by the shared `VoxelMesher`, nested `Chunk::Mesher`, retained semantic snapshots, and approved visual references.


Current package: **34 epics, 137 standalone implementation tickets, 25 persistent Open Decisions, and 9 capability/root meta-epics.** Acceptance-case totals are intentionally not hand-maintained; completeness is audited from the ticket files rather than constrained to an artificial count.

## Central architecture principle

> **Erelia has one multi-scale voxel representation. A Chunk is a 16×16×16 dynamically generated voxel volume with a one-world-unit voxel scale. A model is the same kind of voxel volume with arbitrary runtime dimensions and a smaller configurable voxel scale. Both use the same voxel Definitions, Shapes, meshing principles and material system; they differ in ownership and gameplay semantics, not in graphical data representation.**

Conceptually:

```text
Voxel::Cell
    ↓
Voxel::Definition
    ↓
normalized Voxel::Shape + material slots

              shared cell representation
                       ↓
                  VoxelVolume
          dimensions + cells + voxelSize
              ↙                    ↘
        Chunk semantics          VoxelModel semantics
      16³ / streaming /         arbitrary dimensions /
       generation / world        imported asset / entity
              ↘                    ↙
       VoxelMesher + shared occlusion cache
          default outside-volume = empty
                       ↑
  Chunk::Mesher overrides external lookup only
                       ↓
 deterministic exact-compatible indexing
                       ↓
 TextureMesh3D(position, normal, atlas UV)
```

[OD-011](open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md) resolves `VoxelVolume` as one concrete vector-owning base with generic batched editing/versioning; Chunk remains fixed at 16³ and inherits its cell ownership and editor/version implementation from that base. [OD-025](open-decisions/OD-025-voxelmesher-chunk-specialization-and-occlusion-cache.md) resolves one generic VoxelMesher owning the common algorithm/cache, with Chunk::Mesher overriding only outside-volume lookup through `Chunk::Collection`. [OD-020](open-decisions/OD-020-mesher-merging-indexing-and-hard-normal-policy.md) adds deterministic exact position/normal/UV reuse without polygon merging or normal smoothing. Palette data and binding remain ST-003-02 work. See [ARCH-007](architecture/ARCH-007-VOXEL-MESH-PALETTE-ASSEMBLY.md).

## Safe migration rule

Do **not** rewrite the working world renderer and material system in one step.

1. Characterize current chunk output with semantic fixtures and manually approved textured PNG references using SparkleTestLibrary image comparison.
2. Extract generic volume access from `Chunk` without changing current output.
3. Introduce one `VoxelMesher` with cached occlusion/default-empty boundaries and its narrow `Chunk::Mesher` outside-neighbor override while retaining `Chunk::Baker` as an oracle.
4. Prove JSON-loaded `VoxelModel`, procedural raw-volume, and filled cross-Chunk behavior through numerical, semantic, and reviewed textured-image evidence.
5. Transition runtime Chunk consumers from `Chunk::Baker` only after the new mesher references and parity evidence are approved.
6. Reuse only exact position/normal/UV-compatible vertices in deterministic first-seen order while preserving polygons, hard normals, semantic snapshots, and all 32 references.
7. Introduce the new Material abstraction alongside the current atlas path.
8. Compare non-textured/palette output against the old textured references, review the resulting failures, and explicitly version approved new visual baselines before migrating content incrementally.
9. Assemble the first articulated Hero from cached rigid voxel-volume parts.

## Character direction

Characters are **assemblies of rigid voxel volumes**, not deforming monolithic volumes and not a separate voxel format:

```text
Hero
├── torso VoxelModel
├── head VoxelModel
├── upper arm VoxelModel
│   └── lower arm VoxelModel
├── upper leg VoxelModel
│   └── lower leg VoxelModel
└── attached equipment VoxelModels
```

The pure VoxelModel contains no anchors, pivots, parent relationships, animation, equipment slots, or world transform. Those belong to a separate `VoxelAssembly`. Animation changes assembly-anchor transforms and must not trigger voxel remeshing. Actual voxel edits invalidate only the affected volume mesh. Equipment reuses named assembly anchors and reusable model parts.

## Asset workflow

The plan explicitly rejects a custom voxel-modeling application. H0 investigates mature external voxel/3D authoring tools and common formats, then implements the smallest importer/converter needed to produce the shared `VoxelVolume` representation. Blender or sidecar metadata may be used for anchor hierarchies, attachments and animation if useful.

## Material direction

The current atlas/UV system remains supported during the refactor. The target rendering abstraction is:

```text
Shape::materialSlot + Shape::outerSide
                    ↓
             MaterialResolver
                    ↓
          paletteElementIndex
                    ↓
      currently bound Palette SSBO
```

Each Palette is an independently bindable CPU/GPU resource. `PaletteCollection` may manage resources on the CPU but is not one mandatory global GPU SSBO. There is no shader-side palette ID: `paletteElementIndex` addresses the currently bound Palette. Chunks normally share a large WorldPalette; model/equipment palettes may be smaller. Changing a Palette changes visible material output without changing geometry or invoking `VoxelMesher`.

## Hierarchy

```text
ME-000 Erelia Implementation
└── Capability Meta-Epics
    └── Epics
        └── Candidate Stories
            └── Acceptance cases
```

## Testing convention

| Level | Backlog source | Intended implementation |
|---|---|---|
| Meta-epic / milestone | End-to-end invariants | system / vertical-slice tests |
| Epic | `Integration Test Catalogue` | integration tests |
| Story | Acceptance cases | unit / component tests |
| Architecture | cross-cutting properties | property, fuzz, soak, chaos, visual-regression and performance tests |

## Planning horizons

- **H0 — Foundations + visual validation:** characterize the current renderer, extract the unified volume/neighbor/mesher contracts, retain chunk parity, import one small model, prototype Materials and prove the multi-scale scene in `VS-000`.
- **H1 — First playable vertical slice:** one World proving the Erelia loop using the validated unified voxel path and an articulated Hero.
- **H2 — Onlineization and scale:** dedicated authority, remote replication, reconciliation, stronger persistence/operations.
- **H3 — Economy/content depth:** robust marketplace procurement, larger content catalog, material effects and scale/performance based on profiling.

See [`milestones/HORIZONS.md`](milestones/HORIZONS.md), [`milestones/VS-000-UNIFIED-VOXEL-VISUAL-VALIDATION.md`](milestones/VS-000-UNIFIED-VOXEL-VISUAL-VALIDATION.md) and [`milestones/VS-001-FIRST-PLAYABLE.md`](milestones/VS-001-FIRST-PLAYABLE.md).

## Rules for refinement

1. Never invent a balance/design value merely to make a story look complete.
2. Turn unresolved GDD points into a `Decision Gate`.
3. Prefer a vertical behavior slice over a large subsystem rewrite.
4. Domain state must remain testable without a Sparkle window or GPU.
5. Rendering is a view of authoritative/simulation state.
6. Visual voxel cells do not automatically become collision, navigation, networking or simulation entities.
7. One generic meshing implementation must serve Chunks and VoxelModels unless measured evidence proves otherwise.
8. Current working Chunk behavior must have regression fixtures before structural refactoring.
9. Material migration follows mesher parity; it does not gate extraction of the unified volume contract.
10. Every epic must pass its integration catalogue before it is Done.
11. Every story must have executable acceptance tests before it is Done.
12. Performance targets are measured with fixed fixtures and distributions, not one timing sample.
13. Deliberate failures originating in Playground C++ code throw `spk::Exception`; do not explicitly throw or expose standard-library exception types for Playground validation and state errors.

## Mandatory execution rule

Every ticket is an implementation contract, not permission to complete gaps by intuition. The executing agent must not invent, infer, or silently choose unspecified behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology. Any missing choice that can affect an observable contract, compatibility, persistence, rendering, acceptance result, or later ticket is an Open Decision: pause that affected work, ask the user a focused question, and record the answer in an existing or new linked OD file. Independent work may continue. Only explicitly delegated, non-observable implementation details may be chosen without clarification.

Use the reusable [Ticket Execution Prompt](templates/TICKET-EXECUTION-PROMPT.md) when assigning a ticket to an implementation agent. The prompt provides the execution procedure; the ticket remains the authoritative feature contract and repeats the no-improvisation rule so it remains safe when copied independently.
