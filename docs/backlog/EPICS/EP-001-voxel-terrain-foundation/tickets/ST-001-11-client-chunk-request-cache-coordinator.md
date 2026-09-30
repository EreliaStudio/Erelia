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

`Column::Coordinate` identifies one Chunk column by X/Z. Its exact representation (`spk::Vector2Int` versus a dedicated X/Z struct) is still to be fixed. `Column::Content` represents the full set of non-empty `Chunk::Coordinate` values belonging to one Column.

`Column::Request` retains the already-approved discovery request fields directly: player world position, center Chunk-column X/Z, Circle/Square type, and non-negative size. The Circle/Square shape selects which Column coordinates around the center are queried; `size == 0` selects only the center Column and boundaries are inclusive.

`Column::Response` is correlated through the originating Sparkle RequestID and returns only the resolved Chunk-coordinate data; it does not carry Chunk values or repeat the request geometry.

The terrain Server owns `Column::Collection`. Core does not declare or implement the Collection. The Server reopens namespace `Column` and provides the authoritative mapping from `Column::Coordinate` to `Column::Content`. `Chunk::Collection` remains unchanged and continues to own Chunk values by full `Chunk::Coordinate`.

The resulting flow is:

```text
Column::Request
    -> Server Column::Collection
    -> Column::Response
    -> Client filters returned Chunk coordinates against Chunk::Collection
    -> existing Chunk::Protocol::Request for Absent coordinates only
```

All earlier references to `Area::Request`, `Area::Response`, or `Area::Collection` in ST-001-11 are superseded by these `Column::*` names.


`Column::Content` is fixed as the Core semantic type representing the complete set of non-empty `Chunk::Coordinate` values belonging to one Column. The previous `Column::ColumnContent` working name is superseded.


### Approved Column coordinate/content/storage direction — 28 September 2026

`Column::Coordinate` is a dedicated Core type rather than an alias to `spk::Vector2Int`. It represents one terrain Chunk column with semantically named signed integer `x` and `z` components. It must support the comparison/hash behavior required by the chosen associative containers.

`Column::Content` is a Core type whose semantic value is the set of all non-empty `Chunk::Coordinate` values belonging to one Column. Its storage is `std::set<Chunk::Coordinate>`, giving uniqueness and deterministic coordinate ordering by construction.

The terrain Server owns `Column::Collection`. Its public lookup surface should follow ordinary map-like naming, including `tryGet`, `at`, and `operator[]`. `operator[]` is mutating and creates an empty `Column::Content` for an absent coordinate, matching standard associative-container expectations.

Column acquisition is separated behind a polymorphic asynchronous `Column::Provider` abstraction mirroring the existing Chunk provider contract:

```cpp
[[nodiscard]] virtual spk::Task<Column::Content>::Answer request(
    const Column::Coordinate& coordinate) = 0;
```

The Server prototype terrain implementation is intended to provide both Chunk acquisition and Column-content generation from the same deterministic terrain rules because the two outputs are correlated. The existing `PrototypeChunkProvider` should therefore implement both `Chunk::Collection::Provider` and `Column::Provider`; no new `Chunk::Generator` abstraction is introduced.


The earlier `Column::Generator` name was a terminology mistake and is superseded. The abstraction is `Column::Provider`. The existing Server `PrototypeChunkProvider` is intended to implement both `Chunk::Collection::Provider` and `Column::Provider` so Chunk data and Column metadata are derived from the same prototype terrain rules.


`Column::Provider` acquisition is asynchronous. Its public contract mirrors `Chunk::Collection::Provider`: `request(const Column::Coordinate&)` returns `spk::Task<Column::Content>::Answer`. The prototype terrain provider implements both asynchronous provider interfaces, using the shared WorkerPool-backed terrain generation path as appropriate.


### Approved Column::Collection parity with Chunk::Collection — 28 September 2026

`Column::Collection` mirrors the existing `Chunk::Collection` acquisition/state model for Column metadata.

It owns the same public state domain:

```cpp
enum class State
{
    Absent,
    Pending,
    Available
};
```

A request for an `Available` Column reuses the stored `Column::Content`; a request for a `Pending` Column reuses the existing pending Provider answer; only an `Absent` Column invokes `Column::Provider::request(...)`. Provider completion publishes the complete `Column::Content` atomically. Provider failure leaves the coordinate non-Available and is represented through the Collection request result in the same style as `Chunk::Collection`.

`Column::Collection` should duplicate the public acquisition/cache shape of `Chunk::Collection` rather than introduce a separate map-like API. Earlier discussion of `at(...)` and `operator[](...)` is superseded and those operations are not part of the target contract.

The request/batch API, pending reuse, completion lifetime protection, and stale-completion behavior should follow the existing `Chunk::Collection` implementation pattern unless a Column-specific semantic difference is explicitly approved later.


The target `Column::Collection` API is intentionally kept in structural parity with `Chunk::Collection`: state lookup, optional value lookup through `tryGet(...)`, asynchronous batched `request(...)`, Provider-backed Absent/Pending/Available handling, and whole-value replacement semantics where needed. No additional `at(...)` or `operator[](...)` surface is required unless a later concrete use case justifies it.


### Approved generic asynchronous cache abstraction — 28 September 2026

