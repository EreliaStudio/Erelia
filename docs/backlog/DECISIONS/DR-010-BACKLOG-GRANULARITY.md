# DR-010 — Small Epics with direct implementation tickets and near-term planning

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Backlog taxonomy, Epic sizing, roadmap horizon

## Context

The backlog needs enough structure for implementation without spending effort detailing far-future work that will likely change.

## Question

How should Epics, Stories/Tickets, and long-range planning be structured?

## Decision

Use a compact hierarchy:

- **Epic** = one coherent family of related features/capabilities.
- **ST-XXX-YY implementation ticket** = the implementation unit directly under that Epic.
- Do not introduce a mandatory intermediate Story layer.

Each Epic folder contains its Epic document and a `tickets/` folder containing ticket documents named `ST-XXX-YY-<implementation-goal>.md`.

Epics should stay relatively small and practical. Around **50 tickets** is a review threshold: if an Epic approaches/exceeds that size, explicitly discuss whether to split it rather than allowing unlimited growth.

Planning is **near-term and just-in-time**. Do not create a detailed far-future roadmap merely for completeness. Create new Epics when they become useful and their direction is sufficiently understood.

## Consequences

- EP-XXX -> ST-XXX-YY is the active backlog hierarchy.
- Far-future GDD capabilities can remain represented by traceability/context without receiving premature Epic/ticket detail.
- Epic decomposition may evolve as implementation reveals better boundaries.
- Ticket detail is still gated by the Definition of Ready.

## Required tests

Not a production behavior decision. Enforcement is by backlog review:

- every implementation ticket belongs to exactly one Epic;
- Epic ticket indexes match files under `tickets/`;
- no mandatory Story layer is introduced without a later decision;
- an Epic approaching 50 tickets triggers explicit split review.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-032 and Q-034.

## Supersession

None.
