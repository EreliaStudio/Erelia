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

Sparkle Version-0.1.3 now also provides the asynchronous composition primitives required by this ticket: `Task<TResult>::Answer::subscribeToCompletion(...)`, thread-safe `ContractProvider`, and `TaskGroup<TResult>`. TaskGroup composes already-running Task Answers without consuming another worker merely to wait.

This ticket remains Blocked only by the remaining Erelia-specific Collection/Provider batch-result contract and Server lifecycle/failure details that still need to be made explicit enough for Ready.

## Invariants

- Server never returns a render mesh.
- A response is sent to the Client that originated the request.
- Canonical generated output is Server-owned.
- One Client cannot cause another Client's request result to be delivered as its own response.

## State transitions

Valid request -> validate -> deduplicate protocol coordinates -> TerrainNode partitions the distinct coordinates into smaller internal batches -> ask `Chunk::Collection` once per internal batch -> add each returned Task Answer to one `spk::TaskGroup` owned by the protocol-request context -> seal the group -> subscribe to grouped completion -> when every child Answer is terminal, compose one terminal `ChunkResponse` using the original RequestID.

The Client does not split its protocol request. One protocol Request may contain up to the ST-001-08 limit of 1024 coordinates. Internal Server batches are worker-scheduling units only: they do not create protocol RequestIDs, partial protocol Responses, or additional Client-visible request lifecycles.

The TaskGroup completion callback is allowed to run on the worker thread that settles the final child Task, or immediately on the subscribing thread if the group is already terminal. The handler implementation must therefore make the captured request/reply state safe for that callback lifetime and must not assume completion callbacks are executed by the TerrainNode dispatch thread.

Malformed typed request -> catch the protocol decoding `spk::Exception` at the terrain consumer boundary -> log a Sparkle Warning -> drop the malformed message without a protocol reply -> no canonical state mutation -> continue serving later messages.

## Failure behavior

OQ-038/DR-022 fix the available protocol result states. ST-001-09 currently has no Server policy/domain rule that produces `Rejected`; generated canonical Chunks map to `Success` and accepted coordinates whose generation/acquisition fails map to `Unavailable`. TaskGroup failure means at least one child Task failed after every child settled; the child Answers remain individually inspectable so Server code can still map each coordinate/batch outcome into the one terminal protocol Response.

Remaining failure behavior to resolve before Ready is Server-specific: the exact Collection batch-result representation and cache transition on child failure, reply/send failure handling, and outstanding-request/disconnect lifecycle.

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
- Use Sparkle Version-0.1.3 `spk::TaskGroup<TResult>`; remove the temporary Erelia-local TaskGroup prototype and its duplicate Core tests once Erelia consumes the merged Sparkle version.
- TaskGroup groups already-running `Task<TResult>::Answer` values. It does not own WorkerPool submission and does not consume a worker merely to wait.
- A TaskGroup Answer becomes terminal only after all child Tasks are terminal; mixed child failures remain inspectable through the child Answers.
- TerrainNode owns the split of one protocol request into smaller internal coordinate batches and owns the protocol-request TaskGroup.
- Keep protocol correlation at the original RequestID: internal work batches never own protocol RequestIDs.
- `Chunk::Collection` must expose a batched acquisition API so TerrainNode can ask for a set of coordinates and receive an asynchronous Answer representing that acquisition.
- `Chunk::Collection::Provider` is to be simplified into a WorkerPool-facing driver: it accepts a batch of coordinates, constructs/submits the corresponding Task through the shared WorkerPool, and returns that Task Answer. The Provider no longer owns an `update(Collection&)` polling phase.
- Collection owns authoritative cache/deduplication semantics. A missing Chunk is represented by pending asynchronous acquisition state rather than by inserting a placeholder/empty Chunk. Overlapping requests must reuse already-pending work instead of asking the Provider to generate the same missing coordinate again.
- The exact public batch Result/Answer shape used between Provider and Collection is still to be finalized before Ready; do not expose the concrete `PrototypeChunkProvider` merely to reach worker Answers.
- The grouped completion callback may execute from a WorkerPool thread. Any captured Endpoint/request state must have a safe lifetime and any network operation performed there must follow Sparkle's thread-safety contract.

## Exact test fixtures

Final Ready fixtures must include:

- one valid coordinate;
- valid multi-coordinate batch including negative coordinate;
- duplicate coordinate case;
- deterministic unavailable/generation-failure case;
- mixed terminal result case where one requested coordinate succeeds and another becomes unavailable;
- malformed payload;
- two Clients issuing distinguishable requests;
- disconnect during an outstanding request;
- generator failure fixture using a deterministic test Provider rather than depending on PrototypeChunkProvider output/failure;
- overlapping requests whose internal batches share at least one coordinate, proving pending work is reused rather than regenerated;
- a grouped request where one child batch completes before another, proving no Client-visible Response is emitted until the TaskGroup is terminal.

## Acceptance tests

### Nominal

Valid single/multi requests return exact canonical Chunks.

### Boundaries

Approved batch-size boundaries.

### Invalid / rejected operations

Per OQ-038 final semantics.

### Failure atomicity

Malformed requests do not mutate canonical terrain state. Mixed child success/failure is represented inside the single terminal `ChunkResponse`; ST-001-09 does not emit partial protocol Responses.

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