The duplicated asynchronous acquisition/cache mechanics currently implemented by `Chunk::Collection` are extracted into a generic Core abstraction named `Cache<TKey, TValue>`.

The template parameter naming is fixed as `TKey` and `TValue`.

Conceptually, `Cache<TKey, TValue>` owns the shared behavior currently implemented by `Chunk::Collection`:

- `State { Absent, Pending, Available }`;
- Provider-backed asynchronous acquisition through `spk::Task<TValue>::Answer`;
- `state(key)` and `tryGet(key)`;
- asynchronous batched `request(std::vector<TKey>)`;
- `BatchResult` with per-key acquired values and failures;
- reuse of already-Pending requests;
- reuse of already-Available values;
- whole-value `replace(key, value)`;
- generation-based stale-completion protection;
- shared Batch/Acquisition implementation details.

The intended generic Provider contract is:

```cpp
template <typename TKey, typename TValue>
class Cache
{
public:
    class Provider
    {
    public:
        virtual ~Provider() = default;

        [[nodiscard]] virtual spk::Task<TValue>::Answer request(
            const TKey& key) = 0;
    };
};
```

`Chunk::Collection` becomes the Chunk-domain alias/specialization over `Cache<Chunk::Coordinate, Chunk>`. The Server-side `Column::Collection` becomes the Column-domain alias/specialization over `Cache<Column::Coordinate, Column::Content>`. `Column::Provider` is the corresponding Provider type for that cache. This keeps `Column` as a namespace; no artificial domain-trait type is introduced solely to enable `Collection<Column>` syntax.


### Approved Column type and Server-only collection direction — 28 September 2026

`Column` is a Core type rather than a namespace. It acts as the domain scope for the shared Column discovery types, including `Column::Coordinate`, `Column::Content`, `Column::Request`, and `Column::Response`.

The Client does not need an authoritative Column cache. It consumes `Column::Response` transiently, filters the returned Chunk coordinates against its current desired area and local `Chunk::Collection`, and then requests only the still-valid Absent Chunks.

`Column::Collection` and `Column::Provider` remain terrain-Server concerns. Core may forward-declare those nested types on `Column` to preserve the `Column::Collection` / `Column::Provider` names, while their concrete definitions live only in the terrain Server and reuse the generic `Cache<TKey, TValue>` machinery with `Column::Coordinate` and `Column::Content`.


### Approved generic Collection<T> domain model — 28 September 2026

The earlier `Cache<TKey, TValue>` / domain-alias direction is superseded by a generic Core `Collection<T>` abstraction used directly at call sites.

`Collection<T>` owns its asynchronous acquisition contract, including its nested `Provider`. Callers therefore use `Collection<Chunk>::Provider` and `Collection<Column>::Provider`; `Chunk::Provider`, `Column::Provider`, `Chunk::Collection`, and `Column::Collection` aliases are not required.

Each domain type supplies the coordinate/key type and stored content type required by the generic Collection. The intended shape is:

```cpp
struct Chunk
{
    using Coordinate = spk::Vector3Int;
    using Content = Chunk;
    // ...
};

struct Column
{
    struct Coordinate
    {
        std::int32_t x;
        std::int32_t z;
    };

    using Content = std::set<Chunk::Coordinate>;

    class Request;
    class Response;
};
```

`Collection<T>` derives its public types from `T::Coordinate` and `T::Content` and provides the generic `State`, `BatchResult`, nested asynchronous `Provider`, `state`, `tryGet`, batched `request`, replacement, pending reuse, and stale-completion protection currently implemented by `Chunk::Collection`.

The prototype terrain provider therefore implements both acquisition contracts:

```cpp
class PrototypeChunkProvider final
    : public Collection<Chunk>::Provider,
      public Collection<Column>::Provider
{
    // ...
};
```

`Collection<Chunk>` may be instantiated by both Client and Server. `Collection<Column>` is instantiated only by the terrain Server even though the generic `Collection<T>` template and `Column` domain type live in Core.


The project owner explicitly approved this `Collection<T>` architecture. The intended migration is to replace the existing `Chunk::Collection` implementation with the generic `Collection<Chunk>` specialization, preserving its existing behavior while moving the shared asynchronous cache/acquisition mechanics into the generic template. `Collection<Column>` will reuse the same implementation on the terrain Server.


### Approved Column value and bundled response entry model — 28 September 2026

A `Column` value does not contain its own `Column::Coordinate`. The coordinate is the external key used by `Collection<Column>`, exactly as `Chunk::Coordinate` is external identity for a `Chunk` value.

The Column value itself contains only the complete sparse set of non-empty full `Chunk::Coordinate` values belonging to that X/Z terrain column.

The Column discovery request no longer uses Circle/Square geometry. It carries the player world-space position plus two `Column::Coordinate` corners describing the requested rectangular Column area. This supersedes the earlier Circle/Square/type/size request contract.

A `Column::Response` returns a bundle of keyed Column values. Each serialized Column entry is conceptually:

```text
[Column::Coordinate]
[serialized Column size]
[serialized Column content bytes]
```

The coordinate belongs to the response/collection entry, not to the `Column` value itself.

