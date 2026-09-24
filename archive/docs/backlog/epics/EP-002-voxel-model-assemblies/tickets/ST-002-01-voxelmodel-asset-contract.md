# ST-002-01 — VoxelModel asset contract

## Intent

Define stable identity and pure VoxelVolume data for imported/runtime model assets; anchors, parent relationships, animation, equipment slots, and world transforms are forbidden.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- The dependency contracts `EP-001, EP-030, EP-032` used by this story are available; an epic dependency denotes a required contract, not global waterfall completion.
- The test begins from an explicitly constructed, valid state with no pending operation unless the case says otherwise.
- Rendering, authoring data, and display names are not authoritative gameplay state.

## Owned behavior

Define stable identity and pure VoxelVolume data for imported/runtime model assets; anchors, parent relationships, animation, equipment slots, and world transforms are forbidden.

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.

## Implementation inputs

### Already defined values/data

- Story intent: Define stable identity and pure VoxelVolume data for imported/runtime model assets; anchors, parent relationships, animation, equipment slots, and world transforms are forbidden.
- Required dependency contracts: EP-001, EP-030, EP-032.
- Test fixture rule: Use the smallest deterministic fixture that exposes the owned behavior. Record exact dimensions, cell coordinates, palette indices, transforms, expected vertices/state, and stable asset IDs in the test source; do not substitute an unspecified representative asset.

### User validation required before implementation

Before implementing behavior controlled by an Open Decision below, ask the user to resolve it and record the approved choice in that OD file. Work that relies only on its fixed constraints may proceed. Before freezing any model, assembly, animation, or equipment serialization, present one complete sample asset and schema to the user for approval.

## Behavioral contract

The operation validates identity, ownership, bounds, and dependency preconditions before authoritative mutation. Success produces only the state and events owned above. Failure is atomic. Ordering and deterministic inputs are explicit and reproducible.

## Acceptance tests

### Nominal behavior and integration interactions

- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `A barrel model and one Chunk both mesh through EP-032 with the same Definition/Shape resolution path.` is observed exactly; no private-state shortcut is used.
- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Editing one part's voxel cells invalidates/remeshes that part without rebuilding unrelated Hero parts.` is observed exactly; no private-state shortcut is used.
- [ ] Model identity is independent of display name/path.
- [ ] A model asset containing an anchor, pivot, parent, animation clip, equipment slot, or world transform is rejected with a source-identifying diagnostic; the last valid asset remains active.
- [ ] Loading a VoxelModel requires no renderer or GPU.
- [ ] Invalid input is rejected atomically and leaves the previously valid state unchanged.
- [ ] Identical deterministic inputs produce identical observable results.
- [ ] The exact lower and upper supported boundaries succeed; one-step-outside values are rejected before mutation.
- [ ] A rejected command leaves state, ownership, resources, version counters, scheduled work, and emitted authoritative events byte-for-byte or semantically unchanged.
- [ ] Repeating the same seed, configuration, starting snapshot, and ordered commands produces the same result and event order.
- [ ] Create → use → serialize where applicable → unload → restore/recreate → retry preserves stable IDs and does not duplicate the operation.
- [ ] The nearest upstream and downstream contracts named in prerequisites are exercised together; dependency failure follows the documented fail-closed or rollback behavior.

- [ ] A `3 × 2 × 2`, voxel-size `0.25` fixture with occupied `(0,0,0)→1`, `(1,0,0)→2`, `(1,1,0)→3` loads as pure volume data and preserves exact authored palette indices.
- [ ] The same model asset is referenced by two assemblies without copying cell storage or embedding assembly metadata.
- [ ] Any proposed `.vxm` schema is presented to the user as a complete sample before serialization code is frozen.

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

- [OD-022](../../../open-decisions/OD-022-assembly-anchor-and-animation-authoring-source.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Completion evidence

- Automated test names and passing CI run.
- Exact fixtures and expected values checked into the test resources.
- Error-path assertion proving no partial mutation.
- Decision-file update and user approval reference when a gate was resolved.
- For graphical work: expected, actual, and difference-image artifacts plus approval of any changed baseline.
