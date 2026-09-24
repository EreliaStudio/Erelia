# Current Status

**Updated:** 24 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-06-deterministic-validation-terrain-generator`

## Branch state

Erelia uses `master` as its default branch.

ST-001-01 through ST-001-05 are merged into `master`. PR #12 merged **ST-001-05 — Voxel::Volume Message serialization** on 24 September 2026 after explicit project-owner approval.

The active ST-001-06 branch was created from current `master` at `ab2e56c60b8294da1e9bbbbf26b13594ebdc4b14`.

The branch contains planning/decision documentation only so far; no ST-001-06 production implementation has been performed. It also contains the previously approved ST-001-12 documentation clarification for structural multi-Shape occlusion fixtures.

## What exists on master

Implemented EP-001 foundations include:

- shared terrain coordinate conversion;
- packed 32-bit `Voxel::Cell` / `Voxel::Definition::ID`;
- immutable owning `Voxel::Volume` + Builder + pooled Buffer/Lease storage as implemented by ST-001-03;
- shared voxel Shape/Definition/Catalog contract and first terrain resources from ST-001-04;
- shared Sparkle-native generic `Voxel::Volume` Message serialization from ST-001-05.

ST-001-03 and ST-001-05 remain historically Done. Their current deep-copy/direct-Lease and same-destination-buffer-reuse details are explicitly superseded for the next implementation by DR-019.

## Decisions completed on the active branch

### OQ-039 / DR-015

OQ-039 is now **Resolved**.

DR-015 records the exact first validation terrain:

- infinite X/Z cube baseline at world Y=0;
- cube walls on X=0 and Z=0, Y=1..3;
- Definition IDs: 1 cube, 2 slope, 3 stair, 4 slab;
- material IDs `<shape-id>-<slot-id>`;
- slope Chunk `(1,0,1)`, stair Chunk `(2,0,1)`, slab Chunk `(1,0,2)`;
- exact ground and elevated transform-placement tables;
- all eight Orientation/Flip combinations;
- horizontal adjacency and vertical stacking;
- all other Cells Empty, including world Y<0;
- approved validation Chunks `(0,0,0)`, `(1,0,1)`, `(2,0,1)`, `(1,0,2)`, `(-1,0,0)`, `(0,0,-1)`, `(-1,0,-1)`, `(0,-1,0)`.

### DR-019 — immutable Volume / Chunk Collection/Provider

The project owner approved:

- built Volume/Chunk copies share immutable backing Cell content;
- Builder owns mutable pooled storage only until build;
- the final shared owner returns the Buffer Lease to its pool;
- generic Volume Message decode no longer overwrites/reuses the destination's old Buffer in place;
- reusable Volume/Builder internals needed by derived semantic types become protected;
- `Chunk::Builder` derives from `Voxel::Volume::Builder`, fixes 16³ / 1.0f and returns Chunk;
- public checked `Chunk(Voxel::Volume&&)` validates the Chunk invariants;
- Chunk does not store its own coordinate;
- Core owns `Chunk::Collection` and nested `Chunk::Collection::Provider`;
- Collection exclusively owns its Provider in a private `std::unique_ptr<Provider>`; callers construct the Collection from a concrete Provider rvalue through the approved constrained templated constructor; the concrete Provider is moved into the owned allocation, lvalue construction is rejected, and null/absent Provider state is unrepresentable;
- Collection caches Chunks by coordinate and returns Chunk values;
- published Chunks are immutable and complete-value replacement is used instead of Cell mutation;
- copied old Chunks remain alive safely while another thread replaces the Collection entry;
- a future Client request Provider may return an empty valid placeholder and later replace it with canonical Server data;
- a future dedicated Chunk codec transfers only 4096 Cells, omitting fixed dimensions/unit size; ST-001-08 owns that codec/protocol work.

### ST-001-06 readiness — Prototype provider coordinate domain

The project owner explicitly resolved the first ST-001-06 readiness decision:

- `PrototypeChunkProvider` accepts every representable `Chunk::Coordinate` (`spk::Vector3Int`);
- negative X, Y and Z coordinates are valid;
- the prototype provider has no coordinate-domain rejection path;
- coordinates where DR-015 places no occupied Cells deterministically produce a valid empty Chunk.

### DR-018 clarification — catalog file forms

Generic `spk::JSON::Catalog<TElement>::load(path)` is planned to accept both aggregate `{"elements":[...]}` and direct single-element `{"id":...,"data":...}` roots through one shared element parser.

ST-001-06 will deliberately exercise both forms:

- shared `resources/voxels/shapes.json`: cube + slab;
- individual `resources/voxels/shapes/slope.json`, `stair.json`;
- shared `resources/voxels/definition.json`: cube + slab;
- individual `resources/voxels/definitions/slope.json`, `stair.json`.

## Current implementation phase

EP-001 remains Draft overall.

### Completed

- **ST-001-01 — Shared terrain coordinate conversion:** Done.
- **ST-001-02 — Packed Voxel::Cell value type:** Done.
- **ST-001-03 — Owning Voxel::Volume:** Done; selected ownership details superseded by DR-019 for ST-001-06.
- **ST-001-04 — First terrain Definition and Shape contract:** Done.
- **ST-001-05 — Voxel::Volume Message serialization:** Done; same-destination-buffer reuse detail superseded by DR-019.

### Ready / active

- **ST-001-06 — Deterministic validation terrain provider and Chunk collection foundation:** Ready / active.

### Next

The next dependency-ordered ticket remains **ST-001-06 — Deterministic validation terrain provider and Chunk collection foundation**.

OQ-039 no longer blocks it. The first ST-001-06 readiness decision is also resolved: `PrototypeChunkProvider` accepts every representable `Chunk::Coordinate`, including negative coordinates, with no coordinate-domain rejection.

The project owner also resolved the Collection replacement contract:

- the whole-Chunk replacement API is implemented in **ST-001-06** rather than deferred to ST-001-11;
- replacement is an upsert: an existing coordinate is replaced, while an absent coordinate is inserted immediately;
- replacement does not invoke the Provider.

The project owner also resolved the Provider construction/ownership contract:

- Collection keeps exclusive ownership through a private `std::unique_ptr<Provider>`;
- callers pass a concrete Provider rvalue to a constrained templated Collection constructor rather than passing a pointer;
- the concrete Provider is moved into the Collection-owned allocation;
- lvalues are rejected by constraint;
- null/absent Provider construction is not representable.

The project owner clarified the final testing boundary:

- `PrototypeChunkProvider` is temporary validation-world scaffolding and its exact DR-015 Cell layout is not a dedicated unit-test contract;
- reusable Provider/Collection behavior is tested in Core through a purpose-built test implementation of `Chunk::Collection::Provider`;
- that test Provider supplies controlled Chunk values and observable call state so caching, ownership, replacement/upsert, lifetime, negative coordinates, and concurrency can be verified precisely;
- the prototype still implements the DR-015 temporary world for later integration/visual use, but ST-001-06 does not freeze all 4096 Cells of each prototype Chunk in Server unit tests.

The ST-001-06 ticket was reviewed against `DEFINITION-OF-READY.md`: parent/owners/dependencies, public behavior, ownership, state transitions, failure semantics, concurrency, temporary fixture values, and the reusable Provider/Collection test boundary are now explicit. No material implementation decision remains for the coding agent.

**ST-001-06 is Ready and active.** Production implementation may now begin.

## Relevant approved planning constraints

EP-001 is constrained by DR-001 through DR-004, DR-007, DR-009 through DR-019 and ARCH-001 through ARCH-004 as listed in the Epic.

The canonical decision/architecture indexes remain `DECISIONS/README.md` and `ARCHITECTURE/README.md`.

## Explicit non-goal

Do not implement Client networking/provider behavior, Chunk protocol serialization, meshing/rendering, production terrain generation, or other later-ticket scope while completing ST-001-06.
