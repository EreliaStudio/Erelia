# Current Status

**Updated:** 26 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-09-server-chunk-request-handler`

## Branch state

ST-001-01 through ST-001-08 are completed on `master`.

The latest `master` removes the earlier temporary Erelia-local Sparkle prototypes after those reusable facilities were upstreamed into Sparkle Version-0.1.3. Sparkle Version-0.1.3 now also contains the merged generic Task settlement contract, direct-callable WorkerPool TaskJob execution, thread-safe ContractProvider, and TaskGroup required by ST-001-09. Erelia should consume these Sparkle-owned facilities directly rather than retain the temporary TaskGroup prototype currently present on the active feature branch.

ST-001-08 is merged through PR #16 and provides the historically reviewed first batched Chunk Request/Response/Error protocol implementation and dedicated tests. ST-001-09 planning now refines the future terminal Response and diagnostic model without invalidating that historical completion evidence.

## Validation / review state

OQ-038 is Resolved.

DR-022 records the original resolved ST-001-08 wire contract plus the later ST-001-09 refinement toward nested Response Success/Failure entries and a future generic diagnostic mechanism.

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
- historical distinct duplicate-coordinate `ChunkError` diagnostics; ST-001-09 now plans to move non-terminal diagnostics to a future generic diagnostic message whose wire contract is still unresolved;
- historical `ChunkError` before the terminal `ChunkResponse` ordering;
- safe session-scoped RequestID reuse only after all outstanding terminal Responses;
- strict Core malformed-input validation;
- the approved typed construction APIs and exact Core test matrix.

## Next implementation step

The next dependency-ordered ticket is **ST-001-09 — Server Chunk request handler**.

ST-001-09 remains **Blocked**, but its Collection/Provider asynchronous ownership is now resolved. Sparkle Version-0.1.3 provides the generic manually-settled `spk::Task<TResult>`, direct-callable WorkerPool execution, completion subscriptions, thread-safe ContractProvider, and `spk::TaskGroup<TResult>` needed by the selected design.

The approved direction is:

- TerrainNode keeps each Client protocol Request intact and partitions its distinct coordinates into smaller internal batches;
- `Chunk::Collection::request(vector<Coordinate>)` returns one manually-settled `Task<BatchResult>::Answer` per internal batch;
- `Chunk::Collection::BatchResult` is fixed with nested `Acquired { coordinate, chunk }` and `Failed { coordinate, std::exception_ptr exception }` entries, stored in `acquired` and `failed` vectors;
- Collection reuses already-Pending coordinate Answers, copies already-Available Chunks, and asks Provider only for Absent coordinates;
- `Chunk::Collection::Provider` accepts exactly one coordinate and returns one WorkerPool-produced `Task<Chunk>::Answer`;
- Collection subscribes to those coordinate Answers and settles its batch Task only after every coordinate is terminal;
- after every coordinate Task is terminal, Collection validates the BatchResult containing all success/failure outcomes; ordinary per-coordinate failure does not fail the batch Task;
- TerrainNode groups the Collection batch Answers in one Sparkle TaskGroup.

ST-001-09 has additionally refined the terminal wire model: `Chunk::Protocol::Response` owns nested `Response::Success { coordinate, chunk }` and `Response::Failure { coordinate, Failure::Code, message }` entries. Finalized Responses remain Message-backed. The old `Rejected/Unavailable` grouping and Chunk-specific Error message are no longer the preferred future target; non-terminal diagnostics are expected to move to a generic diagnostic message, whose exact contract is still unresolved.

The remaining ST-001-09 blockers are Server/protocol-specific: the internal batch-size rule; mapping `BatchResult::Failed::exception` into the `Response::Failure::Code` set and variable-length failure-string wire encoding; the future generic diagnostic-message contract; and outstanding-request/reply/disconnect/shutdown lifetime behavior.

The temporary Erelia-local TaskGroup scaffold currently on the feature branch is now obsolete and should be removed when implementation is reconciled with the merged Sparkle dependency.

The next planning pass should also include the transport-level reception smoke coverage already identified for ST-001-09: prove that a real `ChunkRequest` crosses Client -> NodeRouter -> RemoteNode -> terrain Endpoint byte-for-byte before testing request parsing/handling semantics.

## Explicit non-goals

Do not start Client loading/cache/retry policy, rendering, production terrain generation, or gameplay systems before their owning tickets are Ready.
