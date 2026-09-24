# EP-006 — World and Encounter Simulation Clocks

**Dependencies:** EP-000  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Provide explicit deterministic time domains: a continuously advancing Region Clock plus independent Encounter Clocks that can pause for tactical decisions without stopping the world or other encounters.

## Starting state

- Contracts supplied by `EP-000` exist at the narrowest story/API level needed by this epic.
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

- Deterministic Region Clock
- Independent Encounter Clock
- Time-domain ownership
- Clock snapshot/replay

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
| Deterministic Region Clock | [ST-006-01](tickets/ST-006-01-deterministic-region-clock.md) | [ST-006-01](tickets/ST-006-01-deterministic-region-clock.md) |
| Independent Encounter Clock | [ST-006-02](tickets/ST-006-02-independent-encounter-clock.md) | [ST-006-02](tickets/ST-006-02-independent-encounter-clock.md) |
| Time-domain ownership | [ST-006-03](tickets/ST-006-03-time-domain-ownership.md) | [ST-006-03](tickets/ST-006-03-time-domain-ownership.md) |
| Clock snapshot/replay | [ST-006-04](tickets/ST-006-04-clock-snapshot-replay.md) | [ST-006-04](tickets/ST-006-04-clock-snapshot-replay.md) |

## Ticket index

- [ST-006-01 — Deterministic Region Clock](tickets/ST-006-01-deterministic-region-clock.md) — Implement explicit fixed-step Region Clock advancement.
- [ST-006-02 — Independent Encounter Clock](tickets/ST-006-02-independent-encounter-clock.md) — Create encounter-owned clocks with pause/resume.
- [ST-006-03 — Time-domain ownership](tickets/ST-006-03-time-domain-ownership.md) — Track exactly one simulation clock owner per entity.
- [ST-006-04 — Clock snapshot/replay](tickets/ST-006-04-clock-snapshot-replay.md) — Serialize and replay clock state deterministically.

## Epic integration acceptance tests

1. Region Clock continues while one Encounter Clock is paused.
2. Two encounters advance independently.
3. Explicit fixed-step advancement is deterministic.
4. Entities belong to exactly one active time domain.
5. Transfer between time domains is atomic.
6. Clock serialization/reload preserves semantic scheduling.
7. Long-step and equivalent repeated short-step fixtures produce documented equivalent behavior where required.
8. Presentation frame rate cannot change authoritative clock advancement.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-014](../../open-decisions/OD-014-fixed-simulation-tick-rates.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
