# ST-007-03 — Hero visual definition

## Intent

Bind Hero identity to reusable articulated VoxelModel assembly and Material palette/reference set.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- The dependency contracts `EP-002, EP-024` used by this story are available; an epic dependency denotes a required contract, not global waterfall completion.
- The test begins from an explicitly constructed, valid state with no pending operation unless the case says otherwise.
- Rendering, authoring data, and display names are not authoritative gameplay state.

## Owned behavior

Bind Hero identity to reusable articulated VoxelModel assembly and Material palette/reference set.

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.

## Implementation inputs

### Already defined values/data

- Story intent: Bind Hero identity to reusable articulated VoxelModel assembly and Material palette/reference set.
- Required dependency contracts: EP-002, EP-024.
- Epic behavioral catalogue: stable Hero ownership; an unbounded roster data model; exactly three active Heroes per player; duplicate and foreign-Hero rejection; squad changes; no class or level system; visual assets grant no stats; multiplayer ownership preservation.
- Test fixture rule: Use stable IDs `player-a`, `hero-a`, `hero-b`, and `hero-c`, an empty authoritative state, and explicit commands in the order written by each test. Where a numeric value is not fixed by the GDD, load it from a named test configuration and test `minimum - 1`, `minimum`, `maximum`, and `maximum + 1`; do not embed a new balance value in production code.

### User validation required before implementation

Before implementing behavior controlled by an Open Decision below, ask the user to resolve it and record the approved choice in that OD file. Work that relies only on its fixed constraints may proceed.

## Behavioral contract

The operation validates identity, ownership, bounds, and dependency preconditions before authoritative mutation. Success produces only the state and events owned above. Failure is atomic. Ordering and deterministic inputs are explicit and reproducible.

## Acceptance tests

### Nominal behavior and integration interactions

- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Hero visual definition/palette is presentation content and does not imply combat stats.` is observed exactly; no private-state shortcut is used.
- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Invalid duplicate active selection is rejected.` is observed exactly; no private-state shortcut is used.
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

- [ ] Render a deterministic `512 × 512` fixture with fixed camera/projection, viewport, lighting, asset versions, animation time, and supported GPU runner; compare it with the reviewed PNG using the fixture-recorded tolerance.
- [ ] Assert authoritative and presentation state semantically in addition to the image comparison. A mismatch retains the old reference and publishes actual/difference images for review.

## Rendering impact

Yes. Visible presentation is owned. Structural changes must preserve the reviewed baseline; intentional changes require human approval before a new versioned reference is accepted.

## Open decisions

- [OD-001](../../../open-decisions/OD-001-exact-hero-combat-statistic-set-and-defaults.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Completion evidence

- Automated test names and passing CI run.
- Exact fixtures and expected values checked into the test resources.
- Error-path assertion proving no partial mutation.
- Decision-file update and user approval reference when a gate was resolved.
- For graphical work: expected, actual, and difference-image artifacts plus approval of any changed baseline.
