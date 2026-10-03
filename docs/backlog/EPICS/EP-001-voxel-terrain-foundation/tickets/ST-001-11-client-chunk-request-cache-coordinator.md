# ST-001-11 — Client Chunk request/cache coordinator

**Status:** In Progress — implementation delivered; revised integration CI and owner review outstanding
**Epic:** EP-001
**Production target(s):** Core + Client + Terrain node
**Test suite(s):** EreliaCoreTestSuite, EreliaClientTestSuite, EreliaServerTestSuite, EreliaIntegrationTestSuite

## Intent

Introduce the generic asynchronous Collection foundation required by Client terrain streaming, migrate Chunk acquisition onto it, add the shared Column occupancy domain, provide generic Collection request/response/update/error networking, and integrate player-centered Client Column -> Chunk streaming with bounded Client retention.

## User / system value

The Client requests only terrain that is relevant to its current inspection position, reuses cached or already-Pending acquisitions, receives canonical Server data through one generic Collection protocol model, and bounds its local Column/Chunk storage while moving through the world.

## Starting state / prerequisites

- ST-001-06, ST-001-08, ST-001-09, and ST-001-10 are Done on `master`.
- DR-019 fixes immutable shared Chunk/Volume lifetime and the asynchronous Collection/Provider direction.
- DR-022 records the historical ST-001-08/ST-001-09 Chunk wire contract. ST-001-11 intentionally supersedes its fixed Chunk MessageID numbering, standalone Diagnostic MessageID, request-ID recycling policy, and Chunk-specific protocol framing with the generic Collection protocol defined here.
- Sparkle Version-0.1.3 now exposes immutable `spk::Message` values, `spk::Message::Writer` for construction, and independent `spk::Message::Reader` objects for decoding.
- ST-001-10 owns connection establishment/reconnection. ST-001-11 consumes the existing `Service::client()` transport and lifecycle events from `Service::clientEventCenter()`.

## Product ownership

- Core owns the generic `Collection<TKey, TElement>` abstraction, shared Column/Chunk protocol codecs, generic Collection protocol framing, and payload-level diagnostics.
- Client owns view/unload policy, Client Collection instances, network-backed Providers, message dispatch, and player-centered streaming behavior.
- Terrain node owns authoritative Column/Chunk generation and request handling.
- The central Server only routes Collection Request MessageIDs to the terrain node through `spk::NodeRouter`.

## Explicitly not owned

- Server-side cache budgets or eviction;
- Server production/broadcast policy for unsolicited Collection Update messages;
- terrain meshing/rendering;
- free-flight input/control implementation;
- production gameplay interest management;
- timeout/recovery semantics for request-level Error messages that do not produce a Response;
- logical Response splitting across multiple network messages.

## Generic Collection domain

ST-001-11 replaces the Chunk-specific asynchronous cache mechanics with:

```cpp
template <
    spk::MessageSerializable TKey,
    spk::MessageSerializable TElement>
class Collection;
```

The public state remains:

```text
Absent
Pending
Available
```

Collection storage contains only Available values. Pending acquisition state belongs to the Collection Provider.

### Public API

The target public shape is:

```cpp
template <
    spk::MessageSerializable TKey,
    spk::MessageSerializable TElement>
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

    using AvailableEvent =
        Core::Event<const TKey&, const TElement&>;
    using RemovedEvent =
        Core::Event<const TKey&>;

    [[nodiscard]]
    AvailableEvent& availableEvent();

    [[nodiscard]]
    RemovedEvent& removedEvent();

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

`tryRead` retains the Collection protected-data reader during the callback and passes `const TElement&`. The reference must not escape that callback.

Single-key request semantics are:

```text
Available -> immediately completed Answer containing the current value
Pending   -> reuse the Provider-owned Pending Answer
Absent    -> start one new Provider acquisition
```

Batch request preserves the caller's key order exactly. It returns one TaskGroup child per input key. Network batching partitions only the newly Absent acquisitions and preserves their relative input order.

### Mutation semantics

```text
insert:
    Available -> throw
    Pending   -> invalidate/fail Pending, then insert
    Absent    -> insert

replace:
    Available -> replace
    Pending   -> invalidate/fail Pending; strict replace still requires an Available value
    Absent    -> throw

remove:
    Available -> remove
    Pending   -> invalidate/fail Pending and remove any Available value
    Absent    -> no-op
