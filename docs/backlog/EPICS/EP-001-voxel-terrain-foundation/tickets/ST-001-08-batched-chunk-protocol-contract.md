# ST-001-08 — Batched Chunk request/response protocol contract

**Status:** Done
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
- `Chunk::Protocol::Request::Builder::build()` generates a non-zero request ID from a thread-safe atomic sequence and stores it in the finalized Sparkle Message header; request ID 0 remains the uncorrelated/default value;
- correlated `ChunkResponse` and `ChunkError` messages reuse the originating request ID;
- one request carries 1..1024 Chunk coordinates; its payload is exactly the contiguous coordinate entries with no serialized count, and the decoder derives cardinality from payload size;
- local protocol construction uses nested Builders; finalized `Request`, `Error`, and `Response` objects store no mirrored semantic containers and use their inherited `spk::Message` payload as the single persistent representation;
- `Chunk::Protocol::Request::Builder` preserves insertion order in a temporary `std::vector<Chunk::Coordinate>` and performs duplicate detection only as a Debug-build developer guard; Release builds do not spend runtime work deduplicating outgoing Request construction;
- duplicate coordinates remain protocol misuse on incoming raw Messages: defensive decoding exposes one distinct duplicate coordinate for each repeated value so the Server can report one `DuplicateCoordinate` diagnostic per coordinate; the Error payload is a contiguous sequence of fixed `[Code:uint8][Chunk::Coordinate]` entries with no serialized count;
- `ChunkError` diagnostics are sorted X/Y/Z and, when present, are sent before the normal response;
- `ChunkResponse` is always the terminal Chunk-protocol message for its request ID;
- responses group entries as Success, Rejected, then Unavailable, with X/Y/Z lexicographic ordering inside each group;
- `Chunk::Protocol::Response::State : std::uint8_t` is `Success = 0`, `Rejected = 1`, or `Unavailable = 2`;
- only Success entries carry the fixed 4096-Cell Chunk payload;
- the response payload starts with absolute `std::uint32_t` byte offsets `successOffset`, `rejectedOffset`, and `unavailableOffset`; no redundant response count is serialized because each group count is derived from its byte range and fixed entry size;
- strict Core decoding throws `spk::Exception` for malformed protocol payloads; later network consumers own Warning logging, dropping the message, and continuing processing;
- a Client may reuse request IDs only after it has stopped issuing new requests and all outstanding requests in the connection/session have received their terminal `ChunkResponse`; the operational recycle threshold belongs to ST-001-11.


### Public API direction

Request construction uses a Builder:

```cpp
Chunk::Protocol::Request::Builder builder;
builder.add(coordinate);

Chunk::Protocol::Request request = std::move(builder).build();
```

The Builder owns temporary coordinate storage and the finalized `Chunk::Protocol::Request` owns only its Message payload. `build()` assigns the generated non-zero Sparkle RequestID, resizes the final payload once, and writes the contiguous coordinate block with `spk::Message::edit()`. Request coordinate count/access and duplicate diagnostics are derived from Message storage rather than cached semantic containers.

Response construction uses:

```cpp
Chunk::Protocol::Response::Builder builder(requestID);

builder.addSuccess(coordinate, chunk);
builder.addRejected(coordinate);
builder.addUnavailable(coordinate);

Chunk::Protocol::Response response = std::move(builder).build();
```

Error construction uses:

```cpp
Chunk::Protocol::Error::Builder builder(requestID);

builder.add(
    Chunk::Protocol::Error::Code::DuplicateCoordinate,
    coordinate);

Chunk::Protocol::Error error = std::move(builder).build();
```

Response/Error Builders receive the originating non-zero `spk::Message::RequestID`. Builders own temporary semantic containers and canonicalization. `build()` computes the exact final Message payload size, calls `resize()` once, and writes with `edit()`; callers do not serialize offsets, states, codes, coordinates, or Chunk Cells manually.

Response construction accepts additions in any order; the Builder owns canonical grouping and X/Y/Z ordering in the encoded payload.

A validated Response exposes `successOffset()`, `rejectedOffset()`, and `unavailableOffset()`. Later consumers may use those protocol-owned boundaries together with Sparkle `readAt()` for parallel reads without mutating the Message cursor. Typed incoming Request/Response/Error objects must validate the complete underlying Message before exposing it as valid protocol data.

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

Final protocol values own their serialized `spk::Message` payload and do not retain a second semantic copy of that payload. Incoming raw Messages may be moved into typed protocol values to transfer the payload buffer without an extra semantic reconstruction. No Message-buffer lifetime leak.

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

The implementation test matrix must include:

