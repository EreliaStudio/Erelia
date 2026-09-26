# ST-001-09 — Server Chunk request handler

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Server
**Test suite(s):** EreliaServerTestSuite

## Intent

Handle the EP-001 Chunk request message family inside the separate terrain node process and return canonical generated Chunk results to the originating Client.

## User / system value

A real dedicated Server can answer Client terrain requests without exposing Client policy or presentation concerns.

## Starting state / prerequisites

- Depends on ST-001-06, ST-001-07, and ST-001-08.
- DR-016 and DR-021 fix routing through NodeRouter / RemoteNode to the separate terrain Endpoint process.
- OQ-038 is resolved by DR-022 for duplicate handling, limits, result states, ordering, correlation, and malformed protocol behavior.

## Product ownership

Server terrain node owns request validation, canonical generation lookup, and response/rejection.

## Allowed dependencies

EreliaTerrainNodeLibrary, EreliaCore, Sparkle Core networking, standard library, ST-001-06 `Chunk::Collection` / terrain-node `PrototypeChunkProvider`, ST-001-08 protocol.

## Forbidden dependencies

Client/rendering code, Client view radius/cache policy, terrain mesh generation, extra network libraries.

## Owned behavior

- Receive routed Chunk requests in the terrain node's `spk::RemoteNode::Endpoint` process.
- Add a transport-level smoke fixture proving a real `ChunkRequest` crosses Client -> NodeRouter -> RemoteNode -> terrain Endpoint with Message type, RequestID, size, and payload bytes preserved before parsing/handler semantics are asserted.
- Validate request according to the final ST-001-08 contract.
- Keep each Client protocol request intact at the wire level, then split its distinct coordinates inside `TerrainNode` into smaller internal work batches.
- Resolve each internal batch through the Server `Chunk::Collection` backed by `PrototypeChunkProvider`.
- Group the returned batch Task Answers with Sparkle Version-0.1.3 `spk::TaskGroup<TResult>`.
- Subscribe once to grouped completion and, only when every internal batch is terminal, compose one terminal `ChunkResponse` using the original Client RequestID.
- Return canonical coordinate + Chunk results through the router to the originating Client.
- Keep two concurrent Client connections correlated correctly.
- Reject malformed/invalid requests without mutating authoritative terrain state.

## Explicitly not owned

Client retry/cache policy, production interest management, persistent terrain editing, rendering.

## Public contract

The shared request limits, duplicate semantics, result-state format, correlation, ordering, and malformed-input contract are fixed by the completed ST-001-08/DR-022 work.

Sparkle Version-0.1.3 now provides the complete asynchronous composition model required here:

- `spk::Task<TResult>` is a generic manually-settled asynchronous result with `validate(TResult)` / `fail(std::exception_ptr)`;
- `Task<TResult>::Answer::subscribeToCompletion(...)` provides race-safe completion subscription;
- `spk::WorkerPool::submit(callable)` runs executable work and returns a `Task<TResult>::Answer`;
- `spk::TaskGroup<TResult>` passively groups arbitrary Task Answers without consuming a worker merely to wait;
- thread-safe `ContractProvider` owns completion-contract synchronization.

The Erelia Collection/Provider ownership is now fixed semantically:

- TerrainNode splits one protocol Request into internal coordinate batches;
- `Chunk::Collection` accepts one coordinate vector per internal batch and returns one `spk::Task<BatchResult>::Answer`;
- a successful `BatchResult` contains every requested coordinate paired with a shallow-copied immutable Chunk;
- Collection owns Available/Pending/Absent lookup, pending-work reuse, subscriptions, and batch aggregation;
- `Chunk::Collection::Provider` generates exactly one Absent coordinate per request and returns one WorkerPool-produced `spk::Task<Chunk>::Answer`;
- Collection's batch Task is created directly and manually settled; it is never submitted to WorkerPool;
- the batch stays Pending until every coordinate dependency is terminal;
- if every coordinate succeeds, Collection validates the complete BatchResult;
- if any coordinate Task fails, the whole Collection batch Task is Failed and exposes no partial BatchResult.

The exact private Collection state representation and exact concrete C++ container/name used for `BatchResult` remain implementation details as long as they preserve this semantic contract.

This ticket remains Blocked on the remaining Server-specific decisions: how a failed Collection batch / failed outer TaskGroup maps into the already-fixed DR-022 wire protocol, the internal batch-size rule, and outstanding-request/reply/disconnect/shutdown lifetime behavior.

## Invariants

- Server never returns a render mesh.
- A response is sent to the Client that originated the request.
- Canonical generated output is Server-owned.
- One Client cannot cause another Client's request result to be delivered as its own response.

## State transitions

Valid request -> validate -> diagnose/deduplicate protocol coordinates -> TerrainNode partitions the distinct coordinates into smaller internal batches -> ask `Chunk::Collection` once per internal batch -> add each returned `Task<BatchResult>::Answer` to one `spk::TaskGroup<BatchResult>` owned by the protocol-request context -> seal the group -> subscribe once to grouped completion -> handle one terminal protocol outcome using the original RequestID.