```

Once a Pending acquisition has been invalidated/removed, a later network Response for it is stale and cannot republish data.

Collection lifecycle notifications are instance-local typed `Core::Event` channels. `availableEvent()` fires after an authoritative value enters or replaces Available storage through direct mutation, Provider completion, or Update Set. `removedEvent()` fires only when an actually Available value is erased; removing Absent state or invalidating Pending-only state does not emit it. Both channels are triggered outside the acquisition mutex so subscribers may safely query/request Collection state.

## Provider lifecycle

`Collection<TKey, TElement>::Provider` owns synchronized `TKey -> Pending` state.

Each Pending entry retains both the acquisition Answer and its completion Contract:

```cpp
struct Pending
{
    spk::Task<TElement>::Answer answer;
    spk::Task<TElement>::Answer::CompletionContract contract;
};
```

Registration order must remain safe for synchronous completion:

1. create the acquisition Answer;
2. insert Pending state;
3. subscribe to completion;
4. reacquire Pending bookkeeping;
5. retain the Contract only if the Pending entry still exists.

Successful completion publishes the element into Collection storage before Pending state disappears. Failed completion publishes nothing and removes Pending state.

### GeneratingProvider

`GeneratingProvider` reuses the generic Provider lifecycle and specializes only local generation/acquisition.

### RequestingProvider

`RequestingProvider` is generic and network-backed. It owns:

- new Pending Tasks;
- request batching;
- RequestID generation/correlation;
- outgoing Request serialization/send;
- incoming Response parsing;
- remembered terminal Server refusals for the current connection.

It maintains:

```text
TKey      -> active Pending acquisition
RequestID -> keys represented by one emitted Request
TKey      -> remembered terminal Response::Failure for current connection
```

A Server `Response::Failure` is remembered for the lifetime of the current Client connection. A later request for that key returns an already-failed acquisition using the retained Failure information and does not emit another network Request.

A transport disconnect:

- fails all active network-backed Pending acquisitions locally;
- clears RequestID correlation;
- clears remembered Server refusals;
- leaves Available Collection values untouched.

After reconnect, previously refused/transport-failed Absent keys are requestable again.

## RequestID contract

RequestID sequences are owned per Collection Request type / RequestingProvider.

- Chunk Requests start at RequestID 1.
- Column Requests independently start at RequestID 1.
- Each sequence increments monotonically as `spk::Message::RequestID` / `std::uint64_t`.
- RequestID 0 remains reserved for uncorrelated messages such as Update.
- Chunk and Column may therefore use the same numeric RequestID concurrently because routing delivers each Response to the matching RequestingProvider.
- RequestIDs are not recycled.
- Disconnect/reconnect does not reset an existing RequestingProvider's sequence.
- No drain/recycle threshold or reset handshake exists in ST-001-11.

## Domain-owned protocol tuning constants

Chunk and Column each own static compile-time tuning constants for:

- maximum elements per emitted Collection Request;
- elements per serialized Response section.

These values are implementation tuning knobs rather than durable wire identifiers. Tests should reference the domain constants instead of duplicating their numeric values. They may be adjusted later without changing the generic protocol architecture.

The existing historical Chunk limit may be used as the initial Chunk tuning value during migration; ST-001-11 does not promote numeric tuning values into a new protocol decision record.

## Column domain

`Column::Coordinate` is a Core semantic X/Z key with signed `x` and `z` components and the comparison/hash behavior needed by Collection storage.

A `Column` value is the complete sparse set of non-empty full `Chunk::Coordinate` values for one X/Z terrain column. It does not redundantly contain its own key.

Both Client and terrain Server instantiate:

```cpp
Collection<Column::Coordinate, Column>
```

The Client uses a network-backed RequestingProvider. The terrain Server uses a generating Provider derived from the same deterministic prototype terrain rules as Chunk generation, so Column membership and Chunk contents remain authoritative and consistent.

Chunk storage/acquisition remains:

```cpp
Collection<Chunk::Coordinate, Chunk>
```

Column discovery does not replace full Chunk identity or Chunk acquisition.

## Generic Collection protocol

### MessageType family

ST-001-11 intentionally supersedes the historical fixed numeric Chunk IDs.

Collection MessageIDs are declared from one family macro:

```cpp
#define COLLECTION_MESSAGES(Name) \
    Name##Request,                 \
    Name##Response,                \
    Name##Update,                  \
    Name##Error

enum class Networking::MessageType : spk::Message::Type
{
    Invalid = 0,

