# EP-016 — Statuses, Combat Hooks, Temporary Entities, and Summons

**Dependencies:** EP-013, EP-015  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Provide event-driven statuses and encounter-owned temporary entities/summons that reuse normal combat primitives without modifying immutable terrain.

## Starting state

- Contracts supplied by `EP-013, EP-015` exist at the narrowest story/API level needed by this epic.
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

- Status lifecycle
- Combat event hooks
- Temporary entities
- Summons

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
| Status lifecycle | [ST-016-01](tickets/ST-016-01-status-lifecycle.md) | [ST-016-01](tickets/ST-016-01-status-lifecycle.md) |
| Combat event hooks | [ST-016-02](tickets/ST-016-02-combat-event-hooks.md) | [ST-016-02](tickets/ST-016-02-combat-event-hooks.md) |
| Temporary entities | [ST-016-03](tickets/ST-016-03-temporary-entities.md) | [ST-016-03](tickets/ST-016-03-temporary-entities.md) |
| Summons | [ST-016-04](tickets/ST-016-04-summons.md) | [ST-016-04](tickets/ST-016-04-summons.md) |

## Ticket index

- [ST-016-01 — Status lifecycle](tickets/ST-016-01-status-lifecycle.md) — Apply, refresh/remove and serialize statuses.
- [ST-016-02 — Combat event hooks](tickets/ST-016-02-combat-event-hooks.md) — Dispatch bounded status triggers deterministically.
- [ST-016-03 — Temporary entities](tickets/ST-016-03-temporary-entities.md) — Create encounter-owned walls/zones/traps/totems/portals/turrets.
- [ST-016-04 — Summons](tickets/ST-016-04-summons.md) — Create temporary CombatUnits through shared combat pipeline.

## Epic integration acceptance tests

1. Status hooks run only on subscribed authoritative events.
2. EverySecond uses Encounter Clock semantics.
3. Temporary walls/zones/traps do not mutate persistent terrain cells.
4. All encounter-owned temporary entities are cleaned up at encounter end.
5. Summons use normal CombatUnit readiness/stats/targeting pipeline.
6. Removing a status stops future hooks without leaving callbacks registered.
7. Hook ordering is deterministic/documented.
8. Presentation-only model/material effects can visualize statuses without becoming the status truth.

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