The response begins with an offset/index table that provides deserialization entry points at 50-Column intervals. The Client will use those offsets to partition response deserialization into multiple asynchronous tasks. The per-Column serialized `size` is the number of `Chunk::Coordinate` entries that immediately follow for that Column; it is not a byte length. The exact integer type used for this count and the exact response offset-table header layout remain to be fixed before implementation.


The per-Column serialized count is semantic: it is exactly the number of `Chunk::Coordinate` values in the serialized Column content. A reader consumes that count and then reads exactly that many contiguous Chunk coordinates for the entry.


### Approved Column::Response offset-table framing — 28 September 2026

`Column::Response` begins with an explicit integer count describing how many block offsets follow. The payload is therefore framed conceptually as:

```text
[offsetCount]
[offset 0]
[offset 1]
...
[offset offsetCount - 1]
[serialized Column stream]
```

Each offset identifies the start of one deserialization block containing up to 50 serialized Columns. Each serialized Column entry remains:

```text
[Column::Coordinate]
[Chunk-coordinate count]
[Chunk::Coordinate × count]
```

`offsetCount`, every block offset, and every per-Column Chunk-coordinate count are serialized as `std::uint32_t`.


All `Column::Response` framing integers are fixed as `std::uint32_t`: the offset-table count, each offset entry, and each serialized Column's `Chunk::Coordinate` count.


Each `Column::Response` block offset is absolute from byte 0 of the response payload. Offsets are not relative to the end of the offset table or to the start of the serialized Column stream. A reader can therefore seek directly to `payload.data() + offset` for the corresponding 50-Column deserialization block.


### Approved Column::Response block boundaries — 28 September 2026

Each deserialization block uses the absolute offset table to define an independent byte range. For block `i`:

```text
start = offsets[i]
end   = offsets[i + 1]
```

For the final block, `end` is the total `Column::Response` payload size. This lets the Client assign each block to an independent asynchronous deserialization task without scanning preceding Columns.


### Approved Column::Request corner ordering — 28 September 2026

`Column::Request` carries an already-normalized rectangular Column range. The first corner is the minimum X/Z corner and the second corner is the maximum X/Z corner; therefore a valid request satisfies `first.x <= second.x` and `first.z <= second.z`.

The Server must not silently reorder malformed corners. A request whose corners violate that ordering is invalid and must be rejected through the existing diagnostic-message mechanism, correlated with the originating RequestID when available. No Column acquisition is started for such an invalid request. The stable diagnostic contract mirrors malformed Chunk requests: severity `Networking::Diagnostic::Severity::Error`, translation key exactly `"Column_Request_Malformed"`, and the originating RequestID when available.


### Approved Column malformed-request diagnostic — 28 September 2026

Column request validation mirrors the existing malformed Chunk request path. A malformed `Column::Request`, including incorrectly ordered rectangular corners, is rejected with a generic `Networking::Diagnostic` using:

```text
severity = Networking::Diagnostic::Severity::Error
message  = "Column_Request_Malformed"
requestID = originating Column RequestID when available
```

No `Column::Error` message type is introduced for this case. No `Collection<Column>` acquisition starts after validation failure.


### Approved Column response ordering — 28 September 2026

`Column::Response` serializes Column entries in deterministic X-major order: sort by `Column::Coordinate::x` first, then by `Column::Coordinate::z` within each X value. The 50-Column deserialization blocks are cut from this ordered sequence.


### Approved Column message type IDs — 28 September 2026

`Networking::MessageType` preserves the existing values and adds the Column discovery messages as:

```cpp
ChunkRequest    = 1,
ChunkResponse   = 2,
ChunkError      = 3,
Diagnostic      = 4,
ColumnRequest   = 5,
ColumnResponse  = 6
```


### Approved Column acquisition failure semantics — 28 September 2026

`Column::Response` mirrors the existing terminal `Chunk::Protocol::Response` success/failure model for Provider acquisition outcomes.

A successful requested Column is represented as a success entry carrying the external `Column::Coordinate` key and the serialized `Column` value. A failed requested Column is represented as a failure entry carrying the same Column coordinate, a typed failure code, and a human-readable failure message. The initial failure-code domain mirrors Chunk acquisition with `AcquisitionFailed = 0`.

Ordinary per-Column Provider failure does not fail the whole request and does not use a diagnostic message. Successful and failed Columns may coexist in one terminal `Column::Response`.

A true outer Collection batch / TaskGroup aggregation failure that prevents construction of a valid terminal Column result mirrors the Chunk path: emit one correlated generic `Networking::Diagnostic` with severity `Error`, using the stable key `"Column_Request_Aggregation_Failure"`, and emit no `Column::Response` for that request.

No dedicated `ColumnError` message type is required by this contract. Malformed Column requests use generic `Networking::Diagnostic`, while terminal per-Column acquisition failures live inside `Column::Response`, exactly as terminal Chunk acquisition failures live inside `Chunk::Protocol::Response`.


### Approved Column-specific diagnostic direction — 28 September 2026

The earlier conclusion that no dedicated Column error message was needed is superseded. Column request-level validation and misuse diagnostics should mirror the existing Chunk diagnostic architecture.

