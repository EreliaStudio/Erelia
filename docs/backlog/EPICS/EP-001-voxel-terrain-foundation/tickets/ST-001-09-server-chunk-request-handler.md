# ST-001-09 — Server Chunk request handler

**Status:** Done
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

- Receive routed Chunk requests in the terrain node's `spk::RemoteNode::Endpoint` process. `TerrainNodeApplication` owns terrain-side message dispatch: after Endpoint dispatch, it drains `spk::RemoteNode::Endpoint::requests()`, switches on the contained Message type, and forwards each supported type to a dedicated handler.
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

The shared request limits, duplicate-detection semantics, correlation, and strict malformed-input validation originate from the completed ST-001-08/DR-022 work. ST-001-09 planning has since refined the terminal Response representation: `Chunk::Protocol::Response` owns nested `Success` and `Failure` entry types, replacing the earlier target `Success / Rejected / Unavailable` state grouping.

Sparkle Version-0.1.3 now provides the complete asynchronous composition model required here:

- `spk::Task<TResult>` is a generic manually-settled asynchronous result with `validate(TResult)` / `fail(std::exception_ptr)`;
- `Task<TResult>::Answer::subscribeToCompletion(...)` provides race-safe completion subscription;
- `spk::WorkerPool::submit(callable)` runs executable work and returns a `Task<TResult>::Answer`;
- `spk::TaskGroup<TResult>` passively groups arbitrary Task Answers without consuming a worker merely to wait;
- thread-safe `ContractProvider` owns completion-contract synchronization.

The Erelia Collection/Provider ownership is now fixed semantically:

- TerrainNode splits one protocol Request into internal coordinate batches;
- `Chunk::Collection` accepts one coordinate vector per internal batch and returns one `spk::Task<BatchResult>::Answer`;
- `Chunk::Collection::BatchResult` owns nested networking-agnostic `Acquired` and `Failed` entry types:
  - `BatchResult::Acquired { coordinate, chunk }`;
  - `BatchResult::Failed { coordinate, std::exception_ptr exception }`;
- one BatchResult contains every requested coordinate exactly once across its `acquired` and `failed` collections;
- Collection owns Available/Pending/Absent lookup, pending-work reuse, subscriptions, and batch aggregation;
- `Chunk::Collection::Provider` generates exactly one Absent coordinate per request and returns one WorkerPool-produced `spk::Task<Chunk>::Answer`;
- Collection's batch Task is created directly and manually settled; it is never submitted to WorkerPool;
- the batch stays Pending until every coordinate dependency is terminal;
- each coordinate's terminal acquisition outcome is retained in the BatchResult;
- once every coordinate dependency is terminal, Collection validates the complete BatchResult even when one or more coordinate acquisitions failed;
- the Collection batch Task becomes Failed only when a batch/aggregation-level failure prevents Collection from producing a valid BatchResult.

The exact private Collection state representation remains an implementation detail. The public BatchResult shape is fixed as `Collection::BatchResult::Acquired` / `Collection::BatchResult::Failed`, with `std::vector<Acquired> acquired` and `std::vector<Failed> failed`.

`Chunk::Protocol::Response` owns the terminal wire-entry semantics:

```cpp
Response::Success { coordinate, chunk }
Response::Failure { coordinate, Failure::Code, message }

Response::Failure::Code {
    AcquisitionFailed = 0
}
```

These protocol entry types must not leak downward into `Chunk::Collection`; TerrainNode translates acquisition outcomes into them.

Finalized Response objects remain Message-backed. Builder-side temporary Success/Failure containers are discarded after encoding. The public decoding API is `failureOffset()`, `successCount()`, `success(index)`, `failureCount()`, and `failure(index)`. Builder exposes `addSuccess(coordinate, chunk)` and `addFailure(coordinate, Failure::Code, std::string)`. The refined Response payload contains one `std::uint32_t failureOffset` header, then sorted Success entries, then sorted Failure entries. Success occupies `[4, failureOffset)` and Failure occupies `[failureOffset, message.size())`; `failureOffset == 4` means no Success entries and `failureOffset == message.size()` means no Failure entries.

