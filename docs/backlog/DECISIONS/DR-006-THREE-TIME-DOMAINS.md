# DR-006 — Separate Encounter, World, and real-time domains

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Server simulation, encounters, world simulation, persistence, scheduling

## Context

Erelia contains gameplay time that can pause selectively, continuously running world simulation, and synchronization against real-world wall time. Treating these as one clock would make pause, scheduling, persistence, and replay semantics ambiguous.

## Already-fixed constraints

- Encounter time can pause while a player makes a tactical decision.
- World activity outside that Encounter continues.
- Persistent systems require synchronization with real-world time.

## Question

How many distinct authoritative time domains exist at the architectural level?

## Decision

Erelia has **three distinct time domains**:

1. **Encounter Time** — owned by an Encounter. It may pause for that Encounter while player input is required.
2. **World Time** — authoritative simulation time for the active world/region. It does not pause for tactical decision-making and advances in discrete simulation ticks.
3. **Real Time** — wall-clock/persistent time used where Server state must relate to real-world elapsed time or timestamps.

These domains must use explicit types/conversions rather than interchangeable raw numeric values.

Exact tick frequencies, integer units, timestamp representation, catch-up rules, and the precise relationship between Encounter advancement and Server simulation ticks remain implementation decisions to resolve when their owning systems are designed.

## Consequences

- Encounter pause never implies World Time pause.
- Real Time is not substituted for deterministic World simulation progress.
- Persistent schedules such as rotation epochs may use Real Time while runtime simulation uses World/Encounter Time.
- APIs and serialized contracts must identify which time domain a duration/timestamp belongs to.

## Required tests

- Encounter pause does not stop World Time.
- Encounter Time does not advance while that Encounter is paused.
- World Time advances only through its defined tick progression.
- Cross-domain conversions are explicit and tested where introduced.
- Persistence tests distinguish wall timestamps from simulation durations.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-017 and Q-018.

## Supersession

None.
