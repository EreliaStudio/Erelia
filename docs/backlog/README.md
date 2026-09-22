# Erelia Greenfield Backlog

This folder is the planning workspace for the new Erelia implementation backlog.

## Purpose

The backlog is built from the current GDD and explicit design decisions made during planning. It is not a migration of the archived backlog, archived code, Playground implementation, or earlier architecture.

The backlog should reduce product and architecture decisions left to coding. A Ready implementation ticket must be precise enough that an implementation agent decides how to implement an approved contract, not what the contract should be.

## Source-of-truth order

1. The user's latest explicit design decision.
2. Resolved decision records in this backlog.
3. The current GDD snapshot recorded in SOURCE-BASELINE.md.
4. Durable architecture documents created from approved decisions.
5. Epics and implementation tickets derived from the above.

Current code is evidence of repository state, not automatic evidence of desired architecture. The archive is reference material only unless explicitly reintroduced.

## Recommended reading order

1. PROJECT-CONTEXT.md — dense fast-start note.
2. CURRENT-STATUS.md — what exists now, what just changed, and what comes next.
3. OPEN_QUESTIONS/README.md — architecture/design questions, their status, alternatives, and chosen resolutions.
4. DECISIONS/README.md — decision rules and index.
5. DEFINITION-OF-READY.md and DEFINITION-OF-DONE.md.
6. INDEX.md and GLOSSARY.md.
7. TRACEABILITY/GDD-TRACEABILITY.md.
8. Architecture, Epic, and Ticket documents as they are approved.

## Folder map

- ARCHITECTURE/ — durable cross-cutting architecture contracts.
- DECISIONS/ — explicit decision records for real alternatives.
- OPEN_QUESTIONS/ — one traceable OQ document per architecture/design question.
- EPICS/ — coherent capabilities and later implementation-ticket folders.
- TRACEABILITY/ — GDD capability ownership and later dependency views.
- templates/ — reusable planning templates.

## Planning progression

GDD → gameplay requirements → technical questions → approved decisions → system boundaries → implementation roadmap → epics → implementation tickets.

Do not skip directly from the GDD to detailed implementation tickets.

## Active state

No new greenfield Epic or implementation ticket has been approved yet. The current task is architecture discovery and decision capture.
