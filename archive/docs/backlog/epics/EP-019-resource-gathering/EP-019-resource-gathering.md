# EP-019 — Shared Resource Nodes and Gathering

**Dependencies:** EP-004, EP-018, EP-024  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Generate shared resource spawn points whose availability is authoritative server state, gate gathering by tool family/tier, and respawn nodes without modifying terrain.

## Starting state

- Contracts supplied by `EP-004, EP-018, EP-024` exist at the narrowest story/API level needed by this epic.
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

- Resource-site generation
- Gather command
- Shared depletion
- Respawn scheduling

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
| Resource-site generation | [ST-019-01](tickets/ST-019-01-resource-site-generation.md) | [ST-019-01](tickets/ST-019-01-resource-site-generation.md) |
| Gather command | [ST-019-02](tickets/ST-019-02-gather-command.md) | [ST-019-02](tickets/ST-019-02-gather-command.md) |
| Shared depletion | [ST-019-03](tickets/ST-019-03-shared-depletion.md) | [ST-019-03](tickets/ST-019-03-shared-depletion.md) |
| Respawn scheduling | [ST-019-04](tickets/ST-019-04-respawn-scheduling.md) | [ST-019-04](tickets/ST-019-04-respawn-scheduling.md) |

## Ticket index

- [ST-019-01 — Resource-site generation](tickets/ST-019-01-resource-site-generation.md) — Place deterministic valid spawn sites independent of availability state.
- [ST-019-02 — Gather command](tickets/ST-019-02-gather-command.md) — Validate tool/tier and grant yield atomically.
- [ST-019-03 — Shared depletion](tickets/ST-019-03-shared-depletion.md) — Represent server-wide depleted/available state.
- [ST-019-04 — Respawn scheduling](tickets/ST-019-04-respawn-scheduling.md) — Restore availability after configured world-clock duration.

## Epic integration acceptance tests

1. One node depletion is visible to all players sharing server state.
2. Gather validation checks required tool/tier.
3. Successful gather transfers resources atomically then marks node depleted.
4. Terrain VoxelDefinitions are unchanged by depletion.
5. Respawn timer uses authoritative world time.
6. Concurrent gather attempts cannot duplicate yield.
7. Node visual material/model may change with state without changing terrain.
8. Save/reload preserves depletion/respawn schedule.

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
