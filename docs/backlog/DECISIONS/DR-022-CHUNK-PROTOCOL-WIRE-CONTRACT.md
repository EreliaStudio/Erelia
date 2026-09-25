# DR-022 — Batched Chunk protocol wire contract

**Status:** Resolved
**Date opened:** 2026-09-25
**Date resolved:** 2026-09-25
**Applies to:** Core Chunk protocol, Server terrain-node requests, Client Chunk streaming
**Supersedes:** DR-014 details where this record is more specific

## Context

DR-014 selected batched Client-driven Chunk requests, while OQ-038 retained the exact wire representation, duplicate handling, result states, ordering, correlation, and malformed-input behavior for later resolution.

Sparkle Version-0.1.3 now provides native `spk::Message::RequestID`, `requestID()`, `setRequestID()`, cursor-independent checked `readAt()`, protocol-version-2 framing with RequestID in the network header, and RequestID preservation through RemoteNode envelopes.

ST-001-08 therefore owns only the Erelia Chunk protocol carried inside Sparkle Messages.

## Decision

### Message types

Erelia owns:

```cpp
enum class Networking::MessageType : spk::Message::Type
{
    ChunkRequest = 1,
    ChunkResponse = 2,
    ChunkError = 3
};
```

Typed protocol messages derive from `spk::Message`, set their own Message type, and hide ordinary payload serialization behind Chunk-domain operations.

### Correlation

`Chunk::Protocol::Request` assigns a non-zero `spk::Message::RequestID` from a thread-safe atomic monotonically increasing sequence. Request ID 0 remains Sparkle's uncorrelated/default value.

`ChunkResponse` and `ChunkError` reuse the originating RequestID. RequestID is Sparkle header metadata and is not duplicated in any Erelia payload.

Request-ID reuse is connection/session scoped. A Client may reset its Chunk request-ID generator so the next generated ID is 1 only after it has stopped issuing new requests and every outstanding request in that session has received its terminal `ChunkResponse`. The numeric threshold that triggers draining/reuse is Client policy owned by ST-001-11.

### Request payload

A Request carries 1..1024 coordinates.

Its payload is exactly:

```text
[Chunk::Coordinate]
[Chunk::Coordinate]
...
```

There is no serialized count. Cardinality is derived from payload size divided by `sizeof(Chunk::Coordinate)`.

Construction uses:

```cpp
Chunk::Protocol::Request request;
const bool inserted = request.add(coordinate);
```

The Request keeps its accepted coordinates in a `std::set<Chunk::Coordinate>`. `add()` returns `true` and appends the coordinate to the payload only when the coordinate was not already present. A duplicate add returns `false` and leaves the payload unchanged.

### Duplicate coordinates

The typed construction API prevents duplicate coordinates from being serialized. Duplicate coordinates can still exist in an incoming raw Sparkle Message produced by an invalid or non-conforming peer, so Request decoding remains defensive.

For decoded input, only one instance participates in normal Chunk resolution. The Request exposes the distinct duplicated coordinates through its duplicate-coordinate set, and the Server emits one diagnostic in a correlated `ChunkError` for each of them.

```cpp
enum class Chunk::Protocol::Error::Code : std::uint8_t
{
    DuplicateCoordinate = 0
};
```

Error payload:

```text
[Code:uint8][Chunk::Coordinate]
[Code:uint8][Chunk::Coordinate]
...
```

There is no serialized count. Entries are sorted lexicographically by coordinate X, then Y, then Z.

Construction uses:

```cpp
Chunk::Protocol::Error error(requestID);
error.add(
    Chunk::Protocol::Error::Code::DuplicateCoordinate,
    coordinate);
```

When an Error exists for a request, it is sent before the normal Response.

### Response states

```cpp
enum class Chunk::Protocol::Response::State : std::uint8_t
{
    Success = 0,
    Rejected = 1,
    Unavailable = 2
};
```

