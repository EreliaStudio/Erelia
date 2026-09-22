# OQ-017 — What time representation is authoritative?

**Status:** Partially resolved
**Decision records:** [DR-006](../DECISIONS/DR-006-THREE-TIME-DOMAINS.md)
**Affected areas:** Time types, simulation, persistence

## Question

What time representation is authoritative?

## Problem / context

Erelia has tactical time that may pause, world simulation time that continues in ticks, and wall-clock time used for real-world synchronization. Raw interchangeable numbers would make pause/persistence semantics unsafe.

## Known constraints

- Encounter Time, World Time and Real Time are distinct domains.
- Cross-domain conversions must be explicit.

## Possible solutions

1. Use strong chrono/custom types for each domain.
2. Represent simulation as integer ticks and Real Time as persisted wall-clock timestamps.
3. Use a hybrid strong-type model with explicit tick/duration/timestamp conversions.

## Remaining ambiguity

Exact units, integer widths, timestamp representation and conversion APIs are still open.

## Chosen solution

Three explicit time domains are chosen: Encounter Time, discrete World Time and Real Time. Their exact storage/unit types remain unresolved.
