# OQ-038 — What are the first Chunk request / streaming semantics?

**Status:** Partially resolved
**Decision records:** [DR-014](../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md), [DR-019](../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md), [DR-020](../DECISIONS/DR-020-HEADLESS-ASYNC-TASK-INFRASTRUCTURE.md)
**Affected areas:** EP-001, Client streaming, TerrainNode

## Question

What are the first Chunk request / streaming semantics?

## Problem / context

The Client needs to request nearby Chunks efficiently without making the Server responsible for Client view distance or rendering policy.

## Known constraints

- One request can batch multiple Chunk coordinates.
- Server returns coordinate + immutable `Chunk` results.
- Client alone owns its view/loading region policy.

## Possible solutions

1. Suppress duplicate outstanding requests and keep a Client cache with a configurable load/retain policy.
2. Allow duplicate requests initially and keep every received Chunk for the lifetime of the inspection session.
3. Use a minimal hard-coded radius first, then add configuration/eviction after the end-to-end path works.

## Remaining ambiguity

Core-local duplicate suppression is now resolved: `Chunk::Collection` owns explicit Absent/Pending/Available state and does not invoke its Provider again while a coordinate is Pending or Available. Each Pending request carries a monotonically increasing generation and stale asynchronous results are rejected.

Network-level retry timing, cache eviction/retention, batch/request limits, partial-success behavior, stale/unsolicited network response behavior, and invalid/unavailable-coordinate response semantics remain open.

DR-019 also removes the former empty-Chunk placeholder idea from the generic Collection contract. Pending is represented as state, not as fake voxel content. Existing copied Available Chunk values remain valid through immutable shared Volume content.

## Chosen solution

Use batched Client-driven Chunk requests. The Client chooses its view region (hard-coded initially or startup-configured).

Use the Core `Chunk::Collection` / nested Provider abstraction established by DR-019 when ST-001-11 is implemented. A missing requested coordinate becomes Pending; no placeholder Chunk is published. The Client Provider may perform asynchronous network work and later publish the canonical complete Chunk only for the matching generation.

Remaining network retry timing, absent/stale/unsolicited response policy, cache/retention, batch limits and partial-response details are not yet chosen.
