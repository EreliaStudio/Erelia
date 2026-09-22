# OQ-034 — How much long-term roadmap should be materialized initially?

**Status:** Resolved
**Decision records:** [DR-010](../DECISIONS/DR-010-BACKLOG-GRANULARITY.md)
**Affected areas:** Backlog workflow, roadmap horizon

## Question

How much long-term roadmap should be materialized initially?

## Problem / context

Detailed far-future planning is likely to diverge before implementation reaches it. The backlog should remain useful without becoming speculative.

## Known constraints

- The GDD still provides long-term capability context/traceability.
- Detailed tickets must not require implementers to invent missing behavior.

## Possible solutions

1. Detail the whole GDD immediately.
2. Create only the next implementation ticket.
3. Keep near-term Epics detailed and materialize later Epics just-in-time.

## Chosen solution

Plan near-term and just-in-time. Keep far-future capability context coarse until its implementation horizon approaches.
