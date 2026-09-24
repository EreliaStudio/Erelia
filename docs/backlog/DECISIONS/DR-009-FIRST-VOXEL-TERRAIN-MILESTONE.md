# DR-009 — First implementation milestone is the end-to-end voxel terrain pipeline

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Initial roadmap, Core voxel data, Server terrain generation/networking, Client rendering

## Context

The project needs a concrete first implementation milestone. The GDD recommends visual voxel validation before broad gameplay production.

The project owner wants to validate both the Client rendering path and Server simulation/network boundary before designing production character controls.

## Question

What end-to-end capability should be built first?

## Decision

The first implementation Epic will validate this vertical technical pipeline:

1. Server owns a basic Chunk generator capable of constructing simple terrain.
2. Client connects to the real dedicated Server process.
3. Client requests terrain Chunks around its inspection position.
4. Server returns authoritative Chunk data through the network boundary.
5. Client converts received Chunk data into renderable voxel terrain and displays it.
6. Client provides a temporary free-flight 3D inspection controller with keyboard movement and camera control so the developer can move around the generated terrain and visually inspect rendering.

This controller is explicitly a **validation tool**, not the final exploration controller and not the implementation of DR-005 movement prediction.

The milestone exists to validate the first foundational brick: voxel data → Server generation → network delivery → Client meshing/rendering → human visual inspection.

## Consequences

- Voxel/chunk contracts and rendering are planned before production character locomotion.
- Networking must be real Client/Server networking from this first vertical slice.
- Terrain generation may initially be intentionally simple; production world-generation complexity is out of scope.
- The Epic must not silently commit final exploration physics, collision, follower behavior, or production camera behavior.
- Exact voxel coordinate/data/meshing/network contracts must be resolved before their detailed tickets become Ready.

## Required tests

At Epic completion:

- deterministic fixture Chunk generation is testable headlessly;
- Server can answer valid Chunk requests and reject invalid ones;
- Client can receive and decode Chunk data from a separate Server process;
- deterministic voxel fixtures produce expected mesh semantics;
- visual reference images are reviewed and explicitly approved by a human according to the eventual golden-image policy;
- manual/free-flight inspection demonstrates multiple adjacent Chunks and boundary correctness.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-028 and defining the desired first technical milestone.

## Supersession

None.
