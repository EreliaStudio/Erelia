# Current Status

**Updated:** 25 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-09-server-chunk-request-handler`

## Branch state

ST-001-01 through ST-001-08 are completed on `master`.

The latest `master` removes the temporary Erelia-local Sparkle prototypes after those reusable facilities were upstreamed into Sparkle Version-0.1.3. Erelia consumes the Sparkle-owned ArgumentParser, JSON Catalog/error helpers, ThreadSafeSet, ThreadSafeQueue, Task, WorkerPool, Singleton, and the protocol-v2 networking support required by the Chunk protocol.

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

ST-001-09 remains **Blocked** until its remaining Server-specific lifecycle/failure behavior and the exact Collection/Provider bridge for per-protocol-request grouped completion are explicitly resolved. ST-001-08 and OQ-038 no longer block it.

The active feature branch now contains the first approved batching scaffold: an Erelia-local `spk::TaskGroup<TResult>` prototype with focused Core tests, grouped `PrototypeChunkProvider` WorkerPool submission, and TerrainNode ownership/update-driving of the authoritative `Chunk::Collection`. TaskGroup aggregation is passive: it does not occupy a worker while waiting for child Tasks.

The next planning pass should also include the transport-level reception smoke coverage already identified for ST-001-09: prove that a real `ChunkRequest` crosses Client -> NodeRouter -> RemoteNode -> terrain Endpoint byte-for-byte before testing request parsing/handling semantics.

## Explicit non-goals

Do not start Client loading/cache/retry policy, rendering, production terrain generation, or gameplay systems before their owning tickets are Ready.
