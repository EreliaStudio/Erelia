# ST-001-04 — First terrain Definition and Shape contract

**Status:** Draft
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Define the minimal shared terrain Definition/Shape data required for EP-001 cubes, slabs, slopes, and stairs without importing production content-system scope.

## User / system value

Server generation and Client meshing must assign the same meaning to a non-empty `Voxel::Cell` Definition ID and its Orientation/Flip fields.

## Starting state / prerequisites

- Depends on ST-001-02.
- `Voxel::Definition` already exists as the semantic owner of `Voxel::Definition::ID`, which is an alias of `std::uint32_t`; this ticket must extend that type rather than replace its identity contract.
- EP-001 requires cubes/slabs/slopes/stairs for visual validation.
- OQ-039 still lacks exact fixture Definition IDs/material choices.
- The active greenfield backlog does not yet specify the minimal first Definition/Shape representation or normalized geometry in enough detail for tests-first implementation.

## Product ownership

Core owns shared headless-safe Definition/Shape data needed by both Server and Client.

## Allowed dependencies

C++ standard library and headless-safe Sparkle Core.

## Forbidden dependencies

Client rendering resources, GPU objects, Server authority/state, production asset-editor/import requirements.

## Owned behavior

The eventual ticket should own only the minimal EP-001 shared semantic description needed to resolve a Cell Definition ID into normalized geometry/material-facing information for cubes, slabs, slopes, and stairs.

## Explicitly not owned

- production content import/editor pipeline;
- GPU meshes/material resources;
- terrain generator authored coordinates;
- Client meshing algorithm;
- world generation.

## Public contract

**Draft:** exact Definition fields, Shape representation, normalized polygon/occlusion semantics, invalid Definition ID behavior, and the minimum material-facing contract are not yet approved in the active greenfield backlog.

## Invariants

Must eventually preserve:

- Cell Definition ID 0 means empty;
- Orientation/Flip transformations are interpreted consistently by generation and meshing;
- Core remains graphics/headless safe.

## State transitions

Expected to be immutable/read-only semantic data for EP-001, but lifecycle/loading details are not yet fixed.

## Failure behavior

Draft until missing/unknown Definition IDs and invalid Shape data behavior are specified.

## Determinism / ordering

Shape geometry and transformation semantics must be deterministic; exact normalized data is not yet specified.

## Lifecycle / ownership

Draft until catalog/resource ownership is specified.

## Serialization / persistence

No requirement is approved for serializing Definition/Shape resources in EP-001.

## Networking / authority

Server sends Cell/Volume data, never render meshes. Whether Definition data is pre-shared or otherwise available to Client must be explicit before Ready.

## Implementation constraints

Do not copy archived Definition/Shape APIs as requirements. Reuse ideas only after explicit greenfield approval.

## Exact test fixtures

Not yet available. OQ-039 must at least supply fixture Definition IDs/material choices, and a separate explicit contract is still needed for normalized cube/slab/slope/stair geometry and transformations.

## Acceptance tests

### Nominal

Draft: exact Shape/Definition fixtures required.

### Boundaries

Draft.

### Invalid / rejected operations

Draft.

### Failure atomicity

Not applicable if immutable construction-only data is chosen; otherwise must be specified.

### Determinism

Exact normalized geometry/transform fixtures required.

### Lifecycle / ownership

Draft.

### Serialization / persistence

Not applicable unless a later explicit decision adds it.

### Retry / idempotency

Not applicable.

### Concurrency / cancellation

Not applicable for immutable data.

### Authority / trust boundary

Client must not define authoritative terrain semantics independently of the shared contract.

### Dependency failure

Draft until resource ownership/loading is specified.

### Cross-system integration

Later Server generator and Client mesher must consume the same Core definition contract.

### Performance

No timing budget; data must remain usable by headless Core/Server.

### Client-visible / golden-image validation

Owned by later rendering/validation tickets.

## Decisions / unresolved questions

- [DR-012](../../../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [DR-013](../../../DECISIONS/DR-013-CLIENT-TERRAIN-MESHING.md)
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md)

Additional specification gap: the active greenfield backlog needs an explicit minimal Definition/Shape geometry and resource-availability contract before this ticket can become Ready.

## Completion evidence

Promote to Ready only after exact shared Definition/Shape fixtures, transformations, lookup/failure behavior, and lifecycle/resource availability are documented.
