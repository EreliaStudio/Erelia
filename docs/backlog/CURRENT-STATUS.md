# Current Status

**Updated:** 26 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-09-server-chunk-request-handler`

## Branch state

ST-001-01 through ST-001-08 are completed on `master`.

The latest `master` removes the earlier temporary Erelia-local Sparkle prototypes after those reusable facilities were upstreamed into Sparkle Version-0.1.3. Sparkle Version-0.1.3 now also contains the merged generic Task settlement contract, direct-callable WorkerPool TaskJob execution, thread-safe ContractProvider, and TaskGroup required by ST-001-09. Erelia should consume these Sparkle-owned facilities directly rather than retain the temporary TaskGroup prototype currently present on the active feature branch.

ST-001-08 is merged through PR #16. Core now owns the finalized batched Chunk Request/Response/Error protocol and its dedicated tests.

## Validation / review state

OQ-038 is Resolved.

DR-022 records the final batched Chunk Request/Response/Error wire contract.

ST-001-08 is **Done** after project-owner review. CI run #356 (run ID `36138476545`) passed on the reviewed PR head `a4c0e29059cec422bee848dff2c465ad26c53493`.

The merged contract includes:

- `Networking::MessageType` values `ChunkRequest = 1`, `ChunkResponse = 2`, `ChunkError = 3`;
- non-zero Sparkle RequestID correlation with protocol-owned atomic Request generation;
- count-less Request and Error payloads;
- a three-offset count-less Response summary;
- Success / Rejected / Unavailable result states;
- deterministic state grouping and X/Y/Z ordering;
- Request/Error/Response declarations split one message per public header while `Chunk::Protocol` remains the semantic nested scope;
- nested Request/Error/Response Builders owning temporary construction containers, with finalized protocol values using only their `spk::Message` payload as persistent storage;
- one-shot `resize()` + `edit()` encoding;
- Debug-only Request Builder duplicate validation plus defensive on-demand duplicate inspection from finalized/raw Request payloads;
- distinct duplicate-coordinate diagnostics;
- `ChunkError` before the terminal `ChunkResponse`;
- safe session-scoped RequestID reuse only after all outstanding terminal Responses;
- strict Core malformed-input validation;
- the approved typed construction APIs and exact Core test matrix.

## Next implementation step

The next dependency-ordered ticket is **ST-001-09 — Server Chunk request handler**.

ST-001-09 remains **Blocked**, but its Collection/Provider asynchronous ownership is now resolved. Sparkle Version-0.1.3 provides the generic manually-settled `spk::Task<TResult>`, direct-callable WorkerPool execution, completion subscriptions, thread-safe ContractProvider, and `spk::TaskGroup<TResult>` needed by the selected design.

The approved direction is:

- TerrainNode keeps each Client protocol Request intact and partitions its distinct coordinates into smaller internal batches;
- `Chunk::Collection::request(vector<Coordinate>)` returns one manually-settled `Task<BatchResult>::Answer` per internal batch;
- a successful BatchResult contains every requested coordinate with a shallow-copied immutable Chunk;
- Collection reuses already-Pending coordinate Answers, copies already-Available Chunks, and asks Provider only for Absent coordinates;
- `Chunk::Collection::Provider` accepts exactly one coordinate and returns one WorkerPool-produced `Task<Chunk>::Answer`;
- Collection subscribes to those coordinate Answers and settles its batch Task only after every coordinate is terminal;
- if any coordinate Task fails, the entire Collection batch Task is Failed and no partial BatchResult is exposed;
- TerrainNode groups the Collection batch Answers in one Sparkle TaskGroup.

The remaining ST-001-09 blockers are now Server-specific: the internal batch-size rule; the mapping from a failed Collection batch / failed outer TaskGroup onto the fixed DR-022 wire protocol; and outstanding-request/reply/disconnect/shutdown lifetime behavior. DR-022 currently has only per-coordinate Success / Rejected / Unavailable Response states and DuplicateCoordinate as a ChunkError code, so no new request-level failure representation may be invented implicitly.

The temporary Erelia-local TaskGroup scaffold currently on the feature branch is now obsolete and should be removed when implementation is reconciled with the merged Sparkle dependency.

The next planning pass should also include the transport-level reception smoke coverage already identified for ST-001-09: prove that a real `ChunkRequest` crosses Client -> NodeRouter -> RemoteNode -> terrain Endpoint byte-for-byte before testing request parsing/handling semantics.

## Explicit non-goals

Do not start Client loading/cache/retry policy, rendering, production terrain generation, or gameplay systems before their owning tickets are Ready.
