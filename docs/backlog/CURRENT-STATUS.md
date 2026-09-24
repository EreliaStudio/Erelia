# Current Status

**Updated:** 24 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-04-definition-shape-contract`

## Branch state

Erelia uses `master` as its default branch.

The first three EP-001 implementation tickets are merged into `master`:

- ST-001-01 through PR #7;
- ST-001-02 through PR #8;
- ST-001-03 through PR #9 after project-owner approval and green CI run #105.

The former planning branch is no longer the active implementation baseline. ST-001-04 is actively being implemented and validated on `feat/st-001-04-definition-shape-contract` through PR #11. The latest implementation commit before this status update is `6423e789b69770f8f15df1e16bacda23f54a17fb`.

## What exists now

The project was restarted on 22 September 2026 and currently contains:

- top-level CMake project version 0.1.0;
- C++23;
- Core static library;
- Server static library plus EreliaServer executable;
- Client static library plus EreliaClient executable;
- Sparkle 0.1.3 Core dependency for headless/Core work;
- Sparkle graphics dependency for the Client;
- GoogleTest when BUILD_TESTING is enabled;
- independent Core, Server, and Client test folders;
- CMake presets and VS Code launch/tasks;
- Linux/Windows headless CI for Core/Server;
- Windows graphical Client CI in Debug and Release;
- formatting checks;
- current GDD and illustration assets under `docs/gdd/`;
- historical source/backlog isolated under `archive/`.

Implemented EP-001 foundations now include:

- shared terrain coordinate conversion;
- packed 32-bit `Voxel::Cell` / `Voxel::Definition::ID`;
- immutable owning `Voxel::Volume` + Builder + pooled Buffer/Lease storage.

## What was just resolved

The ST-001-04 contract review is complete.

DR-018 now fixes the first shared voxel Shape/Definition/Catalog contract, including:

- shared semantic Shape and Definition resources for Server and Client;
- string Shape IDs and numeric Definition IDs;
- `Voxel::Material::ID` string identity with `Material::InvalidID == "InvalidID"`;
- discrete `spk::Vector3Int` Shape vertices quantized from normalized JSON with `Voxel::Shape::VertexPrecision = 0.001f`;
- convex planar CCW polygons with semantic slots and derived normals;
- revised Cell Orientation ordering `PositiveX=0, NegativeZ=1, NegativeX=2, PositiveZ=3`, directly representing CCW quarter-turn count;
- `NegativeY` mirroring around `Y=0.5` with polygon rewinding;
- lazy, mutex-protected eight-way oriented polygon caching with atomic `spk::UUID` publication and lock-free published reads;
- aggregate `Voxel::Catalog` JSON loading and typed Shape/Definition lookup behavior through the Erelia-local `spk::JSON::Catalog<TElement>` abstract base;
- catalog-created Definition ID 0 Air;
- incremental load/failure behavior;
- first cube/slab/slope/stair resources derived from the validated archive fixtures;
- explicit exclusion of occlusion algorithms from ST-001-04.

OQ-039 remains Partially resolved, but its remaining exact generator-scene coordinates, Definition IDs, and material choices now block ST-001-06 rather than ST-001-04.

## Current implementation phase

EP-001 is still Draft overall, but implementation is active.

### Completed

- **ST-001-01 — Shared terrain coordinate conversion:** Done.
- **ST-001-02 — Packed Voxel::Cell value type:** Done.
- **ST-001-03 — Owning Voxel::Volume:** Done.

### In progress

**ST-001-04 — First terrain Definition and Shape contract** is **In Progress** on PR #11.

The branch contains the first shared `Voxel::Shape` / `Voxel::Definition` / `Voxel::Catalog` implementation, `Voxel::Material::ID`, `Voxel::Material::SlotID`, the revised Cell Orientation ordering, JSON catalog loading, exact eight-way Orientation/Flip geometry transforms, the lazy UUID-published oriented polygon cache, focused tests, and active cube/slab/slope/stair Shape resources. Shape and Definition catalogs derive publicly from an Erelia-local `spk::JSON::Catalog<TElement>` base that owns JSON envelope iteration and direct element-value storage; they inherit `load` and lookup behavior directly and override only key/element parsing, with `Definition::Catalog` additionally retaining its Shape-catalog reference. `Voxel::Definition` stores a non-owning `const Shape&`; Air references a private catalog-owned empty Shape sentinel with zero polygons. No Erelia `detail` namespace is used for this abstraction. Catalog implementations are split by class across `shape_catalog.cpp`, `definition_catalog.cpp`, and aggregate `catalog.cpp`. File/path-aware JSON validation errors use the single shared `spk::JSON::throwAt` helper.

The Shape loader preserves polygon vertex order authored in JSON and derives normals from that order. Vertices are authored using Sparkle's `[x, y, z]` Vector3 JSON representation, stored as semantic 32-bit `Voxel::Vertex` values after quantization, and validated with 64-bit integer intermediates where cross/dot products need additional range. It validates polygon geometry without inventing an outward-facing direction; only the required `NegativeY` mirror reverses transformed polygon winding.

Current validation evidence:

- PR #11 is open.
- The catalog uses direct `std::unordered_map<ID, Element>` storage with no `shared_ptr`, with dedicated generic-catalog and reference-stability tests.
- Shape/Definition subcatalogs publicly inherit the base `load`/lookup API directly, with no no-op forwarding wrappers.
- Shape vertices use semantic `Voxel::Vertex = spk::Vector3Int`, authored as Sparkle Vector3 JSON arrays `[x, y, z]`; cached normals remain floating `spk::Vector3`.
- JSON/resource source-aware errors use the single `spk::JSON::throwAt` helper.
- Shape, Definition, and aggregate Catalog implementations are split by class.
- CI run #256 (run ID `35972200952`) passed the complete matrix for code head `7277609a680ab23b501c1b2423d3e7cbaffd8d1f`.
- Sparkle issues #14 and #15 are tracked in `OPEN_REQUESTS/` with exact follow-up edit locations.
- Required project-owner approval has not yet been recorded.

ST-001-04 is technically complete and remains In Progress only until the project owner explicitly approves the ticket.

### Next

1. Record project-owner approval and mark ST-001-04 Done only after its Definition of Done is actually satisfied.
4. After ST-001-04, reassess the dependency order rather than skipping unresolved gates.
5. Resolve OQ-039's remaining generator-scene details before ST-001-06.
6. Resolve OQ-037/OQ-038 before the dependent Chunk networking/request tickets.
7. Resolve OQ-036 before boundary-aware Client meshing.
8. Resolve OQ-029 through OQ-031 before final visual/performance validation.
## Relevant approved planning constraints

EP-001 is constrained by DR-001 through DR-004, DR-007, DR-009 through DR-018 and ARCH-001 through ARCH-004 as listed in the Epic.

The canonical decision/architecture indexes remain `DECISIONS/README.md` and `ARCHITECTURE/README.md`.

## Explicit non-goal

Do not materialize broad speculative game implementation while near-term architectural contracts remain unresolved.
