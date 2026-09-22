# EP-023 — Dungeon Rooms, Entrances, Generation, and Rotation

**Dependencies:** EP-004, EP-029, EP-032  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Create per-group dungeon instances from handcrafted voxel room modules, support multiple entrances/start locations, and generate deterministic layouts for a server rotation window.

## Starting state

- Contracts supplied by `EP-004, EP-029, EP-032` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Room module contract
- Deterministic assembly
- Rotation key
- Entrances and reset

## Out of scope

- Responsibilities explicitly assigned to neighboring epics.
- Final choices tracked by unresolved Open Decisions.
- Unapproved balance values, content, serialization syntax, or technology selections.

## Architecture / behavioral overview

- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Room module contract | [ST-023-01](tickets/ST-023-01-room-module-contract.md) | [ST-023-01](tickets/ST-023-01-room-module-contract.md) |
| Deterministic assembly | [ST-023-02](tickets/ST-023-02-deterministic-assembly.md) | [ST-023-02](tickets/ST-023-02-deterministic-assembly.md) |
| Rotation key | [ST-023-03](tickets/ST-023-03-rotation-key.md) | [ST-023-03](tickets/ST-023-03-rotation-key.md) |
| Entrances and reset | [ST-023-04](tickets/ST-023-04-entrances-and-reset.md) | [ST-023-04](tickets/ST-023-04-entrances-and-reset.md) |

## Ticket index

- [ST-023-01 — Room module contract](tickets/ST-023-01-room-module-contract.md) — Define room geometry/metadata/connectors consumed by generation.
- [ST-023-02 — Deterministic assembly](tickets/ST-023-02-deterministic-assembly.md) — Assemble valid room graph/layout using WFC/similar algorithm.
- [ST-023-03 — Rotation key](tickets/ST-023-03-rotation-key.md) — Derive stable layout key for current server rotation.
- [ST-023-04 — Entrances and reset](tickets/ST-023-04-entrances-and-reset.md) — Instantiate per-group runtime from selected entrance and reset ephemeral state.

## Epic integration acceptance tests

1. Room modules load as validated voxel-room/content assets whose authoritative layout does not depend on GPU meshes.
2. Generation uses ServerSeed, RotationEpoch, RotationDuration and DungeonID deterministically.
3. Same rotation re-entry produces same structural layout.
4. Runtime encounter/loot state resets on re-entry as specified.
5. Next rotation can produce a different layout.
6. Multiple entrances map to distinct start positions in same generated instance.
7. Generation rejects incompatible room connectors rather than overlap/corrupt space.
8. Dungeon generation is headless and does not require GPU meshes/material resources.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

None.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
