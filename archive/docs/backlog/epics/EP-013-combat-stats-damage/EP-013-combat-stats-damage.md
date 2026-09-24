# EP-013 — Combat Stats, Damage, Criticals, and Effects Vocabulary

**Dependencies:** EP-000  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Provide bounded arithmetic and effect primitives shared by Heroes, enemies, spells, statuses, and summons.

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

- Combat statistic set
- Damage pipeline
- Critical resolution
- Effect vocabulary

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
| Combat statistic set | [ST-013-01](tickets/ST-013-01-combat-statistic-set.md) | [ST-013-01](tickets/ST-013-01-combat-statistic-set.md) |
| Damage pipeline | [ST-013-02](tickets/ST-013-02-damage-pipeline.md) | [ST-013-02](tickets/ST-013-02-damage-pipeline.md) |
| Critical resolution | [ST-013-03](tickets/ST-013-03-critical-resolution.md) | [ST-013-03](tickets/ST-013-03-critical-resolution.md) |
| Effect vocabulary | [ST-013-04](tickets/ST-013-04-effect-vocabulary.md) | [ST-013-04](tickets/ST-013-04-effect-vocabulary.md) |

## Ticket index

- [ST-013-01 — Combat statistic set](tickets/ST-013-01-combat-statistic-set.md) — Define configurable authoritative statistic container.
- [ST-013-02 — Damage pipeline](tickets/ST-013-02-damage-pipeline.md) — Implement ordered damage computation.
- [ST-013-03 — Critical resolution](tickets/ST-013-03-critical-resolution.md) — Inject deterministic RNG and critical modifiers.
- [ST-013-04 — Effect vocabulary](tickets/ST-013-04-effect-vocabulary.md) — Define bounded reusable gameplay effect operations.

## Epic integration acceptance tests

1. Configured stat values serialize deterministically.
2. Damage pipeline applies formula -> raw -> crit -> defense -> final in documented order.
3. Defense uses selected diminishing-return formula once decision gate is resolved.
4. Crit behavior is deterministic under injected RNG.
5. No implicit Mana resource exists in baseline.
6. Effect vocabulary rejects unknown operations in content.
7. Boundary/overflow inputs fail or clamp only according to explicit contract.
8. Summons/enemies can use same statistic primitives as Heroes.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-001](../../open-decisions/OD-001-exact-hero-combat-statistic-set-and-defaults.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-005](../../open-decisions/OD-005-defense-and-critical-formulas.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
