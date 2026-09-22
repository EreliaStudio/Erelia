# OQ-008 — What may the Client assert versus request?

**Status:** Resolved
**Decision records:** [DR-004](../DECISIONS/DR-004-COMMAND-AUTHORITY-SEMANTICS.md)
**Affected areas:** Trust boundary, command validation

## Question

What may the Client assert versus request?

## Problem / context

A Client can legitimately supply desired targets/directions/choices, but must not be able to declare the resulting shared state as truth.

## Known constraints

- Server owns authoritative state.
- Client values can be inputs to a request but are not proof of the outcome.

## Possible solutions

1. Client submits final resulting state.
2. Client submits only semantic intent/input and Server computes the result.
3. Permit per-feature exceptions without an explicit contract.

## Chosen solution

Client sends intent/input values only. It may request a target, movement, recipe, item transfer, etc., but the Server derives and commits the resulting authoritative state.
