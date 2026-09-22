# Current Status

**Updated:** 22 September 2026
**Planning branch:** backlog/greenfield-planning
**Active implementation branch observed:** master
**Observed master HEAD:** 3a37ff2cd3b69024a8098bc9dbf6fc4d443f9f6d

## Branch state

Erelia currently uses master as its default branch.

No main branch existed when backlog/greenfield-planning was created. The eventual merge target therefore needs an explicit decision if the repository is intended to move from master to main.

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
- historical source and historical backlog isolated under archive/.

The current status implementations/tests are smoke scaffolding only. They verify project wiring, linking, and test discovery rather than established gameplay architecture.

## What was just implemented

After restart commit ba32a17771b123fd3ebda2009daa3cca6fb5f8d0:

1. a08c8d0d3e8cef7a26db519a4475c10561e5e33d — base folder architecture for the new Erelia project.
2. 3a37ff2cd3b69024a8098bc9dbf6fc4d443f9f6d — tests moved under their owning layers, with CMake/CI/VS Code paths updated.

## Current planning phase

Architecture discovery.

The GDD is detailed enough to identify gameplay capabilities, but major implementation contracts remain unresolved: ownership, identity, persistence, semantic client/server commands, time and determinism, module boundaries, concurrency, serialization, content schemas, and visual-testing infrastructure.

## Next

1. Resolve the highest-impact architecture questions with the user.
2. Record real choices in DECISIONS/.
3. Create architecture documents only for approved durable cross-cutting contracts.
4. Propose high-level system boundaries and roadmap.
5. Challenge/correct the decomposition with the user.
6. Create the first Epic only when boundaries are sufficiently understood.
7. Create detailed tickets only when they satisfy the Definition of Ready.

## Explicit non-goal

Do not fill this branch with a full game backlog yet. Far-future work should remain at capability/roadmap level until its architecture is understood.
