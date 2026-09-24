# EP-028 — Networking, Replication, Interest Management, and Client Reconciliation

**Dependencies:** EP-024  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Connect remote clients to the authoritative host, replicate only relevant dynamic state, preserve deterministic chunk generation where possible, and reconcile predicted exploration without giving clients authority.

## Starting state

- Contracts supplied by `EP-024` exist at the narrowest story/API level needed by this epic.
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

- Connection/session identity
- Replication snapshots/events
- Interest management
- Prediction and reconciliation

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
| Connection/session identity | [ST-028-01](tickets/ST-028-01-connection-session-identity.md) | [ST-028-01](tickets/ST-028-01-connection-session-identity.md) |
| Replication snapshots/events | [ST-028-02](tickets/ST-028-02-replication-snapshots-events.md) | [ST-028-02](tickets/ST-028-02-replication-snapshots-events.md) |
| Interest management | [ST-028-03](tickets/ST-028-03-interest-management.md) | [ST-028-03](tickets/ST-028-03-interest-management.md) |
| Prediction and reconciliation | [ST-028-04](tickets/ST-028-04-prediction-and-reconciliation.md) | [ST-028-04](tickets/ST-028-04-prediction-and-reconciliation.md) |

## Ticket index

- [ST-028-01 — Connection/session identity](tickets/ST-028-01-connection-session-identity.md) — Authenticate/associate client session with player authority.
- [ST-028-02 — Replication snapshots/events](tickets/ST-028-02-replication-snapshots-events.md) — Serialize relevant dynamic authoritative state.
- [ST-028-03 — Interest management](tickets/ST-028-03-interest-management.md) — Track region/encounter relevance.
- [ST-028-04 — Prediction and reconciliation](tickets/ST-028-04-prediction-and-reconciliation.md) — Reconcile continuous exploration movement under latency.

## Epic integration acceptance tests

1. Client cannot authoritatively grant items/gold/progression/combat outcomes.
2. Interest management limits replicated dynamic entities to relevant regions/encounters.
3. Immutable deterministic chunk data may be generated locally only when content/generation versions match.
4. Mismatch falls back to authoritative compatible source policy.
5. Movement prediction/reconciliation does not alter server truth.
6. Duplicate/reordered/lost message fixtures preserve transactional invariants.
7. Reconnect restores current state without duplicating entities/items.
8. VoxelModel/assembly/material asset IDs replicate as presentation references, not trusted gameplay payloads.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-016](../../open-decisions/OD-016-network-transport-and-library.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