`Column::Error` is a Column-specific specialization of `Networking::Diagnostic`, analogous to `Chunk::Protocol::Error`. It is distinct from terminal per-Column acquisition failures, which remain represented inside `Column::Response::Failure`.

The intended separation is:

```text
Malformed / invalid / semantically inconsistent Column request
    -> Column::Error / diagnostic path

Column Provider acquisition failure
    -> Column::Response::Failure

Outer aggregation failure preventing any valid terminal response
    -> generic Networking::Diagnostic
```

Examples of request-level Column errors include invalidly formatted requests and a player world position inconsistent with the requested Column range. The exact Column-specific payload carried by `Column::Error`, its stable diagnostic keys, and its message-type numeric assignment still need to be fixed explicitly.


### Approved Column::Error wire contract — 28 September 2026

`Column::Error` mirrors `Chunk::Protocol::Error` as closely as possible. It derives from `Networking::Diagnostic` and appends a Column-specific coordinate list encoded as:

```text
[Networking::Diagnostic prefix]
[coordinateCount:uint32]
[Column::Coordinate × coordinateCount]
```

The coordinate list is contextual and may be empty when a malformed request cannot be decoded far enough to recover safe Column coordinates.

Approved request-validation mappings are:

- malformed/invalidly encoded Column request -> severity `Error`, key `"Column_Request_Malformed"`; append any safely decoded relevant coordinates, otherwise an empty list;
- invalid corner ordering -> severity `Error`, key `"Column_Request_Malformed"`; append the two supplied corners in request order;
- player world position outside the requested rectangular Column range -> severity `Error`, key `"Column_Player_Outside_Request_Range"`; append the derived player `Column::Coordinate` followed by the request's minimum and maximum corners.

`Column::Error` requires the originating non-zero RequestID when it is available, matching the correlated Chunk-specific error model. These validation failures do not start `Collection<Column>` acquisition.

`Networking::MessageType` adds `ColumnError = 7`, after `ColumnRequest = 5` and `ColumnResponse = 6`.


### Approved Client/Server Collection<Column> provider architecture — 29 September 2026

The Column abstraction remains part of the shared Core domain and is not replaced by a separate Interest/AOI protocol abstraction. A Column is exactly the sparse vertical Chunk membership data the Client needs in order to know which full `Chunk::Coordinate` values are worth acquiring.

Both Client and Server may instantiate `Collection<Column>`, but with different Provider implementations:

```text
Client Collection<Column>
    -> network-backed Collection<Column>::Provider
    -> Column request message
    -> Server

Server Collection<Column>
    -> terrain-generation Collection<Column>::Provider
    -> authoritative Column value
```

The player/streaming controller does not manually own Column networking. It requests the desired Column coordinates from the Client `Collection<Column>`. The generic Collection returns cached Available values, reuses Pending acquisitions, and delegates only Absent values to its Provider. The Client Column Provider owns request/response correlation and settles the `spk::Task<Column>` Answers returned to the Collection when network responses arrive.

After a requested Column becomes Available, the controller iterates that Column's contained full `Chunk::Coordinate` values and submits them to the Client `Collection<Chunk>`. That Collection follows the same pattern: cached/Pending Chunks are reused and Absent Chunks are delegated to its Client network-backed Provider.

The terrain Server uses its local deterministic terrain Provider(s) for both `Collection<Column>` and `Collection<Chunk>`, so Column membership and generated Chunk data are derived from the same canonical terrain rules.

This architecture deliberately exposes Column as the actual shared terrain occupancy domain rather than introducing an additional protocol concept whose only purpose would be to hide Column from the Client.


### Approved single terminal response policy — 29 September 2026

For ST-001-11, one valid Collection protocol request produces exactly one terminal response message. The protocol does not yet split one logical response across multiple network messages.

The response payload may still be divided into internal serialized sections through section offsets so the Client can deserialize independent sections in parallel. `elementsPerSection` therefore controls payload sectioning only; it does not create additional response messages.

Any future support for splitting oversized logical responses across multiple network messages is explicitly deferred. No response index/count/final-message framing is introduced in ST-001-11.

Accordingly, generic request treatment should conceptually return one response rather than `std::vector<Response>`:

```cpp
Response treatRequest(
    const Request& request,
    const ResponsePolicy& policy);
```

`ResponsePolicy` may still contain limits such as maximum accepted/requested element count and elements per serialized section. Exceeding a protocol/request limit is handled as validation/diagnostic behavior rather than by automatically emitting multiple response messages.


### Approved Provider-owned pending acquisition state — 30 September 2026

The generic Collection must not duplicate pending acquisition Answers inside its own storage. Pending acquisition state belongs to the Provider.

The Collection storage owns only data that has actually become available. Conceptually, the Collection state for a key is derived as follows:

```text
Collection contains key
    -> Available

Collection does not contain key
and Provider reports key as pending
    -> Pending

Collection does not contain key
and Provider does not report key as pending
    -> Absent
```

The Provider therefore owns the pending Answer/correlation state required by its acquisition mechanism. In particular, a network-backed RequestingProvider retains the Answers needed to represent outstanding requests and correlate received responses. The Collection does not retain a second copy of those Answers merely to represent Pending state.

Repeated acquisition of an already-pending key must reuse the Provider's existing Answer rather than create a duplicate acquisition.

