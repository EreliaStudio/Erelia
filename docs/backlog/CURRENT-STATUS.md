# Current Status

**Updated:** 25 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-08-batched-chunk-protocol-contract`

## Branch state

ST-001-01 through ST-001-07 are completed on `master`.

The latest `master` also removes the temporary Erelia-local Sparkle prototypes after those reusable facilities were upstreamed into Sparkle Version-0.1.3. Erelia now consumes the Sparkle-owned ArgumentParser, JSON Catalog/error helpers, ThreadSafeSet, ThreadSafeQueue, Task, WorkerPool, and Singleton facilities.

Sparkle Version-0.1.3 additionally provides the ST-001-08 prerequisites: native `spk::Message::RequestID`, `requestID()` / `setRequestID()`, checked cursor-independent `readAt()`, protocol-version-2 network framing carrying RequestID, and RequestID preservation through RemoteNode envelopes.

The active feature branch is rebased on that `master` state and now contains the ST-001-08 Core protocol implementation, dedicated Core tests, and the synchronized protocol documentation.

## Validation / review state

OQ-038 is Resolved.

DR-022 records the final batched Chunk Request/Response/Error wire contract.

ST-001-08 is **In Progress** only because project-owner review is still pending. The Core implementation is complete on PR #16, and CI run #352 (run ID `36133142739`) passed the complete required matrix on code head `94d9196afa2f79608e2ec3ff549c94aa6d101221`.

The resolved contract fixes:

- `Networking::MessageType` values `ChunkRequest = 1`, `ChunkResponse = 2`, `ChunkError = 3`;
- non-zero Sparkle RequestID correlation with protocol-owned atomic Request generation;
- count-less Request and Error payloads;
- a three-offset count-less Response summary;
- Success / Rejected / Unavailable result states;
- deterministic state grouping and X/Y/Z ordering;
- nested Request/Error/Response Builders own temporary construction containers, while finalized protocol values use only their `spk::Message` payload as persistent storage;
- Builders allocate the finalized payload once with `resize()` and encode it through `edit()`; Request duplicate checks are Debug-only developer validation, while finalized/raw Request payloads retain defensive on-demand duplicate inspection;
- distinct duplicate-coordinate diagnostics;
- `ChunkError` before the terminal `ChunkResponse`;
- safe session-scoped RequestID reuse only after all outstanding terminal Responses;
- strict Core malformed-input validation and later network-boundary Warning/drop behavior;
- the approved typed construction APIs and exact test matrix.

## Next implementation step

Review **ST-001-08 — Batched Chunk request/response protocol contract** in PR #16.

The implementation remains Core-only. Do not start ST-001-09 Server request handling or ST-001-11 Client retry/cache/coordinator policy as part of this review. After project-owner approval and merge, the dependency status can advance accordingly.

## Explicit non-goals

Do not implement Server Chunk handling, Client loading/cache/retry policy, rendering, production terrain generation, or gameplay systems beyond the owning Ready ticket.
