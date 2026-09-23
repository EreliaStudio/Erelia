# Current Status

**Updated:** 23 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-04-definition-shape-contract`

## Branch state

Erelia uses `master` as its default branch.

The first three EP-001 implementation tickets are merged into `master`:

- ST-001-01 through PR #7;
- ST-001-02 through PR #8;
- ST-001-03 through PR #9 after project-owner approval and green CI run #105.

The former planning branch is no longer the active implementation baseline. The resolved ST-001-04 planning/decision updates and its forthcoming implementation live on `feat/st-001-04-definition-shape-contract` until that ticket is completed and merged.

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
- aggregate `Voxel::Catalog` JSON loading and typed Shape/Definition lookup behavior;
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

### Next Ready ticket

**ST-001-04 — First terrain Definition and Shape contract** is **Ready**.

The implementation must follow DR-018 and the complete ticket contract. In particular, it must update the existing Cell Orientation enum/tests to the new quarter-turn ordering while preserving the 32-bit packed layout.

### Existing later blockers

- OQ-036 — missing-neighbor/remesh behavior for terrain meshing;
- OQ-037 — remaining scalar wire portability/decode contract;
- OQ-038 — request/cache/retention/partial-response/retry semantics;
- OQ-039 — exact deterministic generator-scene coordinates/Definition IDs/material choices;
- OQ-029 — golden-image platform;
- OQ-030 — image comparison metric/tolerances;
- OQ-031 — performance evidence methodology.

### Remaining Draft-only specification gaps

- exact EP-001 Server endpoint and Client connection lifecycle/configuration;
- deterministic render fixture/material realization plus render-resource failure/lifecycle behavior;
- complete temporary ZQSD/free-flight input map and numeric camera/movement semantics.

## Next

1. Continue ST-001-04 implementation on the existing `feat/st-001-04-definition-shape-contract` branch; do not recreate it from `master`, because this branch already contains the approved Ready contract and DR-018.
2. After ST-001-04, reassess the dependency order rather than skipping unresolved gates.
3. Resolve OQ-039's remaining generator-scene details before ST-001-06.
4. Resolve OQ-037/OQ-038 before the dependent Chunk networking/request tickets.
5. Resolve OQ-036 before boundary-aware Client meshing.
6. Resolve OQ-029 through OQ-031 before final visual/performance validation.

## Relevant approved planning constraints

EP-001 is constrained by DR-001 through DR-004, DR-007, DR-009 through DR-018 and ARCH-001 through ARCH-004 as listed in the Epic.

The canonical decision/architecture indexes remain `DECISIONS/README.md` and `ARCHITECTURE/README.md`.

## Explicit non-goal

Do not materialize broad speculative game implementation while near-term architectural contracts remain unresolved.
