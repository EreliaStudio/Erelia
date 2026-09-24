# OQ-018 — Fixed-step or variable-step simulation?

**Status:** Partially resolved
**Decision records:** [DR-006](../DECISIONS/DR-006-THREE-TIME-DOMAINS.md)
**Affected areas:** World simulation, encounter simulation

## Question

Fixed-step or variable-step simulation?

## Problem / context

Simulation stepping affects determinism, timing, load behavior and testability. World simulation must advance continuously even when an Encounter pauses.

## Known constraints

- World Time advances in discrete ticks.
- An Encounter may pause independently while the World continues.

## Possible solutions

1. Use one fixed step for both World and Encounter simulation.
2. Use variable delta time.
3. Use separate fixed/discrete stepping policies per World and Encounter domain.

## Remaining ambiguity

Exact World tick rate and how Encounter Time maps onto Server simulation steps remain open.

## Chosen solution

World Time is tick-based/discrete. The exact tick frequency and Encounter stepping model are not yet chosen.
