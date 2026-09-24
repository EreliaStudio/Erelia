# ARCH-002 — Client/Server authoritative protocol semantics

**Status:** Approved
**Applies to:** Core protocol contracts, Server, Client, all authoritative state-changing features
**Established by decisions:** ../DECISIONS/DR-001-PRODUCT-BOUNDARIES.md, ../DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md, ../DECISIONS/DR-004-COMMAND-AUTHORITY-SEMANTICS.md, ../DECISIONS/DR-008-AUTHORITATIVE-OPERATION-ORDERING.md

## Invariant

The Client sends intent across a real process/network boundary. The Server validates, orders, decides, and applies authoritative state transitions.

A Client request never becomes authoritative merely because it was computed locally or arrived first from the Client's perspective.

## Why it exists

Erelia is a persistent multiplayer game with a dedicated Server. Explicit semantic authority prevents trust, retry, concurrency, and state ownership from being hidden in transport-specific code.

## Allows

- typed request/command messages describing intent;
- Client-provided target/input parameters;
- Server acknowledgements/results;
- explicit rejection reasons;
- Server events/state deltas;
- authoritative snapshots;
- shared protocol structures in Core;
- Client prediction that later reconciles with Server state.

## Forbids

- Client submission of authoritative resulting state as truth;
- Client-side mutation of persistent/shared authoritative state;
- transport implementation silently defining gameplay semantics;
- “last Client packet wins” behavior for conflicting authoritative mutations;
- Core network helpers bypassing Server-owned validation/state transition.

## Boundary

This applies to every state-changing action crossing Client/Server authority, including future movement, inventory, crafting, economy, combat, progression, resources, and terrain requests when requests are stateful or access-controlled.

Read-only data retrieval may use simpler request/result semantics but still uses Server-owned validation and canonical data.

## Ownership and dependencies

- Core may define shared message/value types and serialization helpers.
- Server owns validation, ordering, authoritative result computation, mutation, and rejection.
- Client owns request creation, presentation, prediction where approved, and reconciliation.

## Failure / invalid-state rules

- invalid, unauthorized, stale, or conflicting requests must not partially mutate authoritative state;
- Server may reject a request when state changed before processing;
- duplicate/retry semantics must be explicit for commands that can cause persistent mutation;
- disconnect/transport failure must not be interpreted as successful authoritative mutation without confirmation.

## Enforcement

Every protocol-facing ticket must state:

- Client input/intent;
- Server validation;
- accepted result;
- rejection cases;
- resulting authoritative state;
- ordering semantics;
- retry/idempotency requirements when relevant.

## Tests

- valid request acceptance;
- invalid/unauthorized rejection;
- stale/conflicting request behavior;
- serialization round trips;
- cross-process integration;
- concurrent competing request cases where relevant.

## Affected backlog

All Server/Client Epics, beginning with EP-001 voxel terrain delivery.
