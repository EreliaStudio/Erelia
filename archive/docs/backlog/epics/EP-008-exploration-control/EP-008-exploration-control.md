# EP-008 — Third-Person Movement and Camera

**Dependencies:** EP-004, EP-005, EP-007  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Deliver continuous third-person control for the selected Hero using character-controller collision against voxel terrain, with camera transitions to tactical targeting views.

## Starting state

- Contracts supplied by `EP-004, EP-005, EP-007` exist at the narrowest story/API level needed by this epic.
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

- Character controller
- Third-person camera
- Movement command contract
- Top-down targeting transition

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
| Character controller | [ST-008-01](tickets/ST-008-01-character-controller.md) | [ST-008-01](tickets/ST-008-01-character-controller.md) |
| Third-person camera | [ST-008-02](tickets/ST-008-02-third-person-camera.md) | [ST-008-02](tickets/ST-008-02-third-person-camera.md) |
| Movement command contract | [ST-008-03](tickets/ST-008-03-movement-command-contract.md) | [ST-008-03](tickets/ST-008-03-movement-command-contract.md) |
| Top-down targeting transition | [ST-008-04](tickets/ST-008-04-top-down-targeting-transition.md) | [ST-008-04](tickets/ST-008-04-top-down-targeting-transition.md) |

## Ticket index

- [ST-008-01 — Character controller](tickets/ST-008-01-character-controller.md) — Implement authoritative-friendly continuous movement/collision.
- [ST-008-02 — Third-person camera](tickets/ST-008-02-third-person-camera.md) — Implement exploration follow/orbit camera.
- [ST-008-03 — Movement command contract](tickets/ST-008-03-movement-command-contract.md) — Separate sampled client intent from authoritative movement result.
- [ST-008-04 — Top-down targeting transition](tickets/ST-008-04-top-down-targeting-transition.md) — Switch presentation mode for explicit exploration actions.

## Epic integration acceptance tests

1. Movement consumes continuous input rather than node-to-node commands.
2. Character controller collides with authoritative world collision, not articulated visual VoxelModel meshes by default.
3. Camera state does not affect authoritative movement validity.
4. Changing visual animation pose cannot move the authoritative collision shape unless explicit gameplay logic updates it.
5. Movement is deterministic enough under the chosen authoritative/prediction contract.
6. Blocked movement cannot tunnel through fixture geometry.
7. Top-down targeting view can be entered/exited without pausing Region Clock.
8. Headless movement validation works without camera.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-013](../../open-decisions/OD-013-authoritative-collision-primitive-for-articulated-entities.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-014](../../open-decisions/OD-014-fixed-simulation-tick-rates.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

Graphical tickets use deterministic `512 × 512` Sparkle TestLibrary comparisons. Structural refactors preserve reviewed baselines. Intentional visual migrations retain old references until actual/difference images receive human approval, then create a versioned baseline.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
