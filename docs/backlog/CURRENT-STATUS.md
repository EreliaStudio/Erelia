# Current Status

**Updated:** 22 September 2026
**Planning branch:** backlog/decision-gates-and-initial-epics
**Active implementation branch observed:** master
**Observed master HEAD at branch creation:** 65a5842c22bf12bd02b9046027758431e504f536

## Branch state

Erelia currently uses master as its default branch.

No main branch existed when backlog/greenfield-planning was created. The eventual merge target therefore needs an explicit decision if the repository is intended to move from master to main.

The planning branch was initially cut from master at 3a37ff2cd3b69024a8098bc9dbf6fc4d443f9f6d. When master later advanced with the GDD import, the planning branch was synchronized through fa27429734a406fc4cfaea43207e0ac7f5e68b88 without force-updating either branch.

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

- Read the complete current GDD.
- Created the greenfield backlog planning structure.
- Created a high-density PROJECT-CONTEXT.md.
- Recorded separate Definitions of Ready and Done.
- Added navigation, glossary, GDD traceability, question tracking, and reusable templates.
- Kept Architecture, Decisions, and Epics intentionally empty pending explicit design decisions.
- Did not create detailed implementation tickets.
- Did not adopt archived architecture or old backlog numbering.

## Current planning phase

Initial Epic decomposition — EP-001 voxel terrain foundation.

Q-001 through Q-008, Q-019, Q-020, Q-028, Q-032, and Q-034 have approved direction. Q-009 and Q-017/Q-018 have approved architectural direction with detailed mechanics intentionally deferred.

EP-001 now defines the first implementation milestone: deterministic basic Server terrain Chunks -> real network delivery -> Client meshing/rendering -> temporary free-flight visual inspection.

The immediate blockers are now narrow and technical: terrain coordinates/data representation, Client meshing neighbor rules, first transport/serialization choice, Chunk request/streaming semantics, exact deterministic terrain fixture, and visual/performance validation policy.

See OPEN_QUESTIONS/, DECISIONS/, and ARCHITECTURE/.

## Next

1. Resolve the remaining EP-001 voxel/network questions tracked as OQ-035 through OQ-039.
2. Resolve visual/performance validation policy OQ-029 through OQ-031 before final visual acceptance tickets become Ready.
3. Materialize the first ST-001 implementation tickets as each contract becomes sufficiently explicit.
4. Keep unrelated future architecture questions deferred until their owning Epic approaches.
5. Create detailed tickets only when they satisfy the Definition of Ready.

## Explicit non-goal

Do not fill this branch with a full game backlog yet. Far-future work should remain at capability/roadmap level until its architecture is understood.


## Decisions resolved on the current branch

- DR-001 — Long-term Core / Server / Client boundaries.
- DR-002 — Core may depend on Sparkle Core.
- DR-003 — Dedicated authoritative Server from the first playable.
- ARCH-001 — Product boundaries and authority model.


## First Epic

EP-001 — Voxel Terrain Delivery and Visual Validation is now Draft.

It intentionally excludes production Hero movement, collision, followers, combat, resources, and production world generation. The temporary free-flight controller exists only to inspect rendered terrain.
