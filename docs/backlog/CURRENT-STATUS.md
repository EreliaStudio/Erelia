# Current Status

**Updated:** 26 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-09-server-chunk-request-handler`

## Branch state

ST-001-01 through ST-001-08 are completed on `master`.

The active ST-001-09 branch now consumes Sparkle Version-0.1.3 directly for generic Task settlement, direct-callable WorkerPool execution, thread-safe ContractProvider, and `spk::TaskGroup<TResult>`. The obsolete Erelia-local `task_group.hpp` and duplicate Core tests have been removed. Sparkle Version-0.1.3 also moved `ArgumentParser` to the dedicated `<system/argument_parser.hpp>` path; Erelia and its server-node template use that new path.

ST-001-08 is merged through PR #16 and provides the historically reviewed first batched Chunk Request/Response/Error protocol implementation and dedicated tests. ST-001-09 subsequently refined and implemented the terminal Response and diagnostic model without invalidating that historical completion evidence.

## Validation / review state

OQ-038 is Resolved.

DR-022 records the original resolved ST-001-08 wire contract plus the implemented ST-001-09 refinement to nested Response Success/Failure entries and the generic diagnostic mechanism.

ST-001-08 is **Done** after project-owner review. CI run #356 (run ID `36138476545`) passed on the reviewed PR head `a4c0e29059cec422bee848dff2c465ad26c53493`.

The merged contract includes:

- `Networking::MessageType` values `ChunkRequest = 1`, `ChunkResponse = 2`, `ChunkError = 3`;
- non-zero Sparkle RequestID correlation with protocol-owned atomic Request generation;
- count-less Request and Error payloads;
- a three-offset count-less Response summary;
- historical ST-001-08 Success / Rejected / Unavailable result states, now superseded as the future target by the ST-001-09 Response::Success / Response::Failure refinement;
- deterministic state grouping and X/Y/Z ordering;
- Request/Error/Response declarations split one message per public header while `Chunk::Protocol` remains the semantic nested scope;
- nested Request/Error/Response Builders owning temporary construction containers, with finalized protocol values using only their `spk::Message` payload as persistent storage;
- one-shot `resize()` + `edit()` encoding;
- Debug-only Request Builder duplicate validation plus defensive on-demand duplicate inspection from finalized/raw Request payloads;
- historical distinct duplicate-coordinate `ChunkError` diagnostics; ST-001-09 supersedes the independent Error format with the implemented generic `Networking::Diagnostic` base plus Chunk-specific Error specialization;
- historical `ChunkError` before the terminal `ChunkResponse` ordering;
- safe session-scoped RequestID reuse only after all outstanding terminal Responses;
- strict Core malformed-input validation;
- the approved typed construction APIs and exact Core test matrix.

## Next implementation step

ST-001-09 — Server Chunk request handler is **Done** on `feat/st-001-09-server-chunk-request-handler`. CI run #464 (run ID `36233964005`) passed the full matrix on code head `b15896137e9eb13b92b2ed150541383fe0e4bff9`.

A dedicated cross-system integration layer now lives under `tests/integration/`. `EreliaIntegrationTestSuite` links `EreliaClientLibrary`, `EreliaServerLibrary`, and the required Server-node libraries, and is registered with the CTest `integration` label. The initial Chunk fixture exercises the real network route through Router and TerrainNode, validates canonical DR-015 Chunk content, duplicate-coordinate diagnostics, and malformed-request diagnostics. Until ST-001-10/ST-001-11 provide the Erelia Client networking API, the outer transport edge uses `spk::Client`; the integration harness is explicitly intended to switch to the real Erelia Client API once available. CI run #467 (run ID `36235423837`) passed the complete matrix on integration-pipeline code head `440b2f961177184a1d347b3d9b6f072d15e55d03`, including separate component-test and integration-test phases in Windows Debug and Release.

The next dependency-ordered ticket is **ST-001-10 — Client dedicated-Server connection**, which remains Draft pending its own endpoint/connection-lifecycle specification.

The implemented ST-001-09 contract reports a true Collection batch/outer TaskGroup aggregation failure as one correlated generic Diagnostic with severity `Error`, translation key `"Chunk_Request_Aggregation_Failure"`, and the original RequestID; no ChunkResponse is emitted because no valid BatchResult exists. Sparkle Version-0.1.3 provides the generic manually-settled `spk::Task<TResult>`, direct-callable WorkerPool execution, completion subscriptions, thread-safe ContractProvider, and `spk::TaskGroup<TResult>` used by the implementation.

The approved direction is:

- TerrainNode keeps each Client protocol Request intact and partitions its distinct coordinates into smaller internal batches;
- `Chunk::Collection::request(vector<Coordinate>)` returns one manually-settled `Task<BatchResult>::Answer` per internal batch;
- `Chunk::Collection::BatchResult` is fixed with nested `Acquired { coordinate, chunk }` and `Failed { coordinate, std::exception_ptr exception }` entries, stored in `acquired` and `failed` vectors;
- Collection reuses already-Pending coordinate Answers, copies already-Available Chunks, and asks Provider only for Absent coordinates;
- `Chunk::Collection::Provider` accepts exactly one coordinate and returns one WorkerPool-produced `Task<Chunk>::Answer`;
- Collection subscribes to those coordinate Answers and settles its batch Task only after every coordinate is terminal;
- after every coordinate Task is terminal, Collection validates the BatchResult containing all success/failure outcomes; ordinary per-coordinate failure does not fail the batch Task;
- TerrainNode groups the Collection batch Answers in one Sparkle TaskGroup.

ST-001-09 implements the refined terminal wire model: `Chunk::Protocol::Response` owns nested `Response::Success { coordinate, chunk }` and `Response::Failure { coordinate, Failure::Code, message }` entries, with `Response::Failure::Code::AcquisitionFailed = 0` as the currently defined terminal acquisition-failure code. Failure messages use Sparkle's existing `uint32_t` byte-length-prefixed Message string encoding with no null terminator. Finalized Responses remain Message-backed. The old `Rejected/Unavailable` grouping is superseded by the implemented Success/Failure response model. Non-terminal diagnostics use the implemented generic `Networking::Diagnostic` base, while `Chunk::Protocol::Error` remains its Chunk-specific coordinate-list specialization.

The internal batch size is fixed at 1024 coordinates as a TerrainNode implementation constant rather than configuration or protocol state. Since the current Chunk Request maximum is also 1024, every valid request currently maps to one Collection batch. The generic diagnostic payload is fixed to severity + stable string translation key only, with later contextual extensions explicitly left for future work. Its public type is fixed as `Networking::Diagnostic` in the generic networking layer, with `Trace = 0`, `Info = 1`, `Warning = 2`, and `Error = 3`. `Chunk::Protocol::Error` is retained as a specialization deriving from Diagnostic and adds only a problematic-coordinate list; its serialization reuses the Diagnostic prefix. The Chunk diagnostic coordinate count is fixed as `std::uint32_t`, followed by that many contiguous coordinates. Message types preserve `ChunkError = 3` and add `Diagnostic = 4`. Diagnostic correlation is fixed: generic Diagnostic may be uncorrelated with RequestID 0 or correlated with a non-zero originating RequestID; `Chunk::Protocol::Error` requires and reuses the originating non-zero Chunk RequestID. Malformed Chunk input is correlated only when a valid RequestID remains available. TerrainNode request lifetime is also fixed: completion callbacks publish into a shared thread-safe mailbox, `dispatch()` owns Endpoint replies, shutdown unsubscribes/discards without waiting for acquisition, disconnect does not cancel acquisition, and reply/send exceptions are logged and dropped. The Success/Failure Response byte layout is fixed to one `uint32_t failureOffset` followed by sorted Success entries then sorted Failure entries. Diagnostic strings are stable translation keys rather than user-facing prose. Terrain-side dispatch ownership is fixed: `TerrainNodeApplication` drains `RemoteNode::Endpoint::requests()`, switches on message type, and delegates each supported message type to a dedicated handler while retaining the Endpoint Request envelope for reply routing. The main Server registers `ChunkRequest -> "terrain"` immediately after constructing `Router`. Duplicate-coordinate diagnostics are `Warning / "Chunk_Coordinates_Duplication"`; malformed-request diagnostics are `Error / "Chunk_Request_Malformed"`. The terrain dispatcher/reply path and integration tests are implemented and validated by CI run #464. Exception-to-message mapping is fixed: `spk::Exception::message()`, otherwise `std::exception::what()`, otherwise exactly `"Unknown acquisition failure"`.

The Core implementation is now reconciled with Sparkle: `Networking::Diagnostic`, specialized `Chunk::Protocol::Error`, the refined Success/Failure `Chunk::Protocol::Response`, the batched asynchronous `Chunk::Collection`, and single-coordinate WorkerPool-backed `PrototypeChunkProvider` are implemented with dedicated deterministic tests. Main Server routing now registers `ChunkRequest -> "terrain"`, and `TerrainNode` exposes its Endpoint request queue for the application dispatcher.


## Explicit non-goals

Do not start Client loading/cache/retry policy, rendering, production terrain generation, or gameplay systems before their owning tickets are Ready.
