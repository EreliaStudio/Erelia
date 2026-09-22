# DR-014 — Batched Client-driven Chunk request protocol

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** EP-001 networking, Server Chunk service, Client Chunk streaming

## Context

EP-001 requires a first semantic protocol for retrieving terrain Chunks from a dedicated Server.

## Already-fixed constraints

- Client sends intent/requests; Server owns canonical results.
- Server and Client are separate processes.
- Server sends voxel data, never terrain meshes.
- No new third-party runtime library may be added: implementation is limited to Sparkle, the C++ standard library, and platform facilities already necessary to build/run the programs.

## Decision

Use a **batched Client-driven Chunk request** semantic.

A request message contains:

- a message kind identifying a Chunk request;
- a list of `spk::Vector3Int` Chunk coordinates.

The Server resolves/generates requested Chunks and returns a response containing a list conceptually equivalent to:

`{ spk::Vector3Int coordinate, Voxel::Volume volume }`

for Chunks successfully supplied.

The Client alone owns its view/loading-region policy. The Server does not dictate the Client render/view radius.

For the first milestone the Client view policy may be:

- hard-coded; or
- supplied through Client startup configuration (for example JSON).

The protocol should allow that policy to change without changing Server terrain semantics.

## Consequences

- one network message may request multiple Chunks;
- the protocol is coordinate-based, not player-object-based;
- Server does not need to know why the Client wants a Chunk in order to return canonical terrain data;
- Client-side loading-radius policy is not part of the Server contract;
- transport/framing uses Sparkle Version-0.1.3 networking; Erelia still owns the payload byte layout;
- duplicate-coordinate handling, partial-success/rejection semantics, cache/eviction policy, and exact payload encoding remain unresolved detailed contracts.

## Required tests

When exact protocol encoding is resolved:

- one-coordinate batch;
- multi-coordinate batch;
- negative Chunk coordinates;
- response coordinate/Volume association;
- duplicate request semantics;
- partial failure/rejection semantics;
- malformed message rejection;
- serialization round trip;
- separate-process integration.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-037 and Q-038. Transport selection was subsequently narrowed to Sparkle Version-0.1.3 networking by DR-016.

## Supersession

None.
