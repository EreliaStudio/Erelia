# OQ-025 — How strict should data-driven content schemas be in the first playable?

**Status:** Open
**Decision records:** None.
**Affected areas:** Items, spells, recipes, statuses, content loading

## Question

How strict should data-driven content schemas be in the first playable?

## Problem / context

Many gameplay families can be data-authored, compiled into C++, or mixed. The boundary affects iteration, validation, mod/editor compatibility and test fixtures.

## Known constraints

- Invalid content behavior must eventually be explicit.
- Future editor compatibility should not silently dictate first-playable scope.

## Possible solutions

1. Keep most first-playable content compiled in code.
2. Use strict external schemas for most content from the start.
3. Use a mixed approach: stable content families data-driven, complex/immature rules compiled until their schema settles.

## Chosen solution

None yet. The first-playable content/schema boundary remains open.