For one Collection batch, each coordinate follows exactly one of these paths:

```text
Available
    -> shallow-copy the Chunk directly into the batch result

Pending
    -> reuse the existing Task<Chunk>::Answer
    -> subscribe to its completion

Absent
    -> ask Provider::request(coordinate)
    -> store the returned Task<Chunk>::Answer as the unique pending work
    -> subscribe to its completion
```

The Provider operation is single-coordinate and WorkerPool-backed. The Collection batch itself is a generic `spk::Task<BatchResult>` that is not submitted to WorkerPool.

The Collection batch does not become terminal until all coordinate dependencies are terminal. If all succeeded, it calls `validate(BatchResult)` once with all requested coordinate/Chunk pairs. If at least one failed, it calls `fail(...)` for the whole batch and no partial BatchResult is exposed.

The Client does not split its protocol request. One protocol Request may contain up to the ST-001-08 limit of 1024 coordinates. Internal Server batches are scheduling/composition units only: they do not create protocol RequestIDs, partial protocol Responses, or additional Client-visible request lifecycles.

The outer TaskGroup completion callback may run on the thread that settles the final child batch Task, or immediately on the subscribing thread if the group is already terminal. The handler implementation must therefore make the captured request/reply state safe for that callback lifetime and must not assume completion callbacks execute on the TerrainNode dispatch thread.

Malformed typed request -> catch the protocol decoding `spk::Exception` at the terrain consumer boundary -> log a Sparkle Warning -> drop the malformed message without a protocol reply -> no canonical state mutation -> continue serving later messages.

## Failure behavior

Collection batch failure is atomic at the Answer/result level. A batch never returns a partial `BatchResult`: after every coordinate dependency becomes terminal, one failed coordinate Task makes the whole `Task<BatchResult>` Failed.

Consequently, the TerrainNode's outer `spk::TaskGroup<BatchResult>` becomes Failed after every child batch is terminal if at least one Collection batch failed. Successful child batch Answers remain individually inspectable, while a failed child batch exposes its failure rather than a partial result.

This supersedes the earlier ST-001-09 assumption that an individual generation failure would automatically become a per-coordinate `Unavailable` while the rest of that Collection batch remained successful.

DR-022 still defines `Success`, `Rejected`, and `Unavailable` as per-coordinate Response states, and `ChunkError` currently only defines `DuplicateCoordinate`. Therefore the final **wire-level** mapping of a failed Collection batch / failed outer TaskGroup is still unresolved. This ticket must not invent a new request-level failure state or `ChunkError` code while implementing the Collection failure rule.

ST-001-09 still has no selected Server policy/domain rule that produces `Rejected`.

Remaining failure behavior to resolve before Ready is Server-specific: failed-batch-to-wire mapping, cache transition/retention semantics around successful coordinates in a failed batch if any ambiguity remains in implementation, reply/send failure handling, and outstanding-request/disconnect/shutdown lifecycle.

## Determinism / ordering

Provider/Collection output follows ST-001-06. Response ordering/association follows ST-001-08; no extra Server-specific ordering may be invented.

## Lifecycle / ownership

The terrain node process owns request-processing lifetime. Connection references used for replies must not outlive/disconnect unsafely; exact Sparkle lifecycle must be reflected in final tests.

## Serialization / persistence

Uses ST-001-08's dedicated Chunk codec/protocol. Generic ST-001-05 Volume serialization remains available but is not the Chunk payload format. No persistence.

## Networking / authority

Server validates and returns canonical results. Client only requests coordinates.

## Implementation constraints

