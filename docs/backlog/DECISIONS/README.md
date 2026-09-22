# Decision Records

Decision Records capture real choices that materially constrain implementation.

## Statuses

- Open — alternatives exist and the user has not resolved the choice.
- Resolved — the selected option and consequences are explicit.
- Superseded — a later decision replaces this record; preserve history and link the replacement.

## When to create a Decision Record

Create a DR when:

- two or more materially different valid choices exist;
- the choice affects public behavior, architecture, interoperability, testing, ownership, persistence, authority, or performance;
- the choice should remain understandable in future sessions.

Do not create fake alternatives to make a document look formal.

If only one solution follows from already-approved constraints, record the resulting durable invariant as architecture instead.

## Index

Resolved greenfield decisions are indexed below. Remaining unresolved questions are tracked in ../QUESTIONS.md.

| ID | Title | Status | Applies to | Supersedes |
| --- | --- | --- | --- | --- |
| DR-001 | [Long-term Core / Server / Client boundaries](DR-001-PRODUCT-BOUNDARIES.md) | Resolved | Core, Server, Client, authority | — |
| DR-002 | [Core may depend on Sparkle Core](DR-002-SPARKLE-CORE-DEPENDENCY.md) | Resolved | Core, dependency policy | — |
| DR-003 | [Dedicated authoritative server from the first playable](DR-003-DEDICATED-SERVER-FIRST.md) | Resolved | Runtime topology, networking boundary | — |
| DR-004 | [Client intent and Server authority semantics](DR-004-COMMAND-AUTHORITY-SEMANTICS.md) | Resolved | Client/Server protocol | — |
| DR-005 | [Client movement prediction is required](DR-005-CLIENT-MOVEMENT-PREDICTION.md) | Resolved | Exploration movement | — |
| DR-006 | [Separate Encounter, World, and real-time domains](DR-006-THREE-TIME-DOMAINS.md) | Resolved | Simulation and persistence time | — |
| DR-007 | [Semantic rather than bit-for-bit runtime determinism](DR-007-SEMANTIC-DETERMINISM.md) | Resolved | Determinism, generation | — |
| DR-008 | [Server serializes conflicting authoritative mutations](DR-008-AUTHORITATIVE-OPERATION-ORDERING.md) | Resolved | Concurrency, shared state | — |
| DR-009 | [First milestone is end-to-end voxel terrain pipeline](DR-009-FIRST-VOXEL-TERRAIN-MILESTONE.md) | Resolved | Initial roadmap, voxel terrain | — |
| DR-010 | [Small Epics with direct implementation tickets](DR-010-BACKLOG-GRANULARITY.md) | Resolved | Backlog workflow | — |
| DR-011 | [Voxel and Chunk coordinate conventions](DR-011-VOXEL-COORDINATES.md) | Resolved | Voxel/world coordinates | — |
| DR-012 | [Packed Voxel::Cell and generic Voxel::Volume direction](DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md) | Resolved | Core voxel representation | — |
| DR-013 | [Terrain meshes are Client-owned](DR-013-CLIENT-TERRAIN-MESHING.md) | Resolved | Terrain meshing/rendering | — |
| DR-014 | [Batched Client-driven Chunk request protocol](DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md) | Resolved | Chunk networking semantics | — |
| DR-015 | [First deterministic terrain validation scene](DR-015-FIRST-TERRAIN-VALIDATION-SCENE.md) | Resolved | Terrain validation fixture | — |
| DR-016 | [Use Sparkle networking and plan Server as a node router](DR-016-SPARKLE-NETWORK-NODE-ROUTER.md) | Resolved | Networking, Server topology | Q-040 |
