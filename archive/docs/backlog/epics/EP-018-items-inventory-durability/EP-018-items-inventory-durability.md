# EP-018 — Items, Equipment, Expedition Inventory, Bank, and Durability

**Dependencies:** EP-007, EP-024  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Represent persistent physical items with ownership/location, equipment slot rules, stats/Type/Tags, fixed-size expedition inventory, global bank, and configurable durability lifecycle.

## Starting state

- Contracts supplied by `EP-007, EP-024` exist at the narrowest story/API level needed by this epic.
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

- Item identity and ownership
- Equipment slots
- Expedition inventory and bank
- Durability lifecycle

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
| Item identity and ownership | [ST-018-01](tickets/ST-018-01-item-identity-and-ownership.md) | [ST-018-01](tickets/ST-018-01-item-identity-and-ownership.md) |
| Equipment slots | [ST-018-02](tickets/ST-018-02-equipment-slots.md) | [ST-018-02](tickets/ST-018-02-equipment-slots.md) |
| Expedition inventory and bank | [ST-018-03](tickets/ST-018-03-expedition-inventory-and-bank.md) | [ST-018-03](tickets/ST-018-03-expedition-inventory-and-bank.md) |
| Durability lifecycle | [ST-018-04](tickets/ST-018-04-durability-lifecycle.md) | [ST-018-04](tickets/ST-018-04-durability-lifecycle.md) |

## Ticket index

- [ST-018-01 — Item identity and ownership](tickets/ST-018-01-item-identity-and-ownership.md) — Create persistent item instances and one-location invariant.
- [ST-018-02 — Equipment slots](tickets/ST-018-02-equipment-slots.md) — Equip/unequip with hand occupancy validation.
- [ST-018-03 — Expedition inventory and bank](tickets/ST-018-03-expedition-inventory-and-bank.md) — Move items atomically between player storage locations.
- [ST-018-04 — Durability lifecycle](tickets/ST-018-04-durability-lifecycle.md) — Damage, repair and destroy equipment using configurable rules.

## Epic integration acceptance tests

1. One item has exactly one authoritative owner/location.
2. Equipment validates Main/Off Hand, armor/accessory slots and two-handed occupancy.
3. Item has exactly one Type and zero or more Tags.
4. Fixed-size expedition inventory rejects overflow atomically.
5. Bank moves preserve item identity and ownership.
6. Durability reaching zero permanently destroys equipment per configured rule.
7. Repair changes durability according to decision-gated rules without duplicating item.
8. Visual item VoxelModel/material presentation can vary independently of the authoritative item stat instance.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-002](../../open-decisions/OD-002-equipment-stat-budget-and-generated-item-distribution.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-004](../../open-decisions/OD-004-durability-damage-repair-and-maximum-durability.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
