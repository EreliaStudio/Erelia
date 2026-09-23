# EP-001 — Voxel Terrain Delivery and Visual Validation

**Status:** Draft
**Roadmap phase:** Foundation — first implementation milestone
**Dependencies:** DR-001, DR-002, DR-003, DR-004, DR-007, DR-009, DR-010, DR-011, DR-012, DR-013, DR-014, DR-015, DR-016, DR-017, DR-018; ARCH-001, ARCH-002, ARCH-003, ARCH-004
**Primary ownership:** Core + Server + Client

## Purpose

Prove Erelia's first foundational end-to-end technical pipeline: a dedicated Server constructs deterministic terrain Chunks, a separate Client requests and receives those Chunks over the real network boundary, and the Client meshes/renders the received voxel terrain for visual inspection.

The Epic deliberately validates terrain/render/network foundations before production exploration movement or broad gameplay systems.

## Gameplay / system requirements

From the GDD and approved decisions:

- terrain uses a voxel-volume representation;
- terrain Chunks are 16×16×16 cells;
- terrain cell scale is one world unit per cell;
- permanent voxel terrain is immutable during ordinary gameplay;
- terrain supports normalized voxel Shapes/material concepts, including cubes/slabs/slopes/stairs as the architecture evolves;
- Server is authoritative;
- Client and Server are separate processes from the first playable;
- seeded generation requires semantic determinism;
- visual validation precedes broad gameplay production;
- the first milestone includes basic Server terrain generation, network Chunk retrieval, Client rendering, and free-flight visual inspection.

## Architecture

Constrained by:

- ../../ARCHITECTURE/ARCH-001-PRODUCT-BOUNDARIES.md
- ../../ARCHITECTURE/ARCH-002-AUTHORITATIVE-PROTOCOL.md
- ../../ARCHITECTURE/ARCH-003-TIME-DETERMINISM-ORDERING.md
- ../../ARCHITECTURE/ARCH-004-SERVER-NODE-ROUTING.md

Key rules:

- Core owns shared Chunk/voxel representations and generic algorithms that both Server and Client require.
- Server owns Chunk generation and the canonical terrain data returned to a Client.
- EreliaServer starts on `spk::NodeRouter`; EP-001 Chunk handling belongs to the first in-process terrain `spk::LocalNode`.
- Client never generates authoritative terrain as a substitute for Server data.
- Client owns meshing/rendering unless a later explicit decision changes that boundary.
- the inspection controller is temporary validation tooling, not the production exploration movement controller;
- terrain generation in this Epic is intentionally simple and deterministic rather than production world generation.

## Responsibilities by product/module

### Core

- shared terrain Chunk/value representation;
- Chunk coordinate/address representation;
- shared voxel cell/definition representation required by both Server and Client;
- shared `spk::Message` serialization for `Voxel::Volume`, with friend `operator<<` / `operator>>` declarations on `Voxel::Volume` so callers use direct `message << volume` / `message >> volume` syntax while serializing logical fields/cell storage rather than raw `std::vector` internals;
- deterministic helpers required by generation/serialization/meshing contracts;
- protocol data structures that both processes need.

### Server

- own the basic deterministic terrain Chunk generator;
- accept valid Chunk data requests from connected Clients;
- generate/obtain canonical requested Chunk data;
- receive Chunk request messages through the Server `spk::NodeRouter` terrain route;
- return canonical Chunk data through Sparkle Version-0.1.3 networking;
- reject invalid requests according to the protocol contract.

### Client

- connect to the dedicated Server;
- determine which Chunks are required around the inspection position;
- request missing Chunks;
- receive and decode canonical Chunk data;
- mesh/render received voxel terrain;
- maintain the Client-side runtime/cache state needed for inspection;
- provide temporary keyboard + camera free-flight in 3D to inspect terrain.

## Explicitly out of scope

- production Hero movement;
- movement prediction/reconciliation implementation beyond preserving its future architectural requirement;
- character collision;
- gravity, slopes/stairs as locomotion rules, or player physics;
- followers;
- combat-cell extraction;
- dynamic terrain editing/mining/building;
- resource nodes;
- production biome/world generation;
- dungeon generation;
- model/character voxel import;
- production asset editor;
- final networking scalability/interest-management design beyond what this milestone needs.

## Public contracts introduced

Exact signatures are intentionally deferred while this Epic remains Draft.

The Epic will require deliberate contracts for:

- Chunk coordinates/addressing;
- 16×16×16 terrain Chunk data;
- voxel cell/definition data sufficient for first rendering;
- deterministic Chunk generation input/output;
- Client Chunk request;
- `Voxel::Volume` friend serialization operators against `spk::Message`;
- Server Chunk response/rejection;
- Client Chunk lifetime/cache identity;
- voxel-data-to-render-mesh conversion.

