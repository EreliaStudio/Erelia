# EP-009 — Followers, Formation, Detection, and Exploration Actions

**Dependencies:** EP-005, EP-008  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Make non-controlled squad Heroes physically follow/stay, reproduce special traversal where possible, and participate in vision-driven enemy detection/chase.

## Starting state

- Contracts supplied by `EP-005, EP-008` exist at the narrowest story/API level needed by this epic.
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

- Follow/hold behavior
- Vision detection
- Real-time chase
- Explicit traversal actions

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
| Follow/hold behavior | [ST-009-01](tickets/ST-009-01-follow-hold-behavior.md) | [ST-009-01](tickets/ST-009-01-follow-hold-behavior.md) |
| Vision detection | [ST-009-02](tickets/ST-009-02-vision-detection.md) | [ST-009-02](tickets/ST-009-02-vision-detection.md) |
| Real-time chase | [ST-009-03](tickets/ST-009-03-real-time-chase.md) | [ST-009-03](tickets/ST-009-03-real-time-chase.md) |
| Explicit traversal actions | [ST-009-04](tickets/ST-009-04-explicit-traversal-actions.md) | [ST-009-04](tickets/ST-009-04-explicit-traversal-actions.md) |

## Ticket index

- [ST-009-01 — Follow/hold behavior](tickets/ST-009-01-follow-hold-behavior.md) — Implement physical follower locomotion modes.
- [ST-009-02 — Vision detection](tickets/ST-009-02-vision-detection.md) — Implement initial enemy awareness by vision.
- [ST-009-03 — Real-time chase](tickets/ST-009-03-real-time-chase.md) — Transition detected enemies into pursuit before engagement.
- [ST-009-04 — Explicit traversal actions](tickets/ST-009-04-explicit-traversal-actions.md) — Implement jump/climb target selection and follower replication rules.

## Epic integration acceptance tests

1. Followers occupy real world positions and can trigger detection/traps.
2. Follow/hold changes behavior without teleporting followers.
3. Follower traversal attempts valid actions and holds if impossible.
4. Vision detection uses authoritative positions/occlusion inputs.
5. Detection transitions to chase before encounter handoff.
6. Explicit jump/climb targeting does not pause Region Clock.
7. Encounter start cancels uncommitted targeting according to handoff rules.
8. Follower visuals are independent of navigation state.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-013](../../open-decisions/OD-013-authoritative-collision-primitive-for-articulated-entities.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
