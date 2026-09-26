# EP-001 Tickets

Implementation tickets for EP-001 use:

`ST-001-YY-<implementation-goal>.md`

The tickets are ordered by dependency, not by implementation status.

| Ticket | Status | Depends on |
| --- | --- | --- |
| [ST-001-01 — Shared terrain coordinate conversion](ST-001-01-shared-terrain-coordinate-conversion.md) | **Done** | — |
| [ST-001-02 — Packed Voxel::Cell value type](ST-001-02-packed-voxel-cell.md) | **Done** | Cell portion of OQ-035 resolved |
| [ST-001-03 — Owning Voxel::Volume](ST-001-03-owning-voxel-volume.md) | **Done** | ST-001-02; OQ-035 resolved |
| [ST-001-04 — First terrain Definition and Shape contract](ST-001-04-first-terrain-definition-shape-contract.md) | **Done** | ST-001-02; DR-018 |
| [ST-001-05 — Voxel::Volume Message serialization](ST-001-05-voxel-volume-message-serialization.md) | **Done** | ST-001-02, ST-001-03; OQ-037 resolved |
| [ST-001-06 — Deterministic validation terrain provider and Chunk collection foundation](ST-001-06-deterministic-validation-terrain-generator.md) | **Done** | ST-001-01 through ST-001-05; DR-019; DR-020; OQ-039 resolved |
| [ST-001-07 — Server NodeRouter remote terrain-node bootstrap](ST-001-07-server-node-router-terrain-node-bootstrap.md) | **Done** | ST-001-06; DR-016; DR-021 |
| [ST-001-08 — Batched Chunk request/response protocol contract](ST-001-08-batched-chunk-protocol-contract.md) | **Done** | ST-001-01, ST-001-03, ST-001-05; OQ-038 resolved; DR-022 |
| [ST-001-09 — Server Chunk request handler](ST-001-09-server-chunk-request-handler.md) | **Done** | ST-001-06, ST-001-07, ST-001-08 |
| [ST-001-10 — Client dedicated-Server connection](ST-001-10-client-dedicated-server-connection.md) | **Draft** | ST-001-07; endpoint/connection-lifecycle specification |
| [ST-001-11 — Client Chunk request/cache coordinator](ST-001-11-client-chunk-request-cache-coordinator.md) | **Blocked** | ST-001-08, ST-001-10; Client cache/retry/recycle policy specification |
| [ST-001-12 — Client terrain mesher](ST-001-12-client-terrain-mesher.md) | **Blocked** | ST-001-03, ST-001-04; OQ-036 |
| [ST-001-13 — Client terrain rendering integration](ST-001-13-client-terrain-rendering-integration.md) | **Draft** | ST-001-01, ST-001-12; render-fixture/material/lifecycle specification |
| [ST-001-14 — Temporary free-flight inspection controller](ST-001-14-temporary-free-flight-inspection-controller.md) | **Draft** | ST-001-13; full input/numeric camera-control specification |
| [ST-001-15 — Adjacent-Chunk cross-process integration](ST-001-15-adjacent-chunk-cross-process-integration.md) | **Blocked** | ST-001-06, ST-001-09 through ST-001-13; OQ-036 |
| [ST-001-16 — Visual and performance validation evidence](ST-001-16-visual-performance-validation.md) | **Blocked** | ST-001-14, ST-001-15; OQ-029, OQ-030, OQ-031 |

## Current implementation state

**ST-001-01 — Shared terrain coordinate conversion** is **Done**. PR #7 was merged on 22 September 2026 after the required implementation, headless CI evidence, and human review/approval.

**ST-001-02 — Packed Voxel::Cell value type** is **Done** and was merged through PR #8 after CI run #59 and project-owner approval.

**ST-001-03 — Owning Voxel::Volume** is **Done** through PR #9 after project-owner approval and green CI run #105. Its historical implementation used direct per-Volume pooled Leases and deep-copy Volume copies. DR-019, approved during ST-001-06 planning, supersedes those ownership/copy details with shared immutable backing Cell content while preserving the immutable built API, Builder-before-build mutation model, optional `tryGet()`, Y/X/Z indexing and deterministic power-of-two pool classes.

**ST-001-04 — First terrain Definition and Shape contract** is **Done** and merged through PR #11 on 24 September 2026.

**ST-001-05 — Voxel::Volume Message serialization** is **Done** and merged through PR #12 on 24 September 2026 after project-owner approval. OQ-037 and DR-017 fix the generic Sparkle-native wire contract, field order, contiguous Cell block, validation/failure semantics and deterministic-byte scope. DR-019 supersedes only the successful-decode same-destination-buffer reuse optimization: shared immutable Volume content requires fresh decoded storage before destination replacement. CI run #289 (run ID `35986211668`) remains the historical completion evidence for ST-001-05.

**ST-001-07 — Server NodeRouter remote terrain-node bootstrap** is **Finished** after project-owner review. It establishes the remote terrain-node process, router reconnect/configuration behavior, reusable node generator/template, automatic node discovery, and lifecycle/connectivity/signal-shutdown coverage. PR #14 is the completion/merge vehicle.

**ST-001-08 — Batched Chunk request/response protocol contract** is **Done** and merged through PR #16 after project-owner review. It historically implemented the first Core `Networking::MessageType` contract and Message-backed `Chunk::Protocol::{Request, Response, Error}` values with nested Builders, strict decoding/canonical encoding, and dedicated protocol TU coverage. CI run #356 (run ID `36138476545`) passed on reviewed head `a4c0e29059cec422bee848dff2c465ad26c53493`. ST-001-09 subsequently refined and implemented the terminal `Response::Success` / `Response::Failure` model and generic diagnostic base while preserving ST-001-08 as historical completion evidence.

**ST-001-09 — Server Chunk request handler** is **Done** on `feat/st-001-09-server-chunk-request-handler`. It implements the asynchronous Collection/Provider batch contract, generic diagnostics, refined terminal Response codec, terrain application dispatch, safe completion mailbox/reply lifetime, main Router registration, and real routed Server integration coverage. CI run #464 (run ID `36233964005`) passed the full matrix on code head `b15896137e9eb13b92b2ed150541383fe0e4bff9`.

## Remaining blockers

Existing OQs:

- OQ-036 — missing-neighbor/remesh policy;
- OQ-038 — resolved for ST-001-08 by DR-022; later Client cache/retry/recycle-threshold policy remains ST-001-11;
- OQ-039 — resolved exact generator/Definition fixture (DR-015);
- OQ-029 / OQ-030 — golden platform and comparison policy;
- OQ-031 — performance evidence methodology.

Additional Draft-ticket specification gaps exposed by decomposition:

- Client connection lifecycle/configuration after the now-resolved ST-001-07 Server endpoint/runtime contract;
- deterministic first render fixture/material binding and render-resource failure/lifecycle behavior;
- complete temporary free-flight input map and numeric camera/movement semantics.

ST-001-01 through ST-001-08 are completed on `master`. OQ-039 and the ST-001-06 readiness decisions are resolved. ST-001-06 is **Done** and merged through PR #13. ST-001-07 is **Finished/Done** through PR #14 after project-owner review. ST-001-08 is **Done** and merged through PR #16 after project-owner review. ST-001-09 is **Done** on its feature branch with green CI run #464; the next dependency-ordered ticket is ST-001-10, which remains Draft pending its own connection-lifecycle specification.
