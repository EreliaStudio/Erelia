# OQ-007 — What are the first command / response / event semantics?

**Status:** Resolved
**Decision records:** [DR-004](../DECISIONS/DR-004-COMMAND-AUTHORITY-SEMANTICS.md)
**Affected areas:** Client/Server protocol semantics

## Question

What are the first command / response / event semantics?

## Problem / context

Transport alone does not define what a message means. The project needs a consistent semantic model for Client intent, Server validation, accepted results, rejected requests, state updates and snapshots.

## Known constraints

- Client is untrusted.
- Server computes authoritative outcomes.
- Different features may use acknowledgements, rejections, events/deltas or snapshots as appropriate.

## Possible solutions

1. Send resulting state from Client and let Server verify it.
2. Send typed intent/commands; Server validates and derives authoritative results.
3. Use ad-hoc message semantics independently per subsystem.

## Chosen solution

Client sends typed intent. Server validates, orders and computes the authoritative result, then communicates acceptance/result, rejection with a machine-usable reason, events/state deltas and snapshots as appropriate to the capability.
