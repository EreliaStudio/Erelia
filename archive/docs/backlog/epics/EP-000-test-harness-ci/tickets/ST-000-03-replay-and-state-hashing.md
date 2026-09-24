# ST-000-03 — Replay and state hashing

## Intent

Add accepted-command recording, deterministic replay, and semantic state hashing for regression tests.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- The dependency contracts `None` used by this story are available; an epic dependency denotes a required contract, not global waterfall completion.
- The test begins from an explicitly constructed, valid state with no pending operation unless the case says otherwise.
- Rendering, authoring data, and display names are not authoritative gameplay state.

## Owned behavior

Add accepted-command recording, deterministic replay, and semantic state hashing for regression tests.

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.

## Implementation inputs

### Already defined values/data

- Story intent: Add accepted-command recording, deterministic replay, and semantic state hashing for regression tests.
- Required dependency contracts: None.
- Test fixture rule: Use the smallest deterministic fixture that exposes the owned behavior. Record exact dimensions, cell coordinates, palette indices, transforms, expected vertices/state, and stable asset IDs in the test source; do not substitute an unspecified representative asset.

### User validation required before implementation

None identified at ticket authoring. If implementation exposes an unspecified observable choice, apply the execution decision policy and ask the user before proceeding with that portion.

## Behavioral contract

The operation validates identity, ownership, bounds, and dependency preconditions before authoritative mutation. Success produces only the state and events owned above. Failure is atomic. Ordering and deterministic inputs are explicit and reproducible.

## Acceptance tests

### Nominal behavior and integration interactions

- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `A deterministic fixture can record an accepted command stream and replay it to the same terminal state hash.` is observed exactly; no private-state shortcut is used.
- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `A failing integration test prevents the owning epic from being considered Done.` is observed exactly; no private-state shortcut is used.
- [ ] A recorded fixture replays to the same state hash.
- [ ] Changing an authoritative state field changes the semantic hash.
- [ ] Presentation-only state is excluded from the authoritative hash.
- [ ] Invalid input is rejected atomically and leaves the previously valid state unchanged.
- [ ] Identical deterministic inputs produce identical observable results.
- [ ] The exact lower and upper supported boundaries succeed; one-step-outside values are rejected before mutation.
- [ ] A rejected command leaves state, ownership, resources, version counters, scheduled work, and emitted authoritative events byte-for-byte or semantically unchanged.
- [ ] Repeating the same seed, configuration, starting snapshot, and ordered commands produces the same result and event order.
- [ ] Create → use → serialize where applicable → unload → restore/recreate → retry preserves stable IDs and does not duplicate the operation.
- [ ] The nearest upstream and downstream contracts named in prerequisites are exercised together; dependency failure follows the documented fail-closed or rollback behavior.

### Boundaries and invalid/rejected operations

- [ ] Missing IDs, foreign ownership, malformed content, stale versions, and unsupported enum/tag values are rejected with the documented error category.
- [ ] Empty/minimum/maximum fixtures are exercised where the public contract permits them; unsupported empty state is rejected atomically.

### Determinism and lifecycle / retry / persistence

- [ ] Deterministic iteration never depends on pointer values, hash-table accident, render frame rate, or wall-clock timing.
- [ ] A duplicate/retried operation is either idempotent or rejected as already applied, according to the story contract, without duplicating state or events.

### Rendering / golden images

- [ ] Not applicable; the ticket is proven headlessly through semantic/value assertions.

## Rendering impact

None. This ticket changes no visible geometry, material, camera, effect, sprite, or UI output.

## Open decisions

None.

## Completion evidence

- Automated test names and passing CI run.
- Exact fixtures and expected values checked into the test resources.
- Error-path assertion proving no partial mutation.
- Decision-file update and user approval reference when a gate was resolved.
- For graphical work: expected, actual, and difference-image artifacts plus approval of any changed baseline.
