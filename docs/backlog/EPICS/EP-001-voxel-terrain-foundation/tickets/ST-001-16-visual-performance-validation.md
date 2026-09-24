# ST-001-16 — Visual and performance validation evidence

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Client validation + EP-001 integration evidence
**Test suite(s):** EreliaClientTestSuite; approved GPU CI/manual validation; performance fixture per OQ-031

## Intent

Capture the final human-reviewed visual references and approved repeatable performance evidence for the EP-001 terrain pipeline.

## User / system value

The first voxel terrain milestone is validated visually and structurally before broader gameplay production begins.

## Starting state / prerequisites

- Depends on ST-001-14 and ST-001-15.
- The Client already has a basic SparkleTestLibrary golden-image harness and project-owned expected/result paths.
- OQ-029, OQ-030, and OQ-031 are open.
- OQ-039 is resolved; DR-015 defines the exact scene used by final views.

## Product ownership

Client owns visual capture. Integration/CI owns evidence execution. Human reviewers own approval of reference changes.

## Allowed dependencies

Existing Erelia Client test stack, Sparkle::TestLibrary, approved CI/platform facilities.

## Forbidden dependencies

Automatic golden-reference replacement/approval, invented pixel tolerances, invented performance budgets.

## Owned behavior

After policy resolution, this ticket owns deterministic named camera/view fixtures, golden/reference capture/comparison, candidate/difference artifact retention on mismatch, explicit human approval record for references, repeatable OQ-031 performance/structural evidence, and a manual free-flight inspection checklist covering above/below and multiple Chunk boundaries.

## Explicitly not owned

Changing generator/mesher behavior to make images pass, production rendering optimization, production gameplay controls.

## Public contract

Blocked until canonical platform(s), image comparison metric/tolerances, and performance evidence methodology/gating status are explicitly decided.

## Invariants

- Expected images are never silently replaced.
- A missing/changed reference yields candidate/comparison evidence for human review.
- Visual fixture uses the exact approved generator/renderer contracts.
- Performance evidence does not invent a pass/fail threshold.

## State transitions

Approved reference exists -> render/capture -> compare -> pass or produce candidate/difference -> human review separately approves any reference change.

Performance fixture -> capture approved metrics -> record evidence.

## Failure behavior

Mismatch/missing reference must fail the automated comparison under the final policy while preserving artifacts. Exact policy is blocked by OQ-029/OQ-030.

## Determinism / ordering

Named camera/scene fixtures must be stable enough for the chosen comparison policy. Performance fixture inputs/order must be fixed by OQ-031.

## Lifecycle / ownership

Generated actual/difference artifacts belong to test results/CI evidence; approved references remain project-owned resources.

## Serialization / persistence

Golden references and evidence artifacts are test resources/evidence, not gameplay persistence.

## Networking / authority

Validation exercises Server-canonical terrain through ST-001-15; no Client-local substitute.

## Implementation constraints

- Reuse existing SparkleTestLibrary support.
- Never auto-approve or overwrite expected images.
- Do not select a platform, tolerance, metric, benchmark target, or gating threshold before OQ resolution.

## Exact test fixtures

Blocked. Final fixtures must specify canonical rendering environment(s), exact viewport sizes, exact scene/generator version and Chunk set, exact camera transforms, exact image comparison metric/thresholds, named expected image files, and exact performance fixture/methodology/gating rule.

## Acceptance tests

### Nominal

Approved views compare successfully and approved performance evidence is produced.

### Boundaries

Views cover floor, walls, underside, slab, slope, stair, Orientation/Flip, and adjacent Chunk boundaries.

### Invalid / rejected operations

Missing reference and mismatch both produce explicit failure/artifacts; malformed performance setup fails visibly.

### Failure atomicity

A failed comparison never changes the expected reference.

### Determinism

Repeated canonical fixture behaves within the approved comparison policy.

### Lifecycle / ownership

Result/reference paths remain separated and artifacts are retained according to final CI policy.

### Serialization / persistence

Not applicable to gameplay.

### Retry / idempotency

Repeated comparison does not mutate references.

### Concurrency / cancellation

Not applicable unless final CI runner requires it.

### Authority / trust boundary

Visual fixture originates from Server-canonical terrain.

### Dependency failure

Missing GPU/context/reference/Server fixture behavior under approved CI policy.

### Cross-system integration

Consumes complete ST-001-15 pipeline.

### Performance

Primary acceptance area; exact methodology blocked by OQ-031.

### Client-visible / golden-image validation

Primary acceptance area; exact platform and comparison policy blocked by OQ-029/OQ-030.

## Decisions / unresolved questions

- [OQ-029](../../../OPEN_QUESTIONS/OQ-029-GOLDEN-IMAGE-PLATFORM.md) — blocking.
- [OQ-030](../../../OPEN_QUESTIONS/OQ-030-IMAGE-COMPARISON-POLICY.md) — blocking.
- [OQ-031](../../../OPEN_QUESTIONS/OQ-031-FIRST-MILESTONE-PERFORMANCE-EVIDENCE.md) — blocking.
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) — resolved; exact scene fixture is DR-015.

## Completion evidence

Approved golden references, comparison artifacts/results, explicit human approval record, manual inspection evidence, and OQ-031-approved performance evidence are recorded and linked before Done.
