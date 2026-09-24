# EP-027 — Telemetry, Profiling, and Operational Observability

**Dependencies:** EP-000  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Emit authoritative gameplay telemetry and technical performance measurements without coupling simulation correctness to analytics availability.

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

- Gameplay event envelope
- Domain event families
- Technical profiling
- Telemetry sink boundary

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
| Gameplay event envelope | [ST-027-01](tickets/ST-027-01-gameplay-event-envelope.md) | [ST-027-01](tickets/ST-027-01-gameplay-event-envelope.md) |
| Domain event families | [ST-027-02](tickets/ST-027-02-domain-event-families.md) | [ST-027-02](tickets/ST-027-02-domain-event-families.md) |
| Technical profiling | [ST-027-03](tickets/ST-027-03-technical-profiling.md) | [ST-027-03](tickets/ST-027-03-technical-profiling.md) |
| Telemetry sink boundary | [ST-027-04](tickets/ST-027-04-telemetry-sink-boundary.md) | [ST-027-04](tickets/ST-027-04-telemetry-sink-boundary.md) |

## Ticket index

- [ST-027-01 — Gameplay event envelope](tickets/ST-027-01-gameplay-event-envelope.md) — Define versioned authoritative event metadata.
- [ST-027-02 — Domain event families](tickets/ST-027-02-domain-event-families.md) — Emit combat/economy/dungeon/world/etc. telemetry.
- [ST-027-03 — Technical profiling](tickets/ST-027-03-technical-profiling.md) — Instrument CPU/GPU/content pipeline/runtime hotspots.
- [ST-027-04 — Telemetry sink boundary](tickets/ST-027-04-telemetry-sink-boundary.md) — Buffer/export events without coupling to gameplay success.

## Epic integration acceptance tests

1. Gameplay events originate from authoritative accepted outcomes.
2. Telemetry failure never rolls back otherwise valid gameplay unless explicitly configured.
3. Events use stable versioned schemas/IDs.
4. PII/secrets are not placed in generic event payloads.
5. Profiler can measure model compile/load/chunk bake/render preparation separately.
6. Dropped/backpressured analytics follows documented policy.
7. Offline/local runs can disable telemetry.
8. Event ordering/time domain is explicit enough for later analysis.

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
