# Engineering Requirements Document (ERD)

The ERD records **only owner-approved, verifiable internal engineering guarantees**, separate from the SDD's product behavior (e.g. concurrency, ownership, memory/correctness or resource guarantees).

## Domain catalog

No ERD domains or prefixes are approved yet. Each prefix must be explicitly owner-approved before assigning stable IDs (e.g. `ERD-THR-001`).

An engineering guarantee may be identified during Story technical design; approve it **before developing associated tests**. Reuse existing meaningful requirements rather than inventing per-class labels. Retire, preserve and trace old IDs rather than reusing them.

## Requirement table convention

Each ERD family will contain owner-authored approved IDs, names and descriptions, plus automatically generated Story/Test reference links. Generator and CI enforcement are an early M1 deliverable, not something already operating.

See [traceability](../project/traceability.md).