## Capability coverage

| Promised capability | Implementation owner |
| --- | --- |
| Shared terrain coordinate/address conversion | [ST-001-01](tickets/ST-001-01-shared-terrain-coordinate-conversion.md) — Done |
| Packed shared voxel Cell | [ST-001-02](tickets/ST-001-02-packed-voxel-cell.md) — Done |
| Shared owning Voxel::Volume | [ST-001-03](tickets/ST-001-03-owning-voxel-volume.md) |
| Shared first terrain Definition/Shape contract | [ST-001-04](tickets/ST-001-04-first-terrain-definition-shape-contract.md) |
| Shared Volume Message serialization | [ST-001-05](tickets/ST-001-05-voxel-volume-message-serialization.md) |
| Deterministic basic Server Chunk generation | [ST-001-06](tickets/ST-001-06-deterministic-validation-terrain-generator.md) |
| Server NodeRouter + terrain LocalNode runtime | [ST-001-07](tickets/ST-001-07-server-node-router-terrain-node-bootstrap.md) |
| Client Chunk request / Server response protocol | [ST-001-08](tickets/ST-001-08-batched-chunk-protocol-contract.md) + [ST-001-09](tickets/ST-001-09-server-chunk-request-handler.md) |
| Dedicated Client -> Server connection | [ST-001-10](tickets/ST-001-10-client-dedicated-server-connection.md) |
| Client Chunk cache/request coordination | [ST-001-11](tickets/ST-001-11-client-chunk-request-cache-coordinator.md) |
| Client voxel terrain meshing | [ST-001-12](tickets/ST-001-12-client-terrain-mesher.md) |
| Client terrain rendering | [ST-001-13](tickets/ST-001-13-client-terrain-rendering-integration.md) |
| Temporary 3D free-flight inspection controls | [ST-001-14](tickets/ST-001-14-temporary-free-flight-inspection-controller.md) |
| Adjacent-Chunk cross-process integration validation | [ST-001-15](tickets/ST-001-15-adjacent-chunk-cross-process-integration.md) |
| Golden-image / human visual / performance validation | [ST-001-16](tickets/ST-001-16-visual-performance-validation.md) |

## Ticket index

The complete dependency-ordered table is maintained in [tickets/README.md](tickets/README.md).

- **Done:** ST-001-01, ST-001-02, ST-001-03.
- **In Progress:** ST-001-04.
- **Blocked:** ST-001-05, ST-001-06, ST-001-08, ST-001-09, ST-001-11, ST-001-12, ST-001-15, ST-001-16.
- **Draft:** ST-001-07, ST-001-10, ST-001-13, ST-001-14.

The first three implementation tickets are complete and merged into `master`: **ST-001-01 — Shared terrain coordinate conversion** through PR #7, **ST-001-02 — Packed Voxel::Cell value type** through PR #8, and **ST-001-03 — Owning Voxel::Volume** through PR #9. ST-001-03 uses the finalized immutable Builder + direct pooled `Voxel::Volume::Buffer::Lease` design, deep-copy Volume semantics, optional `tryGet()` lookup, dedicated Chunk pooling, and ordered general size-class pooling. Project-owner approval is recorded and CI run #105 passed. OQ-035 is Resolved. DR-018 resolves the first shared Shape/Definition/Catalog contract. ST-001-04 remains In Progress on `feat/st-001-04-definition-shape-contract` / PR #11. CI run #127 passed the full matrix before the latest catalog/ownership refactor; the current design uses an Erelia-local abstract `spk::JSON::Catalog<TElement>` base with two pure virtual Reader-based parsing hooks returning values, plus non-owning `Definition -> const Shape&` references and a private zero-polygon Shape sentinel for Air. No Erelia `detail` namespace is used. Fresh validation of that current head and explicit project-owner approval remain required. No later-ticket behavior has been implemented.

## Epic-level integration scenarios

### Nominal end-to-end terrain inspection

1. Start dedicated Server.
2. Start Client.
3. Client connects to Server.
4. Client establishes an inspection position.
5. Client determines missing nearby Chunk coordinates.
6. Client requests those Chunks.
7. Server deterministically constructs canonical Chunk data.
8. Server returns Chunk data.
9. Client stores/meshes/renders the data.
10. Developer moves the free-flight camera across several Chunk boundaries and visually inspects the result.

### Repeated deterministic request

Given the same generator version/seed/Chunk coordinate, Server returns semantically identical canonical terrain data.

### Adjacent Chunk boundary

