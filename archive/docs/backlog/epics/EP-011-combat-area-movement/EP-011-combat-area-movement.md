# EP-011 — Combat Cells, Elastic Area, and Tactical Movement

**Dependencies:** EP-005, EP-010  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Build cell-based tactical movement directly from current voxel traversal surfaces and maintain the elastic combat area around participants.

## Starting state

- Contracts supplied by `EP-005, EP-010` exist at the narrowest story/API level needed by this epic.
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

- Combat-cell extraction
- Elastic area membership
- Tactical pathing
- Movement execution

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
| Combat-cell extraction | [ST-011-01](tickets/ST-011-01-combat-cell-extraction.md) | [ST-011-01](tickets/ST-011-01-combat-cell-extraction.md) |
| Elastic area membership | [ST-011-02](tickets/ST-011-02-elastic-area-membership.md) | [ST-011-02](tickets/ST-011-02-elastic-area-membership.md) |
| Tactical pathing | [ST-011-03](tickets/ST-011-03-tactical-pathing.md) | [ST-011-03](tickets/ST-011-03-tactical-pathing.md) |
| Movement execution | [ST-011-04](tickets/ST-011-04-movement-execution.md) | [ST-011-04](tickets/ST-011-04-movement-execution.md) |

## Ticket index

- [ST-011-01 — Combat-cell extraction](tickets/ST-011-01-combat-cell-extraction.md) — Convert traversal surfaces into encounter cells.
- [ST-011-02 — Elastic area membership](tickets/ST-011-02-elastic-area-membership.md) — Maintain combat area around participants.
- [ST-011-03 — Tactical pathing](tickets/ST-011-03-tactical-pathing.md) — Resolve MP-cost movement through valid combat cells.
- [ST-011-04 — Movement execution](tickets/ST-011-04-movement-execution.md) — Commit movement and events through Encounter Clock rules.

## Epic integration acceptance tests

1. Combat cell positions correspond to valid traversable voxel surfaces.
2. Cells retain 1x1 horizontal gameplay footprint regardless of visual VoxelModel resolution/voxel scale.
3. Elastic area equals union of configured radius around combatants.
4. Adding/removing/moving combatants updates area deterministically.
5. Movement cannot target invalid/unreachable cells.
6. Cell graph respects slope/slab traversal metadata.
7. World base terrain remains immutable during combat.
8. Rendering mesh detail does not create extra tactical cells.

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
