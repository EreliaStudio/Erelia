# EP-012 — Readiness, Turns, AP/MP, and Flee

**Dependencies:** EP-006, EP-010, EP-011  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Implement TurnInterval-based readiness, turn resources, decision pauses, turn completion, per-Hero fleeing, defeat recovery, and return to world control.

## Starting state

- Contracts supplied by `EP-006, EP-010, EP-011` exist at the narrowest story/API level needed by this epic.
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

- Readiness accumulation
- Turn resources
- Per-Hero flee
- Defeat and encounter exit

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
| Readiness accumulation | [ST-012-01](tickets/ST-012-01-readiness-accumulation.md) | [ST-012-01](tickets/ST-012-01-readiness-accumulation.md) |
| Turn resources | [ST-012-02](tickets/ST-012-02-turn-resources.md) | [ST-012-02](tickets/ST-012-02-turn-resources.md) |
| Per-Hero flee | [ST-012-03](tickets/ST-012-03-per-hero-flee.md) | [ST-012-03](tickets/ST-012-03-per-hero-flee.md) |
| Defeat and encounter exit | [ST-012-04](tickets/ST-012-04-defeat-and-encounter-exit.md) | [ST-012-04](tickets/ST-012-04-defeat-and-encounter-exit.md) |

## Ticket index

- [ST-012-01 — Readiness accumulation](tickets/ST-012-01-readiness-accumulation.md) — Advance readiness from Encounter Clock and TurnInterval.
- [ST-012-02 — Turn resources](tickets/ST-012-02-turn-resources.md) — Initialize/spend/end AP and MP.
- [ST-012-03 — Per-Hero flee](tickets/ST-012-03-per-hero-flee.md) — Resolve explicit flee attempts and outside-wait state.
- [ST-012-04 — Defeat and encounter exit](tickets/ST-012-04-defeat-and-encounter-exit.md) — Resolve defeated participants and world return.

## Epic integration acceptance tests

1. Lower TurnInterval reaches readiness sooner under same elapsed encounter time.
2. Only the deciding encounter pauses; Region Clock/other encounters continue.
3. AP and MP are refreshed/consumed according to configured rules.
4. A Hero can flee independently while owner remains in fight with other Heroes.
5. Owner fully detaches only when all owned participants have fled/been defeated.
6. Re-entry follows reinforcement rules.
7. Defeated Heroes recover at 1 HP after battle per baseline.
8. Full team defeat returns to last Respawn Point without default currency/durability/XP penalty.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-006](../../open-decisions/OD-006-elastic-combat-area-and-flee-tuning.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
