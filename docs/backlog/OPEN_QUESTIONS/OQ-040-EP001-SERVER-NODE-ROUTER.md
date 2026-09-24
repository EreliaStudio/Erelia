# OQ-040 — Should EP-001 use NodeRouter from the first Server implementation?

**Status:** Resolved
**Decision records:** [DR-016](../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md), [DR-021](../DECISIONS/DR-021-REMOTE-SERVER-NODES-FROM-FIRST-IMPLEMENTATION.md)
**Affected areas:** EP-001 Server topology, future Server decomposition

## Question

Should EP-001 use NodeRouter from the first Server implementation?

## Problem / context

The final Server routes coherent game capability families to nodes. Starting with a bare `spk::Server` loop would later require ownership/message-dispatch migration.

## Known constraints

- Sparkle Version-0.1.3 provides `NodeRouter`, `LocalNode`, `RemoteNode`, and `RemoteNode::Endpoint`.
- Client-facing ingress remains one NodeRouter.
- The project owner now wants every logical Server node to exist as a separate process from its first implementation.

## Possible solutions

1. Start with bare `spk::Server` and migrate later.
2. Start with `spk::NodeRouter` and in-process LocalNodes.
3. Start router-first and connect separate node processes through `RemoteNode`.

## Chosen solution

Option 3.

`EreliaServer` owns the public `spk::NodeRouter`. The first terrain capability runs as a separate process using `spk::RemoteNode::Endpoint`, reached through a router-owned `spk::RemoteNode`.

DR-021 supersedes only DR-016's original in-process deployment detail. Router-first ingress remains unchanged.