    COLLECTION_MESSAGES(Chunk),
    COLLECTION_MESSAGES(Column)
};
```

The generated ordering is the current protocol contract. No compatibility requirement preserves the old ST-001-08/ST-001-09 numeric values.

There is no standalone `Networking::MessageType::Diagnostic`.

Each domain declares its family directly inside `Chunk::Protocol::MessageTypes` or `Column::Protocol::MessageTypes`, using the four-parameter `Networking::CollectionProtocol::MessageTypes<Request, Response, Update, Error>` template. Generic codecs and Client subscription bindings read `TElement::Protocol::MessageTypes`; no external per-element specialization is required. The lightweight declaration header is separate from the full `CollectionProtocol::Codec<TKey, TElement>` implementation to avoid circular domain/Collection includes. `Networking::ChunkProtocol` and `Networking::ColumnProtocol` remain convenience aliases for those codecs. This owner-requested declaration refactor preserves all MessageID values and wire formats.


### Diagnostic

`Networking::Diagnostic` is a serializable payload value, not a routable Message.

It contains:

```cpp
enum class Severity : std::uint8_t
{
    Trace = 0,
    Info = 1,
    Warning = 2,
    Error = 3
};

Severity severity;
std::string message;
```

Request correlation belongs to the enclosing Collection protocol Message header.

### Message serialization model

Finalized `spk::Message` values are immutable.

Writing uses `spk::Message::Writer`:

```cpp
spk::Message::Writer writer(messageType);
writer.setRequestID(requestID);
writer << key;
writer << element;
spk::Message message = std::move(writer).build();
```

Decoding uses independent Readers:

```cpp
auto reader = message.reader();
reader >> key;
reader >> element;
```

A Reader owns its own cursor and retains the immutable pooled Message payload needed for its lifetime. Several parsing jobs may therefore own independent Readers over the same Message storage.

The generic `MessageSerializable` requirement is defined against `spk::Message::Writer` insertion and `spk::Message::Reader` extraction, not mutable operators on `spk::Message` itself.

### Request

A Collection Request carries a non-zero RequestID in its Message header and serializes only keys:

```text
[TKey]
[TKey]
...
```

`RequestingProvider` splits newly Absent acquisitions according to the domain's static maximum-elements-per-request constant. Each emitted network Request receives its own RequestID. The caller still observes one logical TaskGroup in original input order.

### Response

One valid Collection Request produces exactly one terminal Response Message in ST-001-11. Logical Responses are not split across network Messages.

The generic Response type itself owns an offset table at the beginning of the payload. The table provides absolute entry points for independently deserializable sections across the complete Response, including both Success and Failure data, and identifies the Success/Failure boundary. Chunk and Column do not implement separate response-table formats.

Serialized semantic entries are:

```text
Success:
    [TKey][TElement]

Failure:
    [TKey][Failure]
```

with:

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

Sections are cut according to the domain's static elements-per-response-section constant. A section never straddles the Success/Failure boundary.

The offset table and section boundaries are generic Response framing; WorkerPool parsing jobs create independent `Message::Reader` instances at those offsets.

A received Success or Failure is applied only while the matching RequestingProvider still owns the corresponding active Pending acquisition. Otherwise it is stale and ignored.

### Update

Update is unsolicited canonical Server state. Its RequestID is 0.

Update uses the same generic section/offset-table infrastructure and carries Set and Remove entries.

```text
Set + Absent    -> insert
Set + Available -> replace
Set + Pending   -> Update wins and settles Pending successfully

