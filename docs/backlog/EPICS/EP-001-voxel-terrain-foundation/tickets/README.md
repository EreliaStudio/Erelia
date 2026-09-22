# EP-001 Tickets

Implementation tickets for EP-001 use:

`ST-001-YY-<implementation-goal>.md`

The tickets are ordered by dependency, not by implementation status.

| Ticket | Status | Depends on |
| --- | --- | --- |
| [ST-001-01 — Shared terrain coordinate conversion](ST-001-01-shared-terrain-coordinate-conversion.md) | **Done** | — |
| [ST-001-02 — Packed Voxel::Cell value type](ST-001-02-packed-voxel-cell.md) | **Done** | Cell portion of OQ-035 resolved |
| [ST-001-03 — Owning Voxel::Volume](ST-001-03-owning-voxel-volume.md) | **Ready** | ST-001-02; OQ-035 resolved |
| [ST-001-04 — First terrain Definition and Shape contract](ST-001-04-first-terrain-definition-shape-contract.md) | **Draft** | ST-001-02; OQ-039 + Definition/Shape specification gap |
| [ST-001-05 — Voxel::Volume Message serialization](ST-001-05-voxel-volume-message-serialization.md) | **Blocked** | ST-001-02, ST-001-03; OQ-037 |
| [ST-001-06 — Deterministic validation terrain generator](ST-001-06-deterministic-validation-terrain-generator.md) | **Blocked** | ST-001-01 through ST-001-04; OQ-039 |
| [ST-001-07 — Server NodeRouter terrain-node bootstrap](ST-001-07-server-node-router-terrain-node-bootstrap.md) | **Draft** | Server endpoint/lifecycle specification |
| [ST-001-08 — Batched Chunk request/response protocol contract](ST-001-08-batched-chunk-protocol-contract.md) | **Blocked** | ST-001-01, ST-001-03, ST-001-05; OQ-037, OQ-038 |
| [ST-001-09 — Server Chunk request handler](ST-001-09-server-chunk-request-handler.md) | **Blocked** | ST-001-06, ST-001-07, ST-001-08; OQ-038 |
| [ST-001-10 — Client dedicated-Server connection](ST-001-10-client-dedicated-server-connection.md) | **Draft** | ST-001-07; endpoint/connection-lifecycle specification |
| [ST-001-11 — Client Chunk request/cache coordinator](ST-001-11-client-chunk-request-cache-coordinator.md) | **Blocked** | ST-001-08, ST-001-10; OQ-038 |
| [ST-001-12 — Client terrain mesher](ST-001-12-client-terrain-mesher.md) | **Blocked** | ST-001-03, ST-001-04; OQ-036 |
| [ST-001-13 — Client terrain rendering integration](ST-001-13-client-terrain-rendering-integration.md) | **Draft** | ST-001-01, ST-001-12; render-fixture/material/lifecycle specification |
| [ST-001-14 — Temporary free-flight inspection controller](ST-001-14-temporary-free-flight-inspection-controller.md) | **Draft** | ST-001-13; full input/numeric camera-control specification |
| [ST-001-15 — Adjacent-Chunk cross-process integration](ST-001-15-adjacent-chunk-cross-process-integration.md) | **Blocked** | ST-001-06, ST-001-09 through ST-001-13; OQ-036, OQ-038, OQ-039 |
| [ST-001-16 — Visual and performance validation evidence](ST-001-16-visual-performance-validation.md) | **Blocked** | ST-001-14, ST-001-15; OQ-029, OQ-030, OQ-031, OQ-039 |

## Current implementation state

**ST-001-01 — Shared terrain coordinate conversion** is **Done**. PR #7 was merged into the planning branch on 22 September 2026 after the required implementation, headless CI evidence, and human review/approval.

**ST-001-02 — Packed Voxel::Cell value type** is **Done** and was merged through PR #8 into the planning branch at `f03894f76fc996d5fba3241e2e51ead848783cad` after CI run #59 and project-owner approval.

**ST-001-03 — Owning Voxel::Volume** is **Ready** after the project owner resolved the remaining OQ-035 Volume storage, validation, editor/versioning, view-lifetime, and copy/move contracts on 23 September 2026.

## Remaining blockers

Existing OQs:

- OQ-036 — missing-neighbor/remesh policy;
- OQ-037 — scalar wire portability and remaining decode contract;
- OQ-038 — duplicate/outstanding request, limits, cache/retention, retry and partial-response semantics;
- OQ-039 — exact generator/Definition fixture;
- OQ-029 / OQ-030 — golden platform and comparison policy;
- OQ-031 — performance evidence methodology.

Additional Draft-ticket specification gaps exposed by decomposition:

- minimal greenfield Definition/Shape geometry/resource-availability contract;
- exact EP-001 Server endpoint and Client connection lifecycle/configuration;
- deterministic first render fixture/material binding and render-resource failure/lifecycle behavior;
- complete temporary free-flight input map and numeric camera/movement semantics.

The planning branch remains the backlog baseline and now contains the merged ST-001-01 production implementation. Future implementation tickets should continue to use dedicated feature branches cut from the current planning baseline.
