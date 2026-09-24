# EP-010 — Encounter Creation, Handoff, Surprise, and Reinforcements

**Dependencies:** EP-006, EP-009  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Create tactical encounters from live world state, synchronize initial participants, determine Surprise, and admit later entities as zero-readiness reinforcements.

## Starting state

- Contracts supplied by `EP-006, EP-009` exist at the narrowest story/API level needed by this epic.
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

- Encounter trigger and participant lock
- World-to-encounter handoff
- Surprise initialization
- Reinforcement admission

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
| Encounter trigger and participant lock | [ST-010-01](tickets/ST-010-01-encounter-trigger-and-participant-lock.md) | [ST-010-01](tickets/ST-010-01-encounter-trigger-and-participant-lock.md) |
| World-to-encounter handoff | [ST-010-02](tickets/ST-010-02-world-to-encounter-handoff.md) | [ST-010-02](tickets/ST-010-02-world-to-encounter-handoff.md) |
| Surprise initialization | [ST-010-03](tickets/ST-010-03-surprise-initialization.md) | [ST-010-03](tickets/ST-010-03-surprise-initialization.md) |
| Reinforcement admission | [ST-010-04](tickets/ST-010-04-reinforcement-admission.md) | [ST-010-04](tickets/ST-010-04-reinforcement-admission.md) |

## Ticket index

- [ST-010-01 — Encounter trigger and participant lock](tickets/ST-010-01-encounter-trigger-and-participant-lock.md) — Determine initial participants and create encounter identity.
- [ST-010-02 — World-to-encounter handoff](tickets/ST-010-02-world-to-encounter-handoff.md) — Complete/cancel world actions and transfer time-domain ownership.
- [ST-010-03 — Surprise initialization](tickets/ST-010-03-surprise-initialization.md) — Initialize readiness based on awareness relationship.
- [ST-010-04 — Reinforcement admission](tickets/ST-010-04-reinforcement-admission.md) — Admit later valid participants without restarting the encounter.

## Epic integration acceptance tests

1. Initial participants are locked exactly once during handoff.
2. Uncommitted targeting is cancelled while committed actions finish.
3. Final world positions are snapshotted before combat cells are derived.
4. Each participant leaves Region Clock ownership and enters exactly one Encounter Clock atomically.
5. Unaware defender state grants the documented surprise readiness to the attacking side.
6. Later arrivals do not restart initial synchronization.
7. Reinforcement readiness begins at zero/fresh TurnInterval.
8. Failed handoff cannot strand an entity between world and encounter ownership.

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
