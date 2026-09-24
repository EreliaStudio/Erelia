# DR-021 — Start logical Server nodes as remote processes

**Status:** Resolved
**Date opened:** 2026-09-24
**Date resolved:** 2026-09-24
**Applies to:** Server topology, Server-node deployment, runtime configuration, development tooling
**Supersedes:** DR-016 initial in-process LocalNode deployment choice only

## Context

DR-016 fixed Sparkle networking and a router-first Server, initially choosing one in-process terrain `spk::LocalNode` for EP-001.

Before ST-001-07 implementation, the project owner chose to establish the process boundary immediately for every logical Server node rather than introduce a LocalNode phase that would later be migrated.

Sparkle Version-0.1.3 already provides the matching primitives: `spk::RemoteNode` in the router process and `spk::RemoteNode::Endpoint` in the node process.

## Decision

Every logical Server node is created as its own executable/process from its first implementation.

The main `EreliaServer` process owns the single Client-facing `spk::NodeRouter` and one `spk::RemoteNode` per configured logical node. The first EP-001 node is the separate terrain process, which owns a `spk::RemoteNode::Endpoint`.

Every Server executable receives its JSON configuration path explicitly through the shared Erelia `spk::ArgumentParser`: `--config <path>`, `--config=<path>`, or `-c <path>`. There is no compiled-in production configuration path.

Router schema:

```json
{
  "server config": {
    "port": 2550,
    "nodeReconnectDelayMs": 1000
  },
  "nodes": [
    {
      "name": "terrain",
      "address": "127.0.0.1",
      "port": 2551
    }
  ]
}
```

The numeric values above illustrate the schema only. Runtime values come from the selected JSON file. `nodeReconnectDelayMs` is required and greater than zero; there is no hidden reconnect-delay default.

Each node owns a separate JSON file. ST-001-07's terrain bootstrap requires only:

```json
{
  "server config": {
    "port": 2551
  }
}
```

Later terrain-specific settings extend the terrain node file rather than the main router file.

Failure to connect a configured node does not prevent the main router from listening. Every failed attempt logs a Warning identifying that node/endpoint and schedules the next attempt after the configured delay.

Server nodes live under `server/nodes/<node>/`, each with CMake, `include/`, `src/`, `resources/`, and `tests/`. Server CMake and `tools/run-client-server.ps1` discover node directories automatically. Node tests contribute to the single `EreliaServerTestSuite`.

`tools/create-server-node.ps1` creates a node from `tools/templates/server-node/`. Automatic discovery means the generator does not edit a central node list, `CMakePresets.json`, or the launcher for every node.

## Consequences

- The process/network boundary is exercised from the first terrain-node implementation.
- Node deployment can move independently without changing the Client-facing Server endpoint or Erelia protocol.
- Node availability becomes explicit runtime state.
- Router configuration owns node connectivity only; node-specific data remains node-local.
- Tests may use port 0 for Sparkle-selected ephemeral listeners.
- DR-016 remains active for Sparkle networking, NodeRouter ingress, message routing, authority, and serialization direction.

## Required tests

ST-001-07 covers router and terrain Endpoint lifecycle, config rejection, a real RemoteNode/Endpoint connection, unavailable-node warning/retry/reconnection, node-local tests in EreliaServerTestSuite, and executable CLI smoke coverage.

## Resolution provenance

Resolved directly by the project owner on 24 September 2026 while preparing ST-001-07.
