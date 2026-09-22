# OQ-030 — What image comparison policy is acceptable?

**Status:** Open
**Decision records:** None.
**Affected areas:** Golden images, rendering tests

## Question

What image comparison policy is acceptable?

## Problem / context

Exact pixel equality may be too brittle for real GPU rendering, while overly broad tolerance can hide actual geometry/material regressions.

## Known constraints

- Comparison must fail on meaningful visual regressions.
- Human approval remains the authority for changing references.

## Possible solutions

1. Require exact pixel equality.
2. Use bounded RGB/alpha per-pixel tolerances and a maximum differing-pixel rule.
3. Use a perceptual/structural metric, possibly with masks for known unstable regions.

## Chosen solution

None yet. Comparison metric and thresholds remain open.
