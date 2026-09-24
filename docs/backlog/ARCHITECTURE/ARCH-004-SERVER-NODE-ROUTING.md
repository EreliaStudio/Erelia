# ARCH-004 — Server node-routing topology

**Status:** Approved
**Applies to:** EreliaServer, Server subsystems, Client/Server message dispatch
**Established by decisions:** ../DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md, ../DECISIONS/DR-004-COMMAND-AUTHORITY-SEMANTICS.md, ../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md

## Invariant

The dedicated Erelia Server exposes one Client-facing Sparkle network endpoint through `spk::NodeRouter`.

Game-specific Server behavior is owned by logical nodes behind that router rather than accumulated directly in the executable/network ingress layer.

EP-001 begins with one in-process terrain/Chunk `spk::LocalNode`.

## Why it exists

The final product needs a dedicated authoritative Server that will grow into several coherent feature families. Sparkle already provides local and remote node routing, so adopting the routing boundary from the first implementation avoids migrating a monolithic message loop later.

## Allows

- one LocalNode during the first milestone;
- adding further LocalNodes as coherent Server ownership boundaries emerge;
- replacing selected LocalNodes with `spk::RemoteNode` / `RemoteNode::Endpoint` when process separation is justified;
- routing message families by `spk::Message::Type`;
- subsystem-specific secondary dispatch inside the owning node.

## Forbids

- implementing EP-001 first as a separate bare-`spk::Server` game-message loop;
- placing all future message handling directly in EreliaServer's entry point;
- treating a logical node as merely a class-per-message organizational device;
- forcing remote/distributed deployment before a subsystem needs it.

## Boundary

`NodeRouter` is network ingress and first-level message-family routing.

A logical node owns one coherent Server capability family. Exact future node families are decided just-in-time.

Sparkle's current router dispatches by message type. Routing by World, Region, encounter, account, or another instance key remains inside the owning node unless a future architecture decision extends the routing layer.

## EP-001 composition

```
spk::Client
    |
    v
EreliaServer / spk::NodeRouter
    |
    v
Terrain LocalNode
    |
    +-- batched Chunk request
    +-- Chunk response
```

The terrain node remains in the Server process for EP-001.

## Failure / invalid-state rules

- an unrecognized/unrouted message type must not silently mutate Server state;
- a node response must remain correlated with the originating Client;
- node failure must not transfer authority to the Client;
- future RemoteNode failure must be surfaced as Server-side capability/unavailability behavior, not bypass the node boundary.

## Enforcement

- Server bootstrap constructs/owns the NodeRouter;
- game message types are explicitly routed to nodes;
- Server tickets identify their owning node;
- future direct handling in the Server entry point requires an explicit architectural exception.

## Tests

- Client -> NodeRouter -> terrain LocalNode -> originating Client round trip;
- two Clients retain response correlation;
- unknown/unrouted message behavior;
- future local-to-remote node substitution tests when RemoteNode is first introduced.

## Affected backlog

EP-001 and all later Server-owned feature Epics.
