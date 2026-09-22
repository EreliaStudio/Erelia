# OQ-016 — What are retry / idempotency requirements for persistent commands?

**Status:** Open
**Decision records:** None.
**Affected areas:** Networking, persistence, retries

## Question

What are retry / idempotency requirements for persistent commands?

## Problem / context

A Client can lose a response and retry a command that already committed. Without idempotency semantics, purchases/crafts/transfers could execute twice.

## Known constraints

- Transport failure does not prove a command failed.
- Persistent mutation must not be duplicated by an ambiguous retry.

## Possible solutions

1. Require request/idempotency IDs for every persistent mutation.
2. Forbid transparent retries and force full state resynchronization after uncertainty.
3. Apply explicit idempotency only to commands whose duplicate execution would be harmful.

## Chosen solution

None yet. Retry keys, retention duration and per-command retry behavior remain open.
