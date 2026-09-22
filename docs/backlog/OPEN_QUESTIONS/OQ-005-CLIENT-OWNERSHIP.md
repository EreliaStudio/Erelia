# OQ-005 — What is the Client's ownership boundary?

**Status:** Resolved
**Decision records:** [DR-001](../DECISIONS/DR-001-PRODUCT-BOUNDARIES.md)
**Affected areas:** Client presentation, input, prediction

## Question

What is the Client's ownership boundary?

## Problem / context

A responsive realtime Client needs local behavior, but local behavior must not become authoritative. The boundary must distinguish presentation/prediction from trusted game state.

## Known constraints

- Server authority is absolute for shared state.
- The game needs smooth local responsiveness.
- Rendering/audio/UI are Client-only concerns.

## Possible solutions

1. Client owns only passive rendering and waits for every Server result.
2. Client owns input, rendering, audio, UI/presentation and may run explicitly non-authoritative prediction/speculation.
3. Client may authoritatively update selected gameplay state.

## Chosen solution

Client owns input, rendering, UI, audio and presentation, and may run approved speculative/predictive copies of logic. Server state always wins and Client prediction is never authoritative.
