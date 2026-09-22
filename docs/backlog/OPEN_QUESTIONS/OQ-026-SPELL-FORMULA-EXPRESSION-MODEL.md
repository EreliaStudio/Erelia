# OQ-026 — What expression model powers constrained spell formulas?

**Status:** Open
**Decision records:** None.
**Affected areas:** Spells, formulas, validation, content

## Question

What expression model powers constrained spell formulas?

## Problem / context

Spells need configurable formulas without arbitrary scripts. The representation must be bounded, serializable/testable and safe for authoritative evaluation.

## Known constraints

- Arbitrary unrestricted scripting is outside the intended design.
- Invalid formula behavior must be deterministic.

## Possible solutions

1. Use a small parsed AST/expression language.
2. Use a declarative operation graph.
3. Expose only a catalog of approved formula primitives/compositions.

## Chosen solution

None yet. Formula representation and invalid-expression behavior remain open.
