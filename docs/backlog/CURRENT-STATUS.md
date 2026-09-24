# Current Status

**Updated:** 24 September 2026
**Default baseline:** `master`
**Active ticket branch:** `feat/st-001-07-server-node-router-terrain-node-bootstrap`

## Branch state

ST-001-06 is merged into current `master` through PR #13. ST-001-07 is active from merged `master` commit `26c95e46ce82bbbec50087b32b408222772675fb`.

The branch contains the Erelia-owned shared `spk::ArgumentParser` adaptation and Core tests.

## ST-001-07 decisions

DR-021 replaces the old in-process terrain LocalNode deployment detail:

- `EreliaServer` remains the one Client-facing `spk::NodeRouter`;
- terrain is a separate process using `spk::RemoteNode::Endpoint`;
- the router owns a corresponding `spk::RemoteNode`;
- router/node config paths are explicit `--config` arguments;
- router JSON owns its port, required configurable reconnect delay, and all node names/addresses/ports;
- each node owns a separate JSON for its listen port and later node-specific data;
- unavailable nodes log Warning and retry without preventing Router startup;
- Server CMake and development tooling discover `server/nodes/*` automatically;
- every node owns include/src/resources/tests folders;
- node tests contribute to one EreliaServerTestSuite;
- `tools/create-server-node.ps1` uses a maintained node template.

ST-001-07 does not invent Chunk message IDs or handlers; those remain ST-001-08/ST-001-09.

## Current implementation phase

**ST-001-07 is Ready and implementation is in progress on the active branch.**

## Explicit non-goals

Do not implement Client networking policy, Chunk protocol/handler behavior, rendering, production terrain generation, or gameplay node families beyond the terrain bootstrap.
