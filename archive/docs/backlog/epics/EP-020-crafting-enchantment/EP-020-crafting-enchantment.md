# EP-020 — Refining, Crafting, and Enchantment

**Dependencies:** EP-018, EP-019  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Convert resources into refined materials and player-made finished items, then apply deterministic enchantment transformations with authoritative RNG success and explicit costs.

## Starting state

- Contracts supplied by `EP-018, EP-019` exist at the narrowest story/API level needed by this epic.
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

- Headless refining, crafting, and enchantment domain operations
- Atomic inputs, outputs, and configured failure
- Deterministic authoritative RNG
- No structural dependency on town/building entities

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
| Headless refining, crafting, and enchantment domain operations | [ST-020-01](tickets/ST-020-01-refining-recipes.md) | [ST-020-01](tickets/ST-020-01-refining-recipes.md) |
| Atomic inputs, outputs, and configured failure | [ST-020-02](tickets/ST-020-02-crafting-recipes-and-stat-rolls.md) | [ST-020-02](tickets/ST-020-02-crafting-recipes-and-stat-rolls.md) |
| Deterministic authoritative RNG | [ST-020-03](tickets/ST-020-03-enchantment-operations.md) | [ST-020-03](tickets/ST-020-03-enchantment-operations.md) |
| No structural dependency on town/building entities | [ST-020-04](tickets/ST-020-04-enchantment-resolution.md) | [ST-020-04](tickets/ST-020-04-enchantment-resolution.md) |

## Ticket index

- [ST-020-01 — Refining recipes](tickets/ST-020-01-refining-recipes.md) — Convert raw to refined resources.
- [ST-020-02 — Crafting recipes and stat rolls](tickets/ST-020-02-crafting-recipes-and-stat-rolls.md) — Create finished item instances from recipes and configured ranges.
- [ST-020-03 — Enchantment operations](tickets/ST-020-03-enchantment-operations.md) — Define deterministic tag/stat/type transformations.
- [ST-020-04 — Enchantment resolution](tickets/ST-020-04-enchantment-resolution.md) — Consume costs and resolve success/failure atomically.

## Epic integration acceptance tests

1. Refining consumes recipe inputs atomically and creates configured outputs.
2. Higher tiers can require increasing raw material quantities without profession levels.
3. Crafting rolls configured stat ranges through authoritative deterministic RNG source.
4. Finished persistent equipment/spells originate from crafting/trading path, not ordinary monster generation.
5. Enchantment intended result is deterministic for selected operation.
6. Failure consumes configured cost and leaves target item unchanged in base rule.
7. Tag add/remove/replace enforces tradeoffs/type changes explicitly.
8. Crafted item visual/material variant can be selected without remeshing geometry when its voxel cells are unchanged.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-002](../../open-decisions/OD-002-equipment-stat-budget-and-generated-item-distribution.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
