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
- OQ-037 is resolved for generic Volume/native representation. OQ-038 still leaves material request/response state semantics unresolved.

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

Blocked until the following are explicit:

- Erelia message type identifiers/encoding;
- scalar byte order/platform policy from OQ-037;
- duplicate-coordinate semantics;
- request batch limits;
- partial-success/rejection format;
- invalid/unavailable-coordinate result semantics;
- malformed-payload failure signaling.

## Invariants

- Response Chunk is always explicitly associated with its Chunk coordinate.
- Protocol never carries a terrain render mesh.
- Client-provided coordinates are requests, not authoritative terrain state.
- Negative Chunk coordinates preserve exact integer values.

## State transitions

Codec is stateless. Request/response processing state belongs to Server/Client consumer tickets.

## Failure behavior

Blocked by OQ-037/OQ-038.

## Determinism / ordering

Request/response list ordering semantics must be fixed by OQ-038 before Ready; consumers must not infer an unstated order.

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
- [OQ-038](../../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — blocking.

## Completion evidence

Promote to Ready only after wire, duplicate, batch-limit, ordering, partial-response, rejection, and malformed-payload semantics are explicit.
