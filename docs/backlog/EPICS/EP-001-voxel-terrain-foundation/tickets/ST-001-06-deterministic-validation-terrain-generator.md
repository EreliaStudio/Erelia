# ST-001-06 — Deterministic validation terrain provider and Chunk collection foundation

**Status:** Ready
**Epic:** EP-001
**Production target(s):** Core + Server
**Test suite(s):** EreliaCoreTestSuite; EreliaServerTestSuite

## Intent

Introduce the shared immutable Chunk construction/storage/provider foundation in Core and implement the first Server-owned deterministic `PrototypeChunkProvider` using the exact DR-015 validation terrain.

This ticket deliberately establishes reusable Chunk acquisition machinery without implementing the future Client network provider.

## User / system value

Server code can request/cached deterministic canonical Chunks through one Core Collection/Provider abstraction, while the same immutable-value/lifetime model is suitable for later Client request-driven replacement and concurrent render/update use.

The exact validation terrain also supplies stable semantic input for later networking, meshing and visual-validation tickets.

## Starting state / prerequisites

- ST-001-01 through ST-001-05 are Done and merged.
- OQ-039 is Resolved; DR-015 now contains the exact validation-scene fixture.
- DR-018 defines Shape/Definition/Catalog semantics and the first Shape resources.
- DR-019 supersedes the old deep-copy Volume ownership and same-buffer network-decode reuse details and defines the approved Chunk Builder/Collection/Provider architecture.
- This branch already contains one ST-001-12 documentation clarification for later multi-Shape meshing fixtures; it is not production mesher implementation.

## Product ownership

### Core

Core owns reusable representation and acquisition machinery shared by Server and future Client:

- shared immutable `Voxel::Volume` backing-content semantics;
- `Chunk` construction invariants and `Chunk::Builder`;
- `Chunk::Collection` and nested `Chunk::Collection::Provider`;
- Collection lifetime/thread-safety contract;
- generic JSON Catalog support for aggregate and single-element resource files.

### Server

Server owns the canonical temporary validation terrain implementation:

- concrete `PrototypeChunkProvider`;
- deterministic mapping from requested Chunk coordinate to exact Chunk Cells.

### Client

No Client production implementation is owned by this ticket.

Future Client work may implement a request-driven Provider that returns an empty valid Chunk placeholder and later replaces the Collection entry with Server-canonical data. ST-001-11/OQ-038 own the detailed request/outstanding/retry/response semantics.

## Allowed dependencies

- Core: C++ standard library, Sparkle Core, existing Erelia Core voxel contracts.
- Server: EreliaCore, C++ standard library, Sparkle Core.
- Existing project JSON/resource infrastructure.

## Forbidden dependencies

- Client/graphics code;
- Client request provider implementation;
- Chunk protocol message IDs/handlers;
- dedicated Chunk wire codec;
- cache eviction/view-radius policy;
- terrain meshing/rendering;
- production noise/biomes/world generation;
- dungeon generation;
- persistence/dynamic terrain editing;
- archived generator/collection code as an authority.

## Owned behavior

### 1. Shared immutable `Voxel::Volume` content

Implement DR-019:

- built Volumes share immutable backing Cell storage across copies;
- Builder remains the mutable pre-build owner of a pooled Buffer Lease;
- build publishes immutable shared content;
- Volume copies do not deep-copy Cell arrays;
- copies remain valid independently when another Volume value is moved/replaced/destroyed;
- moved-from Volume remains canonical default-empty;
- any retained `Builder(Volume&&)` path cannot mutate Cell content still observed by another copied Volume;
- the final shared owner returns the pooled Buffer to its originating pool.

Update generic Volume Message extraction so successful decode always constructs/replaces with fresh immutable content rather than overwriting the destination's current Buffer in place. Preserve all other DR-017 wire/failure semantics.

### 2. Chunk construction

Keep `Chunk : public Voxel::Volume`.

Make only the reusable base state/construction mechanisms genuinely needed by derived semantic Volume types `protected`; unrelated implementation helpers remain private.

`Voxel::Volume::Builder` is no longer final and exposes the protected Builder state needed by `Chunk::Builder`.

Add nested `Chunk::Builder : public Voxel::Volume::Builder`:

