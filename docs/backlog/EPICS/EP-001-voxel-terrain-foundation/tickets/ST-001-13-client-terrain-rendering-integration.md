# ST-001-13 — Client terrain rendering integration

**Status:** Draft
**Epic:** EP-001
**Production target(s):** Client
**Test suite(s):** EreliaClientTestSuite; EreliaClientSmoke

## Intent

Integrate Client-owned terrain mesh data into the Sparkle rendering runtime so one or more canonical terrain Chunks are displayed at the correct world positions.

## User / system value

EP-001 reaches the first visible output of the canonical Server-terrain pipeline without moving render ownership into Core or Server.

## Starting state / prerequisites

- Depends on ST-001-01 and ST-001-12.
- Current Client production runtime is smoke-only.
- Sparkle graphical dependency is already linked by EreliaClientLibrary.
- The current Client test suite already contains a basic SparkleTestLibrary golden-image harness.
- Exact first terrain materials/Definition presentation and deterministic camera/view fixture are not yet specified.

## Product ownership

Client owns render resources, scene integration, Chunk transforms, and presentation lifetime.

## Allowed dependencies

EreliaClientLibrary, EreliaCore shared data, Sparkle graphics/rendering, standard library.

## Forbidden dependencies

Server graphics code, Server-emitted meshes, Core GPU/render resource ownership.

## Owned behavior

The final ticket should:

- accept Client mesh data for a Chunk coordinate;
- place it using the shared 16-cell Chunk coordinate convention;
- create/update/remove Client render resources as source meshes appear/change/disappear;
- render multiple adjacent Chunks in one scene.

## Explicitly not owned

Meshing algorithm, request/cache policy, free-flight controls, golden-image approval policy, production materials/content system.

## Public contract

Draft: exact render-resource abstraction, first material/palette binding, deterministic test camera/projection, and mesh replacement/removal lifecycle must be fixed before Ready.

## Invariants

- Chunk world transform follows ST-001-01 with no axis remap.
- Rendering is derived Client state only.
- Replacing/removing a source mesh cannot leave stale visible geometry.
- Server/Core remain graphics-free.

## State transitions

No mesh -> mesh available -> render resource visible.
Mesh replaced -> render resource updated/replaced.
Chunk evicted/removed -> render resource removed.

Exact resource API and frame synchronization are Draft.

## Failure behavior

Draft until GPU/resource creation failure and invalid mesh handling are explicit.

## Determinism / ordering

Semantic placement is deterministic. Pixel-level determinism belongs to ST-001-16 and OQ-029/OQ-030.

## Lifecycle / ownership

Client owns GPU/render resources. Source mesh/cache invalidation must not leave dangling references.

## Serialization / persistence

Not applicable.

## Networking / authority

Consumes Client-derived mesh built from Server-canonical voxel data; no network behavior owned.

## Implementation constraints

- Use existing Sparkle rendering dependency.
- Do not add a new graphics/runtime dependency.
- Keep renderer-specific resources out of Core/Server.
- Existing golden-image harness is enabling infrastructure, not the production rendering contract.

## Exact test fixtures

Ready fixtures must define exact Chunk coordinates including origin and an adjacent Chunk, exact semantic meshes from ST-001-12, a deterministic camera/projection and viewport, and exact material/palette binding sufficient to distinguish expected surfaces.

## Acceptance tests

### Nominal

One Chunk and two adjacent Chunks are placed at exact world positions and render resources are created.

### Boundaries

Negative Chunk placement and adjacency at +/-16 world-unit boundaries.

### Invalid / rejected operations

Invalid mesh/material reference behavior per final render contract.

### Failure atomicity

Resource-creation/update failure must not leak or leave a half-replaced visible resource.

### Determinism

World transforms and semantic render-resource state repeat exactly.

### Lifecycle / ownership

Replacement/removal cleans up prior resources and source lifetimes are respected.

### Serialization / persistence

Not applicable.

### Retry / idempotency

Applying the same unchanged mesh again has the explicitly chosen replacement/no-op behavior.

### Concurrency / cancellation

No asynchronous upload contract is approved; if used, lifetime/cancellation must be specified before Ready.

### Authority / trust boundary

Render state cannot become authoritative terrain state.

### Dependency failure

GPU/resource creation failure according to final contract.

### Cross-system integration

Consumes ST-001-12 output and is later driven by ST-001-11 cache changes.

### Performance

No hard budget until OQ-031.

### Client-visible / golden-image validation

Golden/reference acceptance is deferred to ST-001-16 after OQ-029/OQ-030.

## Decisions / unresolved questions

- [DR-011](../../../DECISIONS/DR-011-VOXEL-COORDINATES.md)
- [DR-013](../../../DECISIONS/DR-013-CLIENT-TERRAIN-MESHING.md)
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md)

Specification still needed before Ready: exact first material/palette presentation, deterministic render fixture/camera, and render-resource failure/lifecycle contract.

## Completion evidence

Client semantic/render integration tests and smoke runtime prove correct Chunk placement/resource lifecycle; golden approval remains outside this ticket.
