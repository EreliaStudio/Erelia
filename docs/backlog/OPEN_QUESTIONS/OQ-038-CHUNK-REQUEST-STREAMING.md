# OQ-038 — What are the first Chunk request / streaming semantics?

**Status:** Partially resolved
**Decision records:** [DR-014](../DECISIONS/DR-014-BATCHED-CHUNK-PROTOCOL-DIRECTION.md), [DR-019](../DECISIONS/DR-019-IMMUTABLE-VOLUME-CHUNK-COLLECTION-PROVIDER.md)
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

Duplicate outstanding request behavior, cache eviction/retention policy, request limits, partial-success behavior, stale/unsolicited response behavior, and invalid/unavailable-coordinate response semantics remain open.

DR-019 fixes only the shared lifetime direction: Core will expose `Chunk::Collection` with a nested Provider; a future Client Provider may issue a request and immediately return an empty valid Chunk placeholder; a canonical Server response replaces the complete stored Chunk value rather than editing it. Existing copied Chunk values remain valid through immutable shared Volume content. The exact Client state machine remains open.

## Chosen solution

Use batched Client-driven Chunk requests. The Client chooses its view region (hard-coded initially or startup-configured).

Use the Core `Chunk::Collection` / nested Provider abstraction established by DR-019 when ST-001-11 is implemented. Client-side acquisition may publish an empty valid placeholder while the request is outstanding, then replace the entire Collection value when the canonical Server Chunk arrives. Placeholder data is never authoritative.

Remaining duplicate/outstanding, absent/stale response, cache/retention, retry, batch-limit and partial-response details are not yet chosen.