Successful acquisition performs an explicit ownership handoff from Provider-pending state to Collection-available state:

```text
Provider owns pending acquisition
    -> Task completes successfully
    -> Collection completion callback acquires write access to Collection storage
    -> acquired element is published into Collection storage
    -> Collection write access is released
    -> Provider pending Answer/correlation may be removed
```

The ordering is significant: the available value must be published before the Provider stops reporting the key as Pending. There must be no observable intermediate state in which the key is absent from the Collection and no longer pending in the Provider, because that could incorrectly start a duplicate request.

On acquisition failure, no value is published into the Collection. Once the failed acquisition is settled, the Provider removes its pending Answer/correlation and the key becomes Absent/retryable.

Collection storage remains synchronized through its existing `spk::ProtectedData<Storage>`. Task completion may therefore publish acquired data from a WorkerPool thread by obtaining `Storage::write()`; updater/rendering-side reads continue to use the corresponding protected read access. No additional Collection-wide asynchronous handoff or second synchronization layer is required.

This supersedes earlier planning text that stored an `std::optional<Task<TElement>::Answer>` inside each Collection entry to represent Pending state.


### Approved generic Provider pending Answer/Contract lifecycle — 30 September 2026

Pending acquisition bookkeeping is implemented once in the generic `Collection<TKey, TElement>::Provider` base class rather than separately in Chunk, Column, GeneratingProvider, or RequestingProvider implementations.

For every pending key, the generic Provider retains both the acquisition Answer and the completion subscription Contract:

```cpp
struct Pending
{
    spk::Task<TElement>::Answer answer;
    spk::Task<TElement>::Answer::CompletionContract contract;
};
```

The Provider's pending state is synchronized and keyed by `TKey`. The Collection itself does not retain a duplicate pending Answer.

The generic Provider is bound to the owning Collection storage only for the generic completion transition. Derived providers do not publish directly into Collection storage.

Pending registration uses the following exact order to remain correct when a Task completes immediately:

1. create/obtain the acquisition Answer;
2. insert the pending entry containing that Answer before subscribing to completion;
3. subscribe to the Answer completion callback;
4. after subscription returns, reacquire the pending bookkeeping state;
5. if the key is still pending, move the returned Contract into that pending entry;
6. if the key has already completed and the completion callback already removed the pending entry, discard the returned Contract.

This order avoids a race where completion occurs synchronously during `subscribeToCompletion(...)` before the Provider has established its pending state.

The generic completion callback performs the Provider-to-Collection state handoff:

- on successful completion, acquire Collection `Storage::write()`, publish the completed `TElement` under its key, release the Collection writer, then remove the Provider's pending entry;
- on failed completion, publish nothing and remove the Provider's pending entry;
- the successful publication must occur before the key stops being reported as Pending, preventing an observable Absent/non-Pending gap that could start a duplicate request.

Removing the pending entry destroys its retained completion Contract. Sparkle's ContractProvider supports Contract resignation/destruction during its own callback by deferring the corresponding registration removal until callback dispatch completes.

`GeneratingProvider` and `RequestingProvider` therefore inherit the same generic pending Answer/Contract reuse and completion behavior. Their only specialization is how a new acquisition is started. `RequestingProvider` additionally owns network-specific request correlation and message batching; it does not duplicate the generic pending lifecycle.


### Approved Diagnostic payload-only networking contract — 30 September 2026

`Networking::Diagnostic` is no longer an independently routable network message and therefore owns no `Networking::MessageType` / MessageID of its own.

It becomes a reusable serializable diagnostic value containing only the diagnostic semantics:

```cpp
class Networking::Diagnostic
{
public:
    enum class Severity : std::uint8_t
    {
        Trace = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    };

    // severity + message payload
};
```

`Networking::Diagnostic` must support insertion/extraction through `spk::Message` so it can be embedded by higher-level protocol messages:

```cpp
spk::Message& operator<<(
    spk::Message& message,
    const Networking::Diagnostic& diagnostic);

const spk::Message& operator>>(
    const spk::Message& message,
    Networking::Diagnostic& diagnostic);
```

The request correlation ID remains the `spk::Message::RequestID` of the enclosing protocol message; it is not part of the `Diagnostic` value.

Collection request-level validation, misuse, malformed-request, and outer aggregation failures are carried by the Collection family's Error message. That Error message embeds a `Networking::Diagnostic` payload and may additionally carry the contextual Collection keys that can safely be identified. Terminal per-element acquisition failures remain inside the normal Collection Response failure section and are not converted to Error messages.

Consequently, the standalone `Networking::MessageType::Diagnostic` entry is removed. Earlier ST-001-11 planning text that emits a generic standalone `Networking::Diagnostic` message for malformed or aggregation failures is superseded by this payload-only Diagnostic contract.


### Approved generic Collection protocol and API freeze — 30 September 2026

The generic Collection networking and public API are now frozen for ST-001-11.

#### Collection MessageType declaration

Collection protocol MessageIDs are declared as one contiguous enum block per Collection family. No family/subtype bit partitioning is introduced at this stage.

