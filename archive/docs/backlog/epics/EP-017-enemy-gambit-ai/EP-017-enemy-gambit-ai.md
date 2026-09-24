# EP-017 — Enemy Combat Model and Gambit AI

**Dependencies:** EP-012, EP-015, EP-016  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Run enemies through the same combat model as Heroes while choosing tactical actions through serialized ordered Condition → Target → Reaction Gambit rules.

## Starting state

- Contracts supplied by `EP-012, EP-015, EP-016` exist at the narrowest story/API level needed by this epic.
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

- Enemy combat definition
- Gambit rule schema
- Gambit evaluation
- AI command execution

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
| Enemy combat definition | [ST-017-01](tickets/ST-017-01-enemy-combat-definition.md) | [ST-017-01](tickets/ST-017-01-enemy-combat-definition.md) |
| Gambit rule schema | [ST-017-02](tickets/ST-017-02-gambit-rule-schema.md) | [ST-017-02](tickets/ST-017-02-gambit-rule-schema.md) |
| Gambit evaluation | [ST-017-03](tickets/ST-017-03-gambit-evaluation.md) | [ST-017-03](tickets/ST-017-03-gambit-evaluation.md) |
| AI command execution | [ST-017-04](tickets/ST-017-04-ai-command-execution.md) | [ST-017-04](tickets/ST-017-04-ai-command-execution.md) |

## Ticket index

- [ST-017-01 — Enemy combat definition](tickets/ST-017-01-enemy-combat-definition.md) — Define authored enemy stats/spells/visual reference.
- [ST-017-02 — Gambit rule schema](tickets/ST-017-02-gambit-rule-schema.md) — Serialize and validate Condition → Target → Reaction entries.
- [ST-017-03 — Gambit evaluation](tickets/ST-017-03-gambit-evaluation.md) — Choose an action from ordered rules.
- [ST-017-04 — AI command execution](tickets/ST-017-04-ai-command-execution.md) — Submit selected actions through normal combat command validation.

## Epic integration acceptance tests

1. Enemy stats/spells do not require equipment ownership.
2. Gambit rules are evaluated in deterministic priority order.
3. Condition/target/reaction content is validated before runtime.
4. First valid rule chooses action according to documented semantics.
5. No valid rule resolves safe fallback/end-turn policy.
6. AI cannot bypass AP/targeting/cooldown rules.
7. Same state and deterministic RNG produce same chosen command.
8. Enemy visual model/palette has no influence on decision logic.

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
