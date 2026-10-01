# Current Status

**Updated:** 1 October 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-11-client-chunk-request-cache-coordinator`; `master` remains the completed ST-001-01 through ST-001-10 baseline

## Active ST-001-11 implementation

[PR #19](https://github.com/EreliaStudio/Erelia/pull/19) contains the implementation on the requested existing branch. ST-001-11 is **In Progress**, implemented, with component/routed integration CI passing; completion is blocked by the inherited executable-disconnect contract conflict and project-owner approval. Local Linux Debug/Release headless builds and CTest pass (3/3 each), including Core 121/121 and Server/Terrain 18/18; focused streaming 9/9 and repository clang-format pass. Final production CI run #610 passes formatting, all four Core/Server jobs and both Client jobs; both routed integration suites pass, while the process smoke fails its inherited exit-on-disconnect assertion. Exact evidence and conflicting ST-001-10 clauses live in the ticket.

The active implementation uses generic Collections/Providers/TaskGroups, shared Column occupancy, generated Collection message families, payload-only diagnostics, independent non-recycled RequestIDs, generic Response section tables and authoritative Client Update handling. ClientNetworkManager uniquely drains the Service Client queue and fans out disconnect; the Player owns the Column -> Chunk streaming Behaviour and horizontal retention policy. TerrainNode uniquely drains its Endpoint; the central Server only redirects ChunkRequest and ColumnRequest.

The historical ST-001-08/ST-001-09 implementation summaries below remain completion provenance and do not override ST-001-11's superseding contract. Error-without-Response non-settlement and the other listed deferred work remain accepted debt.

Next in dependency order is ST-001-12, **Blocked by OQ-036** (missing-neighbor/remesh policy). No later ticket is Ready.

## Branch state

ST-001-01 through ST-001-10 are completed on `master`.

ST-001-07 is **Done** and merged through PR #14. Its current topology is the DR-021 model: one Client-facing `spk::NodeRouter` in `EreliaServer`, with terrain running as a separate process behind `spk::RemoteNode` / `spk::RemoteNode::Endpoint`; the earlier in-process `spk::LocalNode` bootstrap described by DR-016 is superseded for deployment.

ST-001-09 is merged into `master` through PR #17. The delivered implementation consumes Sparkle Version-0.1.3 directly for generic Task settlement, direct-callable WorkerPool execution, thread-safe ContractProvider, and `spk::TaskGroup<TResult>`. The obsolete Erelia-local `task_group.hpp` and duplicate Core tests are removed. Sparkle Version-0.1.3 also moved `ArgumentParser` to the dedicated `<system/argument_parser.hpp>` path; Erelia and its server-node template use that path.

ST-001-10 is **Done** and merged into `master` through PR #18 on 28 September 2026. Final PR CI run #604 (run ID `36409915048`) passed the complete matrix on reviewed head `16e313a54237b107504caefcc743293110872204`, including clang-format, Core/Server Linux Debug+Release, Core/Server Windows Debug+Release, Client Windows Debug+Release, and Integration Windows Debug+Release.

ST-001-08 is merged through PR #16 and provides the historically reviewed first batched Chunk Request/Response/Error protocol implementation and dedicated tests. ST-001-09 subsequently refined and implemented the terminal Response and diagnostic model without invalidating that historical completion evidence.

## Validation / review state

OQ-038 is Resolved.

DR-022 records the original resolved ST-001-08 wire contract plus the implemented ST-001-09 refinement to nested Response Success/Failure entries and the generic diagnostic mechanism.

ST-001-08 is **Done** after project-owner review. CI run #356 (run ID `36138476545`) passed on the reviewed PR head `a4c0e29059cec422bee848dff2c465ad26c53493`.

The historical ST-001-08 merged contract includes:

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

ST-001-09 — Server Chunk request handler is **Done** and merged into `master` through PR #17 on 26 September 2026. Final PR CI run #478 (run ID `36260702871`) passed the full matrix on reviewed head `18a2a52dbd43215c4a9a51d42e872ff7d32731f7`, including clang-format, Core/Server Linux Debug+Release, Core/Server Windows Debug+Release, Client Windows Debug+Release, and dedicated Integration Windows Debug+Release.

A dedicated cross-system integration layer now lives under `tests/integration/`. `EreliaIntegrationTestSuite` links `EreliaClientLibrary`, `EreliaServerLibrary`, and the required Server-node libraries, and is registered with the CTest `integration` label. The delivered Chunk fixtures exercise the real network route through Router and TerrainNode and validate canonical DR-015 single- and multi-coordinate Chunk results, duplicate-coordinate diagnostics, malformed-request diagnostics, two concurrent Clients with correctly correlated responses, and disconnect during an outstanding acquisition followed by a successful request from a new Client. ST-001-10 now provides the Erelia Client connection lifecycle at the outer transport edge through `ClientRuntime`; the existing ST-001-09 Chunk request fixtures intentionally continue to use raw `spk::Client` only where send/message/request coordination is owned by ST-001-11. Final PR CI run #478 (run ID `36260702871`) validated this integration coverage in dedicated `Integration (Windows, Debug)` and `Integration (Windows, Release)` GitHub Actions jobs, while Client jobs ran component tests separately.

**ST-001-10 — Client dedicated-Server connection** is **Done** on `master` through PR #18. The delivered Client uses explicit endpoint configuration, process-wide Service-owned Sparkle dependencies, `MainApplicationWidget` composition, and a `ConnectionManager` that serializes synchronous Sparkle connection attempts through the shared WorkerPool, distinguishes Task settlement from live `isConnected()` state, performs bounded three-attempt automatic retry, and exposes `/connect` for a fresh cycle. Client component coverage, real Router integration, and the separate-process `EreliaClient -> EreliaServer` smoke are included. Final PR CI run #604 (run ID `36409915048`) is green across the full required matrix.

ST-001-11 is the active implementation area and is now **In Progress** on `feat/st-001-11-client-chunk-request-cache-coordinator`. Its Client cache/streaming/network policy is finalized: generic `Collection<TKey, TElement>`, Provider-owned Pending state, Column -> Chunk streaming, Client view/unload retention, per-request-type monotonic non-recycled RequestIDs, generic Collection Request/Response/Update/Error families, payload-only `Networking::Diagnostic`, generic Response offset-table sectioning across Success and Failure, and explicit accepted debt for Error-without-Response Pending settlement.

The historical implemented ST-001-09 contract reports a true Collection batch/outer TaskGroup aggregation failure as one correlated generic Diagnostic with severity `Error`, translation key `"Chunk_Request_Aggregation_Failure"`, and the original RequestID; no ChunkResponse is emitted because no valid BatchResult exists. ST-001-11 preserves this as delivered behavior until the generic Collection protocol migration replaces the standalone Diagnostic MessageID with family Error messages. Sparkle Version-0.1.3 provides the generic manually-settled `spk::Task<TResult>`, direct-callable WorkerPool execution, completion subscriptions, thread-safe ContractProvider, and `spk::TaskGroup<TResult>` used by the implementation.

The delivered ST-001-09 acquisition direction, retained as migration baseline, is:

- TerrainNode keeps each Client protocol Request intact and partitions its distinct coordinates into smaller internal batches;
- `Chunk::Collection::request(vector<Coordinate>)` returns one manually-settled `Task<BatchResult>::Answer` per internal batch;
- `Chunk::Collection::BatchResult` is fixed with nested `Acquired { coordinate, chunk }` and `Failed { coordinate, std::exception_ptr exception }` entries, stored in `acquired` and `failed` vectors;
- Collection reuses already-Pending coordinate Answers, copies already-Available Chunks, and asks Provider only for Absent coordinates;
- `Chunk::Collection::Provider` accepts exactly one coordinate and returns one WorkerPool-produced `Task<Chunk>::Answer`;
- Collection subscribes to those coordinate Answers and settles its batch Task only after every coordinate is terminal;
- after every coordinate Task is terminal, Collection validates the BatchResult containing all success/failure outcomes; ordinary per-coordinate failure does not fail the batch Task;
- TerrainNode groups the Collection batch Answers in one Sparkle TaskGroup.

The delivered ST-001-09 historical wire model is: `Chunk::Protocol::Response` owns nested `Response::Success { coordinate, chunk }` and `Response::Failure { coordinate, Failure::Code, message }` entries, with `Response::Failure::Code::AcquisitionFailed = 0` as the currently defined terminal acquisition-failure code. Failure messages use Sparkle's existing `uint32_t` byte-length-prefixed Message string encoding with no null terminator. Finalized Responses remain Message-backed. The old `Rejected/Unavailable` grouping is superseded by the implemented Success/Failure response model. Non-terminal diagnostics use the implemented generic `Networking::Diagnostic` base, while `Chunk::Protocol::Error` remains its Chunk-specific coordinate-list specialization.

In the delivered ST-001-09 implementation, the internal batch size is fixed at 1024 coordinates as a TerrainNode implementation constant rather than configuration or protocol state. Since the current Chunk Request maximum is also 1024, every valid request currently maps to one Collection batch. A multi-batch TerrainNode TaskGroup protocol fixture is therefore intentionally deferred until those limits diverge. The generic diagnostic payload is fixed to severity + stable string translation key only, with later contextual extensions explicitly left for future work. Its public type is fixed as `Networking::Diagnostic` in the generic networking layer, with `Trace = 0`, `Info = 1`, `Warning = 2`, and `Error = 3`. `Chunk::Protocol::Error` is retained as a specialization deriving from Diagnostic and adds only a problematic-coordinate list; its serialization reuses the Diagnostic prefix. The Chunk diagnostic coordinate count is fixed as `std::uint32_t`, followed by that many contiguous coordinates. Message types preserve `ChunkError = 3` and add `Diagnostic = 4`. Diagnostic correlation is fixed: generic Diagnostic may be uncorrelated with RequestID 0 or correlated with a non-zero originating RequestID; `Chunk::Protocol::Error` requires and reuses the originating non-zero Chunk RequestID. Malformed Chunk input is correlated only when a valid RequestID remains available. TerrainNode request lifetime is also fixed: completion callbacks publish into a shared thread-safe mailbox, `dispatch()` owns Endpoint replies, shutdown unsubscribes/discards without waiting for acquisition, disconnect does not cancel acquisition, and reply/send exceptions are logged and dropped. The Success/Failure Response byte layout is fixed to one `uint32_t failureOffset` followed by sorted Success entries then sorted Failure entries. Diagnostic strings are stable translation keys rather than user-facing prose. Terrain-side dispatch ownership is fixed: `TerrainNodeApplication` drains `RemoteNode::Endpoint::requests()`, switches on message type, and delegates each supported message type to a dedicated handler while retaining the Endpoint Request envelope for reply routing. The main Server registers `ChunkRequest -> "terrain"` immediately after constructing `Router`. Duplicate-coordinate diagnostics are `Warning / "Chunk_Coordinates_Duplication"`; malformed-request diagnostics are `Error / "Chunk_Request_Malformed"`. The terrain dispatcher/reply path and integration tests are implemented and validated by final PR CI run #478. Exception-to-message mapping is fixed: `spk::Exception::message()`, otherwise `std::exception::what()`, otherwise exactly `"Unknown acquisition failure"`.

The Core implementation is now reconciled with Sparkle: `Networking::Diagnostic`, specialized `Chunk::Protocol::Error`, the refined Success/Failure `Chunk::Protocol::Response`, the batched asynchronous `Chunk::Collection`, and single-coordinate WorkerPool-backed `PrototypeChunkProvider` are implemented with dedicated deterministic tests. Main Server routing now registers `ChunkRequest -> "terrain"`, and `TerrainNode` exposes its Endpoint request queue for the application dispatcher.


## Approved ST-001-11 contract — 1 October 2026

The merged Sparkle Version-0.1.3 Message redesign is incorporated into the ST-001-11 branch: finalized Messages are immutable, construction uses `spk::Message::Writer`, and decoding uses independent `spk::Message::Reader` instances backed by shared pooled storage.

The ST-001-11 protocol intentionally supersedes the historical fixed Chunk MessageID values and standalone Diagnostic MessageID. Chunk and Column now each use the generic Collection Request/Response/Update/Error family; Diagnostic is payload-only. Chunk and Column RequestID sequences are independent, each starts at 1, increments monotonically as `uint64_t`, and is never recycled.

Chunk and Column own static compile-time tuning constants for maximum elements per Request and elements per Response section. Numeric values are implementation tuning knobs rather than durable protocol identifiers.

The generic Collection Response owns the offset table used for both Success and Failure section entry points. Error messages remain diagnostic-only on the Client for ST-001-11; the case where an Error is emitted without a terminal Response and leaves Pending work unresolved is explicitly accepted technical debt for later reliability work.

## Explicit non-goals

ST-001-11 implementation is delivered on its existing branch and awaits the completion gate above. Do not start later rendering/meshing, production terrain generation, or gameplay systems before their owning tickets are Ready.
