# OQ-031 — What performance evidence should the first visual milestone capture?

**Status:** Open
**Decision records:** None.
**Affected areas:** Performance, voxel meshing, streaming

## Question

What performance evidence should the first visual milestone capture?

## Problem / context

The GDD requests performance measurements, but an arbitrary target would be invented. EP-001 still needs evidence sufficient to reveal obvious architectural problems.

## Known constraints

- No numeric budget has been approved.
- Evidence should be repeatable enough to compare later changes.

## Possible solutions

1. Capture structural counts/timings only, with no pass/fail budget yet.
2. Define hard frame/meshing/network budgets immediately.
3. Record trends across fixed fixtures and promote budgets only after baseline measurements exist.

## Chosen solution

None yet. Benchmark fixtures, metrics and whether they are informational or gating remain open.