```cpp
#define COLLECTION_MESSAGES(Name) \
    Name##Request,                 \
    Name##Response,                \
    Name##Update,                  \
    Name##Error

enum class MessageType : spk::Message::Type
{
    Invalid = 0,

    COLLECTION_MESSAGES(Chunk),
    COLLECTION_MESSAGES(Column)
};
```

The declaration is append-only: existing Collection families and the ordering of the generated Request/Response/Update/Error entries must not be reordered once published.

Both `TKey` and `TElement` must be serializable through `spk::Message` insertion/extraction operators.

#### Generic Request wire contract

A Collection Request uses the normal `spk::Message` header for MessageID and non-zero RequestID. Its payload contains only the serialized requested keys:

```text
[TKey]
[TKey]
[TKey]
...
```

No separate key count is required while the serialized TKey contract lets the parser consume complete keys until the end of the payload.

A network-backed RequestingProvider owns RequestID generation and request splitting. If one logical Collection request exceeds `maxElementsPerRequest`, the Provider emits multiple network Request messages, each with its own RequestID, while the caller still observes one logical TaskGroup.

#### Generic Response wire contract

The Response preserves the current Chunk success/failure section model:

```text
[failureOffset:uint32]

Success section:
    [TKey][TElement]
    [TKey][TElement]
    ...

Failure section:
    [TKey][Failure]
    [TKey][Failure]
    ...
```

The RequestID remains in the `spk::Message` header.

The initial generic Failure model is:

```cpp
struct Failure
{
    enum class Code : std::uint8_t
    {
        AcquisitionFailed = 0
    };

    Code code;
    std::string message;
};
```

A received Response entry is applied only while the RequestingProvider still owns the matching Pending acquisition. A late/stale Response for a key whose Pending state has already been removed or superseded is ignored.

A Response Failure settles the corresponding `spk::Task<TElement>` through the generic network-failure conversion path.

#### Generic Update wire contract

Update is unsolicited canonical Server state and carries no acquisition correlation. Its `spk::Message::RequestID` is zero.

Its payload mirrors the Response section layout:

```text
[removeOffset:uint32]

Set section:
    [TKey][TElement]
    [TKey][TElement]
    ...

Remove section:
    [TKey][Failure]
    [TKey][Failure]
    ...
```

Update semantics are authoritative:

```text
Set + Absent
    -> insert canonical value

Set + Available
    -> replace canonical value

Set + Pending
    -> Update wins
    -> settle the existing Task successfully with the Update value
    -> normal Provider completion publishes the value
    -> later original Response is ignored
```

```text
Remove + Available
    -> remove value

Remove + Absent
    -> no-op

Remove + Pending
    -> Update wins
    -> settle the existing Task as Failed through the same Failure model
       used by Response::Failure
    -> any containing TaskGroup becomes Failed according to normal Sparkle semantics
    -> later original Response is ignored
```

#### Generic Error wire contract

Each Collection family owns an Error MessageID. Collection request validation, misuse, malformed-request handling, and outer aggregation failure use that Error message.

```text
Header:
    MessageID
    RequestID

Payload:
    [Networking::Diagnostic]
    [keyCount:uint32]
    [TKey × keyCount]
```

The contextual key list may be empty when no safe key can be recovered.

Terminal per-element acquisition failures remain inside the normal Response Failure section and do not use Error messages.

`Networking::Diagnostic` is a serializable payload value only and has no MessageID of its own, as fixed by the preceding Diagnostic decision.

#### Frozen Collection<TKey, TElement> public API

The intended public shape is:

```cpp
template <
    MessageSerializable TKey,
    MessageSerializable TElement>
class Collection
{
public:
    enum class State
    {
        Absent,
        Pending,
        Available
    };

    class Provider;
    class GeneratingProvider;
    class RequestingProvider;
    class Updater;

    [[nodiscard]]
    State state(const TKey& key) const;

    template <typename TCallback>
    bool tryRead(
        const TKey& key,
        TCallback&& callback) const;

    [[nodiscard]]
    spk::Task<TElement>::Answer request(
        const TKey& key);

    [[nodiscard]]
    spk::TaskGroup<TElement>::Answer request(
        const std::vector<TKey>& keys);

    void insert(
        const TKey& key,
        TElement element);

    void replace(
        const TKey& key,
        TElement element);

    void remove(
        const TKey& key);
};
```

`state(key)` is derived from available Collection storage first, then Provider Pending state, otherwise Absent.

`tryRead` acquires the Collection's `spk::ProtectedData` Reader and keeps that Reader alive for the full callback invocation. The callback receives `const TElement&`. No generic element copy is required, and callers must not retain the reference beyond the callback.

Single-key `request(key)` follows:

```text
Available
    -> return an already-completed Answer containing the current value

Pending
    -> reuse and return the existing Provider Answer

Absent
    -> start Provider acquisition and return the new Answer
```

Batch `request(keys)` returns `spk::TaskGroup<TElement>::Answer` with exactly one child Answer per requested key. Input order is preserved exactly: `keys[i] <-> group.at(i)`. Available children are immediately completed, Pending children reuse existing Answers, and Absent children start new acquisitions.

Manual Collection mutation is authoritative over Pending acquisition:

