# ST-001-08 — Batched Chunk request/response protocol contract

**Status:** Blocked
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Define and encode the shared Erelia message payloads for batched Client Chunk requests and Server canonical Chunk responses/rejections.

## User / system value

Client and Server can exchange terrain requests/results through one explicit shared protocol rather than ad-hoc field serialization in each process.

## Starting state / prerequisites

- Depends on ST-001-01, ST-001-03, ST-001-05, and the DR-019 Chunk value contract introduced by ST-001-06.
- DR-014 fixes batched Client-driven semantics.
- DR-016 fixes Sparkle transport/message use.
- OQ-037 is resolved for generic Volume/native representation. OQ-038 now resolves the Chunk request/response/error wire semantics required by this ticket.

## Product ownership

Core owns shared protocol data/codec. Server owns authoritative processing; Client owns request selection.

## Allowed dependencies

EreliaCore, Sparkle Core networking, standard library.

## Forbidden dependencies

Server authority implementation, Client cache/rendering implementation, extra networking/serialization libraries.

## Owned behavior

Known approved shape:

- request message kind identifies a Chunk request;
- request contains a list of `spk::Vector3Int` Chunk coordinates;
- response associates each supplied Chunk coordinate with an immutable `Chunk`;
- one message may carry multiple Chunk coordinates;
- the dedicated Chunk payload does **not** serialize dimensions or unit size because Chunk invariants fix them at 16x16x16 / 1.0f;
- the Chunk payload serializes exactly 4096 packed Cells in the inherited Y-fastest, then X, then Z order;
- generic runtime-sized `Voxel::Volume` encoding remains DR-017/ST-001-05 and is not duplicated for Chunk.

## Explicitly not owned

Client loading radius, cache eviction, Server generation, rendering, transport socket implementation.

## Public contract

The protocol uses typed Erelia messages built on `spk::Message`:

- `Networking::MessageType : spk::Message::Type` identifies `ChunkRequest = 1`, `ChunkResponse = 2`, and `ChunkError = 3`;
- `Chunk::Protocol::Request` generates a non-zero request ID from a thread-safe atomic sequence and stores it in the Sparkle Message header; request ID 0 remains the uncorrelated/default value;
- correlated `ChunkResponse` and `ChunkError` messages reuse the originating request ID;
- one request carries 1..1024 Chunk coordinates;
- duplicate coordinates are protocol misuse: only the first occurrence participates in normal resolution, while one `DuplicateCoordinate` diagnostic is reported for each distinct duplicated coordinate;
- `ChunkError` diagnostics are sorted X/Y/Z and, when present, are sent before the normal response;
- `ChunkResponse` is always the terminal Chunk-protocol message for its request ID;
- responses group entries as Success, Rejected, then Unavailable, with X/Y/Z lexicographic ordering inside each group;
- `Chunk::Protocol::Response::State : std::uint8_t` is `Success = 0`, `Rejected = 1`, or `Unavailable = 2`;
- only Success entries carry the fixed 4096-Cell Chunk payload;
- the response payload starts with absolute `std::uint32_t` byte offsets `successOffset`, `rejectedOffset`, and `unavailableOffset`; no redundant response count is serialized because each group count is derived from its byte range and fixed entry size;
- strict Core decoding throws `spk::Exception` for malformed protocol payloads; later network consumers own Warning logging, dropping the message, and continuing processing;
- a Client may reuse request IDs only after it has stopped issuing new requests and all outstanding requests in the connection/session have received their terminal `ChunkResponse`; the operational recycle threshold belongs to ST-001-11.

## Invariants

- Response Chunk is always explicitly associated with its Chunk coordinate.
- Protocol never carries a terrain render mesh.
- Client-provided coordinates are requests, not authoritative terrain state.
- Negative Chunk coordinates preserve exact integer values.

## State transitions

Codec is stateless. Request/response processing state belongs to Server/Client consumer tickets.

## Failure behavior

Core protocol decoding is strict and throws `spk::Exception` for malformed input. This ticket does not log or swallow network failures. Later Server/Client consumers catch protocol-decoding failures at the network boundary, log a Sparkle Warning, drop the malformed message, and continue processing.

## Determinism / ordering

Normal response groups are encoded in the fixed order Success, Rejected, Unavailable. Entries inside every group are sorted lexicographically by Chunk coordinate X, then Y, then Z. Duplicate diagnostics use the same coordinate ordering. When duplicate diagnostics exist, `ChunkError` precedes the terminal `ChunkResponse`.

## Lifecycle / ownership

Decoded payloads own or safely reference their data according to Core value contracts; no Message-buffer lifetime leak.

## Serialization / persistence

This ticket owns Erelia payload encoding/decoding on `spk::Message`. No persistence.

## Networking / authority

Client requests coordinates; Server later validates and returns canonical results. Payload decoding alone grants no authority.

## Implementation constraints

- Use Sparkle Version-0.1.3 `spk::Message`.
- Reuse ST-001-05/DR-017 native Cell-layout assumptions where applicable, but implement the DR-019 dedicated fixed-size Chunk codec rather than serializing redundant Volume metadata.
- No additional runtime dependency.
- Do not couple Server view radius to the payload.

## Exact test fixtures

Final Ready ticket must include:

- one-coordinate request;
- multi-coordinate request with positive/negative coordinates;
- duplicate-coordinate request according to resolved semantics;
- one and multiple response entries;
- partial success/rejection fixture;
- malformed/truncated request and response;
- batch-size boundary values.

Exact expected bytes require OQ-037.

## Acceptance tests

### Nominal

Round-trip exact approved request/response fixtures.

### Boundaries

Minimum/maximum approved batch sizes and negative coordinates.

### Invalid / rejected operations

Malformed payload, invalid batch size, invalid/unavailable coordinate semantics per OQ-038.

### Failure atomicity

Decode failure must not expose a partially valid request/response object.

### Determinism

Same logical payload yields the same approved encoding/order.

### Lifecycle / ownership

Decoded values remain valid after Message lifetime where required.

### Serialization / persistence

Primary acceptance area.

### Retry / idempotency

Request retry semantics are not owned by codec, but duplicate-coordinate behavior inside one batch is.

### Concurrency / cancellation

Not applicable to pure codec.

### Authority / trust boundary

No payload permits Client-authored terrain Chunk data to become Server truth.

### Dependency failure

Malformed Message input only.

### Cross-system integration

Later Server and Client network tickets must share these exact codecs.

### Performance

Respect Sparkle's current 32 MiB payload ceiling; exact request limits remain OQ-038. No timing budget.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-014](../../../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md)
- [DR-016](../../../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)
- [DR-019](../../../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
- [OQ-037](../../../OPEN_QUESTIONS/OQ-037-EP001-NETWORK-SERIALIZATION.md) — resolved for the native representation used by the codec.
- [OQ-038](../../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — resolved for this protocol contract.

## Completion evidence

Promote to Ready only after wire, duplicate, batch-limit, ordering, partial-response, rejection, and malformed-payload semantics are explicit.
