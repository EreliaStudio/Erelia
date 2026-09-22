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
- Created high-density PROJECT-CONTEXT.md and IMPLEMENTATION-CONTEXT.md companion notes.
- Recorded separate Definitions of Ready and Done.
- Added navigation, glossary, GDD traceability, one-file-per-question OQ tracking, reusable planning templates, IMPLEMENTATION-CONTEXT.md, and AI prompts for ticket planning / next-ticket implementation / specific-ticket implementation.
- Kept Architecture, Decisions, and Epics intentionally empty pending explicit design decisions.
- Did not create detailed implementation tickets.
- Did not adopt archived architecture or old backlog numbering.

## Current planning phase

Initial Epic decomposition — EP-001 voxel terrain foundation.

OQ-001 through OQ-008, OQ-019, OQ-020, OQ-021, OQ-028, OQ-032, OQ-034, and OQ-040 are resolved. OQ-009 and OQ-017/OQ-018 have approved architectural direction with detailed mechanics intentionally deferred. OQ-035 through OQ-039 are partially resolved and contain the remaining EP-001 implementation details.

EP-001 now defines the first implementation milestone: deterministic basic Server terrain Chunks -> real network delivery -> Client meshing/rendering -> temporary free-flight visual inspection.

The remaining EP-001 decisions are narrow and technical: final Cell/Volume details, Client meshing neighbor rules, scalar wire portability, Chunk request/cache semantics, exact deterministic terrain fixtures, and visual/performance validation policy. These do not necessarily block every early foundational ticket; each ticket must apply the Definition of Ready independently.

See OPEN_QUESTIONS/, DECISIONS/, and ARCHITECTURE/.

## Next

1. Decompose EP-001 into small ST-001 implementation tickets using the approved contracts and the small-ticket rules in IMPLEMENTATION-CONTEXT.md.
2. Mark only independently specified tickets Ready; keep tickets affected by OQ-035 through OQ-039 Draft/Blocked until their exact contract is resolved.
3. Resolve visual/performance validation policy OQ-029 through OQ-031 before the corresponding visual acceptance tickets become Ready.
4. Make the first dependency-satisfied Ready ticket explicit in this file once tickets are materialized.
5. Keep unrelated future architecture questions deferred until their owning Epic approaches.

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
