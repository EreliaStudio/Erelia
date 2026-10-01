# ST-001-11 — Client Chunk request/cache coordinator

**Status:** In Progress — implementation delivered; validation and owner review pending
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
- ST-001-10 owns connection establishment/reconnection. ST-001-11 consumes the existing `Service::client()` transport and its connection/disconnection notifications.

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
    MessageSerializable TKey,
    MessageSerializable TElement>
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

`ClientNetworkManager : spk::Widget`:

- uses `Service::client()`;
- is the unique drainer of `spk::Client::messages()`;
- forwards received Messages to `MessageDispatcher`;
- retains the Client disconnection Contract;
- propagates disconnect to network-backed RequestingProviders.

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

A node-side dispatcher adapter:

- advances the existing service-owned `spk::RemoteNode::Endpoint`;
- drains `Endpoint::requests()`;
- preserves the complete Endpoint Request envelope;
- dispatches by MessageID;
- lets handlers reply through the same existing Endpoint.

No parser independently drains a Sparkle receive FIFO.

## Client streaming behavior

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
- Provider completion Contract lifetime including synchronous completion.

### Protocol

- Chunk and Column family MessageIDs generated from the macro;
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

The Ready gate was verified on the existing `feat/st-001-11-client-chunk-request-cache-coordinator` branch before production changes. Implementation is delivered in [PR #19](https://github.com/EreliaStudio/Erelia/pull/19); it is not yet Done because the full Windows validation matrix and required project-owner approval have not been recorded.

### Delivered production behavior

- Core: generic `Collection<TKey, TElement>`, Provider-owned Pending Answers/Contracts, ordered Sparkle TaskGroups, strict mutations and stale-completion protection; generic generating/requesting/updating providers; shared Column domain and fixed Chunk serialization.
- Protocol: generated Chunk/Column message families, payload-only Diagnostic, independent monotonic uint64 RequestIDs, generic section offset tables, WorkerPool parsing through independent immutable Message Readers, authoritative Update and diagnostic-only Error handling.
- Client: one Service Client queue drainer, retained dispatcher subscriptions and disconnect fan-out, both requesting Collections, configured inclusive horizontal view/unload regions, and Player-owned Transform-subscribed terrain streaming Behaviour. Successful Columns launch Chunk acquisition; successful Chunks emit the UserValueB placeholder.
- Server/Terrain: both NodeRouter redirects, one Service-owned Endpoint request drainer with complete reply envelopes, shared deterministic Column/Chunk generation, family Errors and one Response per valid Request. No Server Update broadcast or eviction was added.
- Integration: real Client Collection -> Sparkle Client -> NodeRouter -> RemoteNode -> terrain Endpoint -> generic handler/provider -> Response -> ClientNetworkManager -> RequestingProvider -> Collection fixtures. Process smoke now starts all three executables and observes acquisition before testing Server loss.

The historical `Chunk::Collection::BatchResult`, Chunk-specific protocol headers/layout, and standalone Diagnostic MessageID are removed. Generic Volume tests use the current Writer/Reader API without weakening their malformed-input checks. Existing golden references are unchanged.

### Validation recorded so far

Sparkle Version-0.1.3 was inspected and built at `626b86c` (immutable Message redesign). Production validation head: `79b7dcd97c51cf57b5d26e584ba3cc5de0bd8809`.

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
| [PR CI run #607](https://github.com/EreliaStudio/Erelia/actions/runs/36916682709) | Formatting and all Core/Server and Client configurations pass; routed integration suites pass in both configurations. Release process smoke failed on early Client exit; investigation/rerun in progress. |

Local full graphical Client/integration builds cannot run because Sparkle's graphical window backend is Win32-only. The existing Windows CI jobs own that required evidence, including the existing golden regression checks and three-process smoke.

### Remaining completion gate

Record the full required CI outcome and project-owner review/approval before changing this ticket to Done. No golden replacement or new visual approval is requested by this ticket.

The accepted technical debt above remains unchanged: Error without Response may leave Pending indefinitely; Server Update production/broadcast, Server eviction, response splitting, meshing/rendering and movement/production interest management remain deferred.

The next dependency-ordered ticket is ST-001-12, still Blocked by OQ-036 (missing-neighbor/remesh policy). It is not automatically Ready; no later ticket is promoted by this implementation.
