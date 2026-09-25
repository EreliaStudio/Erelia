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
- Resolve each accepted coordinate through the ST-001-06 Server `Chunk::Collection` backed by `PrototypeChunkProvider`.
- Return canonical coordinate + Chunk results through the router to the originating Client.
- Keep two concurrent Client connections correlated correctly.
- Reject malformed/invalid requests without mutating authoritative terrain state.

## Explicitly not owned

Client retry/cache policy, production interest management, persistent terrain editing, rendering.

## Public contract

The shared request limits, duplicate semantics, result-state format, correlation, ordering, and malformed-input contract are fixed by the completed ST-001-08/DR-022 work. This ticket remains Blocked only until its own Server lifecycle/failure details are explicit enough for Ready.

## Invariants

- Server never returns a render mesh.
- A response is sent to the Client that originated the request.
- Canonical generated output is Server-owned.
- One Client cannot cause another Client's request result to be delivered as its own response.

## State transitions

Valid request -> validate -> deduplicate protocol coordinates -> resolve/generate the distinct requested Chunks through the Collection/Provider -> wait until every internal worker result belonging to the request is terminal -> compose one terminal `ChunkResponse` using the original RequestID.

The Client does not split its protocol request. One protocol Request may contain up to the ST-001-08 limit of 1024 coordinates. Server-side worker batching is an implementation detail and does not create additional protocol RequestIDs or partial protocol Responses.

Malformed/invalid request -> reject according to final protocol -> no canonical state mutation.

## Failure behavior

OQ-038/DR-022 already fix partial success/rejection protocol semantics. Remaining failure behavior to resolve before Ready is Server-specific: Provider/Collection failure propagation, reply/send failure handling, and outstanding-request/disconnect lifecycle.

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
- Use the Erelia-local `spk::TaskGroup<TResult>` prototype for grouped WorkerPool work rather than consuming a worker to wait on other workers.
- A TaskGroup Answer becomes terminal only after all of its child Tasks are terminal; mixed child failures remain inspectable so the eventual protocol Response can encode per-coordinate `Success` / `Unavailable`.
- Keep protocol correlation at the original RequestID: internal worker batches do not own protocol RequestIDs.
- `PrototypeChunkProvider` currently groups drained generation jobs with TaskGroup while `TerrainNode` owns and updates the authoritative `Chunk::Collection`.
- Do not expose the concrete Provider from `Chunk::Collection` merely so TerrainNode can reach Provider-owned Task Answers. A direct per-protocol-request TaskGroup handle still requires an explicit Collection/Provider bridge decision.

## Exact test fixtures

Final Ready fixtures must include:

- one valid coordinate;
- valid multi-coordinate batch including negative coordinate;
- duplicate coordinate case;
- invalid/unavailable coordinate case;
- partial-success case if approved;
- malformed payload;
- two Clients issuing distinguishable requests;
- disconnect during an outstanding request;
- generator failure fixture if generator can fail.

## Acceptance tests

### Nominal

Valid single/multi requests return exact canonical Chunks.

### Boundaries

Approved batch-size boundaries.

### Invalid / rejected operations

Per OQ-038 final semantics.

### Failure atomicity

Rejected request does not mutate canonical terrain state; partial response behavior must match final contract exactly.

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

Generator failure and network send failure behavior must be explicit before Ready.

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
