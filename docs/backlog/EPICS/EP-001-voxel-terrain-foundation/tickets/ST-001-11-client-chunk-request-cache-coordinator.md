# ST-001-11 — Client Chunk request/cache coordinator

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Client
**Test suite(s):** EreliaClientTestSuite

## Intent

Own Client-side Chunk identity, player-centered desired-region tracking, missing-Chunk request batching, outstanding-request tracking, canonical response replacement, and retention/eviction behavior for EP-001 inspection.

## User / system value

The Client can request only the terrain it needs and maintain coherent local canonical Chunk data while moving the inspection position.

## Starting state / prerequisites

- Depends on ST-001-06 Core Chunk::Collection/Provider foundation, ST-001-08, and completed ST-001-10 Client connection lifecycle.
- OQ-038/DR-022 establish the batched Client-driven direction and RequestID correlation. ST-001-09 is Done and fixes terminal `Response::Success` / `Response::Failure`, failure-code/string encoding, and the generic diagnostic-message contract.
- Sparkle Version-0.1.3 provides `spk::Engine`, `spk::Entity3D`, `spk::Transform3D::subscribeToEdition(...)`, and world-frame transform positions. ST-001-11 uses those existing engine contracts for its streaming center rather than inventing an Erelia-local transform notification mechanism.

## Product ownership

Client owns loading/view policy, request coordination, and Client-side cache state.

## Allowed dependencies

EreliaClientLibrary, EreliaCore, Sparkle Client networking, standard library.

## Forbidden dependencies

Server-selected view radius, Client terrain generation as canonical data, Server/Client cross-dependency, rendering ownership inside the cache.

## Owned behavior

The final ticket should own:

- one designated Client `spk::Entity3D` acting as the Chunk-streaming center;
- a retained subscription to that entity's `spk::Transform3D` edition notifications;
- deriving the current center Chunk coordinate from the entity's world position;
- recomputing streaming demand only when the derived center Chunk coordinate changes;
- a `viewRange` that determines which Chunk coordinates around the center must be available/requested;
- an `unloadRange`, greater than or equal to `viewRange`, that determines which cached coordinates are eligible to be removed when they are too far from the center;
- identification of missing cached coordinates;
- batching request coordinates under final limits;
- outstanding-request suppression/retry behavior;
- insertion/replacement of canonical responses;
- retention/eviction policy;
- disconnect/outstanding-request cleanup;
- application of terminal `Response::Success` / `Response::Failure` entries once the refined protocol contract is finalized.

## Explicitly not owned

How the streaming-center entity is moved or controlled, meshing/rendering, Server generation, production interest management. ST-001-14 may later move the same designated entity through temporary free-flight input without taking ownership of Chunk streaming policy.

## Public contract

Blocked by remaining Client policy plus the refined ST-001-09 protocol details: exact floating world-position -> Chunk-coordinate conversion, exact view/unload range geometry and boundary semantics, duplicate outstanding suppression, cache retention/eviction mechanics, request batching policy, retry behavior, Response Failure handling, generic diagnostic handling where relevant, and response replacement rules.

## Invariants

- Cache identity is by Chunk coordinate.
- Server response data is canonical.
- Client never treats a locally fabricated/placeholder Chunk as authoritative Server terrain.
- A received response remains associated with its declared coordinate.

## State transitions

Designated `spk::Entity3D` Transform edition -> derive world-space center Chunk coordinate -> if unchanged, no streaming-region transition -> if changed, recompute the view region and unload boundary -> request missing desired coordinates and evict coordinates outside the approved unload boundary. Exact missing -> outstanding -> cached/failed/retryable/evicted transitions still require the remaining policy decisions.

## Failure behavior

Blocked only by this ticket's remaining Client coordinator policy: duplicate-outstanding suppression, cache retention/eviction, request retry behavior, disconnect/outstanding cleanup, and stale/unsolicited response handling. OQ-038 and the ST-001-10 connection/disconnect lifecycle are resolved.

## Determinism / ordering

The desired set is derived from the designated streaming-center Chunk coordinate plus the approved `viewRange`; it is not supplied as an arbitrary external coordinate collection. Desired-coordinate ordering into request batches must still be explicit if observable/tested.

## Lifecycle / ownership

Core `Chunk::Collection` owns coordinate->immutable-Chunk storage and returns cheap Chunk values whose backing Cell content is shared immutably (DR-019).

Pending acquisition uses the implemented Core Collection `Absent / Pending / Available` state; no fake empty Chunk placeholder is published. When canonical Server data arrives, the Client publishes/replaces the complete Collection Chunk value. Any renderer/mesher still holding an older copied Chunk keeps its old immutable content alive.

The coordinator retains the `spk::Transform3D::OnEditionContract` for as long as it observes the designated entity. ST-001-14 may later change that transform through input, but does not own the subscription or streaming policy.

OQ-038/DR-022 fixes the shared wire contract and terminal-response semantics. Exact floating-position conversion, range geometry/boundaries, eviction mechanics, retry timing, recycle threshold, disconnect handling, and stale/unsolicited response policy remain this ticket's own unresolved Client-coordinator specification.

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
- transform edits that stay inside the current center Chunk and therefore do not recompute the streaming region;
- transform edits crossing positive and negative Chunk boundaries;
- exact `viewRange` membership once its geometry is approved;
- exact `unloadRange` retention/eviction boundary once its geometry is approved;
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

Promote to Ready only when the streaming-center conversion/range boundaries and every cache/outstanding/retry/Success/Failure transition have exact test fixtures, the refined Response/diagnostic contracts are stable, and no Server-side view policy is introduced.

### Approved ST-001-11 direction — 28 September 2026

The project owner selected a player-centered Client streaming model:

- the Client introduces/uses a Sparkle game `Engine` containing a designated `spk::Entity3D` streaming-center object;
- the coordinator observes that entity through a retained subscription to its `spk::Transform3D` edition contract;
- the coordinator reads the transform in world space and derives the Chunk coordinate containing that position;
- transform edits that do not change the containing Chunk coordinate do not trigger a streaming-region change;
- crossing into another Chunk causes the Client to derive the required surrounding Chunk set from `viewRange`;
- `unloadRange` is required to be greater than or equal to `viewRange` and provides hysteresis so Chunks may remain cached after leaving the immediate view region, while Chunks beyond the unload boundary are removed to bound memory/resource growth;
- movement/input ownership is separate: ST-001-14 may later move this entity, while ST-001-11 owns observation and Chunk streaming consequences.

Approved floating-point conversion: each component of the streaming-center world position is mathematically floored to its containing global terrain Cell coordinate, then converted through the existing `Chunk::toCoordinate(...)` floor-division contract. This therefore preserves exact negative-boundary behavior (for example `-0.1 -> cell -1 -> chunk -1`).

Still unresolved before Ready: exact three-dimensional range shape/distance metric and inclusive boundaries, numeric/configuration source for both ranges, plus the previously listed request/retry/disconnect/response/recycle policies.
