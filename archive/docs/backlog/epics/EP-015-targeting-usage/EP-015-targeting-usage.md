# EP-015 — Spell Targeting, LoS, AoE, and Usage Restrictions

**Dependencies:** EP-011, EP-012, EP-014  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Resolve valid tactical targets using 3D combat cells, range rules, line of sight, AoE patterns, AP cost, cooldown, and usage limits.

## Starting state

- Contracts supplied by `EP-011, EP-012, EP-014` exist at the narrowest story/API level needed by this epic.
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

- Range and target categories
- Line of sight
- AoE patterns
- Usage restrictions

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
| Range and target categories | [ST-015-01](tickets/ST-015-01-range-and-target-categories.md) | [ST-015-01](tickets/ST-015-01-range-and-target-categories.md) |
| Line of sight | [ST-015-02](tickets/ST-015-02-line-of-sight.md) | [ST-015-02](tickets/ST-015-02-line-of-sight.md) |
| AoE patterns | [ST-015-03](tickets/ST-015-03-aoe-patterns.md) | [ST-015-03](tickets/ST-015-03-aoe-patterns.md) |
| Usage restrictions | [ST-015-04](tickets/ST-015-04-usage-restrictions.md) | [ST-015-04](tickets/ST-015-04-usage-restrictions.md) |

## Ticket index

- [ST-015-01 — Range and target categories](tickets/ST-015-01-range-and-target-categories.md) — Validate selectable target cells/entities.
- [ST-015-02 — Line of sight](tickets/ST-015-02-line-of-sight.md) — Resolve obstruction in 3D cell space.
- [ST-015-03 — AoE patterns](tickets/ST-015-03-aoe-patterns.md) — Generate affected cells/targets deterministically.
- [ST-015-04 — Usage restrictions](tickets/ST-015-04-usage-restrictions.md) — Track AP, cooldown and use limits atomically.

## Epic integration acceptance tests

1. Min/max range boundaries are exact.
2. Fixed-range spells ignore only the modifiers they explicitly forbid.
3. LoS uses authoritative combat/world geometry contract, not rendered pixels.
4. Linear/diagonal restrictions accept/reject expected cell vectors.
5. AoE resolves deterministic target cell sets.
6. AP/cooldown/uses-per-turn/target/encounter are all enforced before mutation.
7. Invalid target command spends no AP and consumes no usage counter.
8. Target preview and authoritative resolution share rules but preview has no authority.

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
