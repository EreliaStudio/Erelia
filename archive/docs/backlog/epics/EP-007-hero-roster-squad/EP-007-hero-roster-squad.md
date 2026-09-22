# EP-007 — Hero Roster, Active Squad, and Voxel Identity

**Dependencies:** EP-002, EP-024  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Represent unlimited blank Heroes per player, exactly three active expedition Heroes, and model-definition based Hero presentation independent from class/level systems.

## Starting state

- Contracts supplied by `EP-002, EP-024` exist at the narrowest story/API level needed by this epic.
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

- Hero identity and roster
- Active squad selection
- Hero visual definition
- Party composition

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
| Hero identity and roster | [ST-007-01](tickets/ST-007-01-hero-identity-and-roster.md) | [ST-007-01](tickets/ST-007-01-hero-identity-and-roster.md) |
| Active squad selection | [ST-007-02](tickets/ST-007-02-active-squad-selection.md) | [ST-007-02](tickets/ST-007-02-active-squad-selection.md) |
| Hero visual definition | [ST-007-03](tickets/ST-007-03-hero-visual-definition.md) | [ST-007-03](tickets/ST-007-03-hero-visual-definition.md) |
| Party composition | [ST-007-04](tickets/ST-007-04-party-composition.md) | [ST-007-04](tickets/ST-007-04-party-composition.md) |

## Ticket index

- [ST-007-01 — Hero identity and roster](tickets/ST-007-01-hero-identity-and-roster.md) — Create persistent blank Hero records with stable ownership.
- [ST-007-02 — Active squad selection](tickets/ST-007-02-active-squad-selection.md) — Select and validate expedition Heroes.
- [ST-007-03 — Hero visual definition](tickets/ST-007-03-hero-visual-definition.md) — Bind Hero identity to reusable articulated VoxelModel assembly and Material palette/reference set.
- [ST-007-04 — Party composition](tickets/ST-007-04-party-composition.md) — Combine participating players’ active Heroes without transferring ownership.

## Epic integration acceptance tests

1. A player may own more than three Heroes while selecting exactly three active for an expedition when required.
2. No level/class/proficiency field is required to create a Hero.
3. Hero visual definition/palette is presentation content and does not imply combat stats.
4. Changing active squad does not duplicate/loss inventory ownership.
5. Multiplayer party composition preserves each player’s own active Heroes.
6. Stable Hero identity survives save/reload.
7. Invalid duplicate active selection is rejected.
8. Headless server can create/manage Heroes without loading GPU meshes.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-001](../../open-decisions/OD-001-exact-hero-combat-statistic-set-and-defaults.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

Graphical tickets use deterministic `512 × 512` Sparkle TestLibrary comparisons. Structural refactors preserve reviewed baselines. Intentional visual migrations retain old references until actual/difference images receive human approval, then create a versioned baseline.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