- default construction fixes dimensions to `{16,16,16}`;
- default construction fixes unit size to `1.0f`;
- inherited checked `set()` remains the mutation mechanism;
- rvalue-qualified `build()` returns `Chunk`;
- the trusted build path transfers the Builder Cell storage directly to Chunk without first constructing a temporary Volume and without redundant dimension/unit-size validation.

Keep a public checked:

```cpp
explicit Chunk(Voxel::Volume&& volume);
```

It accepts only exactly `16x16x16` and unit size `1.0f`, throwing `spk::Exception` otherwise.

`Chunk` does not store its own `Chunk::Coordinate`.

### 3. `Chunk::Collection` and nested Provider

Introduce:

```cpp
class Chunk::Collection
{
public:
    class Provider;
};
```

Provider is an abstract Core contract conceptually equivalent to:

```cpp
class Chunk::Collection::Provider
{
public:
    virtual ~Provider() = default;

    [[nodiscard]] virtual Chunk provide(
        const Chunk::Coordinate& coordinate) = 0;
};
```

The Collection owns its Provider with unique ownership through a private `std::unique_ptr<Provider>`, but callers do not pass pointer ownership explicitly.

The public construction contract takes a concrete Provider rvalue and moves it into the Collection-owned polymorphic allocation:

```cpp
template<typename TProvider>
    requires
        std::derived_from<
            std::remove_cvref_t<TProvider>,
            Provider> &&
        (!std::is_lvalue_reference_v<TProvider>)
explicit Collection(TProvider&& provider)
    : _provider(
        std::make_unique<std::remove_cvref_t<TProvider>>(
            std::forward<TProvider>(provider)))
{
}
```

The constructor therefore requires a concrete Provider derived from `Provider`, rejects lvalue construction by constraint, and requires the concrete Provider to be movable. A successfully constructed Collection always owns exactly one Provider; absent/null Provider state is not representable through the public construction API.

Collection identity is `Chunk::Coordinate`; coordinate does not move into the Chunk value.

Collection state is exactly `Absent`, `Pending`, or `Available`.

`request(coordinate)` atomically transitions only an Absent coordinate to Pending, assigns a monotonically increasing generation, and forwards `Collection::Request { coordinate, generation }` to the owned Provider. Requests for coordinates already Pending or Available return without forwarding duplicate work.

The Provider contract is asynchronous/update-driven: `request(Request)` buffers/schedules work and `update(Collection&)` consumes asynchronous completion. A successful result is published only through `publish(request, chunk)`; failures use `fail(request)`.

Publication/failure is accepted only if the Collection still contains the exact Pending generation. Stale older tasks therefore cannot overwrite a later request/replacement.

`tryGet(coordinate)` returns `std::optional<Chunk>`. Available values are copied by value while a short shared `spk::ProtectedData` Reader is held, then remain valid independently through shared immutable Cell backing.

No Collection lock is held during provider generation work.

The approved replacement semantic is whole-value publication: a new Chunk for a coordinate replaces the stored Chunk value rather than mutating Cells of the old Chunk. If the coordinate is absent, replacement inserts the supplied Chunk immediately. This is an upsert operation and does not invoke the Provider. Existing copied Chunk values continue using the old immutable content.

### 4. Generic Catalog dual root formats

Refactor the Erelia-local generic `spk::JSON::Catalog<TElement>` parser so `load(path)` accepts both:

```json
{"elements":[{"id":"...","data":{}}]}
```

and:

```json
{"id":"...","data":{}}
```

Centralize one-element parsing/insertion so both forms use the same private behavior and derived `_parseKey` / `_parseElement` hooks.

Retain existing incremental/duplicate/error-context semantics unless the two-root-form change necessarily requires a narrow adjustment.

### 5. Validation resources

Keep both catalog forms deliberately exercised.

Shapes:

```text
resources/voxels/shapes.json           -> cube + slab
resources/voxels/shapes/slope.json     -> slope only
resources/voxels/shapes/stair.json     -> stair only
```

Definitions:

```text
resources/voxels/definition.json        -> cube + slab
resources/voxels/definitions/slope.json -> slope only
resources/voxels/definitions/stair.json -> stair only
```

Exact non-Air Definition IDs:

```text
1 = cube
2 = slope
3 = stair
4 = slab
```

Each Definition binds every used slot to `<shape-id>-<slot-id>`, exactly as DR-015 specifies.

Air remains catalog-created Definition 0 rather than an authored JSON Definition.

### 6. Server `PrototypeChunkProvider`

Implement a Server-owned concrete provider deriving from `Chunk::Collection::Provider`.

Its only terrain-generation input is the requested `Chunk::Coordinate`; no seed/version/catalog argument is part of this prototype provider contract.

The provider buffers deduplicated request identities, submits `spk::Task<Chunk>` work to the Server's singleton `spk::WorkerPool`, retains `Task<Chunk>::Answer` objects, and consumes them from its update pass. Final Collection publication/failure happens from that update path rather than directly from worker threads.

Generation produces `Chunk`, not `Voxel::Volume`.

The exact terrain output is DR-015:

- Definition-1 infinite X/Z baseline at world Y=0;
- Definition-1 walls at X=0 and Z=0 for world Y=1..3;
- slope fixture Chunk `(1,0,1)`, Definition 2;
- stair fixture Chunk `(2,0,1)`, Definition 3;
- slab fixture Chunk `(1,0,2)`, Definition 4;
- exact common ground/elevated transform matrices from DR-015;
- every other Cell Empty/Definition 0.

Generation itself produces no images.

`PrototypeChunkProvider` accepts every representable `Chunk::Coordinate` (`spk::Vector3Int`), including negative X, Y and Z coordinates. No Chunk coordinate is rejected by the prototype provider. Coordinates where DR-015 places no occupied Cells deterministically produce an empty Chunk.

## Explicitly not owned

- Client `RequestChunkProvider` or equivalent;
- request batching/outstanding/retry/disconnect semantics;
- exact Client response replacement behavior when no placeholder/current entry exists;
- dedicated Chunk `spk::Message` operators;
- Chunk request/response message IDs;
- Server NodeRouter handler;
- meshing, rendering or golden images;
- production terrain generation;
- persistence.

## Public contract

Approved shape:

```cpp
struct Chunk : public Voxel::Volume
{
    using Coordinate = spk::Vector3Int;

    class Builder;
    class Collection;

    inline static constexpr std::int32_t Extent = 16;

    explicit Chunk(Voxel::Volume&& volume);

    // existing coordinate conversion helpers
};

class Chunk::Collection
{
public:
    class Provider;

    template<typename TProvider>
        requires
            std::derived_from<
                std::remove_cvref_t<TProvider>,
                Provider> &&
            (!std::is_lvalue_reference_v<TProvider>)
    explicit Collection(TProvider&& provider);

    // lookup and whole-Chunk replacement/upsert API
};

```

The Collection owns its Provider. Provider returns `Chunk` synchronously for a requested coordinate.

The temporary Server provider is `PrototypeChunkProvider`.

## Invariants

- built `Voxel::Volume`/Chunk Cell content is immutable;
- Volume/Chunk copies share immutable backing Cell content;
- a Chunk is always exactly 16x16x16 with unit size 1.0f;
- Chunk never stores its own coordinate;
- Collection owns coordinate identity;
- Collection never mutates a published Chunk's Cells;
- replacement publishes a complete new Chunk value;
- copied old Chunk values remain valid after replacement;
- Provider result for a missing coordinate is stored before subsequent lookups observe it as cached;
- prototype generation is semantically deterministic;
- DR-015 world rules are canonical for the prototype.

## State transitions

### Collection request / availability

```text
Absent
    -> request(coordinate)
    -> Pending(generation)
    -> Provider::request(Request)

Pending / Available
    -> repeated request
    -> no duplicate Provider request

Provider update
    -> Task Answer Completed
    -> publish(Request, Chunk)
    -> Available

Provider update
    -> Task Answer Failed
    -> fail(Request)
    -> Absent

stale generation result
    -> publish/fail rejected
    -> newer Collection state preserved
```

### Replacement

```text
stored old Chunk
    -> complete new Chunk supplied
    -> Collection replaces entry
    -> existing copies keep old shared content alive
    -> new lookups obtain the replacement

absent coordinate
    -> complete new Chunk supplied
    -> Collection inserts it immediately
    -> Provider is not called
    -> subsequent lookups obtain the inserted Chunk
```