Remove + Available -> remove
Remove + Absent    -> no-op
Remove + Pending   -> Update wins and settles Pending as Failed
```

Response-vs-Update ownership of a Pending acquisition must be claimed atomically so only one path settles it.

ST-001-11 implements generic Update framing and Client receiving/application. The terrain Server does not yet produce/broadcast Updates.

### Error

Each Collection family owns an Error MessageID.

The payload is:

```text
[Networking::Diagnostic]
[keyCount:uint32]
[TKey x keyCount]
```

The key list may be empty when no safe key can be recovered.

Error is distinct from terminal per-element `Response::Failure`.

For ST-001-11, incoming ChunkError/ColumnError on the Client are diagnostic-only:

- decode Diagnostic + contextual keys;
- format contextual warning text;
- log at Warning;
- do not mutate Collection storage;
- do not settle/fail/retry Pending acquisitions.

This intentionally leaves known technical debt for request-level failures that emit Error without a Response, including outer aggregation failure: the correlated acquisition can remain Pending until another lifecycle event such as disconnect/removal settles it. ST-001-11 accepts this debt; a future networking/reliability ticket will define terminal Error recovery/timeout behavior.

## Message dispatch topology

Erelia owns a reusable non-Widget `MessageDispatcher` with MessageID -> `spk::ContractProvider` subscriptions.

Destroying a returned Contract removes the subscription. Multiple subscribers for one MessageID are allowed.

### Client

Core owns protocol-free `WorldIdentifier`, `World` and `WorldCollection` abstractions. A World carries its identifier, owns one `spk::Engine`, owned Entities, and lazily-created Chunk/Column Collections. Public `chunkCollection()` / `columnCollection()` accessors return stable non-owning pointers; private virtual factory hooks let each World specialization choose its collection Providers only when first requested. `WorldCollection` owns/caches Worlds by `WorldIdentifier` and owns a concrete Provider passed through the same forwarding-constructor pattern as `Collection<TKey, TElement>`. Its `world(identifier)` method delegates missing-world acquisition to that Provider internally.

The executable composition root creates independent Client roots:

- `ClientRuntime`, a non-Widget lifetime/composition owner for `ConnectionManager` and `ClientNetworkManager`; it tracks the current non-owning `World *` from the Client world lifecycle event;
- `Service::clientWorldCollection()`, backed by `RequestingWorldProvider`, which creates named blank `ClientWorld` instances whose lazy Collections own RequestingProviders, Updaters and MessageDispatcher subscriptions;
- `WorldManager : spk::EngineWidget`, which tracks the active `World *`, binds that World's Engine, and inserts the Player into the World rather than owning either Engine or Player;
- `MainInterface : spk::Widget`, which owns the Console and UI layout only.

The prototype bootstrap currently asks `Service::clientWorldCollection()` for the temporary named `prototype` World because ST-001-11 still has no authoritative world identifier in configuration/session/player data, then announces it through `worldChanged(World *)`. The Terrain node exposes the symmetric `Service::terrainWorldCollection()` backed by `GeneratingWorldProvider`.

`ConnectionManager` and `ClientNetworkManager` remain separate Widgets. They are siblings in the window hierarchy with explicit distinct z-orders, so Sparkle's child traversal guarantees connection lifecycle processing before receive-queue draining without a manual runtime update chain.

`ClientNetworkManager`:

- uses `Service::client()`;
- is the unique drainer of `spk::Client::messages()`;
- forwards received Messages to `MessageDispatcher`.

ConnectionManager retains the transport disconnection Contract and publishes typed Client::EventCenter lifecycle events. ClientWorld subscribes to clientDisconnected() and disconnects only collections that have actually been instantiated, preserving lazy creation while settling network-backed RequestingProviders.

Composition root bindings include:

```text
ChunkResponse  -> Chunk RequestingProvider
ChunkUpdate    -> Chunk Updater
ChunkError     -> Chunk error receiver

ColumnResponse -> Column RequestingProvider
ColumnUpdate   -> Column Updater
ColumnError    -> Column error receiver
```

### Central Server

There is no `ServerNetworkManager`.

The central Server configures only NodeRouter routes:

```text
ChunkRequest  -> terrain
ColumnRequest -> terrain
```

Unconfigured MessageIDs remain invalid under Sparkle's existing strict NodeRouter behavior.

### Terrain node

`GeneratingWorldProvider` is the Terrain-side WorldCollection::Provider. It owns server-side World definitions (`identifier`, `generatorType`, `family`) and selects the matching World construction strategy. The current `Prototype` type creates a `TerrainWorld` whose lazy Chunk/Column Collections use the existing deterministic prototype GeneratingProviders. TerrainNode defines/acquires that prototype World through `Service::terrainWorldCollection()` and routes requests through the returned World. The same `GeneratingWorldProvider + WorldCollection` path is used by the prototype terrain TU, so validation and the real Terrain node share the world-generation entry point. World itself owns no network protocol.

A node-side dispatcher adapter:

- advances the existing service-owned `spk::RemoteNode::Endpoint`;
- drains `Endpoint::requests()`;
- preserves the complete Endpoint Request envelope;
- dispatches by MessageID;
- lets handlers reply through the same existing Endpoint.

No parser independently drains a Sparkle receive FIFO.

## Client streaming behavior

`WorldManager` subscribes to `worldChanged(World *)` and `playerReady(const PlayerInformation &)`. A world change detaches the previous EngineWidget binding, updates its non-owning World pointer, and binds the selected World's Engine; `nullptr` represents no active World. The Player is created only when player information is ready and an active World exists, and `World::addEntity<Player>()` owns that Player inside the World. `PlayerInformation` remains intentionally fieldless in ST-001-11; its authoritative identifier/position/session/world fields belong to the future player-information protocol and are not guessed by this ticket.

The intended lifecycle is:

```text
clientConnected
    -> account/session stage
    -> playerLoadingRequested
    -> player-information request
    -> playerReady(PlayerInformation)
    -> WorldManager
    -> Player construction
