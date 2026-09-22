# OQ-004 — What is the Server's ownership boundary?

**Status:** Resolved
**Decision records:** [DR-001](../DECISIONS/DR-001-PRODUCT-BOUNDARIES.md)
**Affected areas:** Server authority, state mutation, simulation

## Question

What is the Server's ownership boundary?

## Problem / context

Shared deterministic logic may live in Core, but the project needs one place where authoritative outcomes are committed. Without this boundary, Client/Core code could accidentally become trusted state owners.

## Known constraints

- The game is persistent multiplayer.
- Client requests are untrusted intent.
- Core may describe calculations/results but must not commit authoritative state.

## Possible solutions

1. Server owns only networking/persistence while Core owns authoritative gameplay decisions.
2. Server owns validation, authoritative simulation/state transitions, persistence and ordering while reusing Core helpers.
3. Allow Client-authoritative subsystems when convenient.

## Chosen solution

The Server exclusively owns authoritative validation, simulation outcomes, shared/persistent state mutation, transactions, ordering and trust decisions. Core supplies reusable tools; Client executes/presents Server authority.
