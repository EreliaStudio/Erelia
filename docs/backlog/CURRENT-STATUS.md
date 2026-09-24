# Current Status

**Updated:** 24 September 2026
**Default baseline:** `master`
**Active ticket branch:** none

## Branch state

Erelia uses `master` as its default branch.

ST-001-01 through ST-001-05 are merged into `master`.

PR #12 merged **ST-001-05 — Voxel::Volume Message serialization** on 24 September 2026 after explicit project-owner approval.

## What exists now

The project currently includes:

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

Implemented EP-001 foundations include:

- shared terrain coordinate conversion;
- packed 32-bit `Voxel::Cell` / `Voxel::Definition::ID`;
- immutable owning `Voxel::Volume` + Builder + pooled Buffer/Lease storage;
- shared voxel Shape/Definition/Catalog contract and first terrain resources from ST-001-04;
- shared Sparkle-native `Voxel::Volume` Message serialization from ST-001-05.

## What was just completed

**ST-001-05 — Voxel::Volume Message serialization** is Done and merged.

Its delivered contract includes:

- direct ADL-resolved `message << volume` / `message >> volume`;
- `explicit Volume(const spk::Message&)` delegating to the same extraction path;
- native `spk::Vector3UInt dimensions`, native `Volume::UnitSize`, then one contiguous native Cell block;
- Cell count derived from dimensions with checked arithmetic rather than serialized redundantly;
- Y-fastest, then X, then Z Cell ordering;
- symmetric insertion/extraction validation with `spk::Exception` on invalid or malformed data;
- destination preservation on decode failure, while retaining Sparkle's normal Message cursor semantics;
- networking implementation isolated in `core/src/voxel/volume_networking.cpp`;
- networking reconstruction independent from `Volume::Builder`;
- private `Voxel::Volume::obtainCellBuffer()` ownership of pooled Cell-buffer acquisition;
- deterministic power-of-two pool classes derived from logical Cell count;
- same-class decode reuse computed from logical Cell counts, with no stored pool metadata and no reliance on `std::vector::capacity()`.

The final code-bearing head `0baf9d27698c67855c12abf021125a3575fc364d` passed CI run #289 (run ID `35986211668`) across clang-format, Linux/Windows Core+Server Debug/Release, and Windows Client Debug/Release.

## Current implementation phase

EP-001 remains Draft overall, but its first five implementation tickets are complete.

### Completed

- **ST-001-01 — Shared terrain coordinate conversion:** Done.
- **ST-001-02 — Packed Voxel::Cell value type:** Done.
- **ST-001-03 — Owning Voxel::Volume:** Done.
- **ST-001-04 — First terrain Definition and Shape contract:** Done.
- **ST-001-05 — Voxel::Volume Message serialization:** Done and merged through PR #12.

### Ready / active

None.

### Next

The next dependency-ordered ticket is **ST-001-06 — Deterministic validation terrain generator**, but it is **Blocked** by **OQ-039**.

Before production implementation of ST-001-06, resolve OQ-039's remaining material fixture decisions with the project owner, including exact authored world coordinates/regions, Definition IDs/material choices, Orientation/Flip placements, and complete deterministic expected Cell values. Update OQ-039 / DR-015 / ST-001-06 as appropriate, verify the Definition of Ready, and only then promote ST-001-06 to Ready and implement it.

Do not skip this gate by inventing a terrain fixture.

## Relevant approved planning constraints

EP-001 is constrained by DR-001 through DR-004, DR-007, DR-009 through DR-018 and ARCH-001 through ARCH-004 as listed in the Epic.

The canonical decision/architecture indexes remain `DECISIONS/README.md` and `ARCHITECTURE/README.md`.

## Explicit non-goal

Do not materialize broad speculative game implementation while near-term architectural contracts remain unresolved.
