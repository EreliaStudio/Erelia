# OQ-038 — What are the first Chunk request / streaming semantics?

**Status:** Resolved
**Decision records:** [DR-014](../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md), [DR-019](../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md), [DR-020](../DECISIONS/DR-020-HEADLESS-ASYNC-TASK-INFRASTRUCTURE.md), [DR-022](../DECISIONS/DR-022-CHUNK-PROTOCOL-WIRE-CONTRACT.md)
**Affected areas:** EP-001, Client streaming, TerrainNode

## Question

What are the first Client terrain request, caching, correlation, streaming, and retention semantics?

## Resolution history

OQ-038 originally resolved the first ST-001-08 batched Chunk protocol direction. DR-022 records that historical wire contract, and ST-001-09 subsequently implemented its refined terminal Success/Failure response and diagnostic behavior.

ST-001-11 now resolves the remaining Client-side policy that had intentionally been deferred from those tickets and supersedes the older Chunk-only protocol details where its generic Collection protocol is more specific.

## Final ST-001-11 direction

### Client-driven terrain interest

The Client owns view/loading policy. The Server never chooses the Client view radius.

A Player terrain-streaming Behaviour observes the Player Transform, derives the containing Chunk coordinate from world position using mathematical floor + the existing Chunk floor-division contract, and refreshes terrain demand only when that containing Chunk coordinate changes.

The Client requests horizontal Column occupancy first and requests only the full Chunk coordinates returned by successful Column acquisition.

### Client ranges and retention

`viewRange` and `unloadRange` are positive Client configuration values with `unloadRange >= viewRange`.

The Client precomputes the horizontal X/Z relative Column offsets for both ranges and reuses them as the streaming center moves.

On a center-Chunk transition the Client:

- requests desired Columns inside `viewRange`;
- requests relevant Chunks from successful Column contents;
- removes Client Columns outside `unloadRange`;
- removes Client Chunks whose X/Z coordinates are outside the retained horizontal region.

Server-side Column/Chunk eviction remains deferred.

### Generic Collection state

Client and Server use the generic `Collection<TKey, TElement>` abstraction introduced by ST-001-11.

Collection storage contains only Available values. Provider-owned Pending state suppresses duplicate acquisition and reuses the same Task Answer.

The public state remains:

```text
Absent
Pending
Available
```

No placeholder Chunk/Column value represents Pending work.

### Response failure

A terminal Collection `Response::Failure` is remembered by the Client RequestingProvider for the lifetime of the current connection. Re-requesting that key during the same connection does not emit another network Request and resolves through the remembered failure.

Disconnect clears remembered Server refusals and fails/clears active network-backed Pending acquisitions. Available values remain cached.

### RequestID

RequestID generation is per Collection Request type / RequestingProvider.

- Chunk starts at 1.
- Column independently starts at 1.
- RequestID 0 remains uncorrelated.
- Each sequence increments monotonically as `std::uint64_t`.
- RequestIDs are never recycled.
- Existing provider sequences are not reset by disconnect/reconnect.
- No drain threshold or reset handshake is required.

### Message family and diagnostics

ST-001-11 supersedes the old fixed Chunk numeric MessageIDs.

Chunk and Column each use the generic Collection family:

```text
Request
Response
Update
Error
```

`Networking::Diagnostic` is a serializable payload value inside family Error messages and no longer owns a standalone MessageID.

Family Error messages are diagnostic-only on the Client for ST-001-11: they are decoded/logged and do not settle or mutate Pending/Available acquisition state.

The known case where a request-level Error is emitted without a terminal Response may therefore leave correlated Pending acquisition unresolved until a later lifecycle event such as disconnect/removal. This is accepted ST-001-11 technical debt and is intentionally deferred to future reliability work.

### Generic Response sectioning

The generic Collection Response owns one offset table at the beginning of its payload. The table covers independently parseable sections for both Success and Failure data and identifies their boundary.

Chunk and Column do not define separate Response table formats.

Each domain owns static compile-time tuning constants for:

- maximum elements per emitted Request;
- elements per serialized Response section.

Their numeric values are implementation tuning knobs rather than durable protocol identifiers.

### Sparkle Message model

Merged Sparkle Version-0.1.3 uses immutable `spk::Message` payloads.

- construction: `spk::Message::Writer`;
- decoding: independent `spk::Message::Reader` instances over shared pooled immutable storage.

ST-001-11 generic serialization is defined against Writer insertion and Reader extraction rather than mutable `spk::Message << / >>` operations.

## Chosen solution

Use Client-driven Column -> Chunk terrain acquisition through generic asynchronous Collections and network-backed RequestingProviders.

Suppress duplicate Pending work, retain Available canonical data, remember terminal Server refusals for one connection, clear network-only failure state on disconnect, and bound Client storage through configured horizontal view/unload ranges.

Use one generic Collection protocol family for Chunk and Column, with per-request-type monotonic non-recycled RequestIDs, generic Response offset-table sectioning, family Error diagnostics, and Sparkle Writer/Reader serialization.

## Ownership

ST-001-11 is the durable implementation specification for the remaining policy previously deferred from OQ-038.

DR-022 remains the historical record of the completed ST-001-08/ST-001-09 Chunk wire implementation and explicitly records which pieces ST-001-11 later supersedes.
