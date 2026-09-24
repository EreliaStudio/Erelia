# ARCH-004 — Server node-routing topology

**Status:** Approved
**Applies to:** EreliaServer, Server subsystems, Client/Server message dispatch
**Established by decisions:** DR-003, DR-004, DR-016, DR-021

## Invariant

The dedicated Erelia Server exposes one Client-facing Sparkle network endpoint through `spk::NodeRouter`.

Game-specific Server behavior is owned by logical nodes behind that router rather than accumulated in the executable/network ingress layer.

Every logical Server node is a separate process from its first implementation. EP-001 begins with one terrain/Chunk process reached through `spk::RemoteNode` and hosted by `spk::RemoteNode::Endpoint`.

## Why it exists

The Server will grow into coherent capability families. Sparkle already provides routed remote nodes, so using that boundary immediately avoids a later LocalNode-to-process migration.

## Allows

- adding remote node processes as coherent ownership boundaries emerge;
- external JSON configuration of router/node endpoints;
- routing message families by `spk::Message::Type`;
- subsystem-specific dispatch inside the owning node;
- moving node processes independently while preserving the Client-facing router endpoint.

## Forbids

- a separate bare-`spk::Server` game-message loop;
- game-specific handlers accumulating in `main.cpp`;
- Erelia-specific socket/proxy transport in place of Sparkle RemoteNode;
- Client bypass of the authoritative router/node boundary.

## EP-001 composition

```text
spk::Client
    |
    v
EreliaServer / spk::NodeRouter
    |
    v
spk::RemoteNode
    |
    | Sparkle remote envelope
    v
EreliaTerrainNode / spk::RemoteNode::Endpoint
```

The router may start while a configured node is unavailable. It logs a Warning and retries that endpoint after the externally configured reconnect delay.

## Failure / invalid-state rules

- an unrecognized/unrouted message type must not silently mutate Server state;
- a node response must remain correlated with the originating Client;
- node failure never transfers authority to the Client;
- unavailable nodes do not prevent the main router from listening;
- routing objects must not outlive their registered node objects.

## Enforcement

- Server bootstrap owns the NodeRouter and RemoteNodes;
- each node executable owns its Endpoint;
- game message types are explicitly routed only after their protocol ticket defines them;
- Server node directories are automatically discovered by CMake/tooling.

## Tests

- router -> RemoteNode -> terrain Endpoint connectivity;
- router startup with an unavailable node, Warning emission and later reconnection;
- two-Client correlation once the Chunk protocol exists;
- unknown/unrouted message behavior.
