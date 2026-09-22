# ARCH-003 — Time, determinism, and authoritative ordering

**Status:** Approved
**Applies to:** Server simulation, Core deterministic algorithms, persistence, concurrent authoritative state
**Established by decisions:** ../DECISIONS/DR-006-THREE-TIME-DOMAINS.md, ../DECISIONS/DR-007-SEMANTIC-DETERMINISM.md, ../DECISIONS/DR-008-AUTHORITATIVE-OPERATION-ORDERING.md

## Invariant

Erelia distinguishes Encounter Time, World Time, and Real Time; requires semantic determinism where reproducibility matters; and gives Server-owned state one authoritative mutation order.

## Why it exists

Selective encounter pausing, continuously advancing world simulation, real-world scheduling, procedural generation, and concurrent multiplayer requests cannot share ambiguous timing or ordering rules.

## Allows

- Encounter clocks that pause independently;
- World simulation advanced in discrete ticks;
- wall-clock timestamps/durations for persistent real-time scheduling;
- deterministic seeded algorithms with explicitly defined semantic outputs;
- platform-specific floating-point implementation details when observable authoritative outcomes remain valid;
- subsystem-specific concurrency primitives behind one authoritative ordering contract.

## Forbids

- treating Encounter, World, and wall time as interchangeable raw numbers;
- pausing World Time because one Encounter awaits input;
- relying on cross-platform floating-point bit identity unless a subsystem explicitly requires it;
- multiple clients independently committing mutually exclusive authoritative outcomes.

## Boundary

Exact tick frequency, numeric time representations, catch-up policy, transaction mechanism, and aggregate partitioning remain subsystem-level decisions until their owning Epic requires them.

## Ownership and dependencies

- Server owns World/Encounter clock progression and authoritative operation ordering.
- Core may provide explicit time-domain types and deterministic algorithms.
- persistence may store Real Time where real-world synchronization is required.

## Failure / invalid-state rules

- stale/conflicting operations may be rejected after another accepted operation changes state;
- deterministic generators must reject/handle invalid inputs according to their own public contract rather than silently using undefined behavior;
- cross-domain time conversion must be explicit.

## Enforcement

- no generic untyped “time” value in public cross-domain contracts;
- deterministic generators require fixed seed/input fixtures;
- authoritative concurrency tests must exercise conflicting operations.

## Tests

- World Time continues while an Encounter is paused;
- deterministic-generation fixtures repeat exactly at the semantic contract level;
- competing authoritative mutations cannot both commit incompatible results.

## Affected backlog

Voxel generation, simulation, encounters, resources, economy, persistence, and any future scheduled systems.
