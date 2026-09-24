# OQ-033 — What branch will receive the completed backlog?

**Status:** Open
**Decision records:** None.
**Affected areas:** Repository workflow, merge target

## Question

What branch will receive the completed backlog?

## Problem / context

The repository currently uses `master` as its default branch while the user sometimes refers to `main`. The final merge target must be explicit before opening the planning PR.

## Known constraints

- No `main` branch existed when this planning work began.
- The active planning branch was created from `master`.

## Possible solutions

1. Target the current `master` branch.
2. Create/rename to `main` first, then target `main`.
3. Adopt another explicit integration branch.

## Chosen solution

None yet. The final merge target remains open.
