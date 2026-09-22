# OQ-015 — Which economy operations require atomic transactions?

**Status:** Open
**Decision records:** None.
**Affected areas:** Economy, persistence, transactions

## Question

Which economy operations require atomic transactions?

## Problem / context

Crafting, enchantment, marketplace purchases, bank transfers, building contributions and upkeep can touch multiple authoritative records. Partial success could duplicate or destroy value.

## Known constraints

- Server serializes conflicting authoritative mutations.
- Operations that promise all-or-nothing behavior need an explicit transaction boundary.

## Possible solutions

1. Use one atomic transaction for every state-changing economy command.
2. Define atomicity per owning aggregate and use compensating operations across aggregates.
3. Define explicit transaction boundaries per command based on the invariants it must preserve.

## Chosen solution

None yet. Atomic operation boundaries remain open.