- Handler lives in the terrain node process, not either executable `main.cpp`.
- Use Sparkle NodeRouter response path to the originating Client.
- Do not introduce a second transport or a Server-selected view radius.
- Consume Sparkle Version-0.1.3 directly; remove the temporary Erelia-local TaskGroup prototype and its duplicate Core tests.
- `spk::Task<TResult>` is the generic asynchronous result state. Collection may create a Task directly and settle it with `validate(...)` / `fail(...)` without WorkerPool execution.
- `spk::WorkerPool::submit(callable)` is used only for executable work. Provider generation is WorkerPool-backed; Collection batch aggregation is not.
- `spk::TaskGroup<TResult>` groups Task Answers regardless of whether those Tasks were manually settled or WorkerPool-produced. It does not submit Tasks and does not consume a worker merely to wait.
- TerrainNode owns the split of one protocol request into smaller internal coordinate batches and owns the protocol-request TaskGroup.
- Keep protocol correlation at the original RequestID: internal work batches never own protocol RequestIDs.
- `Chunk::Collection::request(vector<Chunk::Coordinate>)` must return one asynchronous `Task<BatchResult>::Answer` representing the complete internal batch.
- A successful BatchResult semantically contains all requested coordinate/Chunk pairs by value. Chunk copies are shallow immutable snapshots through the existing Volume/Chunk ownership model.
- `Chunk::Collection::Provider` is a single-coordinate WorkerPool-facing generator. Its request operation accepts one `Chunk::Coordinate` and returns one `spk::Task<Chunk>::Answer`.
- Provider no longer owns Collection batching, request buffering for batches, or an `update(Collection&)` polling phase.
- Collection owns authoritative cache/deduplication semantics. A missing Chunk becomes Pending asynchronous work; no placeholder/empty Chunk is inserted.
- A Pending coordinate retains/reuses the unique in-flight `Task<Chunk>::Answer`. Overlapping Collection requests subscribe to that same Answer and must not ask Provider to regenerate the same coordinate.
- Collection subscribes to every Pending/new coordinate Answer and settles its own batch Task only after all coordinates are terminal.
- If all coordinate Answers succeed, Collection validates the complete BatchResult. If any coordinate Answer fails, Collection fails the entire batch Task and exposes no partial BatchResult.
- The exact private structs/variant used for Absent/Pending/Available and the exact concrete BatchResult container type are not public-contract requirements.
- The grouped completion callback may execute outside the TerrainNode dispatch thread. Any captured Endpoint/request state must have safe lifetime and any network operation performed there must follow Sparkle's thread-safety contract.
- Do not implement a failed-batch wire representation until its mapping onto DR-022 is explicitly resolved.

## Exact test fixtures

Final Ready fixtures must include:

- one valid coordinate;
- valid multi-coordinate Collection batch including a negative coordinate;
- duplicate protocol-coordinate case;
- all-Available Collection batch completing immediately with all copied Chunks;
- all-Absent Collection batch producing exactly one Provider Task per distinct coordinate;
- mixed Available + Pending + Absent Collection batch;
- overlapping Collection requests that share a Pending coordinate, proving the same in-flight `Task<Chunk>::Answer` is reused and Provider is not called twice;
- completion of one shared Pending coordinate notifying every batch subscribed to that Answer;
- a Collection batch where one coordinate Task succeeds and another fails, proving the batch waits for all dependencies then becomes Failed and exposes no partial BatchResult;
- a TerrainNode grouped request where one Collection batch completes before another, proving no Client-visible terminal handling occurs until the outer TaskGroup is terminal;
- malformed payload;
- two Clients issuing distinguishable requests;
- disconnect during an outstanding request;
- deterministic generator failure through a purpose-built test Provider rather than depending on PrototypeChunkProvider output/failure.

The final Server integration fixture for the **wire result of a failed Collection batch** cannot be fixed until the failed-batch-to-DR-022 mapping is explicitly resolved.

## Acceptance tests

### Nominal

Valid single/multi requests return exact canonical Chunks.

### Boundaries

Approved batch-size boundaries.

### Invalid / rejected operations

Per OQ-038 final semantics.

### Failure atomicity

Malformed requests do not mutate canonical terrain state. A Collection batch with any failed coordinate Task becomes Failed after all coordinate dependencies settle and exposes no partial BatchResult. ST-001-09 does not emit partial protocol Responses; the final wire-level representation of a failed batch/request remains to be resolved.

### Determinism

Repeated valid requests for the same coordinate return semantically identical canonical Chunks.

### Lifecycle / ownership

Disconnect/outstanding-request behavior is explicit and tested.

### Serialization / persistence

Handler uses shared codecs; independent codec tests remain in Core.

### Retry / idempotency

Read-only terrain retrieval must not produce authoritative mutation. Exact duplicate/retry response behavior follows OQ-038.

### Concurrency / cancellation

Two-Client correlation and disconnect case required. No broader gameplay concurrency is owned.

### Authority / trust boundary

Client cannot provide canonical Chunk content or bypass Server Provider/Collection resolution.

### Dependency failure

Generator failure already fails the complete Collection batch Task; its DR-022 wire mapping and network send failure behavior must be explicit before Ready.

### Cross-system integration

Router -> `spk::RemoteNode` -> terrain `spk::RemoteNode::Endpoint` -> Chunk::Collection/PrototypeChunkProvider -> response path is covered in Server integration tests.

### Performance

No numeric budget until OQ-031.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-014](../../../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md)
- [DR-016](../../../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
- [DR-020](../../../DECISIONS/DR-020-HEADLESS-ASYNC-TASK-INFRASTRUCTURE.md)
- [DR-021](../../../DECISIONS/DR-021-REMOTE-SERVER-NODES-FROM-FIRST-IMPLEMENTATION.md)
- [OQ-038](../../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — resolved by DR-022 for the shared Chunk protocol.
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) — resolved; exact prototype terrain is fixed by DR-015.

## Completion evidence

Server tests cover all final request/rejection/correlation/disconnect cases and prove responses contain canonical voxel data only.
