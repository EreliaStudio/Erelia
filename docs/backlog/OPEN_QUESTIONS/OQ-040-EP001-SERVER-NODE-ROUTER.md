# OQ-040 — Should EP-001 use NodeRouter from the first Server implementation?

**Status:** Resolved
**Decision records:** [DR-016](../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)
**Affected areas:** EP-001 Server topology, future Server decomposition

## Question

Should EP-001 use NodeRouter from the first Server implementation?

## Problem / context

The final Server is expected to route coherent game capability families to nodes. Starting with a bare `spk::Server` loop would be simple but would later require ownership/message-dispatch migration.

## Known constraints

- Sparkle Version-0.1.3 already provides `NodeRouter`, `LocalNode` and `RemoteNode`.
- The first milestone should remain simple and does not need distributed services.

## Possible solutions

1. Start with bare `spk::Server` and migrate later.
2. Start with `spk::NodeRouter` and one in-process terrain `LocalNode`.
3. Start distributed immediately using `RemoteNode`.

## Chosen solution

EP-001 starts router-first: `EreliaServer` owns a `spk::NodeRouter`, and one in-process terrain `LocalNode` handles the Chunk message family. More nodes are added only when meaningful ownership boundaries emerge; remoting is deferred until justified.
