# ST-029-01 — Versioned voxel volume/model schema

## Intent

Define versioned runtime content for arbitrary dimensions, uniform voxelSize and packed Voxel::Cell data.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- The dependency contracts `EP-000` used by this story are available; an epic dependency denotes a required contract, not global waterfall completion.
- The test begins from an explicitly constructed, valid state with no pending operation unless the case says otherwise.
- Rendering, authoring data, and display names are not authoritative gameplay state.

## Owned behavior

Define versioned runtime content for arbitrary dimensions, uniform voxelSize and packed Voxel::Cell data.

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.

## Implementation inputs

### Already defined values/data

- Story intent: Define versioned runtime content for arbitrary dimensions, uniform voxelSize and packed Voxel::Cell data.
- Required dependency contracts: EP-000.
- Test fixture rule: Use the smallest deterministic fixture that exposes the owned behavior. Record exact dimensions, cell coordinates, palette indices, transforms, expected vertices/state, and stable asset IDs in the test source; do not substitute an unspecified representative asset.

### User validation required before implementation

Before implementing behavior controlled by an Open Decision below, ask the user to resolve it and record the approved choice in that OD file. Work that relies only on its fixed constraints may proceed. Before freezing any model, assembly, animation, or equipment serialization, present one complete sample asset and schema to the user for approval.

## Behavioral contract

The operation validates identity, ownership, bounds, and dependency preconditions before authoritative mutation. Success produces only the state and events owned above. Failure is atomic. Ordering and deterministic inputs are explicit and reproducible.

## Acceptance tests

### Nominal behavior and integration interactions

- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Existing Shape/Definition fixtures validate under the revised content pipeline.` is observed exactly; no private-state shortcut is used.
- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Derived mesh cache is rejected/rebuilt when its source/Definition/mesher version is incompatible.` is observed exactly; no private-state shortcut is used.
- [ ] Dimensions and cell count are validated against overflow/limits.
- [ ] voxelSize rejects zero/negative/non-finite values.
- [ ] Round-trip preserves exact packed Cell values.
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

- [ ] Render the deterministic fixture at `512 × 512`; compare against its reviewed PNG with the tolerance recorded by the Sparkle TestLibrary fixture. On failure, retain the old expected image and publish actual/difference images for review.
- [ ] Assert semantic geometry/material/transform values independently of the PNG comparison.

## Rendering impact

Yes. The controlled fixture uses a fixed camera, viewport, asset set, lighting, and supported GPU runner. Structural refactors must match the reviewed current baseline; intentional migrations require human review before a new versioned baseline is accepted.

## Open decisions

- [OD-017](../../../open-decisions/OD-017-rich-shape-mapping-metadata-for-imported-assets.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-021](../../../open-decisions/OD-021-serialized-derivative-mesh-cache-versus-rebuild-on-load.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-023](../../../open-decisions/OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Completion evidence

- Automated test names and passing CI run.
- Exact fixtures and expected values checked into the test resources.
- Error-path assertion proving no partial mutation.
- Decision-file update and user approval reference when a gate was resolved.
- For graphical work: expected, actual, and difference-image artifacts plus approval of any changed baseline.
