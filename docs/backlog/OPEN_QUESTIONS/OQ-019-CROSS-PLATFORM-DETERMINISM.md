# OQ-019 — What determinism is required across platforms?

**Status:** Resolved
**Decision records:** [DR-007](../DECISIONS/DR-007-SEMANTIC-DETERMINISM.md)
**Affected areas:** Generation, tests, Windows/Linux behavior

## Question

What determinism is required across platforms?

## Problem / context

Bit-identical floating-point runtime simulation across platforms can be expensive to guarantee, while seeded generation still needs reproducible outcomes.

## Known constraints

- Seeded generation must reproduce intended content.
- Server execution is authoritative runtime truth.
- Client reconciliation already handles runtime numerical divergence.

## Possible solutions

1. Require universal bit-for-bit determinism.
2. Require semantic determinism for meaningful generated/gameplay results.
3. Require determinism only for content generation and leave all runtime behavior unconstrained.

## Chosen solution

Require semantic determinism where reproducibility matters, especially seeded generation and controlled gameplay outcomes. General Client/Server floating-point simulation need not be bit-identical across platforms.