ST-001-11 owns network-specific pending/outstanding/rejected states.

## Failure behavior

Already fixed:

- checked `Chunk(Voxel::Volume&&)` rejects incompatible dimensions/unit size with `spk::Exception`;
- generic Volume/Catalog validation retains existing `spk::Exception`/source-context contracts;
- malformed generic Volume decode leaves destination unchanged;
- Provider/Collection must never expose a partially built mutable Chunk.

Resolved for the prototype provider:

- every representable `Chunk::Coordinate` is valid, including negative coordinates on any axis;
- the provider has no coordinate-domain rejection path;
- coordinates with no DR-015 terrain occupancy return a valid empty Chunk.

Resolved for Collection construction/replacement:

- replacement is implemented in ST-001-06 and uses upsert semantics; an absent coordinate is inserted immediately without invoking the Provider;
- Collection construction takes a concrete Provider rvalue through the constrained templated constructor;
- the Collection moves that concrete Provider into its private `std::unique_ptr<Provider>`;
- lvalues are rejected by the constructor constraint;
- absent/null Provider state is not representable through the public constructor.

No Collection edge-semantic decision remains unresolved before Ready.

## Determinism / ordering

- identical Provider inputs produce semantically identical prototype Chunks;
- no seed/version is part of this prototype input;
- Collection lookup identity is exact `Chunk::Coordinate`;
- no iteration-order guarantee is required unless the final Collection API exposes iteration.

## Lifecycle / ownership

- Builder owns mutable pooled storage before build;
- built Volume/Chunk copies share immutable backing storage;
- Collection owns its Provider and stored immutable Chunk values;
- consumers receive Chunk values and therefore extend Cell-content lifetime without shared-pointer API exposure;
- Collection container access is synchronized for concurrent readers/replacement;
- Provider must not return a Chunk referencing temporary Provider state.

## Serialization / persistence

Generic `Voxel::Volume` Message serialization remains DR-017, except for DR-019's fresh-content decode rule.

Dedicated fixed-size Chunk serialization is deferred to ST-001-08. It will omit dimensions/unit size and transfer only 4096 packed Cells.

No persistence is owned.

## Networking / authority

Server `PrototypeChunkProvider` output is canonical for this validation fixture.

Core Provider/Collection types contain no Server authority by themselves.

A future Client request provider may return an empty placeholder only as local non-authoritative state; canonical Server response replaces it.

## Exact terrain fixture

See DR-015 for the complete authoritative table.

The already-approved Chunk test set is:

```text
(0, 0, 0)
(1, 0, 1)
(2, 0, 1)
(1, 0, 2)
(-1, 0, 0)
(0, 0, -1)
(-1, 0, -1)
(0, -1, 0)
```

## Acceptance tests

### Core — Volume ownership

- copy construction/assignment shares immutable backing Cell storage;
- old copies remain valid after replacement/destruction of another value;
- move leaves source default-empty;
- moved Volume-to-Builder behavior, if retained, cannot mutate another copy;
- generic Message decode into one Volume cannot alter another Volume sharing the destination's old content.

### Core — Chunk

- `Chunk::Builder` fixed dimensions/unit size;
- inherited checked Cell population;
- `build()` returns Chunk;
- public Volume-to-Chunk conversion accepts exact invariants;
- incompatible dimensions/unit size rejected;
- Chunk has no coordinate state.

### Core — Collection/Provider

- first request transitions Absent -> Pending and invokes Provider once;
- repeated request while Pending or Available does not invoke Provider again;
- Provider update can publish Pending -> Available;
- Provider failure can return the matching Pending request to Absent;
- stale generations cannot publish over a newer request;
- returned Chunk copy outlives replacement/removal of the stored value;
- replacement publishes a new complete value without mutating an older copied Chunk;
- concurrent read/replacement contract is exercised according to the final implementation.

### Core — JSON Catalog

- existing aggregate root still loads;
- direct single-element root loads through the same semantic parser;
- duplicate across forms/loads is rejected consistently;
- malformed single-element roots report source/path context;
- voxel resources deliberately load through both forms.

### Core — Provider/Collection contract

