# OQ-012 — What is the lifetime / invalidation rule for references?

**Status:** Open
**Decision records:** None.
**Affected areas:** Lifetime, stale references, error behavior

## Question

What is the lifetime / invalidation rule for references?

## Problem / context

Objects can disappear because an item breaks, an encounter ends, a temporary entity is removed or a dungeon resets. References retained by another subsystem or Client need deterministic stale behavior.

## Known constraints

- Stale references must not silently target a newly reused object.
- Failure behavior must be testable and safe for authoritative state.

## Possible solutions

1. Use stable IDs plus explicit not-found/tombstone semantics.
2. Use generation/versioned handles so reused slots invalidate old references.
3. Use owner-scoped IDs with lifetime bounded to the owner and reject references after owner reset.

## Chosen solution

None yet. The invalidation representation and observable stale-reference behavior remain open.
