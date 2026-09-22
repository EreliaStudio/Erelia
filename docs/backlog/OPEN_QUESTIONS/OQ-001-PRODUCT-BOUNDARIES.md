# OQ-001 — Are Core / Server / Client deliberate long-term product boundaries?

**Status:** Resolved
**Decision records:** [DR-001](../DECISIONS/DR-001-PRODUCT-BOUNDARIES.md)
**Affected areas:** Core, Server, Client, dependency direction, test ownership

## Question

Are Core / Server / Client deliberate long-term product boundaries?

## Problem / context

The restarted repository already contains Core, Server, and Client targets, but a scaffold does not automatically mean those boundaries are intended to survive. If the split were temporary, ownership rules and dependencies built around it would create avoidable migration work.

## Known constraints

- The final product needs a dedicated authoritative Server process and a separate Client.
- Shared functionality should not be duplicated between Client and Server.
- Core must not become a hidden authority layer.

## Possible solutions

1. Keep Core / Server / Client as long-term product boundaries.
2. Treat the current split as temporary and redesign the product topology later.
3. Keep Server / Client but remove a first-class shared Core library.

## Chosen solution

Core / Server / Client are deliberate long-term product boundaries. Core is the shared reusable library, Server owns authoritative decisions/state transitions, and Client owns presentation/input plus approved speculative behavior.