Do not use `PrototypeChunkProvider` as the unit-test oracle for the reusable Provider/Collection abstraction.

Core tests must define a purpose-built test implementation of `Chunk::Collection::Provider` inside the test code. That implementation should expose deterministic, controllable behavior suitable for proving the Collection contract, including:

- which request identities were forwarded and how many times `request()` was called;
- distinct known Chunk values for requested coordinates;
- enough externally observable test state to prove ownership/caching/replacement behavior after the concrete Provider has been moved into the Collection.

Use that test Provider to cover:

- an Absent coordinate invokes the Provider exactly once and becomes Pending;
- repeated requests while Pending/Available do not invoke the Provider again;
- different coordinates are independently requested;
- update publication transitions matching Pending requests to Available;
- stale generations are rejected;
- replacement of an existing coordinate publishes the supplied complete Chunk without invoking the Provider;
- replacement of an absent coordinate inserts the supplied complete Chunk without invoking the Provider;
- copied old Chunk values remain valid after replacement;
- concurrent lookup/replacement follows the Collection synchronization contract;
- Collection construction accepts a concrete Provider rvalue and rejects Provider lvalues according to the approved constructor constraints.

### Server — prototype terrain

`PrototypeChunkProvider` is temporary deterministic scaffolding for the first validation world, not the reusable contract under test.

Do not freeze its exact DR-015 Cell composition in dedicated unit tests. In particular, do not build exhaustive expected 4096-Cell arrays or assert that every non-authored Cell remains Empty as a permanent Server unit-test contract.

Its exact temporary output remains the DR-015 implementation target for this ticket and later visual/integration work may exercise it indirectly. ST-001-06 unit-test effort should focus on the reusable Core Provider/Collection behavior through the test-specific Provider above.

### Boundaries

Provider/Collection boundary behavior is exercised with test-controlled positive and negative `Chunk::Coordinate` values. The DR-015 positive/negative Chunk set remains part of the temporary prototype-world implementation fixture, not a dedicated Server unit-test matrix.

### Invalid / rejected operations

`PrototypeChunkProvider` has no rejected coordinate domain: every representable `Chunk::Coordinate` is accepted. Remaining invalid/rejected Collection operations depend on the unresolved Collection edge-semantics decisions above.

### Failure atomicity

- Volume decode remains destination-preserving on failure;
- Collection never publishes a partially constructed Chunk;
- exact replacement/provider-failure state follows the remaining Ready decisions.

### Determinism

Use the test-specific Core Provider to prove Collection caching/idempotency deterministically. The temporary prototype world remains deterministic by implementation intent, but ST-001-06 does not freeze its exact Cell layout through dedicated unit-test assertions.

### Lifecycle / ownership

Explicitly prove old copied Chunk content remains usable after Collection publishes a newer complete Chunk.

### Serialization / persistence

Only regression coverage for the generic Volume ownership change. Dedicated Chunk serialization is not part of this ticket.

### Retry / idempotency

Repeated requests while Pending or Available do not enqueue duplicate provider work.

### Concurrency / cancellation

Collection read/replacement concurrency is applicable. Network cancellation is not.

### Authority / trust boundary

Only the Server concrete provider is canonical. Core Collection/Provider mechanics are authority-neutral.

### Dependency failure

JSON/resource parse failures retain existing Catalog behavior. Exact Provider construction/resource dependency is otherwise minimal because prototype generation uses fixed Cell Definition IDs and does not require runtime Definition lookup.

### Cross-system integration

Later ST-001-09 uses the Server provider/Collection; later ST-001-11 uses the Core Collection with a Client request provider.

### Performance

No timing budget. Structural requirement: Volume/Chunk copy does not deep-copy the Cell array.

### Client-visible / golden-image validation

Not owned. DR-015 fixture is later consumed by render/golden tickets.

## Decisions / unresolved questions

- [DR-007](../../../DECISIONS/DR-007-SEMANTIC-DETERMINISM.md)
- [DR-015](../../../DECISIONS/DR-015-FIRST-TERRAIN-VALIDATION-SCENE.md)
- [DR-017](../../../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
- [DR-018](../../../DECISIONS/DR-018-VOXEL-SHAPE-DEFINITION-CATALOG.md)
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) — resolved.

