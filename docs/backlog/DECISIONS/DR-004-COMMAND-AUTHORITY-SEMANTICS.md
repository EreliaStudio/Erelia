# DR-004 — Client intent and Server authority semantics

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Client/Server protocol, authoritative commands, shared protocol contracts

## Context

The project requires a dedicated authoritative Server and a separate Client. The semantic boundary must be explicit before transport-specific networking work is detailed.

## Already-fixed constraints

- Server owns authoritative shared/persistent gameplay decisions.
- Client may predict locally but is never authoritative.
- Core may hold shared protocol/data representations.

## Question

What may Client messages assert, and how does Server communicate authoritative outcomes?

## Options

### Option A — Client submits resulting state

Allow the Client to compute desired state changes and submit them for validation.

### Option B — Client submits typed intent

The Client sends typed requests/commands describing intent and required input parameters. Server validates the request, computes the authoritative result, mutates authoritative state if accepted, and communicates the result.

## Decision

Use **Option B**.

Client-to-Server state-changing messages describe **intent**, not authoritative resulting state.

Examples include selecting a target, requesting movement, requesting a craft, selecting a recipe, requesting an inventory transfer, or choosing a combat action.

Server is responsible for:

- validating authorization and preconditions;
- deriving the authoritative outcome;
- ordering the operation against competing authoritative operations;
- applying accepted state changes;
- refusing invalid/stale/conflicting requests;
- communicating authoritative results/state.

Server-to-Client semantics may use explicit acknowledgements/results, rejections with machine-usable reasons, events/state deltas, and snapshots as appropriate to the capability. Exact transport encoding is deliberately not selected by this decision.

## Consequences

- A Client-provided value is evidence of intent/input, not proof that the resulting state is valid.
- Shared command/result/rejection/event structures may live in Core.
- Server may reject requests whose assumptions became stale before processing.
- Protocol tickets must define accepted inputs, rejection reasons, resulting authoritative state, and retry/idempotency semantics where relevant.
- Transport choice must not weaken these semantics.

## Required tests

Per authoritative capability:

- valid intent produces the expected Server-owned result;
- invalid/unauthorized intent is rejected without mutation;
- stale/conflicting intent is rejected or otherwise resolved according to the capability contract;
- Client-computed resulting state cannot bypass Server validation;
- protocol serialization preserves the semantic input/result.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-007 and Q-008.

## Supersession

None.