Non-terminal technical diagnostics use the generic `Networking::Diagnostic` base contract: severity + stable string translation key, with severity values `Trace = 0`, `Info = 1`, `Warning = 2`, and `Error = 3`. The string is intended for a future translation/localization engine and is not user-facing prose. `Chunk::Protocol::Error` remains as a specialized diagnostic deriving from `Networking::Diagnostic`; for ST-001-09 it adds only a list of problematic Chunk coordinates. Its serialization reuses the Diagnostic prefix and appends the coordinate list. The coordinate count is fixed as `std::uint32_t`, followed by that many contiguous `Chunk::Coordinate` values. Message types preserve `ChunkError = 3` and add generic `Diagnostic = 4`. Generic Diagnostic allows RequestID 0 for uncorrelated diagnostics or a non-zero originating RequestID for correlation. `Chunk::Protocol::Error` always requires and reuses the non-zero originating Chunk RequestID. Malformed Chunk input uses a correlated generic Diagnostic only when a valid non-zero RequestID remains available; otherwise it is uncorrelated.

The internal batch size is a fixed TerrainNode implementation constant of 1024 coordinates; it is not configurable and is not part of the wire protocol. With the current protocol maximum of 1024 coordinates, one valid Client request therefore maps to one internal Collection batch. The main `EreliaServer` process registers `ChunkRequest -> "terrain"` immediately after constructing `Router`; terrain-side dispatch remains owned by `TerrainNodeApplication`. Duplicate-coordinate diagnostics use `Networking::Diagnostic::Severity::Warning` with translation key exactly `"Chunk_Coordinates_Duplication"`; malformed-request diagnostics use `Networking::Diagnostic::Severity::Error` with translation key exactly `"Chunk_Request_Malformed"`. A true Collection batch/outer TaskGroup aggregation failure emits one correlated generic `Networking::Diagnostic` with severity `Error`, translation key exactly `"Chunk_Request_Aggregation_Failure"`, and the original non-zero Chunk RequestID. No `Chunk::Protocol::Response` is emitted for that request because no valid `BatchResult` exists to translate.

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

The Collection batch does not become terminal until all coordinate dependencies are terminal. Each coordinate contributes either a successful Chunk outcome or a networking-agnostic acquisition-failure outcome. Once every dependency is terminal, Collection calls `validate(BatchResult)` once with the complete set of outcomes. The batch Task calls `fail(...)` only for a batch/aggregation-level failure that prevents construction of a valid BatchResult.

The Client does not split its protocol request. One protocol Request may contain up to the ST-001-08 limit of 1024 coordinates. Internal Server batches are scheduling/composition units only: they do not create protocol RequestIDs, partial protocol Responses, or additional Client-visible request lifecycles.

The outer TaskGroup completion callback may run on the thread that settles the final child batch Task, or immediately on the subscribing thread if the group is already terminal. The handler implementation must therefore make the captured request/reply state safe for that callback lifetime and must not assume completion callbacks execute on the TerrainNode dispatch thread.

Malformed typed request -> catch the protocol decoding `spk::Exception` at the terrain consumer boundary -> log locally -> emit a generic `Networking::Diagnostic` with severity `Error` and translation key exactly `"Chunk_Request_Malformed"` through the originating reply path when possible, using the approved RequestID correlation rule -> no canonical state mutation -> continue serving later messages.

## Failure behavior

Per-coordinate acquisition/generation failure is part of the completed `BatchResult`, not failure of the Collection batch Task. A batch waits until every coordinate dependency is terminal, then exposes every requested coordinate's outcome together in one valid result.

The TerrainNode's outer `spk::TaskGroup<BatchResult>` therefore remains successful when child batches contain ordinary per-coordinate acquisition failures. A Collection batch Task, and therefore potentially the outer TaskGroup, becomes Failed only for a batch/aggregation-level failure that prevents a valid BatchResult from being produced. That outer failure is reported as `Error / "Chunk_Request_Aggregation_Failure"` using the original RequestID, with no terminal ChunkResponse.

This preserves successful coordinates from the same internal batch and prevents the internal batch partition from changing Client-visible success/failure semantics.

`Chunk::Protocol::Response` now owns terminal `Success` and `Failure` semantic entries. A Failure carries the coordinate, `Response::Failure::Code::AcquisitionFailed`, and a human-readable string. `AcquisitionFailed` is exactly numeric value 0. The string uses Sparkle's existing Message encoding: `uint32_t` byte length followed by the exact message bytes, with no null terminator.

