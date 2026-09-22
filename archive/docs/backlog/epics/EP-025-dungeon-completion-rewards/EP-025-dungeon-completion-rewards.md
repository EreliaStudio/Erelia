# EP-025 — Dungeon Completion, Grand Dungeon, and Reward Vaults

**Dependencies:** EP-022, EP-023, EP-026  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Complete dungeon runs through explicit objectives/vaults, keep Grand Dungeon structurally consistent with normal dungeons, and trigger the server-wide portal reward once.

## Starting state

- Contracts supplied by `EP-022, EP-023, EP-026` exist at the narrowest story/API level needed by this epic.
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

- Dungeon completion state
- Vault lifecycle
- Grand Dungeon specialization
- First-clear progression trigger

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
| Dungeon completion state | [ST-025-01](tickets/ST-025-01-dungeon-completion-state.md) | [ST-025-01](tickets/ST-025-01-dungeon-completion-state.md) |
| Vault lifecycle | [ST-025-02](tickets/ST-025-02-vault-lifecycle.md) | [ST-025-02](tickets/ST-025-02-vault-lifecycle.md) |
| Grand Dungeon specialization | [ST-025-03](tickets/ST-025-03-grand-dungeon-specialization.md) | [ST-025-03](tickets/ST-025-03-grand-dungeon-specialization.md) |
| First-clear progression trigger | [ST-025-04](tickets/ST-025-04-first-clear-progression-trigger.md) | [ST-025-04](tickets/ST-025-04-first-clear-progression-trigger.md) |

## Ticket index

- [ST-025-01 — Dungeon completion state](tickets/ST-025-01-dungeon-completion-state.md) — Track objectives and completed run state.
- [ST-025-02 — Vault lifecycle](tickets/ST-025-02-vault-lifecycle.md) — Unlock and claim generated rewards.
- [ST-025-03 — Grand Dungeon specialization](tickets/ST-025-03-grand-dungeon-specialization.md) — Configure Grand Dungeon through normal dungeon framework.
- [ST-025-04 — First-clear progression trigger](tickets/ST-025-04-first-clear-progression-trigger.md) — Atomically connect first success to portal unlock.

## Epic integration acceptance tests

1. Completion only occurs after configured objective criteria.
2. Vault becomes available only after completion.
3. Reward claim is idempotent per run/claim policy.
4. Grand Dungeon reuses ordinary dungeon instance/generation contracts.
5. First successful Grand Dungeon completion emits exactly one progression unlock mutation.
6. Later completions still produce normal repeat rewards.
7. Leaving/resetting dungeon clears runtime state but not server progression.
8. Reward presentation asset cannot change reward contents.

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
