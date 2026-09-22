# OQ-032 — Do we use Story as a separate backlog level?

**Status:** Resolved
**Decision records:** [DR-010](../DECISIONS/DR-010-BACKLOG-GRANULARITY.md)
**Affected areas:** Backlog taxonomy, Epic sizing

## Question

Do we use Story as a separate backlog level?

## Problem / context

An unnecessary Story layer could add hierarchy without improving implementation clarity, while very large Epics still need a practical split threshold.

## Known constraints

- Implementation units use ST-XXX-YY identifiers.
- Epics should remain coherent and reasonably small.

## Possible solutions

1. Epic → Story → implementation ticket.
2. Epic → ST implementation ticket directly.
3. Use Stories only selectively for oversized areas.

## Chosen solution

Use Epic → ST-XXX-YY implementation ticket directly. No mandatory Story layer. Around 50 tickets is a deliberate point to discuss splitting an Epic.
