# Open Questions

This directory is the canonical question register for the greenfield backlog.

Each question has its own `OQ-XXX-<TITLE>.md` file. A question remains here after resolution so its context, considered alternatives and chosen solution remain traceable. Resolved questions link to the Decision Record(s) that capture the durable choice.

Statuses:

- **Open** — no solution has been selected.
- **Partially resolved** — an architectural direction is chosen, but implementation-relevant details remain open.
- **Resolved** — the question has an explicit chosen solution and corresponding Decision Record(s).

| ID | Question | Status | Decision records |
| --- | --- | --- | --- |
| OQ-001 | [Are Core / Server / Client deliberate long-term product boundaries?](OQ-001-PRODUCT-BOUNDARIES.md) | Resolved | DR-001 |
| OQ-002 | [What is Core allowed to own?](OQ-002-CORE-OWNERSHIP.md) | Resolved | DR-001 |
| OQ-003 | [What may Core depend on?](OQ-003-CORE-DEPENDENCIES.md) | Resolved | DR-002 |
| OQ-004 | [What is the Server's ownership boundary?](OQ-004-SERVER-OWNERSHIP.md) | Resolved | DR-001 |
| OQ-005 | [What is the Client's ownership boundary?](OQ-005-CLIENT-OWNERSHIP.md) | Resolved | DR-001 |
| OQ-006 | [What is the first authoritative runtime topology?](OQ-006-FIRST-RUNTIME-TOPOLOGY.md) | Resolved | DR-003 |
| OQ-007 | [What are the first command / response / event semantics?](OQ-007-COMMAND-RESPONSE-EVENT-SEMANTICS.md) | Resolved | DR-004 |
| OQ-008 | [What may the Client assert versus request?](OQ-008-CLIENT-ASSERTIONS-VS-REQUESTS.md) | Resolved | DR-004 |
| OQ-009 | [What local prediction / reconciliation is required for third-person movement?](OQ-009-MOVEMENT-PREDICTION-RECONCILIATION.md) | Partially resolved | DR-005 |
| OQ-010 | [Which concepts need stable persistent identifiers?](OQ-010-PERSISTENT-IDENTIFIERS.md) | Open | — |
| OQ-011 | [Which concepts need runtime-only identifiers?](OQ-011-RUNTIME-IDENTIFIERS.md) | Open | — |
| OQ-012 | [What is the lifetime / invalidation rule for references?](OQ-012-REFERENCE-LIFETIME-INVALIDATION.md) | Open | — |
| OQ-013 | [What exact Server state must survive restart?](OQ-013-SERVER-RESTART-PERSISTENCE.md) | Open | — |
| OQ-014 | [Do active encounters survive a Server restart?](OQ-014-ENCOUNTER-RESTART-SEMANTICS.md) | Open | — |
| OQ-015 | [Which economy operations require atomic transactions?](OQ-015-ECONOMY-ATOMICITY.md) | Open | — |
| OQ-016 | [What are retry / idempotency requirements for persistent commands?](OQ-016-COMMAND-RETRY-IDEMPOTENCY.md) | Open | — |
| OQ-017 | [What time representation is authoritative?](OQ-017-AUTHORITATIVE-TIME-REPRESENTATION.md) | Partially resolved | DR-006 |
| OQ-018 | [Fixed-step or variable-step simulation?](OQ-018-SIMULATION-STEP-MODEL.md) | Partially resolved | DR-006 |
| OQ-019 | [What determinism is required across platforms?](OQ-019-CROSS-PLATFORM-DETERMINISM.md) | Resolved | DR-007 |
| OQ-020 | [How are concurrent authoritative operations ordered?](OQ-020-AUTHORITATIVE-CONCURRENCY-ORDERING.md) | Resolved | DR-008 |
| OQ-021 | [What coordinate conventions become architectural contracts?](OQ-021-COORDINATE-CONVENTIONS.md) | Resolved | DR-011 |
| OQ-022 | [What is the authoritative gameplay collision representation for articulated voxel entities?](OQ-022-ARTICULATED-ENTITY-COLLISION.md) | Open | — |
| OQ-023 | [What is the first asset authoring / import format?](OQ-023-ASSET-AUTHORING-IMPORT.md) | Open | — |
| OQ-024 | [What is runtime editability of voxel / model resources?](OQ-024-VOXEL-RESOURCE-RUNTIME-EDITABILITY.md) | Open | — |
| OQ-025 | [How strict should data-driven content schemas be in the first playable?](OQ-025-DATA-DRIVEN-CONTENT-SCHEMAS.md) | Open | — |
| OQ-026 | [What expression model powers constrained spell formulas?](OQ-026-SPELL-FORMULA-EXPRESSION-MODEL.md) | Open | — |
| OQ-027 | [Are official and future-editor content formats identical from the first implementation?](OQ-027-OFFICIAL-EDITOR-CONTENT-FORMAT.md) | Open | — |
| OQ-028 | [Is the GDD visual-validation prerequisite the actual first milestone?](OQ-028-FIRST-VISUAL-VALIDATION-MILESTONE.md) | Resolved | DR-009 |
| OQ-029 | [What is the golden-image platform policy?](OQ-029-GOLDEN-IMAGE-PLATFORM.md) | Open | — |
| OQ-030 | [What image comparison policy is acceptable?](OQ-030-IMAGE-COMPARISON-POLICY.md) | Open | — |
| OQ-031 | [What performance evidence should the first visual milestone capture?](OQ-031-FIRST-MILESTONE-PERFORMANCE-EVIDENCE.md) | Open | — |
| OQ-032 | [Do we use Story as a separate backlog level?](OQ-032-BACKLOG-STORY-HIERARCHY.md) | Resolved | DR-010 |
| OQ-033 | [What branch will receive the completed backlog?](OQ-033-BACKLOG-MERGE-BRANCH.md) | Open | — |
| OQ-034 | [How much long-term roadmap should be materialized initially?](OQ-034-ROADMAP-PLANNING-HORIZON.md) | Resolved | DR-010 |
| OQ-035 | [What is the first terrain voxel / cell representation?](OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) | Resolved | DR-012, DR-017 |
| OQ-036 | [Does the Client own terrain meshing, and what neighbor policy applies?](OQ-036-TERRAIN-MESHING-NEIGHBOR-POLICY.md) | Partially resolved | DR-013 |
| OQ-037 | [What networking transport and serialization / framing should EP-001 use?](OQ-037-EP001-NETWORK-SERIALIZATION.md) | Resolved | DR-016, DR-017 |
| OQ-038 | [What are the first Chunk request / streaming semantics?](OQ-038-CHUNK-REQUEST-STREAMING.md) | Partially resolved | DR-014 |
| OQ-039 | [What exact basic terrain generator should be the first deterministic fixture?](OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) | Resolved | DR-015 |
| OQ-040 | [Should EP-001 use NodeRouter from the first Server implementation?](OQ-040-EP001-SERVER-NODE-ROUTER.md) | Resolved | DR-016 |

## Maintenance rule

When a question is resolved, do not delete it. Update its status, link the resolving DR(s), preserve the considered alternatives, and finish the file with the chosen solution. If only part of the question is resolved, keep the status `Partially resolved` and state the remaining ambiguity explicitly.
