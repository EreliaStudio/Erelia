# EP-024 — Authoritative Domain Server and Persistence

**Dependencies:** EP-000, EP-004, EP-006  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Make important shared state authoritative behind command validation, with durable snapshots/transactions and a headless execution path that can first run locally then in a dedicated server process.

## Starting state

- Contracts supplied by `EP-000, EP-004, EP-006` exist at the narrowest story/API level needed by this epic.
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

- Authoritative command boundary
- Persistence snapshot schema
- Transactional mutations
- Local-to-dedicated host boundary

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
| Authoritative command boundary | [ST-024-01](tickets/ST-024-01-authoritative-command-boundary.md) | [ST-024-01](tickets/ST-024-01-authoritative-command-boundary.md) |
| Persistence snapshot schema | [ST-024-02](tickets/ST-024-02-persistence-snapshot-schema.md) | [ST-024-02](tickets/ST-024-02-persistence-snapshot-schema.md) |
| Transactional mutations | [ST-024-03](tickets/ST-024-03-transactional-mutations.md) | [ST-024-03](tickets/ST-024-03-transactional-mutations.md) |
| Local-to-dedicated host boundary | [ST-024-04](tickets/ST-024-04-local-to-dedicated-host-boundary.md) | [ST-024-04](tickets/ST-024-04-local-to-dedicated-host-boundary.md) |

## Ticket index

- [ST-024-01 — Authoritative command boundary](tickets/ST-024-01-authoritative-command-boundary.md) — Validate actor ownership/preconditions before domain mutation.
- [ST-024-02 — Persistence snapshot schema](tickets/ST-024-02-persistence-snapshot-schema.md) — Version and persist shared domain state.
- [ST-024-03 — Transactional mutations](tickets/ST-024-03-transactional-mutations.md) — Group multi-object authoritative changes atomically.
- [ST-024-04 — Local-to-dedicated host boundary](tickets/ST-024-04-local-to-dedicated-host-boundary.md) — Keep authority services runnable in-process first and separately later.

## Epic integration acceptance tests

1. Accounts/inventory/Heroes/progression/infrastructure/resources/camps/market/dungeons/encounters/rewards mutate only through authority boundary.
2. Rejected commands leave no partial persistent state.
3. Stable IDs and schema versions survive restart.
4. Snapshot/journal strategy can restore a consistent state after controlled failure fixture.
5. Server can run without rendering or external asset-authoring/import tooling.
6. Client-provided visual/material data cannot grant gameplay properties.
7. Idempotency/retry policy prevents duplicate transactional effects where applicable.
8. Authoritative state hash excludes presentation-only material animation.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-015](../../open-decisions/OD-015-persistence-backend-and-cloud-topology.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
