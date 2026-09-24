# ST-001-15 — Adjacent-Chunk cross-process integration

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Server + Client integration
**Test suite(s):** cross-process integration fixture; EreliaServerTestSuite; EreliaClientTestSuite

## Intent

Prove the complete non-visual semantic pipeline across separate processes for multiple adjacent Chunks: request -> routed Server generation -> response -> Client cache -> meshing/render placement.

## User / system value

EP-001 demonstrates that its independently tested pieces actually compose at real Chunk boundaries before relying on human visual review.

## Starting state / prerequisites

- Depends on ST-001-06, ST-001-09, ST-001-10, ST-001-11, ST-001-12, and ST-001-13.
- OQ-036 and OQ-038 still block exact boundary/request semantics. OQ-039 is resolved; DR-015 fixes the generator scene.

## Product ownership

Integration coverage spans Client/Server; no new authority domain is introduced.

## Allowed dependencies

Built EreliaServer/EreliaClient components, EreliaCore contracts, Sparkle networking/render test facilities already approved.

## Forbidden dependencies

In-process authority shortcut, Client-local canonical generation, Server render meshes, production movement/gameplay scope.

## Owned behavior

The final integration fixture proves:

- Server and Client are separate runtime processes/boundaries;
- Client requests a fixed adjacent-Chunk set;
- Server routes requests through the terrain `RemoteNode` to the separate terrain Endpoint process and generates exact canonical fixtures;
- Client associates responses with exact coordinates;
- Client caches and meshes them;
- shared boundary faces/placement follow the chosen neighbor policy;
- negative-coordinate adjacency is covered.

## Explicitly not owned

Golden-image platform/tolerance approval, performance budgets, free-flight interaction, production interest management.

## Public contract

This ticket introduces no new public production API. It verifies the already-approved contracts together.

## Invariants

- No shared-memory/in-process bypass substitutes for the network boundary.
- Server remains canonical.
- Adjacent Chunk coordinates map to exact 16-world-unit placement.
- Boundary geometry matches OQ-036 final policy once all required neighbor data is available.

## State transitions

Fixture startup -> Client connects -> request batch -> Server response -> Client cache -> mesh/render semantic state -> clean shutdown.

## Failure behavior

Final fixture must include at least Server disconnect while requests are outstanding and malformed/rejected response/request cases as defined by prerequisite tickets.

## Determinism / ordering

Fixed generator/request fixtures must yield the same semantic cached cells and mesh boundary result across repeated runs.

## Lifecycle / ownership

Both process/network lifecycles terminate cleanly and Client derived state is invalidated on teardown.

## Serialization / persistence

Exercises the shared dedicated Chunk protocol codec and immutable Chunk values; no persistence.

## Networking / authority

Real Sparkle transport boundary is mandatory.

## Implementation constraints

- Do not replace with direct Core calls between Client and Server.
- Keep fixture deterministic and small.
- Use existing product binaries/libraries rather than a second test-only implementation.

## Exact test fixtures

Blocked until the remaining prerequisite OQs resolve. Final fixture must include exact adjacent coordinates spanning at least one positive boundary and one negative boundary and exact expected canonical Cells/semantic mesh counts at their shared boundary.

## Acceptance tests

### Nominal

Separate-process multi-Chunk request completes end-to-end with exact cached/mesh semantic results.

### Boundaries

Positive and negative adjacent-Chunk boundaries.

### Invalid / rejected operations

Protocol rejection/malformed case from prerequisite tickets.

### Failure atomicity

One failed/rejected request cannot corrupt already valid unrelated cached/mesh state.

### Determinism

Repeat complete fixed fixture and compare semantic terrain/mesh outputs.

### Lifecycle / ownership

Clean process, connection, cache, and render-resource teardown.

### Serialization / persistence

Exercises exact wire codec round trips.

### Retry / idempotency

Per final OQ-038 semantics.

### Concurrency / cancellation

Outstanding request during Server disconnect and two-Client correlation where practical.

### Authority / trust boundary

Client never supplies canonical terrain result.

### Dependency failure

Server unavailable/disconnect and generator/protocol failure paths.

### Cross-system integration

Primary owned acceptance category.

### Performance

Metrics may be captured only after OQ-031 defines what evidence is meaningful; no gating threshold here yet.

### Client-visible / golden-image validation

Not required to mark this semantic integration ticket Done; ST-001-16 owns visual approval.

## Decisions / unresolved questions

- [OQ-036](../../../OPEN_QUESTIONS/OQ-036-TERRAIN-MESHING-NEIGHBOR-POLICY.md) — blocking.
- [OQ-038](../../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — blocking.
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md) — immutable Chunk/Collection lifetime contract.
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) — resolved; DR-015 supplies the canonical terrain fixture.

## Completion evidence

Repeatable separate-process test evidence proves exact adjacent canonical terrain and mesh semantics across the approved boundary fixtures.