The old `Rejected` / `Unavailable` terminal state split is no longer the target ST-001-09 response model.

TerrainNode translates `BatchResult::Failed::exception` into the human-readable Failure message by rethrowing it: `spk::Exception` uses `.message()`, other `std::exception` values use `.what()`, and non-standard exceptions use exactly `"Unknown acquisition failure"`. It always uses `Response::Failure::Code::AcquisitionFailed`. Completion callbacks never capture `TerrainNode*` and never send directly through the Endpoint: they publish completed request results into a thread-safe shared mailbox, and `TerrainNode::dispatch()` drains that mailbox and performs replies. TerrainNode shutdown stops accepting/draining replies, unsubscribes completion contracts, and discards pending mailbox entries without waiting for outstanding acquisition tasks. Disconnection does not cancel acquisition. Reply/send exceptions are caught, logged, and the reply is dropped without mutating the completed Collection result.

## Determinism / ordering

Provider/Collection output follows ST-001-06. Response ordering/association follows ST-001-08; no extra Server-specific ordering may be invented.

## Lifecycle / ownership

The terrain node process owns request-processing lifetime. Completion callbacks publish only into the shared thread-safe mailbox; `TerrainNode::dispatch()` owns Endpoint reply emission. Shutdown unsubscribes completion contracts and discards pending replies without waiting for outstanding acquisition tasks, while disconnect does not cancel acquisition.

## Serialization / persistence

Uses ST-001-08's dedicated Chunk codec/protocol. Generic ST-001-05 Volume serialization remains available but is not the Chunk payload format. No persistence.

## Networking / authority

Server validates and returns canonical results. Client only requests coordinates.

## Implementation constraints

- Handler lives in `TerrainNodeApplication` in the terrain node process, not either executable `main.cpp`. Use one dedicated handler method per supported message type. Chunk request handling operates from the original `spk::RemoteNode::Endpoint::Request` envelope so proxy/origin connection context remains available for the later reply; the typed domain message is `Chunk::Protocol::Request`, not a Sparkle-owned Chunk type.
- Use Sparkle NodeRouter response path to the originating Client.
- Do not introduce a second transport or a Server-selected view radius.
- Consume Sparkle Version-0.1.3 directly; remove the temporary Erelia-local TaskGroup prototype and its duplicate Core tests.
- `spk::Task<TResult>` is the generic asynchronous result state. Collection may create a Task directly and settle it with `validate(...)` / `fail(...)` without WorkerPool execution.
- `spk::WorkerPool::submit(callable)` is used only for executable work. Provider generation is WorkerPool-backed; Collection batch aggregation is not.
- `spk::TaskGroup<TResult>` groups Task Answers regardless of whether those Tasks were manually settled or WorkerPool-produced. It does not submit Tasks and does not consume a worker merely to wait.
- TerrainNode owns the split of one protocol request into internal coordinate batches and owns the protocol-request TaskGroup. The batch size is the fixed implementation constant 1024, not terrain-node configuration or protocol state.
- Keep protocol correlation at the original RequestID: internal work batches never own protocol RequestIDs.
- `Chunk::Collection::request(vector<Chunk::Coordinate>)` must return one asynchronous `Task<BatchResult>::Answer` representing the complete internal batch.
- `Chunk::Collection::BatchResult` contains `std::vector<Acquired> acquired` and `std::vector<Failed> failed`, where `Acquired` stores coordinate + Chunk and `Failed` stores coordinate + `std::exception_ptr`. Chunk copies are shallow immutable snapshots through the existing Volume/Chunk ownership model.
- `Chunk::Collection::Provider` is a single-coordinate WorkerPool-facing generator. Its request operation accepts one `Chunk::Coordinate` and returns one `spk::Task<Chunk>::Answer`.
- Provider no longer owns Collection batching, request buffering for batches, or an `update(Collection&)` polling phase.
- Collection owns authoritative cache/deduplication semantics. A missing Chunk becomes Pending asynchronous work; no placeholder/empty Chunk is inserted.
- A Pending coordinate retains/reuses the unique in-flight `Task<Chunk>::Answer`. Overlapping Collection requests subscribe to that same Answer and must not ask Provider to regenerate the same coordinate.
- Collection subscribes to every Pending/new coordinate Answer and settles its own batch Task only after all coordinates are terminal.
- After all coordinate Answers are terminal, Collection validates the complete BatchResult containing all success/failure outcomes. Collection fails the batch Task only when aggregation itself cannot produce a valid BatchResult.
- The exact private structs/variant used for Absent/Pending/Available remain implementation details; the public `BatchResult::Acquired` / `BatchResult::Failed` shape is fixed.
- The grouped completion callback may execute outside the TerrainNode dispatch thread. Any captured Endpoint/request state must have safe lifetime and any network operation performed there must follow Sparkle's thread-safety contract.
- `Chunk::Protocol::Response` owns nested `Success` and `Failure` semantic entries; Collection must stay networking-agnostic and must not depend on those protocol types.
- The finalized Response remains Message-backed; temporary Builder Success/Failure containers are construction-only.
- `Response::Failure::Code` is fixed to `AcquisitionFailed = 0` for ST-001-09. Do not invent additional codes. Failure strings use Sparkle's `uint32_t` byte-length-prefixed Message string representation with no null terminator.
- Implement the approved generic `Networking::Diagnostic` base and retain `Chunk::Protocol::Error` as its Chunk-specific specialization with the approved diagnostic-prefix + `uint32_t` coordinate-count + contiguous-coordinate layout.

