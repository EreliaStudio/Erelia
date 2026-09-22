# OQ-029 — What is the golden-image platform policy?

**Status:** Open
**Decision records:** None.
**Affected areas:** Visual testing, CI, rendering

## Question

What is the golden-image platform policy?

## Problem / context

GPU/render output can vary by platform/driver. Golden-image approval needs a reproducible reference environment or an explicitly multi-platform policy.

## Known constraints

- Golden references require explicit human approval and are never auto-overwritten.
- EP-001 needs visual validation.

## Possible solutions

1. Use one canonical Windows/OpenGL reference runner.
2. Use a software/deterministic renderer environment as the canonical baseline.
3. Maintain separate approved baselines per supported rendering environment.

## Chosen solution

None yet. The canonical golden-image platform policy remains open.