```

Until the account/session and player-information protocol exist, ClientRuntime provides a prototype bridge from `clientConnected` to `playerLoadingRequested`, then to `playerReady` with an empty `PlayerInformation`. The executable acquires the temporary `prototype` ClientWorld from `Service::clientWorldCollection()` and announces it through `worldChanged` before the update loop starts, preserving the current validation Player bootstrap while keeping future authoritative named-world selection explicit.

The current Collection wire messages do not carry a World identifier. Therefore this ticket does not invent cross-world network routing: multiple Worlds may coexist process-locally, but only one network-active Client world is part of the ST-001-11 transport contract.

The Player owns one terrain-streaming `spk::Behaviour`.

The Behaviour retains:

- the Player `spk::Transform3D::OnEditionContract`;
- the last processed center `Chunk::Coordinate`;
- completion Contracts required by outstanding Column/Chunk acquisition chains.

Initialization subscribes to Transform edition and immediately invokes the same internal streaming routine with `owner()->transform()`. No artificial Transform edit or Sparkle force-trigger API is required.

For every Transform edition:

1. read world-space position;
2. mathematically floor each component to its containing global terrain Cell coordinate;
3. convert through the existing Chunk floor-division coordinate contract;
4. if the containing Chunk coordinate is unchanged, do nothing;
5. otherwise refresh Column demand and Client retention.

### View/unload ranges

`viewRange` and `unloadRange` are Client configuration values.

- both are strictly positive integers;
- `unloadRange >= viewRange`;
- configuration rejects invalid values;
- the Client precomputes relative horizontal X/Z Column offsets once and reuses them as the center moves;
- ranges are inclusive axis-aligned squares in Column space centered on the current player Chunk X/Z.

On a center-Chunk transition:

- request desired Columns in the view region;
- unload Client Columns outside the horizontal unload region;
- unload Client Chunks whose X/Z coordinates lie outside that retained region.

Server-side Column/Chunk storage remains unbounded in ST-001-11.

### Column -> Chunk chain

For each requested Column Answer, the Behaviour retains a completion Contract.

Successful Column completion:

```text
Column
    -> enumerate contained full Chunk::Coordinate values
    -> filter against current desired/retained streaming region
    -> request relevant Chunks through Collection<Chunk::Coordinate, Chunk>
