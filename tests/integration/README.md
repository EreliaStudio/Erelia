# Erelia Integration Tests

This directory owns cross-system tests that exercise multiple Erelia runtime libraries through their real transport/integration boundaries.

The integration suite is built only when:

- `BUILD_TESTING=ON`;
- `ERELIA_BUILD_CLIENT=ON`;
- `ERELIA_BUILD_SERVER=ON`.

`EreliaIntegrationTestSuite` links the Client, Server, Core, and required Server-node libraries. It is registered with the CTest label `integration`. GitHub Actions exposes the suite through dedicated `Integration (Windows, Debug)` and `Integration (Windows, Release)` jobs rather than hiding integration execution inside the Client job.

## Current Chunk request fixture

`client_server_chunk_request_test.cpp` currently exercises:

`network client -> Erelia Router -> RemoteNode -> TerrainNodeApplication -> Chunk::Collection / PrototypeChunkProvider -> response path`.

The nominal fixture requests canonical Chunk coordinate `(1, 0, 1)` and validates returned terrain content against the resolved DR-015 scene:

- baseline Definition 1;
- slope fixture Definition 2 with its expected transform;
- an expected empty Cell.

The suite also covers duplicate-coordinate diagnostics and malformed-request diagnostics.

## Client boundary

The current Erelia Client library does not yet expose the dedicated-Server connection and Chunk request/cache APIs owned by ST-001-10 and ST-001-11. Until those APIs exist, the integration fixture uses Sparkle's network Client at the outer transport edge.

When ST-001-10/ST-001-11 implement the Erelia Client networking path, replace that transport-edge usage with the real Erelia Client API. Keep the same integration-suite location, Server/terrain runtime, network path, canonical Chunk assertions, and CTest `integration` label.

Do not invent an interim Erelia Client networking abstraction solely for this test.
