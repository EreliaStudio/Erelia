# DR-008 — Server serializes conflicting authoritative mutations

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Server state mutation, concurrency, shared resources, economy

## Context

Several clients may issue requests against the same authoritative state at nearly the same time. Examples include gathering one resource node or purchasing one marketplace listing.

## Already-fixed constraints

- Server owns authoritative state.
- Client requests may be rejected.
- Client requests do not reserve or establish authoritative results merely by being sent first from the Client perspective.

## Question

How are conflicting operations against the same authoritative state resolved?

## Decision

The Server owns the serialization/order of conflicting authoritative mutations.

Once one accepted operation changes authoritative state, a later competing request is evaluated against the new state and may be rejected because its assumptions/preconditions are no longer valid.

The Server does not use “last Client packet wins” semantics and does not allow independent clients to commit mutually incompatible outcomes.

Exact concurrency primitives and aggregate partitioning are implementation details to choose per subsystem.

## Consequences

- Shared mutable resources require one authoritative ordering point.
- Rejection due to changed/stale state is a normal protocol outcome.
- Multi-state operations that require all-or-nothing behavior must define an atomic transaction boundary when designed.
- Network arrival timing alone must not create duplicate committed outcomes.

## Required tests

- two competing valid-looking requests cannot both consume one exclusive resource;
- the accepted operation leaves authoritative state consistent;
- the losing request receives a defined rejection/result;
- failure during an atomic multi-state operation cannot leave a partial committed result once such operations are introduced.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-020.

## Supersession

None.
