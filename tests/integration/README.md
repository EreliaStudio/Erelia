# Erelia Integration Tests

Cross-system fixtures link Client, Server, Core and terrain-node libraries and use real Sparkle network transport. Build requires `BUILD_TESTING=ON`, `ERELIA_BUILD_CLIENT=ON`, and `ERELIA_BUILD_SERVER=ON`. CTest labels these fixtures `integration`; dedicated Windows Debug/Release CI jobs run them.

## Collection acquisition

`client_server_chunk_request_test.cpp` exercises:

`Client Collection -> Service Sparkle Client -> NodeRouter -> RemoteNode -> terrain Endpoint -> generic Collection provider/handler -> Response -> ClientNetworkManager -> RequestingProvider -> Client Collection`.

It checks canonical multi-Chunk content (including negative and empty underground Chunks), Available reuse, simultaneous Column/Chunk acquisition with independent RequestID 1, the Player Behaviour Column -> Chunk chain and unloading, disconnect/Pending failure with cache survival and reconnect, and stale routed Responses after Pending removal. Client Collections own batching and correlation; the fixture pumps each unique runtime dispatcher instead of sending raw protocol requests.

Malformed/duplicate request diagnostics, one Response per valid Request, and two-Client reply-envelope correlation remain real routed Server/Terrain boundary tests in `server/nodes/terrain/tests/collection_handler_test.cpp`. Core covers refusal memory, generic offset parsing, Error non-settlement and Update races.

## Connection and process boundaries

`client_server_connection_test.cpp` retains the ST-001-10 ConnectionManager integration through the Router transport.

`EreliaClientServerProcessSmoke` launches the actual terrain node, central Server and Client with explicit temporary configurations and dynamically allocated ports. It waits for the live Client connection and successful Chunk acquisition markers, verifies Client liveness, then terminates the Server and verifies the Client exits with failure. The test does not replace canonical library-level content assertions or golden-image regression checks.

Windows CI installs the existing approved Mesa software renderer beside both the integration executable and the actual Client executable so the process smoke can create its OpenGL context. No image references are regenerated.
