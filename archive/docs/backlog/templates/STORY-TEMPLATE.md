# ST-XXX-YY — Title

## Intent

State the independently deliverable outcome.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Implementation details may be selected without asking only when this ticket explicitly delegates them and they cannot alter an observable contract.

## Starting state / prerequisites

List exact contracts and observable state required before the operation. Dependencies identify contracts, not a requirement that an earlier epic be globally complete.

## Owned behavior

List everything this ticket implements.

## Explicitly not owned

List adjacent behavior assigned elsewhere.

## Implementation inputs

### Already defined values/data

Provide exact IDs, dimensions, coordinates, commands, state, configuration, and expected values used by tests.

### User validation required before implementation

List every missing asset, schema, balance value, formula, policy, architecture, or technology choice the implementer must present to the user. Write `None identified at ticket authoring` rather than assuming that no new ambiguity can be discovered during implementation; the execution decision policy still applies.

## Behavioral contract

Describe validation order, successful state transition, failure atomicity, deterministic ordering, and interactions.

## Acceptance tests

### Nominal behavior

- [ ] Given exact starting state, when an exact input is applied, then exact public state and events result.

### Boundaries

- [ ] Exercise exact lower/upper supported values and one-step-outside values.

### Invalid/rejected operations

- [ ] Assert the error and every state/resource/event that remains unchanged.

### Determinism

- [ ] Repeat from the same seed/configuration/snapshot/commands and compare ordered outputs.

### Lifecycle / retry / persistence

- [ ] Cover create, use, save/load where applicable, unload/recreate, and duplicate retry.

### Integration interactions

- [ ] Exercise the nearest upstream and downstream contracts, including dependency failure.

### Rendering / golden images

- [ ] For graphical output, define the fixed scene, camera, viewport, assets, expected image, tolerance, and baseline-review rule. Keep semantic/value tests alongside the image test.

## Rendering impact

None.

Or:

Yes.

Golden-image fixtures:
- exact fixture, camera, viewport, deterministic assets, tolerance, and whether output must remain identical or intentionally change after approval.

## Open decisions

None.

Or link every applicable persistent OD file, its current status, precisely what it blocks, and what may proceed.

## Completion evidence

- Automated tests and CI run.
- Checked-in exact fixtures and expected values.
- Failure-atomicity evidence.
- Decision/user approval provenance where applicable.
- Expected, actual, and difference images for graphical work.
