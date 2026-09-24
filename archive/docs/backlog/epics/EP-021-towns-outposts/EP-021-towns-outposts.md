# EP-021 — Permanent Towns, Building Progression, Outposts, and Respawn Services

**Dependencies:** EP-004, EP-019, EP-020, EP-024  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Implement shared permanent infrastructure with independent building upgrades and predefined public outposts that consume upkeep and expose configured services.

## Starting state

- Contracts supplied by `EP-004, EP-019, EP-020, EP-024` exist at the narrowest story/API level needed by this epic.
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

- Independent building state without Town Level
- Building-gated exposure of EP-020 services
- Outpost lifecycle and upkeep
- Concurrent contributions and respawn service

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
| Independent building state without Town Level | [ST-021-01](tickets/ST-021-01-town-building-state.md) | [ST-021-01](tickets/ST-021-01-town-building-state.md) |
| Building-gated exposure of EP-020 services | [ST-021-02](tickets/ST-021-02-building-services.md) | [ST-021-02](tickets/ST-021-02-building-services.md) |
| Outpost lifecycle and upkeep | [ST-021-03](tickets/ST-021-03-outpost-lifecycle.md) | [ST-021-03](tickets/ST-021-03-outpost-lifecycle.md) |
| Concurrent contributions and respawn service | [ST-021-04](tickets/ST-021-04-respawn-services.md) | [ST-021-04](tickets/ST-021-04-respawn-services.md) |

## Ticket index

- [ST-021-01 — Town/building state](tickets/ST-021-01-town-building-state.md) — Represent permanent towns and independent building upgrades.
- [ST-021-02 — Building services](tickets/ST-021-02-building-services.md) — Expose crafting/refining/enchantment/bank/etc. from configuration.
- [ST-021-03 — Outpost lifecycle](tickets/ST-021-03-outpost-lifecycle.md) — Activate, supply and deactivate temporary public outposts.
- [ST-021-04 — Respawn services](tickets/ST-021-04-respawn-services.md) — Select/use valid persistent Respawn Points.

## Epic integration acceptance tests

1. Town buildings upgrade independently with no Town Level aggregate.
2. Service availability derives from building state.
3. Outposts activate only at predefined valid crystal sites.
4. Outpost upkeep consumes configured resources and deactivates at zero.
5. Public outpost service state is shared server-wide.
6. Respawn Point selection is persistent and valid.
7. Infrastructure visual models can change by state/upgrade without modifying rules unexpectedly.
8. Concurrent contributions/upgrades are transactionally safe.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-007](../../open-decisions/OD-007-building-catalog-costs-and-service-mapping.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
