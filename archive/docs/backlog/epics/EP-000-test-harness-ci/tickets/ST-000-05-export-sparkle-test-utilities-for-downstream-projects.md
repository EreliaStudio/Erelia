# ST-000-05 — Export Sparkle test utilities for downstream projects

## Intent

**Implementation repository/branch:** Sparkle `Version0.1.1`  
**Sparkle-side status:** [Sparkle #1](https://github.com/EreliaStudio/Sparkle/pull/1) is merged into `Version0.1.1`; its final head CI succeeded. The optional exported `Sparkle::TestLibrary`, installed-package consumer support, reusable image comparison, and Sparkle CI validation are complete.  
**Blocks:** ST-001-01 textured Chunk reference-image gate.

Publish the existing image-comparison, path and framebuffer helpers as an optional installed `Sparkle::TestLibrary` target. Downstream projects must consume the installed package without building SparkleTestSuite, depending on GTest, or accessing Sparkle's checkout. Keep existing GPU capture's Windows/WGL requirement explicit.

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

**Implementation repository/branch:** Sparkle `Version0.1.1`  
**Sparkle-side status:** [Sparkle #1](https://github.com/EreliaStudio/Sparkle/pull/1) is merged into `Version0.1.1`; its final head CI succeeded. The optional exported `Sparkle::TestLibrary`, installed-package consumer support, reusable image comparison, and Sparkle CI validation are complete.  
**Blocks:** ST-001-01 textured Chunk reference-image gate.

Publish the existing image-comparison, path and framebuffer helpers as an optional installed `Sparkle::TestLibrary` target. Downstream projects must consume the installed package without building SparkleTestSuite, depending on GTest, or accessing Sparkle's checkout. Keep existing GPU capture's Windows/WGL requirement explicit.

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.

## Implementation inputs

### Already defined values/data

- Story intent: **Implementation repository/branch:** Sparkle `Version0.1.1`  
**Sparkle-side status:** [Sparkle #1](https://github.com/EreliaStudio/Sparkle/pull/1) is merged into `Version0.1.1`; its final head CI succeeded. The optional exported `Sparkle::TestLibrary`, installed-package consumer support, reusable image comparison, and Sparkle CI validation are complete.  
**Blocks:** ST-001-01 textured Chunk reference-image gate.

Publish the existing image-comparison, path and framebuffer helpers as an optional installed `Sparkle::TestLibrary` target. Downstream projects must consume the installed package without building SparkleTestSuite, depending on GTest, or accessing Sparkle's checkout. Keep existing GPU capture's Windows/WGL requirement explicit.
- Required dependency contracts: None.
- Test fixture rule: Use the smallest deterministic fixture that exposes the owned behavior. Record exact dimensions, cell coordinates, palette indices, transforms, expected vertices/state, and stable asset IDs in the test source; do not substitute an unspecified representative asset.

### User validation required before implementation

None identified at ticket authoring. If implementation exposes an unspecified observable choice, apply the execution decision policy and ask the user before proceeding with that portion.

## Behavioral contract

The operation validates identity, ownership, bounds, and dependency preconditions before authoritative mutation. Success produces only the state and events owned above. Failure is atomic. Ordering and deterministic inputs are explicit and reproducible.

## Acceptance tests

### Nominal behavior and integration interactions

- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `A deliberately malformed content fixture fails validation with source-identifying diagnostics and no crash.` is observed exactly; no private-state shortcut is used.
- [ ] Utilities build/install with `SPARKLE_BUILD_TEST_LIBRARY=ON` and `SPARKLE_BUILD_TESTS=OFF`, without GTest or Sparkle's test executables.
- [ ] A separate consumer resolves `find_package(sparkle CONFIG REQUIRED COMPONENTS TestLibrary)` and links `Sparkle::TestLibrary` using only installed headers/targets, including after prefix relocation.
- [ ] A default install omits the optional component; a required-component lookup fails clearly when it is absent.
- [ ] Consumers configure their own reference/result roots; installed binaries embed no Sparkle test source/build paths, and invalid empty roots preserve prior configuration.
- [ ] The installed consumer verifies matching images, differing images and generated diff artifacts without initializing OpenGL; Windows capture and Sparkle's existing GPU tests pass on the supported runner.
- [ ] The vcpkg `test-library` feature enables the export, and Playground documents/enables this dependency before accepting ST-001-01 as Done.
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

None.

## Completion evidence

- Automated test names and passing CI run.
- Exact fixtures and expected values checked into the test resources.
- Error-path assertion proving no partial mutation.
- Decision-file update and user approval reference when a gate was resolved.
- For graphical work: expected, actual, and difference-image artifacts plus approval of any changed baseline.