Two or more neighboring Chunks render together without incorrect gaps, duplicated boundary geometry, or address mismatch.

## Epic-level failure scenarios

- Client requests an invalid/out-of-contract Chunk coordinate.
- malformed Chunk request arrives.
- malformed/incompatible Chunk payload is received by Client.
- Server disconnects while Chunks are outstanding.
- Client requests a Chunk already available locally.
- generation fails for a valid request.
- meshing/rendering receives invalid voxel data.

Exact observable failure behavior remains to be specified by the owning tickets before Ready.

## Determinism / authority / persistence requirements

- Server is canonical source of terrain returned to Client.
- basic terrain generation is deterministic for approved seed/input fixtures.
- semantic determinism is required; generic floating-point bit identity is not.
- permanent terrain is not modified by ordinary gameplay in this Epic.
- no persistent dynamic-world-state system is required by this Epic unless later identified as necessary for Chunk generation identity/versioning.

## Performance considerations

This Epic must collect structural/performance evidence sufficient to discover obvious voxel-pipeline problems, but numeric performance budgets are not invented here.

[OQ-031](../../OPEN_QUESTIONS/OQ-031-FIRST-MILESTONE-PERFORMANCE-EVIDENCE.md) remains open for deliberate benchmark methodology/targets.

## Visual-validation requirements

Rendering is a primary acceptance concern.

The Epic requires:

- semantic mesh tests;
- human visual inspection;
- approved golden/reference images once Q-029 and Q-030 define the platform/comparison policy;
- no automatic replacement/approval of golden images.

## Blocking decisions

Existing:

- [OQ-029](../../OPEN_QUESTIONS/OQ-029-GOLDEN-IMAGE-PLATFORM.md) — golden-image platform;
- [OQ-030](../../OPEN_QUESTIONS/OQ-030-IMAGE-COMPARISON-POLICY.md) — image comparison policy;
- [OQ-031](../../OPEN_QUESTIONS/OQ-031-FIRST-MILESTONE-PERFORMANCE-EVIDENCE.md) — performance evidence methodology.

Epic-specific questions tracked in OPEN_QUESTIONS/:

- [OQ-035](../../OPEN_QUESTIONS/OQ-035-TERRAIN-VOXEL-CELL-REPRESENTATION.md) — first terrain voxel/cell representation;
- [OQ-036](../../OPEN_QUESTIONS/OQ-036-TERRAIN-MESHING-NEIGHBOR-POLICY.md) — Chunk meshing ownership and boundary-neighbor contract;
- [OQ-037](../../OPEN_QUESTIONS/OQ-037-EP001-NETWORK-SERIALIZATION.md) — remaining scalar byte-order/platform-compatibility policy;
- [OQ-038](../../OPEN_QUESTIONS/OQ-038-CHUNK-REQUEST-STREAMING.md) — Chunk request/streaming semantics;
- [OQ-039](../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) — exact basic terrain generator fixture;

[OQ-040](../../OPEN_QUESTIONS/OQ-040-EP001-SERVER-NODE-ROUTER.md) is resolved: EP-001 is router-first with one terrain `LocalNode`. [OQ-022](../../OPEN_QUESTIONS/OQ-022-ARTICULATED-ENTITY-COLLISION.md) character collision and [OQ-023](../../OPEN_QUESTIONS/OQ-023-ASSET-AUTHORING-IMPORT.md) asset import do not block this Epic. Sparkle Version-0.1.3 is the selected networking dependency; no third-party network library is required.

## Required user decisions

Resolve the blocking questions above before promoting the corresponding implementation tickets to Ready.

The decomposition also exposed Draft-only specification gaps that are not yet represented by a dedicated OQ: the EP-001 Server endpoint + Client connection lifecycle/configuration; the deterministic first render fixture/material binding/resource-failure contract; and the complete temporary free-flight input/numeric camera semantics. The former Definition/Shape gap is resolved by DR-018 and ST-001-04.

## Exit criteria

EP-001 is Done only when:

- dedicated Server and Client run as separate processes;
- Client connects to Server;
- Client requests nearby 16×16×16 terrain Chunks through the real network boundary;
- Server constructs deterministic basic terrain Chunks and returns canonical data;
- Client meshes and renders received terrain;
- free-flight keyboard/camera inspection can traverse an area containing multiple adjacent Chunks;
- exact deterministic generator fixtures pass;
- invalid request/payload paths have defined tested behavior;
- adjacent Chunk boundaries pass semantic mesh/integration tests;
- required CI/build configurations pass;
- visual reference images are explicitly reviewed/approved by a human;
- required Epic performance evidence is captured under the approved methodology.
