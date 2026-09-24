# OQ-009 — What local prediction / reconciliation is required for third-person movement?

**Status:** Partially resolved
**Decision records:** [DR-005](../DECISIONS/DR-005-CLIENT-MOVEMENT-PREDICTION.md)
**Affected areas:** Exploration movement, responsiveness, reconciliation

## Question

What local prediction / reconciliation is required for third-person movement?

## Problem / context

Server-authoritative movement without Client prediction risks visible latency. Prediction improves responsiveness but requires a reconciliation contract when local and authoritative simulation diverge.

## Known constraints

- Local movement should feel smooth.
- Server remains authoritative.
- EP-001 intentionally focuses on voxel terrain and uses a temporary free-flight inspection controller, not production Hero movement.

## Possible solutions

1. Wait for Server-authoritative movement before presenting motion.
2. Predict locally and reconcile to Server state.
3. Predict only selected movement phases or use another hybrid scheme.

## Remaining ambiguity

Exact input sequencing, rewind/replay strategy, correction thresholds, remote-entity interpolation and reconciliation UX are deferred to the exploration-movement Epic.

## Chosen solution

Client prediction is required for the locally controlled Hero, with Server authority and reconciliation. Exact reconciliation mechanics are not chosen yet.
