# EP-022 — Worlds, Portals, Frontier Progression, and Enemy Camps

**Dependencies:** EP-004, EP-021, EP-024  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Represent multiple deterministic Worlds per server, permanent server-wide next-World portal unlocks, sparse civilization, tiered content, and rotating camps at predefined valid sites.

## Starting state

- Contracts supplied by `EP-004, EP-021, EP-024` exist at the narrowest story/API level needed by this epic.
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

- World catalog and seeds
- Portal progression state
- Camp site catalog
- Camp rotation lifecycle

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
| World catalog and seeds | [ST-022-01](tickets/ST-022-01-world-catalog-and-seeds.md) | [ST-022-01](tickets/ST-022-01-world-catalog-and-seeds.md) |
| Portal progression state | [ST-022-02](tickets/ST-022-02-portal-progression-state.md) | [ST-022-02](tickets/ST-022-02-portal-progression-state.md) |
| Camp site catalog | [ST-022-03](tickets/ST-022-03-camp-site-catalog.md) | [ST-022-03](tickets/ST-022-03-camp-site-catalog.md) |
| Camp rotation lifecycle | [ST-022-04](tickets/ST-022-04-camp-rotation-lifecycle.md) | [ST-022-04](tickets/ST-022-04-camp-rotation-lifecycle.md) |

## Ticket index

- [ST-022-01 — World catalog and seeds](tickets/ST-022-01-world-catalog-and-seeds.md) — Represent multiple World identities and generation parameters.
- [ST-022-02 — Portal progression state](tickets/ST-022-02-portal-progression-state.md) — Persist server-wide unlocked transitions.
- [ST-022-03 — Camp site catalog](tickets/ST-022-03-camp-site-catalog.md) — Generate/store valid potential camp locations.
- [ST-022-04 — Camp rotation lifecycle](tickets/ST-022-04-camp-rotation-lifecycle.md) — Activate, clear, reward and replace overworld camps.

## Epic integration acceptance tests

1. WorldSeed derives from ServerSeed + WorldID.
2. First Grand Dungeon progression event permanently activates next-World portal exactly once.
3. Later completions remain repeatable without duplicating progression mutation.
4. Camp sites are predefined/generated valid locations separate from active selection.
5. Clearing a camp rewards once then deactivates it.
6. Replacement camp activation follows configured deterministic/server policy.
7. World progression survives restart.
8. Portal/camp visual models do not define authoritative activation state.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-008](../../open-decisions/OD-008-world-count-cadence-and-content-progression.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
