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
- Client-configured `viewRange` and `unloadRange` values loaded from the Client configuration file, allowing different Clients to choose different streaming ranges;
- one precomputed set of relative `Chunk::Coordinate` offsets for the cubic view region and one for the cubic unload region, generated once from configuration rather than recomputing distances after every player movement;
- applying those precomputed offsets to the current center Chunk coordinate to derive absolute desired/retained coordinates;
- an `unloadRange`, greater than or equal to `viewRange`, that determines which cached coordinates are eligible to be removed when they are outside the cubic unload region;
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

The desired set is derived by translating a precomputed relative-offset set by the designated streaming-center Chunk coordinate; it is not supplied as an arbitrary external coordinate collection. Both view and unload regions are axis-aligned cubes in Chunk space. Desired-coordinate ordering into request batches must still be explicit if observable/tested.

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
- View/unload range values come from Client configuration rather than protocol or Server configuration.
- View/unload membership offsets are precomputed once from configuration and reused across center-Chunk changes.
- Both regions are axis-aligned cubes in Chunk space.
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

Approved Collection lifetime support: ST-001-11 may extend generic `Chunk::Collection` with an explicit coordinate-removal operation required by Client `unloadRange` eviction. The Collection owns safe removal mechanics; the Client coordinator owns the policy deciding when a coordinate is outside the unload boundary. Exact removal behavior for Available and Pending entries must be covered by focused Core tests, including stale completion after removal.

Server-side bounded Chunk caching is explicitly deferred from ST-001-11 to a future Server scalability/resource-management Epic. That later work will own TerrainNode cache budgets, Server eviction-selection policy, active/in-flight considerations, regeneration/thrashing policy, observability, and load validation. ST-001-11 must not introduce a Server cache budget or Server eviction policy.

Approved range geometry/source contract: `viewRange` and `unloadRange` are strictly positive integer Client-owned configuration values. The Client precomputes the relative Chunk-coordinate offsets for both regions once after loading configuration. Both regions are axis-aligned cubes centered on the current player Chunk, with inclusive per-axis bounds `[-range, +range]`. Therefore a range `N` contains `(2N + 1)^3` Chunk coordinates. `unloadRange >= viewRange` is required. Configuration loading must reject zero/negative values and `unloadRange < viewRange`.

Approved column-storage direction: Chunk acquisition and storage are revised around sparse X/Z columns rather than independent 3D Chunk coordinates. A resolved Column contains every non-empty canonical Chunk for one (x,z) Chunk-column coordinate. Therefore, once a Column is Available, a missing Y entry is authoritatively known to be empty rather than unknown or Pending. External consumers such as meshing/rendering may continue querying the Collection by full Chunk::Coordinate; the Collection resolves that lookup through the stored Column. Provider acquisition and Client streaming/unloading operate at Column granularity.

This intentionally supersedes the earlier cubic 3D request-region direction for ST-001-11. Client interest becomes horizontal X/Z range selection over Columns; vertical Chunk selection belongs to the authoritative Column provider/generator. The protocol/Core/Server consequences must be revised explicitly rather than treated as Client-only behavior.

Still unresolved before Ready: desired-offset/request ordering and batching, request/retry/disconnect/response/recycle policies, and the exact public coordinator composition/API.


### Approved Provider granularity — 28 September 2026

`Chunk::Collection::Provider` is column-based. Provider acquisition receives an X/Z Column coordinate and returns the complete sparse Column containing every non-empty canonical Chunk for that Column. Collection lookup remains available by full `Chunk::Coordinate` for external consumers.

The temporary prototype terrain must also exercise multi-Chunk Columns rather than only one populated Y layer. Approved non-empty elevated layer membership is:

- Column `(3,3)`: additionally non-empty at Chunk Y = 1;
- Column `(3,4)`: additionally non-empty at Chunk Y = 1 and 2;
- Column `(4,3)`: additionally non-empty at Chunk Y = 1 and 2;
- Column `(4,4)`: additionally non-empty at Chunk Y = 1, 2 and 3.

These memberships define the complete elevated prototype fixture. Every added elevated Chunk at Y=1..3 is fully filled with Definition 1 (`cube`) Cells across all 16×16×16 local coordinates; no Air cells exist inside those added Chunks.


### Approved publication/update split — 28 September 2026

Provider acquisition and completion are Column-granular only: a Provider request targets one X/Z Column and successful Provider completion publishes the complete authoritative `Chunk::Column` for that coordinate atomically.

The Collection must nevertheless support targeted modification of one Chunk inside an already Available Column without requiring the Provider to re-emit/reacquire the entire Column. This targeted operation is whole immutable-Chunk replacement: callers construct a complete replacement `Chunk` and replace the value at one full `Chunk::Coordinate`. Published Chunk Cells are never mutated in place. The operation is valid only when the owning X/Z Column is already `Available`; it must not synthesize an `Available` Column from one isolated Chunk because an Available Column represents the complete known set of non-empty Y layers. Existing copied Chunk values remain valid through their shared immutable backing after replacement.
