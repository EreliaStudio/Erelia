# Current Status

**Updated:** 22 September 2026
**Planning branch:** backlog/ep-001-implementation-tickets
**Active implementation branch observed:** master

## Branch state

Erelia currently uses master as its default branch.

The active ticket-materialization work is isolated on `backlog/ep-001-implementation-tickets`, cut from master. This branch contains backlog/documentation changes only; no production source code is part of the planning task.

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

The current status implementations/tests are smoke scaffolding only. They verify project wiring, linking, and test discovery rather than established gameplay architecture.

## What was just implemented

After restart commit ba32a17771b123fd3ebda2009daa3cca6fb5f8d0:

1. a08c8d0d3e8cef7a26db519a4475c10561e5e33d — base folder architecture for the new Erelia project.
2. 3a37ff2cd3b69024a8098bc9dbf6fc4d443f9f6d — tests moved under their owning layers, with CMake/CI/VS Code paths updated.
3. fa27429734a406fc4cfaea43207e0ac7f5e68b88 — current GDD and its illustration assets added to the active repository.

## Backlog work completed on this branch

- Read the active EP-001 context, implementation conventions, Definition of Ready/Done, relevant OQs/DRs/ARCH documents, and the current Core/Server/Client scaffold/tests.
- Materialized 16 small ST-001 implementation tickets under the EP-001 `tickets/` folder.
- Ordered every ticket by explicit ST dependency.
- Applied Definition of Ready independently rather than promoting implementation-capable but underspecified work.
- Identified **ST-001-01 — Shared terrain coordinate conversion** as the first Ready implementation ticket.
- Kept OQ-035 through OQ-039 and OQ-029 through OQ-031 as blockers only for the tickets they materially affect.
- Recorded four additional Draft-ticket specification gaps rather than inventing contracts: first Definition/Shape geometry/resources; Server/Client endpoint lifecycle/configuration; deterministic render fixture/material/lifecycle; full temporary inspection input/numeric camera semantics.
- Updated the EP-001 capability coverage and ticket index.
- Did not modify any production source file or OQ status.

## Current planning phase

EP-001 ticket decomposition is materialized.

The Epic remains Draft overall because most later contracts still depend on unresolved questions/specification gaps, but implementation can begin with the independent coordinate foundation.

### First Ready implementation ticket

**ST-001-01 — Shared terrain coordinate conversion**

It has no ST prerequisite and is fully constrained by DR-011: 16×16×16 Chunks, one world unit per terrain cell, `spk::Vector3Int` coordinates, mathematical floor division/modulo, exact positive/negative fixtures, and reconstruction/local-range invariants.

### Existing OQ blockers

- OQ-035 — Cell/Volume canonical empty, storage/indexing, validation/editor/lifetime details.
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

1. Implement **ST-001-01 — Shared terrain coordinate conversion**.
2. Resolve OQ-035 before promoting ST-001-02 / ST-001-03.
3. Resolve the minimal Definition/Shape specification gap and OQ-039 before generator/mesher fixtures become Ready.
4. Resolve OQ-037 / OQ-038 before Chunk codec, Server handler, and Client cache/request coordination become Ready.
5. Resolve OQ-036 before Client boundary meshing and adjacent-Chunk integration become Ready.
6. Resolve endpoint/connection lifecycle, render-fixture/material, and inspection-control Draft gaps when those tickets approach implementation.
7. Resolve OQ-029 through OQ-031 before final visual/performance validation.

## Explicit non-goal

Do not fill this branch with a full game backlog yet. Far-future work should remain at capability/roadmap level until its architecture is understood.


## Relevant approved planning constraints

EP-001 is constrained by DR-001 through DR-004, DR-007, DR-009 through DR-017 and ARCH-001 through ARCH-004 as listed in the Epic. The canonical indexes remain `DECISIONS/README.md` and `ARCHITECTURE/README.md`.

## First Epic

EP-001 — Voxel Terrain Delivery and Visual Validation remains Draft, with 16 materialized implementation tickets and one currently Ready ticket (ST-001-01).

It intentionally excludes production Hero movement, collision, followers, combat, resources, and production world generation. The temporary free-flight controller exists only to inspect rendered terrain.
