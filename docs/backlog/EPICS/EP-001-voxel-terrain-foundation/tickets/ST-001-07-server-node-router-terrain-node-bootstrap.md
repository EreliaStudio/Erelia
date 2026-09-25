# ST-001-07 — Server NodeRouter remote terrain-node bootstrap

**Status:** Finished
**Epic:** EP-001
**Production target(s):** Core shared CLI utility; Server; terrain Server node; developer tooling
**Test suite(s):** EreliaCoreTestSuite; EreliaServerTestSuite; EreliaServerSmoke

## Intent

Replace the Server smoke-only entry point with one Client-facing `spk::NodeRouter` and one separately launched terrain process using `spk::RemoteNode::Endpoint`. Establish the reusable filesystem/CMake/tooling convention used by future Server nodes.

## Starting state / prerequisites

- ST-001-01 through ST-001-06 are merged into `master`.
- DR-016 fixes Sparkle networking and NodeRouter ingress.
- DR-021 fixes remote node processes from the start.
- Erelia Core contains the shared Erelia-owned `spk::ArgumentParser` adaptation.
- ST-001-08 owns Chunk message IDs/payloads; ST-001-09 owns the Chunk handler.

## Allowed / forbidden dependencies

Allowed: EreliaServerLibrary, EreliaCore, Sparkle Version-0.1.3 Core networking/JSON/logger, standard library, PowerShell/.NET in developer tooling.

Forbidden: Client/graphics code inside Server nodes, raw C++ socket wrappers, alternate networking libraries, a temporary bare-`spk::Server` game loop, `erelia::server` or `erelia::client` namespaces.

## Public contract

### CLI

Router and node executables require `--config <path>`, `--config=<path>`, or `-c <path>`, except for `--help`.

### Router JSON

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

Numbers are illustrative, not defaults. Router port 0 is valid for tests. `nodeReconnectDelayMs` is required and greater than zero. Node names/addresses are non-empty, remote-node ports are non-zero, names are unique, and unknown fields are rejected.

### Terrain-node JSON

```json
{
  "server config": {
    "port": 2551
  }
}
```

The number is illustrative. Port 0 is valid for tests. Unknown fields are rejected. Terrain-specific gameplay settings are intentionally absent in this ticket.

### Router lifecycle and node connectivity

The Server runtime owns one `spk::NodeRouter` and one `spk::RemoteNode` per configured node.

- construction is stopped;
- repeated `start()` performs orderly stop then restart, matching Sparkle semantics;
- public router listen/start failure propagates and leaves runtime stopped;
- a node connection failure does not fail router startup;
- every failed node connection logs a Warning and schedules retry after the configured delay;
- disconnected nodes retry while the router remains running;
- `stop()` disconnects nodes and stops the NodeRouter and is idempotent;
- the same runtime object can be started again.

ST-001-07 registers the terrain node object but registers **no Chunk message redirection** because ST-001-08 owns message types.

### Process lifetime

Normal router/node executables dispatch headlessly until SIGINT/SIGTERM, then cleanly stop networking. `main.cpp` remains thin.

### Node project convention

```text
server/nodes/<node>/
├── CMakeLists.txt
├── include/
├── src/
├── resources/
└── tests/
```

Server CMake discovers node directories automatically. Node tests live beside their node and add their sources/libraries to the single `EreliaServerTestSuite`.

`tools/templates/server-node/` is copied by `tools/create-server-node.ps1`. `tools/run-client-server.ps1` discovers all node folders, builds them, materializes development JSON in the build tree, launches nodes first, then EreliaServer, then EreliaClient.

## Explicitly not owned

Chunk protocol IDs/codecs, terrain request handling, Client connection policy, gameplay node families beyond terrain, deployment supervision, rendering.

## Invariants

- exactly one public Client-facing NodeRouter;
- node processes accept Sparkle RemoteNode envelopes, not ordinary Client gameplay connections;
- no protocol route exists before its protocol ticket;
- missing node connectivity never transfers authority to Client;
- registered node objects remain alive for the Router lifetime.

## Failure behavior

Missing/invalid config arguments, malformed JSON, missing/unknown fields, invalid ports, zero reconnect delay, empty names/addresses, or duplicate node names fail with `spk::Exception`.

Remote-node connection failure is non-fatal and logs Warning. Unrouted Client messages retain Sparkle NodeRouter's strict exception behavior.

## Exact test fixtures

- temporary JSON files;
- loopback `127.0.0.1`;
- Sparkle port 0 for ephemeral listener ports;
- reconnect fixture obtains an ephemeral terrain port, stops that Endpoint, starts Router against the unavailable port using a short test-only delay, verifies Warning, restarts the Endpoint on that port, and waits with a finite deadline for Connected;
- no production Chunk message type is invented;
- EreliaServerSmoke uses the terminating `--help` path; networking lifecycle is tested directly in EreliaServerTestSuite.

## Acceptance tests

- valid router/node configs parse;
- malformed/unknown/invalid fields reject;
- terrain Endpoint starts/stops/restarts;
- Router starts/stops/restarts and reports actual bound port;
- Router connects a real RemoteNode to terrain Endpoint;
- missing terrain Endpoint leaves Router running, logs Warning and later reconnects;
- node-local tests run in EreliaServerTestSuite;
- CLI/help smoke terminates successfully;
- automatic CMake/node discovery and generator/launcher structure exist.

## Decisions

- [DR-016](../../../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)
- [DR-021](../../../DECISIONS/DR-021-REMOTE-SERVER-NODES-FROM-FIRST-IMPLEMENTATION.md)
- [OQ-040](../../../OPEN_QUESTIONS/OQ-040-EP001-SERVER-NODE-ROUTER.md)

No material ST-001-07 observable behavior remains open.

## Completion evidence

Project-owner review completed on 25 September 2026. PR #14 contains the approved implementation. CI run #325 passed every Linux/Windows Debug/Release build-and-test job; its sole failure was clang-format, which was corrected afterward. Final PR CI covers the formatting correction plus the added SIGINT/SIGTERM node-application shutdown tests before merge.
