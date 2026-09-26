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

The suite also covers:

- a normal multi-coordinate request, including a negative coordinate, with every canonical Chunk returned through the same response;
- duplicate-coordinate diagnostics followed by deduplicated processing;
- malformed-request diagnostics with no terminal Chunk response;
- two concurrent Clients issuing distinguishable requests and receiving only their own correlated responses;
- Client disconnect while acquisition is still outstanding, followed by successful service to a new Client.

The disconnect fixture is deterministic: it occupies the shared WorkerPool before sending the request, confirms Terrain has accepted the request through the duplicate-coordinate diagnostic, disconnects the originating Client, and only then releases acquisition work. This verifies the outstanding request does not rely on a timing race.

## Client boundary

ST-001-10 adds the real Erelia `ClientRuntime` connection lifecycle. `client_server_connection_test.cpp` uses that API against the real Router transport boundary and validates live disconnect after Router shutdown.

The ST-001-09 Chunk request fixtures intentionally continue to use Sparkle's network Client at their outer transport edge. Replacing that usage would require send/message/request-cache behavior owned by ST-001-11, so ST-001-10 does not expose a transport-forwarding API merely for test migration.

When ST-001-11 implements the Client request/cache path, migrate the Chunk fixtures to that real Erelia API while preserving their Server/terrain runtime, real network path, canonical Chunk assertions, and CTest `integration` label.

## Process boundary

The library integration suite orchestrates Erelia runtime libraries inside one test process, but Client/Router/Terrain communication still crosses the real Sparkle network transport boundary.

ST-001-10 also registers `EreliaClientServerProcessSmoke` on Windows. That fixture:
- allocates temporary Router/terrain ports and writes explicit temporary Server and Client configurations;
- starts the real `EreliaServer` process with the production-required terrain route configured;
- starts the real `EreliaClient` process against that Router endpoint;
- waits for the Client's successful connection marker using bounded state/deadline polling;
- verifies the Client remains alive while connected;
- terminates the Server and verifies the Client exits with failure after unexpected transport loss.

The terrain endpoint need not be running for this ST-001-10 smoke because the fixture validates the Client-facing Router connection only. Chunk request/response semantics remain the responsibility of the library integration layer rather than being duplicated through process-log inspection.