`Success` supplies a canonical Chunk. `Rejected` means the coordinate/request was understood but rejected by Server policy/domain validation. `Unavailable` means it was accepted but the canonical Chunk cannot currently be supplied. Unavailable is a normal result state, not a protocol error.

### Response payload

Entries are grouped Success, Rejected, then Unavailable. Coordinates are sorted X/Y/Z inside every group.

The payload begins with three absolute `std::uint32_t` offsets from payload byte 0:

```text
[successOffset:uint32]
[rejectedOffset:uint32]
[unavailableOffset:uint32]

[Success entries...]
[Rejected entries...]
[Unavailable entries...]
```

There is no serialized response-entry count. The summary is 12 bytes and `successOffset == 12`.

Empty sections use equal boundaries:

```text
successOffset == rejectedOffset
rejectedOffset == unavailableOffset
unavailableOffset == message.size()
```

Entry encodings are field-by-field, never raw C++ structs:

```text
Success:
    [Chunk::Coordinate]
    [State:uint8 = Success]
    [4096 contiguous Voxel::Cell packed values]

Rejected:
    [Chunk::Coordinate]
    [State:uint8 = Rejected]

Unavailable:
    [Chunk::Coordinate]
    [State:uint8 = Unavailable]
```

Chunk dimensions/unit size are omitted because Chunk invariants fix them at 16x16x16 and 1.0f. Cell order remains Y-fastest, then X, then Z.

Construction uses:

```cpp
Chunk::Protocol::Response response(requestID);

response.addSuccess(coordinate, chunk);
response.addRejected(coordinate);
response.addUnavailable(coordinate);
```

The Response implementation owns canonical grouping and sorting regardless of add-call order.

The typed Response exposes validated `successOffset()`, `rejectedOffset()`, and `unavailableOffset()` accessors so later WorkerPool consumers can use Sparkle `readAt()` on disjoint ranges.

### Terminal response rule

`ChunkResponse` is always the terminal Chunk-protocol message for its RequestID. No later Chunk-protocol message with that RequestID may be emitted.

### Strict decoding

Typed incoming Request, Response, and Error wrappers validate their underlying `spk::Message`. Core parsing may throw `spk::Exception`; it does not log or swallow malformed input.

Malformed input includes at least:

- wrong Message type;
- RequestID 0 for correlated Chunk Request/Response/Error;
- empty Request payload;
- Request payload size not divisible by `sizeof(Chunk::Coordinate)`;
- derived Request cardinality above 1024;
- truncated coordinate or Chunk Cell data;
- unknown Response state;
- unknown Error code;
- Response offsets outside bounds, out of order, or inconsistent with `successOffset == 12`;
- Response group ranges not divisible by their fixed entry sizes;
- entry state inconsistent with its group;
- duplicate Response coordinates;
- unsorted Response coordinates inside a group;
- Error payload size not divisible by its fixed entry size;
- duplicate Error coordinates;
- unsorted Error coordinates;
- unexpected trailing bytes or any payload shape that cannot be consumed exactly.

Later Server/Client consumers catch protocol exceptions at the network boundary, log a Sparkle Warning, drop the malformed message, and continue processing.

## Consequences

- request, response, and error counts are derived rather than serialized;
- Response ranges can be divided directly for parallel work;
- async completion order does not change Response encoding;
- the typed Request builder prevents duplicate coordinates from being serialized, while defensive decoding keeps duplicate misuse observable without making it a normal result state;
- RequestID reuse needs no Server reset handshake;
- Client retry/cache/disconnect policy remains outside Core;
- Server and Client must consume these shared Core protocol types rather than reimplement payload serialization.

## Required tests

ST-001-08 must cover exact nominal and malformed fixtures for all three Message types, including 1/1024 Request boundaries, negative coordinates, duplicate `add()` refusal without payload mutation, defensive decoding of raw duplicate Request coordinates, every Response state/group combination, empty groups, deterministic sorting, fixed Chunk Cell order, invalid offsets/states/codes/sizes/order/duplicates/trailing bytes, non-zero correlation, and cursor-independent Response section access.
