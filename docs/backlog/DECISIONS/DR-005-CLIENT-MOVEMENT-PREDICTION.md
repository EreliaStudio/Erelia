# DR-005 — Client movement prediction is required

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Exploration movement, Client responsiveness, Server reconciliation

## Context

Third-person movement must feel smooth while remaining Server authoritative.

## Already-fixed constraints

- Server owns authoritative movement state.
- Client may run speculative/predictive logic.
- The first implementation focus is voxel terrain construction/rendering rather than final character-control behavior.

## Question

Should locally controlled movement use Client prediction, or wait for authoritative Server movement before presenting motion?

## Options

### Option A — Server-response-only local movement

Do not move locally until Server state is received.

### Option B — Predict locally and reconcile

Allow the Client to present predicted local movement immediately, while Server independently determines authoritative movement and Client reconciles to Server state.

## Decision

Use **Option B**.

Client prediction is an intended part of the eventual exploration controller and should be integrated early enough that the movement architecture does not have to be redesigned around it later.

However, detailed prediction/reconciliation behavior is **not part of the first voxel-terrain foundation Epic**. That Epic may use a temporary free-flight inspection controller whose purpose is visual validation rather than production movement semantics.

## Consequences

- Future production exploration movement must explicitly define prediction and reconciliation.
- Server state always wins after divergence.
- Exact rewind/replay/correction thresholds and remote-entity interpolation remain deferred until the exploration movement Epic.
- The voxel terrain foundation may deliberately use non-gameplay free 3D inspection movement.

## Required tests

When production movement is implemented:

- predicted local motion is responsive without granting authority;
- divergent Client state converges to Server state;
- rejected movement cannot persist locally as authoritative state;
- reconciliation behavior is deterministic enough to test with exact scripted inputs.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-009.

## Supersession

None.