```text
insert(key, element):
    Available -> throw spk::Exception
    Pending   -> invalidate/fail the Pending acquisition, then insert
    Absent    -> insert

replace(key, element):
    Available -> replace
    Pending   -> invalidate/fail the Pending acquisition, then replace only if an Available
                 value exists under the operation's strict replace precondition
    Absent    -> throw spk::Exception

remove(key):
    Available -> remove
    Pending   -> invalidate/fail the Pending acquisition and remove any Available value
    Absent    -> no-op
```

A late network Response after insert/replace/remove does not republish stale data because it no longer finds the corresponding active Pending acquisition.

### Approved Provider, RequestingProvider, and Updater freeze — 30 September 2026

Pending acquisition lifecycle is generic and owned by `Collection<TKey, TElement>::Provider`. Collection storage contains only Available data.

Conceptually, each Pending entry retains the acquisition Answer and completion subscription Contract:

```cpp
struct Pending
{
    spk::Task<TElement>::Answer answer;
    spk::Task<TElement>::Answer::CompletionContract contract;
};
```

The Provider owns synchronized TKey -> Pending lookup, Pending reuse, completion subscription, successful publication into Collection storage, failure cleanup, and Pending state reporting.

Pending registration keeps the already-approved ordering required for synchronous completion safety:

1. create/obtain the acquisition Answer;
2. insert Pending before subscribing;
3. subscribe to completion;
4. reacquire Pending bookkeeping;
5. if the entry still exists, store the returned Contract;
6. otherwise discard the returned Contract because completion already removed the Pending entry.

On successful completion, the Provider publishes the element into Collection storage before removing Pending state. On failed completion, no value is published and Pending is removed.

#### GeneratingProvider

`GeneratingProvider` reuses the generic Provider lifecycle and specializes only how a new acquisition is produced locally.

#### RequestingProvider

`Collection<TKey, TElement>::RequestingProvider` is a generic concrete network-backed provider. No Chunk- or Column-specific subclass is required when TKey and TElement satisfy the message serialization contract.

It is configured with at least:

```cpp
RequestingProvider(
    spk::Message::Type requestMessageID,
    std::size_t maxElementsPerRequest);
```

The request MessageID is constructor configuration rather than a mutable post-construction requirement because a RequestingProvider without an outgoing Request MessageID is not usable.

The RequestingProvider knows only the MessageID it emits for Request messages. It does not own the Response, Update, or Error MessageIDs used for incoming routing.

For every new network acquisition it creates a real `spk::Task<TElement>` and retains the corresponding Answer through generic Pending state. The acquisition Task remains `Task::Status::Pending` while waiting for the network; no WorkerPool thread blocks waiting for the response.

RequestingProvider maintains both:

```text
TKey -> active Pending acquisition
RequestID -> keys/acquisitions represented by one emitted network Request
```

The first mapping supports Collection state, duplicate suppression, mutation, and Update arbitration. The second supports response correlation.

For a batch request the Provider reuses already-Pending Answers, creates Tasks only for new Absent keys, partitions those new acquisitions by `maxElementsPerRequest`, generates one RequestID per emitted network message, serializes each TKey through `operator<<`, and sends the requests. The caller still receives one TaskGroup representing the full logical input order.

Incoming Response routing is external. The dispatcher invokes:

```cpp
requestingProvider.receive(message);
```

The Provider may submit one WorkerPool parsing job for the complete Response message. That job parses all Success and Failure entries and settles the already-created per-element Tasks:

```text
Success(key, element)
    -> if matching active Pending still exists
       -> task.validate(element)
    -> otherwise ignore as stale

Failure(key, failure)
    -> if matching active Pending still exists
       -> task.fail(convertFailure(failure))
    -> otherwise ignore as stale
```

One parsed Response may therefore settle many individual Tasks. The generic Provider completion path subsequently publishes successful values into Collection storage.

#### Updater

`Collection<TKey, TElement>::Updater` is generic and requires no Chunk- or Column-specific subclass.

The Updater is bound to one Collection instance. It does not expose or require direct public access to the Provider.

Incoming Update routing is external:

```text
ChunkUpdate  -> chunkCollectionUpdater.receive(message)
ColumnUpdate -> columnCollectionUpdater.receive(message)
```

The Updater parses generic Set/Remove entries and delegates semantic application to private Collection operations. The Collection decides whether the operation mutates storage directly or settles an active Provider Pending Task.

The ownership relationship is therefore:

```text
Updater
    -> Collection

Collection
    -> Storage
    -> Provider

Updater
    does not manage Provider internals directly
```

Update always wins over a manually requested Pending acquisition. Taking ownership of a Pending acquisition for Response versus Update settlement must be atomic so that only one path can settle that Task. Once Update has won, the later original Response finds no active Pending acquisition and is ignored.


### Approved disconnect and message-dispatch integration — 30 September 2026

All active network-backed Collection acquisitions become terminally Failed when the underlying Client connection is lost.

For every `Collection<TKey, TElement>::RequestingProvider` attached to that connection:

```text
Client disconnection
    -> every active Pending acquisition is settled as Failed
    -> no value is published into Collection storage
    -> generic Provider completion removes Pending state
    -> any TaskGroup containing one of those Tasks becomes Failed through normal Sparkle semantics
    -> a later explicit request for the same Absent key may start a fresh acquisition
```

