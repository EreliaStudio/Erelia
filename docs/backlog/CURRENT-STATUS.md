# Current Status

**Updated:** 23 September 2026
**Planning branch:** backlog/ep-001-implementation-tickets
**Active implementation branch observed:** none before ST-001-03 implementation branch creation

## Branch state

Erelia currently uses master as its default branch.

The ticket-materialization baseline remains isolated on `backlog/ep-001-implementation-tickets`, cut from master.

ST-001-01 was merged through PR #7 on 22 September 2026. ST-001-02 was merged through PR #8 into the planning branch at `f03894f76fc996d5fba3241e2e51ead848783cad` on 23 September 2026. ST-001-03 has now passed the Definition of Ready after OQ-035 was fully resolved.

No main branch is assumed.

## What exists now

The project was restarted on 22 September 2026. The active codebase is a deliberately small scaffold:

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
- current GDD and its 34 illustration assets under docs/gdd/;
- historical source and historical backlog isolated under archive/.

The original status functions remain smoke scaffolding. Core now contains the first two completed EP-001 contracts on the active implementation history: shared terrain coordinate conversion, plus the packed `Voxel::Cell` / `Voxel::Definition::ID` representation with acceptance coverage in `EreliaCoreTestSuite`. ST-001-02 is merged into the planning baseline through PR #8.

## What was just implemented

After restart commit ba32a17771b123fd3ebda2009daa3cca6fb5f8d0:

1. a08c8d0d3e8cef7a26db519a4475c10561e5e33d — base folder architecture for the new Erelia project.
2. 3a37ff2cd3b69024a8098bc9dbf6fc4d443f9f6d — tests moved under their owning layers, with CMake/CI/VS Code paths updated.
3. fa27429734a406fc4cfaea43207e0ac7f5e68b88 — current GDD and its illustration assets added to the active repository.
4. e67032414ece0c7c00018ee29db03bc1ad842cd4 — initial ST-001-01 Core terrain coordinate conversion and acceptance tests.
5. 22a599a281ea435444bacdc1912cd3125f96a817 — PR #7 merged the completed ST-001-01 implementation into the planning baseline.

## Backlog work completed on this branch

- Read the active EP-001 context, implementation conventions, Definition of Ready/Done, relevant OQs/DRs/ARCH documents, and the current Core/Server/Client scaffold/tests.
- Materialized 16 small ST-001 implementation tickets under the EP-001 `tickets/` folder.
- Ordered every ticket by explicit ST dependency.
- Applied Definition of Ready independently rather than promoting implementation-capable but underspecified work.
- Identified **ST-001-01 — Shared terrain coordinate conversion** as the first Ready implementation ticket.
- Kept OQ-035 through OQ-039 and OQ-029 through OQ-031 as blockers only for the tickets they materially affect.
- Recorded four additional Draft-ticket specification gaps rather than inventing contracts: first Definition/Shape geometry/resources; Server/Client endpoint lifecycle/configuration; deterministic render fixture/material/lifecycle; full temporary inspection input/numeric camera semantics.
- Updated the EP-001 capability coverage and ticket index.
- ST-001-01 is merged into the planning baseline and marked Done; no OQ status was changed by the merge.
- ST-001-02 is Done and merged through PR #8 after CI run #59 and explicit project-owner approval.
- OQ-035 is Resolved after the project owner fixed the complete ST-001-03 Volume contract.
- ST-001-03 passes the Definition of Ready and is the next implementation ticket.
- Future implementation tickets should continue to use dedicated feature branches cut from the current planning baseline.

## Current implementation phase

EP-001 ticket decomposition is materialized and implementation has begun.

The Epic remains Draft overall because most later contracts still depend on unresolved questions/specification gaps.

### Active implementation ticket

**ST-001-03 — Owning Voxel::Volume** is the next Ready ticket; its dedicated implementation branch has not yet been created at this planning-status commit.

**ST-001-02 — Packed Voxel::Cell value type** is **Done** on `feat/st-001-02-packed-voxel-cell`. The implementation exposes `Voxel::Definition::ID` and `Voxel::Cell::PackedType` as semantic aliases of `std::uint32_t`, keeps the Cell exactly one packed value, and places constructors/masks/getters/validation in `cell.cpp`. PR #8 CI run #59 (run ID `35789150331`) passed clang-format and the Linux/Windows headless Core/Server Debug + Release matrix, including CTest. The project owner explicitly approved the final implementation on 22 September 2026. PR #8 is merged into the planning branch.

**ST-001-01 — Shared terrain coordinate conversion** remains **Done**. Its implementation and acceptance tests were validated by the required headless CI matrix, and the project owner's merge of PR #7 on 22 September 2026 records the required human completion approval.

### Next Ready ticket

**ST-001-03 — Owning Voxel::Volume** is Ready. OQ-035 is Resolved. ST-001-04 remains Draft because its Definition/Shape contract and OQ-039 fixture details are not yet sufficiently specified.

### Existing OQ blockers

- OQ-036 — missing-neighbor/remesh behavior.
- OQ-037 — scalar wire portability and remaining decode contract.
- OQ-038 — duplicate/outstanding requests, request limits, cache/retention, partial responses/rejections/retry.
- OQ-039 — exact validation generator/Definition fixture.
- OQ-029 — golden-image platform.
- OQ-030 — image comparison metric/tolerances.
- OQ-031 — performance evidence methodology.

### Draft-only specification gaps exposed by decomposition

- minimal active-greenfield Definition/Shape geometry and resource-availability contract;
- exact EP-001 Server endpoint and Client connection lifecycle/configuration;
- deterministic render fixture/material binding plus render-resource failure/lifecycle behavior;
- complete temporary ZQSD/free-flight input map and numeric camera/movement semantics.

See EP-001 `tickets/README.md` for the full status/dependency table.

## Next

1. Implement ST-001-03 on `feat/st-001-03-owning-voxel-volume` from the current planning baseline.
2. Resolve the minimal Definition/Shape specification gap and OQ-039 before generator/mesher fixtures become Ready.
3. Resolve OQ-037 / OQ-038 before Chunk codec, Server handler, and Client cache/request coordination become Ready.
4. Resolve OQ-036 before Client boundary meshing and adjacent-Chunk integration become Ready.
5. Resolve endpoint/connection lifecycle, render-fixture/material, and inspection-control Draft gaps when those tickets approach implementation.
6. Resolve OQ-029 through OQ-031 before final visual/performance validation.

## Explicit non-goal

Do not fill this branch with a full game backlog yet. Far-future work should remain at capability/roadmap level until its architecture is understood.


## Relevant approved planning constraints

EP-001 is constrained by DR-001 through DR-004, DR-007, DR-009 through DR-017 and ARCH-001 through ARCH-004 as listed in the Epic. The canonical indexes remain `DECISIONS/README.md` and `ARCHITECTURE/README.md`.

## First Epic

EP-001 — Voxel Terrain Delivery and Visual Validation remains Draft, with 16 materialized implementation tickets, ST-001-01 / ST-001-02 Done, and ST-001-03 Ready. OQ-035 is Resolved.

It intentionally excludes production Hero movement, collision, followers, combat, resources, and production world generation. The temporary free-flight controller exists only to inspect rendered terrain.
