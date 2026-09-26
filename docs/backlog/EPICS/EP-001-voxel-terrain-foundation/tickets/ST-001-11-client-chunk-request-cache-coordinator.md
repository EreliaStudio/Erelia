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
- OQ-038/DR-022 establish the batched Client-driven direction and RequestID correlation. ST-001-09 planning later refines terminal Chunk responses toward `Response::Success` / `Response::Failure`; exact failure-code/string encoding and the generic diagnostic-message contract remain open.

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
- disconnect/outstanding-request cleanup;
- application of terminal `Response::Success` / `Response::Failure` entries once the refined protocol contract is finalized.

## Explicitly not owned

How the inspection position is controlled, meshing/rendering, Server generation, production interest management.

## Public contract

Blocked by remaining Client policy plus the refined ST-001-09 protocol details: duplicate outstanding suppression, cache retention/eviction, request batching policy, retry behavior, Response Failure handling, generic diagnostic handling where relevant, and response replacement rules.

## Invariants

- Cache identity is by Chunk coordinate.
- Server response data is canonical.
- Client never treats a locally fabricated/placeholder Chunk as authoritative Server terrain.
- A received response remains associated with its declared coordinate.

## State transitions

Need exact Client contract for missing -> outstanding -> cached/failed/retryable/evicted transitions using terminal `Response::Success` / `Response::Failure` semantics.

## Failure behavior

Blocked by OQ-038, plus final ST-001-10 disconnect behavior.

## Determinism / ordering

Desired-set to request-batch ordering must be explicit if observable/tested; otherwise tests must assert set semantics only. OQ-038 must decide duplicate/order behavior first.

## Lifecycle / ownership

Core `Chunk::Collection` owns coordinate->immutable-Chunk storage and returns cheap Chunk values whose backing Cell content is shared immutably (DR-019).

A Client request Provider may return an empty valid 16x16x16 placeholder immediately after issuing the request. When canonical Server data arrives, the Client replaces the complete Collection Chunk value rather than mutating the placeholder. Any renderer/mesher still holding an older copied Chunk keeps its old immutable content alive.

OQ-038/DR-022 fixes the shared wire contract and terminal-response semantics. Exact eviction, retry timing, recycle threshold, disconnect handling, and stale/unsolicited response policy remain this ticket's own unresolved Client-coordinator specification.

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
- mixed terminal Response containing Success and Failure entries once that wire contract is finalized;
- failed coordinate carrying `Response::Failure::Code` and message;
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

Failed coordinates or failed requests cannot corrupt unrelated cached Chunks; successful Response entries remain authoritative for their own coordinates under the final refined contract.

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

Connection loss, terminal Failure entries, and malformed/diagnostic paths.

### Cross-system integration

Later mesher consumes copied immutable Chunk values from the Core Collection; integration must not bypass this coordinator.

### Performance

No numeric budget; structural request de-duplication/cache policy only after OQ-038.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-014](../../../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md)
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
- [OQ-038](../../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — resolved by DR-022 for the shared Chunk protocol.

## Completion evidence

Promote to Ready only when every cache/outstanding/retry/Success/Failure transition has an exact test fixture, the refined Response/diagnostic contracts are stable, and no Server-side view policy is introduced.
