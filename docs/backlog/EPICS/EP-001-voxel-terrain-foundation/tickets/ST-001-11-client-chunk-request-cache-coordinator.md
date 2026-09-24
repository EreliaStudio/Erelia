# ST-001-11 — Client Chunk request/cache coordinator

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Client
**Test suite(s):** EreliaClientTestSuite

## Intent

Own Client-side Chunk identity, missing-Chunk request batching, outstanding-request tracking, canonical response replacement, and retention/eviction behavior for EP-001 inspection.

## User / system value

The Client can request only the terrain it needs and maintain coherent local canonical Chunk data while moving the inspection position.

## Starting state / prerequisites

- Depends on ST-001-06 Core Chunk::Collection/Provider foundation, ST-001-08, and ST-001-10.
- OQ-038 fixes only the high-level batched Client-driven direction; duplicate outstanding requests, cache retention/eviction, request limits, partial response and unavailable-coordinate behavior remain open.

## Product ownership

Client owns loading/view policy, request coordination, and Client-side cache state.

## Allowed dependencies

EreliaClientLibrary, EreliaCore, Sparkle Client networking, standard library.

## Forbidden dependencies

Server-selected view radius, Client terrain generation as canonical data, Server/Client cross-dependency, rendering ownership inside the cache.

## Owned behavior

The final ticket should own:

- desired Chunk coordinate set supplied by Client inspection/view policy;
- identification of missing cached coordinates;
- batching request coordinates under final limits;
- outstanding-request suppression/retry behavior;
- insertion/replacement of canonical responses;
- retention/eviction policy;
- disconnect/partial-response cleanup.

## Explicitly not owned

How the inspection position is controlled, meshing/rendering, Server generation, production interest management.

## Public contract

Blocked by OQ-038 for duplicate suppression, cache retention/eviction, request batch limits, partial success, unavailable-coordinate semantics, retry behavior, and response replacement rules.

## Invariants

- Cache identity is by Chunk coordinate.
- Server response data is canonical.
- Client never treats a locally fabricated/placeholder Chunk as authoritative Server terrain.
- A received response remains associated with its declared coordinate.

## State transitions

Need exact OQ-038 contract for missing -> outstanding -> cached/rejected/retryable/evicted transitions.

## Failure behavior

Blocked by OQ-038, plus final ST-001-10 disconnect behavior.

## Determinism / ordering

Desired-set to request-batch ordering must be explicit if observable/tested; otherwise tests must assert set semantics only. OQ-038 must decide duplicate/order behavior first.

## Lifecycle / ownership

Core `Chunk::Collection` owns coordinate->immutable-Chunk storage and returns cheap Chunk values whose backing Cell content is shared immutably (DR-019).

A Client request Provider may return an empty valid 16x16x16 placeholder immediately after issuing the request. When canonical Server data arrives, the Client replaces the complete Collection Chunk value rather than mutating the placeholder. Any renderer/mesher still holding an older copied Chunk keeps its old immutable content alive.

Exact eviction, outstanding/retry and stale/unsolicited response rules remain blocked by OQ-038.

## Serialization / persistence

Uses ST-001-08 protocol; no persistence.

## Networking / authority

Client chooses coordinates to request; Server remains canonical source of data.

## Implementation constraints

- Server must not choose Client view/loading radius.
- Keep cache/request logic separate from GPU mesh resources.
- Do not add production movement or world interest-management scope.

## Exact test fixtures

Final Ready fixture set must include:

- desired set containing already cached, outstanding, and missing coordinates;
- repeated desired update;
- duplicate coordinate inputs;
- full successful batch response;
- partial response;
- unavailable/rejected coordinate;
- disconnect while outstanding;
- retention/eviction boundary;
- retry behavior if approved.

## Acceptance tests

### Nominal

Exact final desired/request/cache state transitions.

### Boundaries

Final batch/cache/load-retain boundaries.

### Invalid / rejected operations

Unexpected/duplicate/stale response behavior per final contract.

### Failure atomicity

Failed/partial response cannot corrupt unrelated cached Chunks.

### Determinism

Equivalent desired sets produce equivalent request/cache state under the final ordering contract.

### Lifecycle / ownership

Eviction and replacement invalidate dependent state exactly as documented.

### Serialization / persistence

Not independently owned.

### Retry / idempotency

Primary blocked area under OQ-038.

### Concurrency / cancellation

Outstanding requests and disconnect/cancel behavior must be explicit.

### Authority / trust boundary

Only decoded Server responses enter the canonical Client Chunk cache.

### Dependency failure

Connection loss and rejected/partial responses.

### Cross-system integration

Later mesher consumes copied immutable Chunk values from the Core Collection; integration must not bypass this coordinator.

### Performance

No numeric budget; structural request de-duplication/cache policy only after OQ-038.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-014](../../../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md)
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
- [OQ-038](../../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — blocking.

## Completion evidence

Promote to Ready only when every cache/outstanding/retry/partial-response transition has an exact test fixture and no Server-side view policy is introduced.
