# GDD, SDD, ERD and traceability

## Three distinct purposes

- [GDD](../gdd/Erelia_GDD_Current.md): presentable, evolving game vision. It must remain readable, not a test database.
- [SDD](../sdd/README.md): **owner-approved observable product** capabilities and verifiable functional/nonfunctional requirements.
- [ERD](../erd/README.md): **owner-approved internal engineering guarantees**, such as ownership, memory, synchronization and correctness contracts.

Before major feature work, map the **whole GDD at high level** to SDD requirement families in the central index. Define and approve detailed requirements progressively. An unmapped or unimplemented family is not an error merely because its roadmap capability lies in the future.

## Requirement semantics and identifiers

Requirement statements must be coherent, individually verifiable and stable. Avoid one requirement per variable, method or test. SDD IDs are domain-specific, e.g. `SDD-NET-001`; ERD IDs similarly, e.g. `ERD-THR-001`. The owner approves domain catalogs/prefixes; the AI allocates next unused sequential IDs and never reuses IDs. Retire and preserve old IDs when needed. Keep the ID for clarified requirements with unchanged fundamental guarantee; new guarantees need new IDs. No separate `Implemented`/completion status column belongs in the requirement catalogs.

The SDD and ERD have central indexes and one document per domain/family. Only **approved** requirements appear in authoritative tables; proposed text stays outside. Each requirement links to generated Story/Test references, which do **not** mean the tests passed.

## Story metadata

Each Story records machine-readable visible metadata, e.g.:

```text
StoryID: ST-012
RequirementIDs: SDD-NET-001, ERD-THR-001
DependsOn: ST-011
```

Production code Stories require at least one approved SDD/ERD requirement before implementation; justified documentation-only/management exceptions are allowed. Dependencies encode only real implementation blockers.

## C++ test metadata

Each C++ test within v2 governed sources must have a **globally unique Test ID** and at least one approved requirement, using adjacent comments:

```cpp
//%TestID: TEST-000142
//%RequirementID: SDD-NET-001
//%RequirementID: ERD-THR-001
TEST(Network, ConcurrentRequests) { /* ... */ }
```

C++ tests are automatically discovered. Other scripts, external checks and manual/visual verification belong in Story acceptance evidence, not automatically in the generated C++ Test columns.

## M1 generator and CI contract

An M1 generator must update **only generated Story/Test link columns**, preserving owner-authored requirement IDs/names/descriptions. Generated links are repository-relative paths to Story files and C++ test sources. It must validate IDs, known prefixes, missing links, duplicate IDs, invalid requirements, dependency references and cycles; each CI pipeline verifies strict freshness, never auto-commits. AI regenerates changes before each PR delivery.

A requirement may legitimately have zero implementing Stories or tests. Traced test existence does not imply passing execution. Final evidence comes from CI and acceptance reports.

**Current state:** This document defines the intended methodology only. No generator, requirement catalog, scanner or CI verification has been implemented in the v2 baseline.
