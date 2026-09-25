# OQ-038 — What are the first Chunk request / streaming semantics?

**Status:** Partially resolved
**Decision records:** [DR-014](../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md), [DR-019](../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md), [DR-020](../DECISIONS/DR-020-HEADLESS-ASYNC-TASK-INFRASTRUCTURE.md)
**Affected areas:** EP-001, Client streaming, TerrainNode

## Question

What are the first Chunk request / streaming semantics?

## Problem / context

The Client needs to request nearby Chunks efficiently without making the Server responsible for Client view distance or rendering policy.

## Known constraints

- One request can batch multiple Chunk coordinates.
- Server returns coordinate + immutable `Chunk` results.
- Client alone owns its view/loading region policy.

## Possible solutions

1. Suppress duplicate outstanding requests and keep a Client cache with a configurable load/retain policy.
2. Allow duplicate requests initially and keep every received Chunk for the lifetime of the inspection session.
3. Use a minimal hard-coded radius first, then add configuration/eviction after the end-to-end path works.

## Remaining ambiguity

Core-local duplicate suppression is already resolved: `Chunk::Collection` owns explicit Absent/Pending/Available state and does not invoke its Provider again while a coordinate is Pending or Available. Each Pending request carries a monotonically increasing generation and stale asynchronous results are rejected.

The project owner has now fixed several ST-001-08 protocol details:

- Erelia owns a typed `Networking::MessageType` enum whose underlying type is `spk::Message::Type`;
- the first values are `ChunkRequest = 1` and `ChunkResponse = 2`;
- top-level Chunk protocol messages are domain types such as `Chunk::Protocol::Request` and `Chunk::Protocol::Response` built on `spk::Message`, set their own message type at construction, and expose Chunk-protocol operations so ordinary callers do not manually serialize raw `spk::Message` fields;
- one Chunk request contains at most 1024 coordinates; larger Client demand must be split across multiple messages;
- duplicate coordinates inside one request are protocol misuse rather than a normal Chunk result state: duplicate occurrences are ignored for Chunk resolution, while a separate correlated `ChunkError` message reports the misuse; `Networking::MessageType::ChunkError = 3`, `Chunk::Protocol::Error::Code : std::uint8_t` initially contains `DuplicateCoordinate = 0`, one diagnostic is emitted for each distinct duplicated coordinate, diagnostics are sorted lexicographically by coordinate (X, then Y, then Z), and the error message is sent after the normal response with the same Sparkle `RequestID`;
- a Chunk response entry contains its coordinate plus a typed `Chunk::Protocol::Response::State : std::uint8_t`;
- the response states are `Success = 0`, `Rejected = 1`, and `Unavailable = 2`;
- only `Success` is followed by the fixed 4096-Cell Chunk payload; `Rejected` and `Unavailable` carry no Chunk data;
- `Unavailable` is a normal result state, not a protocol error;
- malformed network/protocol input must not terminate the Server or Client process: Core `Chunk::Protocol::{Request, Response, Error}` decoding is strict and may throw `spk::Exception`, but it does not log or swallow failures; the later Server/Client network-consumer boundary catches the exception, logs a Sparkle Warning, drops the malformed message, and continues processing later traffic;
- Sparkle `spk::Message` gains a native `RequestID` field carried in the network frame header, retrievable with `requestID()` and assignable with `setRequestID()`;
- `spk::Message` does not generate correlation IDs itself: its default `RequestID` is 0, and protocols that require correlation own generation; `Chunk::Protocol::Request` owns a monotonically increasing non-zero request-ID sequence and assigns the generated value to its underlying Message;
- correlated Chunk response/error Messages reuse the originating request ID through `setRequestID()`;
- because correlation is a Sparkle Message header concern, the Erelia Chunk payload does not redundantly serialize a `RequestID`;
- Sparkle `spk::Message` gains checked, cursor-independent random-access reads with the approved API shape `readAt(std::size_t offset, void* destination, std::size_t size) const` plus a trivially-copyable typed `readAt<TValue>(std::size_t offset) const`;
- Chunk responses are grouped by result state in the fixed order `Success`, `Rejected`, then `Unavailable`; inside each state group, entries are sorted lexicographically by `Chunk::Coordinate` components in X, then Y, then Z order, making response encoding deterministic independently of request ordering or asynchronous completion order; the payload begins with three absolute `std::uint32_t` offsets from payload byte 0: `successOffset`, `rejectedOffset`, and `unavailableOffset`; there is no response-entry count because each group cardinality is derived from its byte range and fixed entry size; `successOffset` equals the summary size, empty groups are represented by equal adjacent boundaries, and an empty Unavailable group has `unavailableOffset == message.size()`;
- `count == 0` is malformed Chunk protocol input: the protocol decoder rejects it, and a network consumer catches/logs/drops it without sending a Chunk response;
- Client retry timing, cache retention/eviction, desired-region policy, and handling policy for already-completed/unknown responses remain deferred to ST-001-11.

The remaining ST-001-08 blockers are now narrower:

- the exact request-ID exhaustion/reset protocol, including whether it belongs to ST-001-08 or a later protocol ticket, the reset threshold, and the coordination rule that guarantees old in-flight responses cannot collide with reused IDs.

DR-019 also removes the former empty-Chunk placeholder idea from the generic Collection contract. Pending is represented as state, not as fake voxel content. Existing copied Available Chunk values remain valid through immutable shared Volume content.

## Chosen solution

Use batched Client-driven Chunk requests. The Client chooses its view region (hard-coded initially or startup-configured).

Use the Core `Chunk::Collection` / nested Provider abstraction established by DR-019 when ST-001-11 is implemented. A missing requested coordinate becomes Pending; no placeholder Chunk is published. The Client Provider may perform asynchronous network work and later publish the canonical complete Chunk only for the matching generation.

Use the approved Erelia typed message/state conventions above. Keep protocol misuse (such as duplicate coordinates) distinct from normal per-coordinate availability/rejection state.

Remaining network retry timing and cache/retention policy stay outside ST-001-08 and belong to ST-001-11. The remaining wire-level correlation/error/random-access details above must be resolved before OQ-038 can be marked Resolved and ST-001-08 promoted to Ready.
