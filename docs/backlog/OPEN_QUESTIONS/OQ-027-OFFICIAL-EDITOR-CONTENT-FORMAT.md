# OQ-027 — Are official and future-editor content formats identical from the first implementation?

**Status:** Open
**Decision records:** None.
**Affected areas:** Content formats, future editor, publishing

## Question

Are official and future-editor content formats identical from the first implementation?

## Problem / context

A future external editor should ideally reuse official content formats, but forcing editor concerns into the first implementation may over-constrain schemas before gameplay settles.

## Known constraints

- The future editor is not first-version scope.
- Shared formats are preferred where practical, not mandated at any cost.

## Possible solutions

1. Use exactly the same runtime/source format for official and editor content.
2. Share core schemas but allow editor-only metadata/workspace files.
3. Use separate authoring and runtime formats with an export step.

## Chosen solution

None yet. The relationship between official runtime content and future editor source formats remains open.