```

Failed Column acquisition does not launch Chunk requests.

Successful Chunk completion has no mesher integration yet. ST-001-11 emits only the temporary `SPK_LOG(UserValueB)` observable used until later meshing/rendering work.

The Behaviour never serializes or sends protocol Messages directly.

## Determinism / ordering

- Collection batch TaskGroup child order exactly follows caller input order.
- RequestingProvider preserves relative input order when filtering newly Absent keys and cutting request batches.
- Column values use deterministic Chunk-coordinate ordering.
- Generic Response framing is deterministic for an equivalent ordered set of Success/Failure entries.
- Section boundaries are determined only by the corresponding domain static tuning constant, not asynchronous completion order.

## Failure behavior

- malformed Collection Request -> family Error diagnostic; no acquisition starts for invalid payload;
- terminal per-element Server refusal -> Response::Failure; remembered by RequestingProvider for current connection;
- disconnect -> active network Pending fails locally, remembered refusals clear, Available cache survives;
- stale/unknown Response -> ignored;
- Update beats Pending atomically;
- Error Messages are warning-only for ST-001-11;
- aggregation Error without Response may leave Pending work unresolved as accepted technical debt;
- one failed acquisition must not corrupt unrelated Available values.

## Implementation order

1. Migrate existing Chunk Collection mechanics to generic `Collection<TKey, TElement>`.
2. Introduce Column key/value serialization and deterministic prototype Column generation.
3. Introduce generic Collection Request/Response/Update/Error codecs using Sparkle Writer/Reader and generic Response offset-table sectioning.
4. Migrate Chunk protocol handling to the generic Collection MessageType family and remove the standalone Diagnostic MessageID.
5. Implement generic `GeneratingProvider`, `RequestingProvider`, and `Updater`.
6. Add reusable `MessageDispatcher`, ClientNetworkManager, and node-side dispatcher adapter; keep the central Server router-only.
7. Wire Chunk/Column protocol subscriptions in executable composition roots.
8. Integrate Client Collections and terrain-streaming Behaviour.
9. Migrate the existing Chunk integration fixtures from raw `spk::Client` request coordination to the real Client Collection path where ST-001-11 now owns that behavior.
10. Add focused Core/Client/Server-node/integration tests.

## Required tests

### Generic Collection

- Absent/Pending/Available state;
- single-key acquisition;
- repeated Pending request returns the same Answer;
- Available request returns immediately completed Answer;
- ordered TaskGroup batch request;
- insert/replace/remove strict semantics;
- Pending invalidation by mutation;
- late completion cannot republish removed/replaced data;
- Provider completion Contract lifetime including synchronous completion;
- instance-local availability events for direct mutation, Provider completion and Update Set;
- removal events only for actually Available values, including Update Remove;
- Collection lifecycle callbacks execute outside the acquisition mutex.

### Protocol

- Chunk and Column family MessageIDs generated from the macro;
- domain-owned four-parameter message families, including a custom domain whose IDs drive all four codecs without an external specialization;
- no standalone Diagnostic MessageID;
- Writer/Reader serialization round trips;
- Request split at domain static request limit;
- independent Chunk/Column RequestID sequences both beginning at 1;
- RequestID monotonic progression without recycle/reset;
- generic Response offset table covers Success and Failure sections;
- section boundaries at domain static section constant;
- mixed Success/Failure response;
- malformed offset table / truncated section rejection;
- Update Set/Remove semantics;
- family Error diagnostic parsing;
- stale Response ignored.

### Client networking

- MessageID subscription/Contract lifetime;
- multiple subscribers allowed;
- ClientNetworkManager uniquely drains the Client FIFO;
- disconnection fails active Pending work and clears remembered refusals;
- Available values survive disconnect.

### Terrain/node

- Node dispatcher uniquely drains Endpoint requests;
- ChunkRequest and ColumnRequest route through the central NodeRouter to terrain;
- malformed requests emit family Error;
- valid request emits one Response;
- prototype Column membership matches DR-015 fixture.

### Streaming

- initial streaming uses current Transform without a forced edit;
- transform edits inside the same Chunk are no-op;
- positive and negative Chunk boundary crossing;
- view/unload configuration validation;
- exact inclusive horizontal range membership;
- repeated view refresh reuses Available/Pending values;
- successful Column completion requests its relevant Chunks;
- failed Column does not request Chunks;
- Client Column and Chunk unload;
- removed Pending Chunk cannot be republished by a late Response;
- successful Chunk acquisition emits the UserValueB placeholder log.

### Integration

Preserve the real Client -> NodeRouter -> RemoteNode -> terrain Endpoint path and validate canonical Chunk/Column acquisition through the ST-001-11 Client Collection APIs rather than bypassing the coordinator with raw transport logic.

## Accepted technical debt / deferred work

- Error messages do not settle correlated Pending acquisitions; terminal Error recovery/timeout belongs to later work.
- Server-side Update production/broadcast is deferred.
- Server cache budget/eviction is deferred.
- Logical Response splitting across multiple network Messages is deferred.
- Meshing/render integration remains ST-001-12/ST-001-13.
- Production movement/interest management remains outside EP-001.

## Decisions

- [DR-014](../../../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md)
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
- [DR-022](../../../DECISIONS/DR-022-CHUNK-PROTOCOL-WIRE-CONTRACT.md) — historical Chunk protocol, partially superseded by this ticket's generic Collection protocol.
- [OQ-038](../../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — Resolved; ST-001-11 fixes the remaining Client request/cache/retry/recycle choices.

## Implementation / completion evidence — 1 October 2026

The Ready gate was verified on the existing `feat/st-001-11-client-chunk-request-cache-coordinator` branch before production changes. Implementation is delivered in [PR #19](https://github.com/EreliaStudio/Erelia/pull/19); it is not Done: all production/component and routed integration suites passed, but the inherited executable-disconnect assertion failed under the earlier conflicting policy. The owner has since confirmed bounded retries/stay-alive; revised integration CI and project-owner approval are outstanding.

### Delivered production behavior

- Core: generic `Collection<TKey, TElement>`, Provider-owned Pending Answers/Contracts, ordered Sparkle TaskGroups, strict mutations and stale-completion protection; generic generating/requesting/updating providers; shared Column domain and fixed Chunk serialization.
- Protocol: generated Chunk/Column message families, payload-only Diagnostic, independent monotonic uint64 RequestIDs, generic section offset tables, WorkerPool parsing through independent immutable Message Readers, authoritative Update and diagnostic-only Error handling.
- Client: one Service Client queue drainer, retained dispatcher subscriptions and event-service disconnect subscriptions, both requesting Collections, configured inclusive horizontal view/unload regions, and Player-owned Transform-subscribed terrain streaming Behaviour. Successful Columns launch Chunk acquisition; successful Chunks emit the UserValueB placeholder.
- Server/Terrain: both NodeRouter redirects, one Service-owned Endpoint request drainer with complete reply envelopes, shared deterministic Column/Chunk generation, family Errors and one Response per valid Request. No Server Update broadcast or eviction was added.
- Integration: real Client Collection -> Sparkle Client -> NodeRouter -> RemoteNode -> terrain Endpoint -> generic handler/provider -> Response -> ClientNetworkManager -> RequestingProvider -> Collection fixtures. Process smoke now starts all three executables and observes acquisition before testing Server loss.

The historical `Chunk::Collection::BatchResult`, Chunk-specific protocol headers/layout, and standalone Diagnostic MessageID are removed. Generic Volume tests use the current Writer/Reader API without weakening their malformed-input checks. Existing golden references are unchanged.

### Validation evidence

Sparkle Version-0.1.3 was inspected and built at `626b86c` (immutable Message redesign). Final production validation head: `d74040aaca401c7f08bf9bcd644b154cba4c58d7` (including acquisition-launch identity protection and notification outside acquisition locks).

| Command / configuration | Result |
| --- | --- |
| clang-format 21.1.0 `--style=file --dry-run --Werror` over all active Core/Server/Client/integration C++ sources | Pass |
| `git diff --check` | Pass |
| Linux Clang 18 Debug, Sparkle Core + Erelia headless build | Pass |
| `ctest --test-dir build/headless-debug --output-on-failure` | 3/3 CTest entries pass: Core 121/121, Server/Terrain 18/18, Server smoke |
| Linux Clang 18 Release, Sparkle Core + Erelia headless build | Pass |
| `ctest --test-dir build/headless-release --output-on-failure` | 3/3 CTest entries pass |
| Focused Client `terrain_streaming_test.cpp`, linked against real Sparkle Core Entity/Behaviour and Erelia Core | 9/9 tests pass, including delayed completion-mailbox publication |
| Clang 18 syntax checks of new Client production/networking tests and routed Collection integration fixture | Pass; this is not a substitute for Windows link/runtime validation |
| [PR CI run #613](https://github.com/EreliaStudio/Erelia/actions/runs/36924712583): clang-format | Pass |
| CI Core/Server Linux Debug + Release and Windows Debug + Release | All four jobs pass; 3/3 CTest entries per configuration (Core 124 tests and Server/Terrain 18 tests) |
| CI Client Windows Debug + Release, `ctest --preset <configuration> --output-on-failure --no-tests=error -LE integration` | Both jobs pass; 6/6 component CTest entries per configuration, including existing golden regressions |
| CI Integration Windows Debug + Release, `ctest --preset <configuration> --output-on-failure --no-tests=error -L integration` | Routed `EreliaIntegrationTestSuite` passes in both configurations; `EreliaClientServerProcessSmoke` fails in both configurations on its inherited exit-after-disconnect assertion |


The local rows above record completed pre-recovery runs (Core 121 tests). Final local Debug headless validation also passed after the three added Core concurrency regressions; the execution environment disconnected before final local Release/focused streaming revalidation. The final-code CI matrix above supplies that validation; no interrupted local run is reported as passing.

Local full graphical Client/integration builds cannot run because Sparkle's graphical window backend is Win32-only. The Windows CI results above provide full link/runtime, existing golden regression and routed integration evidence. The process smoke reaches the actual three-process connection and Chunk-acquisition markers before failing on the inherited lifecycle assertion. Earlier runs #607/#608 exposed that Mesa was missing beside the actual Client executable; CI now copies the same approved renderer DLLs there. No image references were replaced.

### Remaining completion gate

Pass the revised process smoke under the owner-confirmed bounded retry/stay-alive policy and record project-owner review/approval before changing this ticket to Done. No golden replacement or new visual approval is requested by this ticket.

The accepted technical debt above remains unchanged: Error without Response may leave Pending indefinitely; Server Update production/broadcast, Server eviction, response splitting, meshing/rendering and movement/production interest management remain deferred.

The next dependency-ordered ticket is ST-001-12, still Blocked by OQ-036 (missing-neighbor/remesh policy). It is not automatically Ready; no later ticket is promoted by this implementation.

### Connection policy resolution — 1 October 2026

The project owner confirmed the implemented reconnect/stay-alive behavior and requested updating the integration test. This supersedes the contradictory historical exit-on-disconnect expectation in ST-001-10 and its process smoke.

The revised process smoke observes Server loss, exactly three ordered reconnect attempts, the stopped-cycle marker and continued Client liveness. The routed ConnectionManager fixture verifies exhaustion and explicit recovery against a restored Server on the same endpoint. Production connection policy is unchanged. The Client now bundles and loads `i18n/en.json` before UI construction; the smoke also rejects missing Client translation keys.

The prior CI failures remain historical evidence. Revised Windows Debug/Release CI and final owner review are still required; this ticket remains In Progress.

### Domain-owned message families — 1 October 2026

The owner requested replacing external `CollectionMessageTypes<TElement>` specializations with four-parameter family declarations directly inside Chunk and Column. Both domains now own `Protocol::MessageTypes`; the generic `CollectionProtocol::Codec` and subscription bindings consume that declaration. All existing wire IDs and payload layouts are preserved.

Validation: a local GCC 13 C++23 executable linked to the real Sparkle Message/Exception implementation passes 13 focused protocol tests, including custom-domain IDs for all four message families and existing Request/Response/Update/Error malformed-input regressions. Core protocol/RequestingProvider tests and TerrainNode pass syntax checks. `git diff --check` passes. Client syntax validation is blocked locally by missing OpenGL/GLEW headers; full Client and cross-process runtime checks remain Windows CI responsibilities. clang-format is unavailable locally, so its CI check remains required.

### Owner decision: separate event services

Core, Client and Terrain each expose an independent typed EventCenter service. Core::Event<TArguments...> reuses spk::ContractProvider for subscriptions and synchronous emission. No inheritance or heterogeneous string map is required. Collection instances also use Core::Event directly for local Available/Removed lifecycle channels; those are deliberately not routed through the global Core::EventCenter because multiple independent Collections can coexist in one process. Client declares connection-request, connection, disconnection, world-change, player-loading, player-ready and player chunk-change events. `worldChanged(World *)` carries a non-owning pointer and accepts `nullptr` for detachment; subscribers update their cached World/collection pointers synchronously before an old World may be destroyed. ConnectCommand publishes Client::ConnectionRequest through connectionRequested(), and ConnectionManager owns the subscription. MainInterface only owns UI; command registration is performed by the executable composition root. ClientRuntime owns the temporary connection-to-player-loading bridge and tracks the selected World, while WorldManager binds the selected World's Engine and inserts Player into that World when playerReady is emitted. Core and Terrain can gain their own named process-wide events as their domain contracts are defined. Event tests cover typed payloads, independent channels, subscription lifetime and the emitting thread. Connection integration checks lifecycle counts and delivery on the update thread; routed acquisition now uses ConnectionManager to exercise disconnect handling through the event service. Windows runtime validation remains a CI responsibility.


### Owner decision: protocol-free World ownership — 3 October 2026

Core now defines `World` as a process-local world-state container rather than a wire protocol. It owns a Sparkle Engine, Entity lifetime and lazily-created Chunk/Column Collections. Derived `ClientWorld` and `TerrainWorld` implement the private collection factories with RequestingProvider and GeneratingProvider strategies respectively. Public collection accessors return stable pointers so systems may cache their current collection directly.

`WorldCollection` mirrors the generic `Collection` ownership model. It owns Worlds by `WorldIdentifier`, owns a polymorphic Provider supplied by its forwarding constructor, returns cached Worlds from `world(identifier)`, and invokes that Provider internally only for a missing World. Client uses `RequestingWorldProvider`: it creates an empty named ClientWorld whose Collections request their contents from the Server. Terrain uses `GeneratingWorldProvider`: definitions select a generation type and currently support the deterministic `Prototype` type used by both TerrainNode and the prototype terrain TU. The Client and Terrain expose separate `Service::clientWorldCollection()` / `Service::terrainWorldCollection()` instances because integration tests link both domains into one process. Client active-world selection remains separate from ownership: `Client::EventCenter::worldChanged(World *)` tells ClientRuntime, WorldManager and future subscribers which World is active. Callers must publish a replacement or `nullptr` before removing an active World so cached non-owning pointers are not left dangling.

No World message family or World protocol is introduced. The current Chunk/Column protocol has no World identifier, so simultaneous network routing for several ClientWorld instances remains future work rather than being implicitly encoded into ST-001-11.
