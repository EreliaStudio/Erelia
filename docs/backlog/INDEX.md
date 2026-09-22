# Backlog Index

This file is the navigation layer over the folder tree.

## Start here

| Need | Document |
| --- | --- |
| Fast project understanding | PROJECT-CONTEXT.md |
| Current code/planning state | CURRENT-STATUS.md |
| Gameplay source provenance | SOURCE-BASELINE.md |
| Open architecture questions | QUESTIONS.md |
| Decision rules/index | DECISIONS/README.md |
| Ready standard | DEFINITION-OF-READY.md |
| Done standard | DEFINITION-OF-DONE.md |
| Terminology | GLOSSARY.md |
| GDD ownership mapping | TRACEABILITY/GDD-TRACEABILITY.md |
| Planning templates | templates/README.md |

## Architecture documents

- [ARCH-001 — Product boundaries and authority model](ARCHITECTURE/ARCH-001-PRODUCT-BOUNDARIES.md)

Architecture documents should be added only after a durable cross-cutting invariant has been explicitly decided.

## Decision records

- [DR-001 — Long-term Core / Server / Client boundaries](DECISIONS/DR-001-PRODUCT-BOUNDARIES.md)
- [DR-002 — Core may depend on Sparkle Core](DECISIONS/DR-002-SPARKLE-CORE-DEPENDENCY.md)
- [DR-003 — Dedicated authoritative server from the first playable](DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md)

QUESTIONS.md contains the remaining question register. Create a DR only when a real choice has concrete alternatives that need durable resolution.

## Epics

None created yet.

This is intentional. The project is still in architecture discovery.

## Implementation tickets

None created yet.

A detailed ticket should not be created until its surrounding architecture is sufficiently understood and it can satisfy the Definition of Ready.

## Keyword navigation

GLOSSARY.md maps important terms to GDD sections and, later, to their owning Epic(s). TRACEABILITY/GDD-TRACEABILITY.md maps larger capabilities to future backlog owners without inventing IDs before decomposition is approved.
