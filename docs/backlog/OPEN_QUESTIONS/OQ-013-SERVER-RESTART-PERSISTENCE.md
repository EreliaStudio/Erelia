# OQ-013 — What exact Server state must survive restart?

**Status:** Open
**Decision records:** None.
**Affected areas:** Persistence, restart recovery, authoritative state

## Question

What exact Server state must survive restart?

## Problem / context

The GDD establishes persistent progression/economy/world state, but does not define restart durability for every runtime category. This affects storage schemas, save cadence and recovery tests.

## Known constraints

- Durable progression/economy/world unlocks must not be lost.
- Transient runtime state may be recoverable or intentionally reset, but that behavior must be explicit.

## Possible solutions

1. Persist a near-complete authoritative snapshot including active runtime state.
2. Persist only durable state and rebuild/reset transient state after restart.
3. Define durability tier-by-tier per subsystem, with explicit restart semantics for each category.

## Chosen solution

None yet. Persistence categories and recovery semantics remain open.
