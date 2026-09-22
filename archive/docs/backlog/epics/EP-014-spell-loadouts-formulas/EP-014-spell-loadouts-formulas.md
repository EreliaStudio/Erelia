# EP-014 — Spell Items, Loadouts, Requirements, and Formulas

**Dependencies:** EP-013, EP-018  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Make spells physical tradable items, equip exactly eight per Hero, validate equipment Type/Tag requirements, and execute constrained data-driven formulas.

## Starting state

- Contracts supplied by `EP-013, EP-018` exist at the narrowest story/API level needed by this epic.
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

- Spell item schema
- Eight-slot loadout
- Equipment requirements
- Constrained formulas

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
| Spell item schema | [ST-014-01](tickets/ST-014-01-spell-item-schema.md) | [ST-014-01](tickets/ST-014-01-spell-item-schema.md) |
| Eight-slot loadout | [ST-014-02](tickets/ST-014-02-eight-slot-loadout.md) | [ST-014-02](tickets/ST-014-02-eight-slot-loadout.md) |
| Equipment requirements | [ST-014-03](tickets/ST-014-03-equipment-requirements.md) | [ST-014-03](tickets/ST-014-03-equipment-requirements.md) |
| Constrained formulas | [ST-014-04](tickets/ST-014-04-constrained-formulas.md) | [ST-014-04](tickets/ST-014-04-constrained-formulas.md) |

## Ticket index

- [ST-014-01 — Spell item schema](tickets/ST-014-01-spell-item-schema.md) — Represent spells as physical item definitions/instances.
- [ST-014-02 — Eight-slot loadout](tickets/ST-014-02-eight-slot-loadout.md) — Equip and validate exactly eight spell slots.
- [ST-014-03 — Equipment requirements](tickets/ST-014-03-equipment-requirements.md) — Evaluate Type/Tag requirements against equipped items.
- [ST-014-04 — Constrained formulas](tickets/ST-014-04-constrained-formulas.md) — Parse/validate/evaluate data-driven spell formulas.

## Epic integration acceptance tests

1. Spell item has persistent ownership and can be traded/stored.
2. Hero exposes exactly eight equipped spell slots.
3. Equipment Type/Tag requirements are validated from current loadout.
4. Tags themselves have no intrinsic effect.
5. Formula evaluator exposes only allowed variables/functions.
6. Malformed/cyclic/unsupported formula content fails validation.
7. Formula result is deterministic for same authoritative inputs.
8. Equipping/unequipping a spell does not alter voxel meshes unless the presentation asset itself explicitly changes cell geometry.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-003](../../open-decisions/OD-003-spell-instance-variable-properties.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
