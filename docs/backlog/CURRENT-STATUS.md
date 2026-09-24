# Current Status

**Updated:** 24 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-05-voxel-volume-message-serialization`

## Branch state

Erelia uses `master` as its default branch.

ST-001-01 through ST-001-04 are merged into `master`. PR #11 merged ST-001-04 on 24 September 2026 at `08f30650f261619df69f129e02821f04b308daa8`.

The dedicated ST-001-05 implementation branch starts from that exact master commit.

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
- shared voxel Shape/Definition/Catalog contract and first terrain resources from ST-001-04.

## What was just resolved

ST-001-05's readiness discussion is complete.

OQ-037 and DR-017 now define the complete `Voxel::Volume` Message serialization contract:

- public API remains direct ADL-resolved `message << volume` / `message >> volume`;
- field order is native `spk::Vector3UInt dimensions`, native `Volume::UnitSize`, then one contiguous native Cell block;
- no explicit Cell-count field is serialized; count is derived from dimensions with checked arithmetic;
- Cell bytes preserve the existing Y-fastest, then X, then Z storage order;
- the wire policy deliberately follows Sparkle's native ABI/endianness/floating representation rather than adding an Erelia fixed-endian layer;
- `{0,0,0}` / `0.0f` / no Cells is the only valid empty representation;
- insertion and extraction validate the same Volume invariants and throw `spk::Exception` on invalid/malformed data;
- extraction validates derived Cell byte requirements before allocation;
- failed extraction leaves the destination Volume unchanged, while the Message cursor keeps Sparkle's normal potentially-partially-consumed behavior;
- decoded Cell storage is independently owned;
- higher-level message IDs remain deferred to protocol tickets such as ST-001-08.

## Current implementation phase

EP-001 remains Draft overall, but implementation is active.

### Completed

- **ST-001-01 — Shared terrain coordinate conversion:** Done.
- **ST-001-02 — Packed Voxel::Cell value type:** Done.
- **ST-001-03 — Owning Voxel::Volume:** Done.
- **ST-001-04 — First terrain Definition and Shape contract:** Done and merged through PR #11.

### Ready / active

- **ST-001-05 — Voxel::Volume Message serialization:** Ready. Its previous OQ-037 and decode-contract blockers are resolved on the dedicated implementation branch.

### Next

Implement and validate ST-001-05 only. After implementation, focused Core tests and the required repository CI/build validation must pass before requesting project-owner approval. Do not mark ST-001-05 Done until that approval is explicit.

After ST-001-05, reassess dependency order against the remaining unresolved OQs rather than skipping their gates.

## Relevant approved planning constraints

EP-001 is constrained by DR-001 through DR-004, DR-007, DR-009 through DR-018 and ARCH-001 through ARCH-004 as listed in the Epic.

The canonical decision/architecture indexes remain `DECISIONS/README.md` and `ARCHITECTURE/README.md`.

## Explicit non-goal

Do not materialize broad speculative game implementation while near-term architectural contracts remain unresolved.