### Resolved readiness decisions

1. **Prototype provider coordinate domain:** `PrototypeChunkProvider` accepts every representable `Chunk::Coordinate`, including negative X/Y/Z coordinates. It does not reject coordinates; coordinates where DR-015 places no occupied Cells produce a valid empty Chunk.
2. **Collection replacement:** the whole-Chunk replacement API is implemented in ST-001-06 rather than deferred to ST-001-11. It has upsert semantics: an existing coordinate is replaced; an absent coordinate is inserted immediately; the Provider is not invoked by replacement.
3. **Collection Provider ownership/construction:** Collection exclusively owns its Provider in a private `std::unique_ptr<Provider>`, while its public constructor is a constrained forwarding constructor accepting only an rvalue concrete Provider derived from `Provider`. The concrete object is moved into the owned allocation; lvalues are rejected and null/absent Provider construction is unrepresentable.
4. **Provider test boundary:** do not make the temporary `PrototypeChunkProvider` Cell layout a unit-test contract. Test the reusable `Chunk::Collection::Provider` / Collection interaction in Core with a purpose-built test Provider whose calls and returned Chunks are controlled by the test. The prototype still implements DR-015 for the temporary validation world, but its exact Cell composition is not frozen by dedicated unit tests.

All readiness decisions are resolved. The ticket is Ready / active.

## Completion evidence

Implementation and regression coverage are present on the active branch.

The implemented scope includes:

- shared immutable Volume backing and updated fresh-content Message decode behavior;
- Chunk checked construction and Chunk::Builder;
- asynchronous Chunk::Collection / Provider with explicit Absent/Pending/Available state, generation counters, stale-result rejection, replacement/upsert, and short spk::ProtectedData synchronization;
- generic aggregate/direct JSON Catalog loading and the mixed validation resources;
- Server PrototypeChunkProvider using asynchronous Task submission;
- Erelia-local headless spk::ThreadSafeSet, spk::ThreadSafeQueue, spk::Task, spk::WorkerPool, and spk::Singleton prototypes;
- Core tests for immutable ownership, Chunk construction, Collection state/caching/replacement/concurrency/generation behavior, Catalog dual roots and resources, plus an intentionally extensive upstream-candidate test matrix for ThreadSafeSet, ThreadSafeQueue, Task, WorkerPool, and Singleton covering ownership, move-only values, stop/wakeup behavior, contention, exactly-once execution, failure isolation, and lifecycle.

CI run #311 (run ID `36018061979`) passed on implementation head `54e93aab35d468725b595e107f099d5d8577a2c3`:

- clang-format: passed;
- Core/Server Linux Debug: passed;
- Core/Server Linux Release: passed;
- Core/Server Windows Debug: passed;
- Core/Server Windows Release: passed;
- Client Windows Debug: passed;
- Client Windows Release: passed.

Do not mark this ticket Done until explicit project-owner approval is given.


## ST-001-06 asynchronous infrastructure refinement

The project owner additionally approved DR-020 during implementation:

- add an Erelia-local `spk::ThreadSafeSet<T>` mirroring Sparkle `ThreadSafeFIFO`'s shared State / Producer / Consumer / Endpoints shape while deduplicating values;
- add `spk::ThreadSafeQueue<T>` with the same State / Producer / Consumer / Endpoints structure as `ThreadSafeFIFO`, plus stop-token-aware single-item `waitPop()` for worker consumption;
- add headless `spk::Task<TResult>` with exactly Pending, Completed, Failed states;
- Completed always means a valid result; Failed stores an exception rather than a successful `std::expected` error value;
- add headless `spk::WorkerPool` with polymorphic `WorkerPool::Job`, internal `TaskJob<TResult> : Job` adapters, and `spk::ThreadSafeQueue<std::unique_ptr<Job>>` as its synchronized job queue;
- add `spk::Singleton<T>` backed by inline static `std::unique_ptr<T>`, with `instanciate(value)`, `instanciate(pointer)`, `instance()`, and `isInstanciated()`;
- instantiate the Server WorkerPool through `spk::Singleton<spk::WorkerPool>` at startup;
- keep all four prototypes in Erelia Core, namespace `spk`, with no graphics/Window dependency until they are ready to propose to Sparkle.
