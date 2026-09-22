# DR-016 — Use Sparkle networking and plan Server as a node router

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Server networking, Client networking, EP-001 Chunk protocol, future Server decomposition

## Context

EP-001 requires a real dedicated Server/Client network boundary without adding third-party runtime libraries beyond Sparkle.

Sparkle Version-0.1.3 already provides a networking stack in `sparkle::core`, including:

- `spk::Client`;
- `spk::Server`;
- `spk::Message`;
- `spk::NodeRouter`;
- `spk::LocalNode`;
- `spk::RemoteNode` and `spk::RemoteNode::Endpoint`.

The implementation uses framed TCP transport internally. Sparkle's frame format carries a 32-bit message type and payload size and currently limits one payload to 32 MiB.

Sparkle integration tests demonstrate routing Client messages by `Message::Type` into `LocalNode`, and forwarding the same routed work through `RemoteNode` while preserving the originating Client connection.

## Decision

### Networking library

Erelia uses **Sparkle Version-0.1.3 networking** for the first implementation.

No additional networking library is introduced.

Erelia protocol messages are represented with `spk::Message`, while Erelia owns the semantic meaning and serialization layout of each message payload.

### Long-term Server shape

The dedicated Server is planned as a **network ingress/router to logical game nodes**.

Logical nodes own coherent subsections/families of Server behavior.

Examples of future node families may include terrain/world data, inventory/economy, encounters, or other ownership boundaries, but exact future nodes are not decided by this record.

Routing by `Message::Type` through Sparkle's `NodeRouter` is the initial mechanism.

### Initial EP-001 shape

EP-001 should begin with:

- one `spk::NodeRouter` as the public Client-facing Server endpoint;
- one in-process `spk::LocalNode` responsible for the EP-001 terrain/Chunk request family;
- Chunk request message types routed to that node;
- the node returning responses through the router to the originating Client.

This deliberately keeps the first implementation simple while avoiding a later architectural rewrite from a monolithic `spk::Server` loop into a routed Server.

A future node may be replaced by `spk::RemoteNode` / `RemoteNode::Endpoint` when process separation becomes justified without changing the Client-facing Server address or the Erelia message semantics.

## Consequences

- Erelia does not implement raw WinSock/BSD socket wrappers.
- Sparkle's public networking API is an approved Core/Server/Client dependency.
- The Server entry point should not directly accumulate all game-specific message handling.
- EP-001's first node may be called/structured as a terrain or world-terrain service, but exact source-level class naming remains ticket-level design.
- Client code connects through `spk::Client`.
- Erelia message payloads must be deliberately serialized; trivially-copyable Sparkle `Message` support does not by itself define a cross-platform wire contract.
- Sparkle's current TCP transport is acceptable for EP-001. Future movement/prediction requirements may justify extending Sparkle networking later, but EP-001 does not invent a second transport.
- Built-in `NodeRouter` routes by message type. If a future subsystem needs routing by World, Region, instance, account, or another key within the same message family, that additional dispatch belongs inside the owning logical node or requires a later routing decision.

## Required tests

EP-001 must include:

- real `spk::Client` -> `spk::NodeRouter` connection;
- Chunk request message routed to the terrain `LocalNode`;
- response returned to the originating Client;
- two-Client correlation test;
- malformed/unknown message type behavior;
- disconnect behavior while Chunk requests are outstanding;
- Erelia payload encode/decode tests independent from Sparkle's own networking tests.

Future node-remoting work must prove that replacing a local node with `RemoteNode` preserves the externally observable Erelia protocol.

## Resolution provenance

Resolved from the project owner's explicit requirement to use only Sparkle/standard facilities, the provided Sparkle Version-0.1.3 network API, and the project owner's stated direction that the Server should ultimately act as a router to nodes responsible for subsections of the game.

The initial router + one LocalNode approach is selected because it preserves that long-term boundary without requiring distributed services in EP-001.

## Supersession

None.
