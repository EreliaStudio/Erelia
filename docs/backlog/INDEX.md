# Backlog Index

This file is the navigation layer over the folder tree.

## Start here

| Need | Document |
| --- | --- |
| Fast product/gameplay understanding | PROJECT-CONTEXT.md |
| Implementation taste and conventions | IMPLEMENTATION-CONTEXT.md |
| Current code/planning state | CURRENT-STATUS.md |
| Gameplay source provenance | SOURCE-BASELINE.md |
| Architecture/design questions | OPEN_QUESTIONS/README.md |
| External dependency follow-up requests | OPEN-REQUESTS.md |
| Decision rules/index | DECISIONS/README.md |
| Ready standard | DEFINITION-OF-READY.md |
| Done standard | DEFINITION-OF-DONE.md |
| Terminology | GLOSSARY.md |
| GDD ownership mapping | TRACEABILITY/GDD-TRACEABILITY.md |
| Planning templates | templates/README.md |

## Architecture documents

- [ARCH-001 — Product boundaries and authority model](ARCHITECTURE/ARCH-001-PRODUCT-BOUNDARIES.md)
- [ARCH-002 — Client/Server authoritative protocol semantics](ARCHITECTURE/ARCH-002-AUTHORITATIVE-PROTOCOL.md)
- [ARCH-003 — Time, determinism, and authoritative ordering](ARCHITECTURE/ARCH-003-TIME-DETERMINISM-ORDERING.md)
- [ARCH-004 — Server node-routing topology](ARCHITECTURE/ARCH-004-SERVER-NODE-ROUTING.md)

Architecture documents should be added only after a durable cross-cutting invariant has been explicitly decided.

## Decision records

- [DR-001 — Long-term Core / Server / Client boundaries](DECISIONS/DR-001-PRODUCT-BOUNDARIES.md)
- [DR-002 — Core may depend on Sparkle Core](DECISIONS/DR-002-SPARKLE-CORE-DEPENDENCY.md)
- [DR-003 — Dedicated authoritative server from the first playable](DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md)
- [DR-004 — Client intent and Server authority semantics](DECISIONS/DR-004-COMMAND-AUTHORITY-SEMANTICS.md)
- [DR-005 — Client movement prediction is required](DECISIONS/DR-005-CLIENT-MOVEMENT-PREDICTION.md)
- [DR-006 — Separate Encounter, World, and real-time domains](DECISIONS/DR-006-THREE-TIME-DOMAINS.md)
- [DR-007 — Semantic determinism](DECISIONS/DR-007-SEMANTIC-DETERMINISM.md)
- [DR-008 — Authoritative operation ordering](DECISIONS/DR-008-AUTHORITATIVE-OPERATION-ORDERING.md)
- [DR-009 — First voxel terrain milestone](DECISIONS/DR-009-FIRST-VOXEL-TERRAIN-MILESTONE.md)
- [DR-010 — Backlog granularity and planning horizon](DECISIONS/DR-010-BACKLOG-GRANULARITY.md)
- [DR-011 — Voxel and Chunk coordinate conventions](DECISIONS/DR-011-VOXEL-COORDINATES.md)
- [DR-012 — Packed Voxel::Cell and generic Voxel::Volume direction](DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [DR-013 — Terrain meshes are Client-owned](DECISIONS/DR-013-CLIENT-TERRAIN-MESHING.md)
- [DR-014 — Batched Client-driven Chunk request protocol](DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md)
- [DR-015 — First deterministic terrain validation scene](DECISIONS/DR-015-FIRST-TERRAIN-VALIDATION-SCENE.md)
- [DR-016 — Sparkle networking and Server node router](DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)
- [DR-017 — Direct Voxel::Volume message serialization](DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)

OPEN_QUESTIONS/ contains the question register, including resolved questions for traceability. Create a DR only when a real choice has concrete alternatives that need durable resolution.

## Epics

- [EP-001 — Voxel Terrain Delivery and Visual Validation](EPICS/EP-001-voxel-terrain-foundation/EP-001-voxel-terrain-foundation.md) — Draft

EP-001 is the active near-term planning focus.

## Implementation tickets

No ST-001 implementation ticket is Ready yet.

EP-001 has a ticket workspace, but its detailed tickets are intentionally waiting on the voxel/network contracts tracked in OPEN_QUESTIONS/. A detailed ticket should not become Ready until its surrounding architecture is sufficiently understood and it satisfies the Definition of Ready.

## Keyword navigation

GLOSSARY.md maps important terms to GDD sections and, later, to their owning Epic(s). TRACEABILITY/GDD-TRACEABILITY.md maps larger capabilities to future backlog owners without inventing IDs before decomposition is approved.
