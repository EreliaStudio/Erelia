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


### Approved sparse per-Chunk update semantics — 28 September 2026

Within an already `Available` Column, the targeted whole-Chunk update API is allowed to change sparse Column membership. It must support all three cases while preserving immutable Chunk values:

- a previously absent/known-empty Y receives a non-empty replacement Chunk and is inserted into the Column;
- an existing non-empty Y receives another non-empty replacement Chunk and is replaced atomically;
- an existing non-empty Y receives an empty replacement and is removed from the Column so the sparse representation continues to store only non-empty Chunks.

The owning Column remains `Available` throughout these targeted updates. These operations never create an Available Column when the Column itself is Absent or Pending; complete Column acquisition/publication remains the Provider boundary.


### Approved area-discovery architecture — 28 September 2026

This direction supersedes the earlier ST-001-11 notes that proposed making `Chunk::Collection` storage or its `Provider` column-based.

`Chunk::Collection` remains unchanged in its fundamental identity and ownership model:

- storage remains keyed by full `Chunk::Coordinate` / `spk::Vector3Int`;
- `Chunk::Collection::Provider` remains a single-Chunk provider;
- external lookup/state/request/replacement remain Chunk-coordinate based;
- DR-019 remains authoritative for Collection/Provider semantics.

The existing Chunk acquisition protocol also remains a specific-Chunk protocol:

- `Chunk::Protocol::Request` continues to request explicit full Chunk coordinates;
- `Chunk::Protocol::Response` continues to return canonical Chunk results for those coordinates;
- DR-022/ST-001-08/ST-001-09 are not redefined into a column protocol.

A separate discovery layer is introduced above Chunk acquisition.

The Client derives a horizontal X/Z interest area from its streaming center and configured horizontal view range. It sends that area to the Server without inventing candidate Y coordinates.

The Server owns an authoritative terrain/world spatial index or equivalent wrapper capable of resolving that horizontal area into the complete set of non-empty `Chunk::Coordinate` values that exist inside it. The discovery layer does not own Chunk values and does not replace `Chunk::Collection`; it describes Chunk identity only.

The Server returns that coordinate manifest to the Client. The Client compares each returned coordinate against its local `Chunk::Collection`:

- `Available`: already cached, do not request again;
- `Pending`: already being acquired, do not request again;
- `Absent`: include in the normal existing Chunk request flow.

Therefore the network flow is:

```text
Client horizontal interest area
    -> area-discovery request
    -> Server world/terrain spatial index
    -> manifest of non-empty Chunk::Coordinate values
    -> Client local Collection-state filtering
    -> existing Chunk::Protocol::Request for missing coordinates only
    -> existing Chunk::Protocol::Response
```

The Server's internal spatial index may be organized by columns or another implementation-specific structure, but it should store/return coordinate identity rather than raw pointers to Chunks. Chunk ownership remains in `Chunk::Collection`.

This design eliminates empty-sky Chunk probing while preserving the existing 3D Chunk cache and wire acquisition contracts.

The previously approved DR-015 multi-layer prototype columns remain useful discovery fixtures. Their non-empty Chunk coordinates must appear in the Server manifest for any queried area that contains those X/Z columns.



### Approved area-discovery request payload — 28 September 2026

The Client discovery request carries exactly these semantic fields:

```cpp
struct AreaRequest
{
    spk::Vector3 playerWorldPosition;
    std::int32_t centerX;
    std::int32_t centerZ;

    enum class Type : std::uint8_t
    {
        Circle = 0,
        Square = 1
    };

    Type type;
    std::int32_t size;
};
```

`playerWorldPosition` is the player's world-space position. `centerX` and `centerZ` are Chunk-column coordinates, not world-space coordinates. `type` selects the horizontal area geometry. `size` is the one shared shape parameter: for `Circle` it is the radius; for `Square` it is the half-size. No shape-specific trailing payload is serialized; both current shapes use the same fixed-size request layout.

`size` is a non-negative integer. `size == 0` is valid and resolves only the center/player Chunk column. Area boundaries are inclusive: a column exactly on the selected area's boundary belongs to the area. `Circle` membership is the Euclidean test in Chunk-column space: for `dx = columnX - centerX` and `dz = columnZ - centerZ`, a column belongs to the Circle when `dx * dx + dz * dz <= size * size`.


### Approved Area namespace and ownership split — 28 September 2026

`Area` is a namespace, not a value-owning Core struct/class.

Core owns the shared discovery wire-domain types under that namespace. The request stores the discovery data directly; there is no separate serializable `Area` value object to wrap inside it. The request therefore carries the already-approved fields directly: player world position, center Chunk-column X/Z, area type, and size.

The discovery reply is only the correlated series of non-empty `Chunk::Coordinate` values resolved for the requested area. It does not repeat the request geometry or own Chunk values.

`Area::Collection` is Server-only and belongs to the terrain Server implementation. It resolves an `Area::Request`/its decoded parameters into the authoritative list of non-empty Chunk coordinates. Core does not need to declare or implement `Area::Collection`; C++ namespace `Area` can be reopened by the Server-specific header for that type.

The resulting ownership is:

```text
Core:
    namespace Area
        Request
        Response

Terrain Server only:
    namespace Area
        Collection
```

`Area::Collection` stores/indexes Chunk coordinate identity only. It does not own Chunk values and does not store raw pointers into `Chunk::Collection`.


The Core discovery reply type is fixed as `Area::Response`, matching the existing Request/Response naming convention. It is correlated to the originating `Area::Request` through the Sparkle RequestID and contains only the returned `Chunk::Coordinate` sequence.


### Approved Column discovery namespace — 28 September 2026

The previously proposed `Area` discovery namespace is superseded. The domain is named `Column` because the abstraction describes and indexes terrain Chunk columns.

Core owns the shared Column discovery types:

```text
namespace Column
    Coordinate
    ColumnContent
    Request
    Response
```

`Column::Coordinate` identifies one Chunk column by X/Z. Its exact representation (`spk::Vector2Int` versus a dedicated X/Z struct) is still to be fixed. `Column::ColumnContent` represents the full set of non-empty `Chunk::Coordinate` values belonging to one Column.

`Column::Request` retains the already-approved discovery request fields directly: player world position, center Chunk-column X/Z, Circle/Square type, and non-negative size. The Circle/Square shape selects which Column coordinates around the center are queried; `size == 0` selects only the center Column and boundaries are inclusive.

`Column::Response` is correlated through the originating Sparkle RequestID and returns only the resolved Chunk-coordinate data; it does not carry Chunk values or repeat the request geometry.

The terrain Server owns `Column::Collection`. Core does not declare or implement the Collection. The Server reopens namespace `Column` and provides the authoritative mapping from `Column::Coordinate` to `Column::ColumnContent`. `Chunk::Collection` remains unchanged and continues to own Chunk values by full `Chunk::Coordinate`.

The resulting flow is:

```text
Column::Request
    -> Server Column::Collection
    -> Column::Response
    -> Client filters returned Chunk coordinates against Chunk::Collection
    -> existing Chunk::Protocol::Request for Absent coordinates only
```

All earlier references to `Area::Request`, `Area::Response`, or `Area::Collection` in ST-001-11 are superseded by these `Column::*` names.
