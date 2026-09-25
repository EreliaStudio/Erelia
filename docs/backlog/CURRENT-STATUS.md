# Current Status

**Updated:** 25 September 2026
**Default baseline:** `master`
**Active ticket branch:** none

## Branch state

ST-001-07 is Finished after project-owner review and is prepared as the post-merge state for PR #14. ST-001-01 through ST-001-07 are treated as completed on `master`.

ST-001-07 establishes:

- the Erelia-owned shared `spk::ArgumentParser` adaptation;
- `EreliaServer` as the one Client-facing `spk::NodeRouter`;
- a separate `EreliaTerrainNode` process using `spk::RemoteNode::Endpoint`;
- router-owned `spk::RemoteNode` connections with Warning + configurable reconnect behavior;
- explicit router/node JSON configuration passed through `--config`;
- development defaults of router port `2550` and terrain-node port `2551`;
- automatic Server-node CMake discovery;
- reusable `tools/create-server-node.ps1` generation from `tools/templates/server-node/`;
- node-local tests contributing to `EreliaServerTestSuite`;
- real loopback connection/reconnection coverage;
- SIGINT/SIGTERM shutdown coverage for generated node applications.

DR-021 supersedes DR-016's original in-process LocalNode deployment detail while retaining the NodeRouter architecture.

## Validation / review state

Project-owner review is complete.

CI run #325 passed every Linux/Windows Debug/Release build-and-test job; its only failure was clang-format. The formatting violations were corrected afterward. The final signal-shutdown additions and status edits are part of the PR head and must be covered by the final PR check before merge.

## Next implementation step

There is no active implementation ticket.

**ST-001-08 — Batched Chunk request/response protocol contract** is the next dependency-ordered ticket, but it remains **Blocked** by OQ-038. Resolve the duplicate/outstanding request, batch-limit, ordering, partial-response, rejection, retry/cache and malformed-payload semantics required by OQ-038 before promoting ST-001-08 to Ready.

## Explicit non-goals

Do not implement Client networking policy, Chunk protocol/handler behavior, rendering, production terrain generation, or gameplay node families beyond the terrain bootstrap until their owning tickets are Ready.