No Pending Task survives a disconnected transport waiting for an old response. Responses from the previous connection that arrive after the Pending state has been cleared cannot republish data because they no longer match an active acquisition.

The failure used for this transition is a local transport/disconnection failure, not a fabricated Server `Response::Failure`.

#### Message dispatch integration

Sparkle Version-0.1.3 does not expose MessageID-specific callbacks on `spk::Client` or `spk::Server`. Their networking contract is queue-based:

- `spk::Client::messages()` exposes a FIFO of `spk::Message`;
- `spk::Server::messages()` exposes a FIFO of `spk::ReceivedMessage`;
- connection/disconnection notifications are exposed separately through retained ContractProvider subscriptions.

Erelia therefore keeps Sparkle unchanged and owns a thin message-dispatch layer above those queues.

That layer drains the appropriate Sparkle message queue, inspects each `spk::Message::Type`, and forwards the message to the parser/receiver object registered for that MessageID.

Application composition is intentionally explicit. Each executable composition root / main-level application setup binds protocol MessageIDs to the concrete objects that know how to parse them. For example, on the Client side:

```text
ChunkResponse  -> Collection<Chunk::Coordinate, Chunk>::RequestingProvider
ChunkUpdate    -> Collection<Chunk::Coordinate, Chunk>::Updater
ChunkError     -> Collection<Chunk::Coordinate, Chunk> network error handling

ColumnResponse -> Collection<Column::Coordinate, Column>::RequestingProvider
ColumnUpdate   -> Collection<Column::Coordinate, Column>::Updater
ColumnError    -> Collection<Column::Coordinate, Column> network error handling
```

The parser objects themselves do not know the incoming MessageID used to reach them. Routing knowledge belongs to the executable/application composition layer.

The Server side follows the same principle while preserving `spk::ReceivedMessage::emitter`: the dispatcher routes by the contained message type and forwards the originating `ConnectionID` together with the message to the registered request parser/handler.

This keeps responsibilities separated:

```text
spk::Client / spk::Server
    -> transport + receive FIFO + connection lifecycle

Erelia dispatcher
    -> MessageID routing

RequestingProvider / Updater / request handler
    -> protocol parsing and semantic handling

main / application composition root
    -> binds MessageID to parser object
```

No Collection protocol parser needs its own network receive loop, and multiple parsers must never independently drain the same Sparkle receive FIFO.


### Approved network-manager MessageID subscription API — 30 September 2026

The Erelia Client and Server network-manager widgets expose MessageID-specific subscriptions directly. Their role is inspired by the former JGL2 ClientManager/ServerManager update widgets, but adapted to the current Sparkle queue-based networking and Erelia Service access.

The managers own no Client/Server transport instance. They access the transport through the relevant Erelia Service and drain its receive queue during their update step.

Each MessageID owns a `spk::ContractProvider`. Subscribing returns a retained Contract; destroying that Contract removes the corresponding callback exactly like other Sparkle subscription APIs.

The Client-side API is conceptually:

```cpp
class ClientNetworkManager : public spk::Widget
{
public:
    using MessageProvider =
        spk::ContractProvider<const spk::Message&>;
    using MessageCallback =
        MessageProvider::callback_type;
    using MessageContract =
        MessageProvider::Contract;

    [[nodiscard]]
    MessageContract subscribeToMessage(
        spk::Message::Type messageID,
        MessageCallback callback);
};
```

During update:

```text
Service::client().messages()
    -> drain messages
    -> lookup MessageProvider by message.type()
    -> trigger provider with const spk::Message&
```

The Server-side API preserves the sender identity:

```cpp
class ServerNetworkManager : public spk::Widget
{
public:
    using MessageProvider =
        spk::ContractProvider<
            spk::ConnectionID,
            const spk::Message&>;
    using MessageCallback =
        MessageProvider::callback_type;
    using MessageContract =
        MessageProvider::Contract;

    [[nodiscard]]
    MessageContract subscribeToMessage(
        spk::Message::Type messageID,
        MessageCallback callback);
};
```

During update:

```text
Service::server().messages()
    -> drain spk::ReceivedMessage values
    -> lookup MessageProvider by received.message.type()
    -> trigger provider(received.emitter, received.message)
```

The exact Service ownership/plumbing for the Server transport must remain consistent with the Server composition already in use; the network manager itself must not instantiate or own a second `spk::Server`.

Application composition retains the returned Contracts. Example bindings are conceptually:

```cpp
_chunkResponseContract =
    networkManager.subscribeToMessage(
        static_cast<spk::Message::Type>(
            Networking::MessageType::ChunkResponse),
        [&chunkProvider](const spk::Message& message)
        {
            chunkProvider.receive(message);
        });

_chunkUpdateContract =
    networkManager.subscribeToMessage(
        static_cast<spk::Message::Type>(
            Networking::MessageType::ChunkUpdate),
        [&chunkUpdater](const spk::Message& message)
        {
            chunkUpdater.receive(message);
        });
```

The same mechanism is used for Column Response/Update/Error messages and for Server-side Collection Request handlers.

Multiple subscribers to one MessageID are permitted by the ContractProvider model. A parser must not independently drain `spk::Client::messages()` or `spk::Server::messages()`; the network-manager widget is the unique queue-draining point for its transport.