- one-coordinate Request with a non-zero RequestID and payload containing exactly one `Chunk::Coordinate`;
- 1024-coordinate Request boundary and rejection of the derived 1025-coordinate payload;
- positive and negative coordinate components preserved exactly;
- empty and misaligned Request payload rejection;
- Debug-build duplicate Request Builder input being rejected as a developer error;
- raw/Release Request duplicate payload input where one distinct duplicate coordinate is exposed for each repeated value;
- Error payload with no count, fixed `[Code:uint8][Coordinate]` entries, deterministic X/Y/Z ordering, and non-zero correlation;
- Response summary exactly three `std::uint32_t` offsets with `successOffset == 12`;
- all-empty-group boundary combinations through equal offsets;
- Success-only, Rejected-only, Unavailable-only, and mixed-group Responses;
- Success entry containing exactly 4096 packed Cells in Y-fastest, then X, then Z order;
- Response additions supplied in non-canonical order but encoded in canonical state-group and X/Y/Z order;
- malformed/truncated coordinate, state, Chunk payload, Error entry, and summary cases;
- invalid offset ordering/ranges/alignment;
- unknown Response state and Error code;
- duplicate or unsorted Response/Error coordinates;
- unexpected trailing bytes;
- RequestID 0 rejection for correlated Request/Response/Error;
- `successOffset()`, `rejectedOffset()`, and `unavailableOffset()` matching the encoded summary;
- Sparkle `readAt()` use from the returned Response ranges without changing the Message read cursor.

## Acceptance tests

### Nominal

Round-trip exact approved request/response fixtures.

### Boundaries

Minimum/maximum approved batch sizes and negative coordinates.

### Invalid / rejected operations

Core decoding rejects with `spk::Exception` at least:

- wrong `spk::Message::Type` for the typed protocol object;
- RequestID 0 where a correlated Chunk request/response/error requires a non-zero ID;
- empty Request payload;
- Request payload size not divisible by `sizeof(Chunk::Coordinate)`;
- derived Request cardinality greater than 1024;
- truncated coordinate or Chunk Cell data;
- unknown `Response::State`;
- unknown `Error::Code`;
- Response summary offsets outside the payload, out of order, or inconsistent with the fixed summary start;
- Response group byte ranges not divisible by the fixed entry size for that group;
- an entry state byte inconsistent with the group in which it appears;
- duplicate response coordinates;
- response entries that violate the required X/Y/Z ordering inside a group;
- Error payload size not divisible by the fixed Error-entry size;
- duplicate Error coordinates or Error coordinates that violate required X/Y/Z ordering;
- unexpected trailing bytes or any payload shape that cannot be consumed exactly by the approved format.

### Failure atomicity

Decode failure must not expose a partially valid request/response object.

### Determinism

Same logical payload yields the same approved encoding/order.

### Lifecycle / ownership

Decoded values remain valid after Message lifetime where required.

### Serialization / persistence

Primary acceptance area.

### Retry / idempotency

Request retry semantics are not owned by codec. Request Builder duplicate detection is Debug-only developer validation; defensive Message inspection identifies duplicates independently of build configuration or peer behavior.

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
- [DR-022](../../../DECISIONS/DR-022-CHUNK-PROTOCOL-WIRE-CONTRACT.md) — exact Chunk Request/Response/Error wire and correlation contract.

## Completion evidence

Implementation is complete, project-owner review is recorded, and PR #16 is treated as merged into `master`.

The Core implementation adds:

- `Networking::MessageType` with the exact DR-022 numeric values;
- typed `Chunk::Protocol::Request`, `Chunk::Protocol::Response`, and `Chunk::Protocol::Error` messages derived from `spk::Message`, each declared in its own protocol header for focused ownership/readability;
- atomic non-zero RequestID generation;
- nested Request/Error/Response Builders that own temporary semantic containers and emit finalized Message-backed protocol values;
- one-shot `resize()` + `edit()` payload construction with no mirrored semantic containers retained by finalized protocol values;
- Debug-only Request Builder duplicate validation plus on-demand duplicate inspection from finalized/raw Request payloads;
- deterministic Error and Response canonicalization;
- the three-offset Response summary and fixed 4096-Cell Success payload;
- strict `spk::Exception` validation of malformed Request/Response/Error messages;
- dedicated Core TU coverage for boundaries, ordering, malformed input, correlation, deterministic bytes, lifetime, and cursor-independent `readAt()`;
- one-message-per-header organization: `chunk_protocol_request.hpp`, `chunk_protocol_error.hpp`, and `chunk_protocol_response.hpp`; the former aggregate `chunk_protocol.hpp` is removed.

CI run #356 (run ID `36138476545`) passed the complete required matrix on reviewed PR head `a4c0e29059cec422bee848dff2c465ad26c53493`: clang-format, Linux Core/Server Debug + Release, Windows Core/Server Debug + Release, and Windows Client Debug + Release.

The ticket is **Done**. ST-001-09 Server handling and ST-001-11 Client coordinator policy remain outside this implementation.