## Exact test fixtures

Ready fixtures include:

- one valid coordinate;
- valid multi-coordinate Collection batch including a negative coordinate;
- duplicate protocol-coordinate case;
- all-Available Collection batch completing immediately with all copied Chunks;
- all-Absent Collection batch producing exactly one Provider Task per distinct coordinate;
- mixed Available + Pending + Absent Collection batch;
- overlapping Collection requests that share a Pending coordinate, proving the same in-flight `Task<Chunk>::Answer` is reused and Provider is not called twice;
- completion of one shared Pending coordinate notifying every batch subscribed to that Answer;
- a Collection batch where one coordinate Task succeeds and another fails, proving the batch waits for all dependencies then completes with both coordinate outcomes preserved in its BatchResult;
- a TerrainNode grouped request where one Collection batch completes before another, proving no Client-visible terminal handling occurs until the outer TaskGroup is terminal;
- malformed payload;
- two Clients issuing distinguishable requests;
- disconnect during an outstanding request;
- deterministic generator failure through a purpose-built test Provider rather than depending on PrototypeChunkProvider output/failure.

The Server integration fixtures are implemented for routed multi-coordinate success, duplicate-coordinate diagnosis plus deduplicated processing, malformed-request diagnosis, and transport preservation. Core tests cover mixed per-coordinate success/failure, pending-work reuse, stale-work protection, and callback lifetime after Collection destruction.

## Acceptance tests

### Nominal

Valid single/multi requests return exact canonical Chunks.

### Boundaries

Approved batch-size boundaries.

### Invalid / rejected operations

Per OQ-038 final semantics.

### Failure atomicity

Malformed requests do not mutate canonical terrain state. A Collection batch with failed coordinate acquisitions still settles only after all coordinate dependencies are terminal, then completes with one outcome per requested coordinate. ST-001-09 still emits one terminal protocol Response for the original Client request; it does not emit partial protocol Responses.

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

Generator failure is retained as a per-coordinate BatchResult outcome; its DR-022 wire mapping and network send failure behavior are fixed by the approved Failure code/message mapping and terrain reply-lifecycle contract.

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

Implementation is complete on `feat/st-001-09-server-chunk-request-handler`. CI run #464 (run ID `36233964005`) passed the full matrix on code head `b15896137e9eb13b92b2ed150541383fe0e4bff9`: clang-format, Core/Server Linux Debug+Release, Core/Server Windows Debug+Release, and Client Windows Debug+Release. Server integration tests cover the real Client -> Router -> RemoteNode -> terrain Endpoint path, routed multi-coordinate responses, duplicate-coordinate diagnostics, and malformed-request diagnostics; Core tests cover the asynchronous Collection/Provider result and lifetime contracts.
