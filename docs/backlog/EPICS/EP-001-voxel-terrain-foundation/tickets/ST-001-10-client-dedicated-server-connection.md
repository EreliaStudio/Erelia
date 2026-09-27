# ST-001-10 — Client dedicated-Server connection

**Status:** In Progress
**Epic:** EP-001
**Production target(s):** Client
**Test suite(s):** EreliaClientTestSuite; EreliaIntegrationTestSuite; executable process smoke

## Intent

Replace the Client smoke-only runtime with the first graphical Sparkle application shell and the Client connection lifecycle required to reach the dedicated EP-001 Server.

## User / system value

Terrain retrieval can exercise the real process/network boundary required from the first playable.

## Starting state / prerequisites

- ST-001-07 provides the real Client-facing Server Router endpoint.
- ST-001-09 provides routed Chunk request handling and cross-system integration, with raw `spk::Client` still used at the outer Client edge.
- DR-003 and DR-016 require a separate Client process using Sparkle networking.
- Sparkle Version-0.1.3 `spk::Client::connect()` is synchronous and throws on connection failure, `disconnect()` is idempotent, remote loss clears the connected state, and the same Client object may connect again.
- Sparkle Version-0.1.3 `spk::WorkerPool::submit(...)` converts a thrown operation into a failed `spk::Task<TResult>::Answer`; Task Answers expose `status()`, `wait()`, `get()`, and completion subscription.

## Product ownership

Client owns connection lifecycle/presentation-side connectivity state. Server remains authoritative.

## Allowed dependencies

EreliaClientLibrary, EreliaCore, Sparkle Version-0.1.3 Client/network/Task/WorkerPool APIs, standard library.

## Forbidden dependencies

In-process Server authority, raw socket wrappers, extra networking libraries, Client-side authoritative terrain generation.

## Owned behavior

This ticket establishes:
- explicit Client endpoint configuration;
- a graphical `spk::Application` with an initial `640x480` `Erelia` window;
- a `MainApplicationWidget` containing a reusable Logger-backed `Console` and a `ConnectionManager` widget;
- asynchronous scheduling of the synchronous Sparkle connection attempt on the shared WorkerPool;
- observable connection-attempt Task state and live connected/disconnected state;
- automatic connection cycles of at most three attempts, with 15 seconds between failed attempts;
- automatic retry stop after the third failed attempt while the graphical Client remains running;
- `/connect` Console command starting a fresh three-attempt connection cycle;
- Client executable startup, connected idle lifetime, signal shutdown, initial-failure handling, and unexpected remote-disconnect handling;
- deterministic component, integration, and separate-process connection coverage.

## Explicitly not owned

Chunk request/cache coordination, protocol payload forwarding APIs, cache policy, terrain generation, terrain rendering, production session/account identity.

## Public contract

### Endpoint configuration

The Client JSON configuration is:

```json
{
  "server config": {
    "address": "127.0.0.1",
    "port": 2550
  }
}
```

Contract:
- `server config.address` is required and must be non-empty;
- `server config.port` is required and must be non-zero;
- unknown JSON fields are rejected;
- there is no compiled-in production configuration path and no hidden address/port default;
- the executable accepts `--config <path>`, `--config=<path>`, and `-c <path>`;
- missing or invalid configuration is an error and the executable returns `EXIT_FAILURE`;
- `--help` prints help and returns `EXIT_SUCCESS`;
- `tools/run-client-server.ps1` generates an explicit Client runtime configuration using `127.0.0.1` and the dynamically allocated Router port.

### Connection attempt

The Client runtime owns one `spk::Client`. It does not introduce a separate connection RAII abstraction.

A connection request submits the blocking `spk::Client::connect(address, port)` call to `spk::WorkerPool` and returns/stores a Task Answer for that attempt.

The Task represents only the connection attempt:
- `Pending`: the connect operation is executing;
- `Completed`: `spk::Client::connect()` returned successfully;
- `Failed`: `spk::Client::connect()` threw, and `Answer::get()` rethrows that failure.

Task completion does not represent the lifetime of the established connection. After a successful attempt, `spk::Client::isConnected()` is the source of truth for the live transport state.

No additional Client connection-state enum is introduced.

### Repeated operations and reconnect

- A connection request while already connected is an Erelia-level no-op and must not tear down/reconnect the underlying Sparkle Client.
- A connection request while a previous attempt is still `Pending` returns/reuses that outstanding attempt rather than submitting concurrent `connect()` calls against the same `spk::Client`.
- Initial connection failure leaves the Client disconnected.
- A failed attempt schedules the next attempt after 15 seconds until three attempts have been made.
- Remote disconnection leaves the Client disconnected.
- A remote disconnection starts a fresh connection cycle.
- The same Client runtime may explicitly submit a new connection attempt after a failed attempt or later disconnection.
- Disconnect while already disconnected is a no-op.
- A deliberate reconnect is therefore `disconnect()` followed by a new connection request.

### Shutdown while connecting

Sparkle Version-0.1.3 WorkerPool Tasks are not cancellable.

If shutdown is requested while a connection attempt is `Pending`, the Client waits for that Task to become terminal. It then disconnects if the attempt established a connection and completes shutdown. Erelia does not destroy the `spk::Client` while a WorkerPool task may still be executing `connect()` on it.

No Erelia-specific TCP connection timeout is added in this ticket.

## Invariants

- Client and Server remain separate processes.
- Client uses Sparkle `spk::Client`.
- The synchronous Sparkle connection operation never blocks the Client's main/runtime thread; it executes as WorkerPool work.
- At most one connection Task may operate on the owned `spk::Client` at a time.
- Loss of Server connectivity never grants local authority.
- Connection-attempt Task state and live transport state are distinct concepts.

## State transitions

Observable connection-attempt/liveness transitions are:

- disconnected + no pending attempt -> submit -> Task `Pending`;
- Task `Pending` -> `Completed` + `isConnected() == true` on success;
- Task `Pending` -> `Failed` + `isConnected() == false` on failure;
- connected -> explicit disconnect -> disconnected;
- connected -> remote loss -> disconnected;
- failed/disconnected -> explicit new connection request -> new Task `Pending`.

After a failed attempt, the manager transitions back to connecting after 15 seconds while fewer than three attempts have run. After attempt three it remains stopped until `/connect` starts a fresh cycle.

## Failure behavior

- Invalid/missing configuration throws/fails startup before connection is attempted.
- Refused/unavailable Server causes the connection Task to fail and leaves `isConnected() == false`.
- After three failed attempts the automatic cycle stops, logs the stopped state, and the graphical Client remains running.
- Unexpected remote Server disconnect starts a fresh connection cycle.
- SIGINT/SIGTERM-requested local shutdown disconnects cleanly and returns `EXIT_SUCCESS`.
- Shutdown requested while the connect Task is pending waits for terminal Task state before destroying/disconnecting the Client.

## Determinism / ordering

For one Client runtime, connection attempts are serialized: a second attempt cannot execute concurrently with an outstanding pending attempt. Local shutdown observes/settles an outstanding attempt before network-object destruction.

## Lifecycle / ownership

The Client runtime owns:
- endpoint configuration;
- one `spk::Client`;
- the current connection-attempt Task Answer when one exists.

The process owns/instantiates the shared `spk::WorkerPool` required to execute the connection Task.

The connection Task never owns the whole connected lifetime. Sparkle's internal Client receive worker owns transport receive activity after `connect()` succeeds.

## Serialization / persistence

Only the JSON endpoint configuration is owned. No gameplay/session persistence is introduced.

## Networking / authority

This ticket establishes transport only. Server remains canonical and Client does not synthesize successful authoritative terrain state while disconnected.

## Implementation constraints

- Use Sparkle Version-0.1.3 networking and WorkerPool/Task APIs.
- No hidden in-process Server shortcut.
- Keep `main.cpp` thin.
- Do not introduce redundant `erelia::client` / `erelia::server` C++ namespaces.
- Boolean-valued expressions follow the project convention of explicit `== true` / `== false`; do not introduce unary `!` for boolean values.
- Do not add Chunk send/receive/cache APIs; ST-001-11 owns those semantics.
- Keep production movement/session scope out of EP-001.

## Exact test fixtures

### Component fixture

Use a deterministic loopback Sparkle listener with port `0` where the listener API supports it. Resolve the assigned port and construct the Client endpoint explicitly.

Cover:
- configuration parsing/validation;
- initial disconnected state;
- successful asynchronous connect Task;
- failed/refused connection Task with disconnected post-state;
- repeated connection call while already connected is a no-op;
- repeated connection call while the first attempt is pending reuses the outstanding Answer;
- clean explicit disconnect and idempotent disconnect;
- explicit reuse/reconnect of the same Client runtime after disconnect;
- remote disconnect updates live connectivity;
- destruction only after pending work has settled.

No arbitrary sleep is permitted when Task `wait()`/`get()`, completion subscription, transport state, or a bounded deadline fixture can prove the condition.

### Cross-system integration fixture

Use the real Erelia Client connection API against the real Erelia Server Router transport boundary.

ST-001-09 Chunk protocol integration remains unchanged where replacing raw `spk::Client` would require ST-001-11-owned send/message coordination. ST-001-10 adds/migrates only fixtures that actually exercise the connection-lifecycle layer.

### Separate-process executable fixture

Start a real `EreliaServer` process with an explicit temporary Router configuration and a real `EreliaClient` process with an explicit temporary Client configuration pointing at that Router.

Prove that the separately running Client reaches the Server endpoint. Process startup/configuration must not use a compiled-in endpoint.

## Acceptance tests

### Nominal

Separate Client connects to separately running Server at the explicit configured endpoint.

### Boundaries

- Repeated explicit connect/disconnect using the same Client runtime.
- Connect while connected is a no-op.
- Connect while an attempt is already pending does not schedule a second concurrent operation.

### Invalid / rejected operations

- Missing/invalid Client config is rejected.
- Empty address is rejected.
- Port zero is rejected.
- Unavailable/refused endpoint settles the connection Task as failed.

### Failure atomicity

Failed connection leaves Client in a well-defined disconnected state and permits a later explicit connection attempt.

### Determinism

Connection attempts against one owned Sparkle Client are serialized.

### Lifecycle / ownership

Clean shutdown, remote disconnect, pending-attempt shutdown ordering, and connection object destruction are covered.

### Serialization / persistence

Strict Client endpoint JSON parsing is covered.

### Retry / idempotency

Automatic retry is bounded to three attempts per cycle with 15 seconds between failed attempts. `/connect` starts a new cycle after automatic attempts stop. Connect-while-connected remains an Erelia-level no-op.

### Concurrency / cancellation

Connection is WorkerPool work. Cancellation is not supported by Sparkle Version-0.1.3; shutdown during a pending attempt waits for task settlement before Client destruction.

### Authority / trust boundary

Disconnected Client cannot replace Server terrain with local canonical state.

### Dependency failure

Server unavailable / network connection failure becomes a failed connection Task and executable startup failure.

### Cross-system integration

Real Erelia Client connection API reaches the real Router transport boundary. Existing routed Chunk tests retain raw Sparkle transport only where the higher-level request/message API belongs to ST-001-11.

### Performance

No timing budget.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-003](../../../DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md)
- [DR-016](../../../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)
- [DR-020](../../../DECISIONS/DR-020-HEADLESS-ASYNC-TASK-INFRASTRUCTURE.md)
- [DR-021](../../../DECISIONS/DR-021-REMOTE-SERVER-NODE-PROCESS-TOPOLOGY.md)

Project-owner decisions approved on 27 September 2026:
- strict explicit JSON endpoint configuration and CLI config path;
- WorkerPool Task for the synchronous Sparkle connection attempt rather than a dedicated connection RAII class;
- Task state represents only attempt progress/result; `spk::Client::isConnected()` represents current liveness;
- graphical `spk::Application` Client with initial `640x480` window;
- bounded automatic connection cycle: three attempts, 15 seconds apart;
- after attempt three fails, stop automatic attempts, log the state, and keep the application alive;
- `/connect` starts a fresh connection cycle;
- Console ordinary text uses Logger `UserValueA`; command-result messages use `UserValueB`;
- serialized/idempotent Erelia connection operations;
- shutdown during a pending attempt waits for task settlement;
- local signal shutdown succeeds, initial-connect failure and unexpected remote disconnect fail the executable;
- real separate-process executable connection evidence remains required.

No unresolved observable ST-001-10 contract question remains. The ticket satisfies the project Definition of Ready.

## Completion evidence

Implementation is complete on `feat/st-001-10-client-dedicated-server-connection` / draft PR #18 and is awaiting project-owner review before this ticket may become Done.

Delivered production behavior:
- `ClientRuntime` owns the explicit endpoint configuration, one `spk::Client`, and the current connection-attempt Task Answer;
- strict JSON endpoint loading rejects missing, unknown, empty-address, zero-port, and invalid-port inputs;
- `ClientRuntime::connect()` submits Sparkle's synchronous `spk::Client::connect()` to the shared WorkerPool, reuses a Pending attempt, and is a no-op while already connected;
- `disconnect()` waits for a Pending attempt before transport teardown and is idempotent when already disconnected;
- the same runtime supports explicit retry/reconnect after failure or disconnect without automatic retry;
- `runClient()` implements explicit-config startup, connected idle lifetime, SIGINT/SIGTERM clean shutdown, initial-connect failure, and unexpected remote-disconnect failure;
- `main.cpp` remains a thin application entry point;
- `tools/run-client-server.ps1` now generates the Client endpoint configuration from the dynamically selected Router port and passes it explicitly.

Delivered tests:
- `EreliaClientTestSuite` covers strict configuration, successful/failed asynchronous connection settlement, live transport state, Pending-attempt reuse, connect/disconnect idempotency, explicit retry/reconnect, remote disconnect, Pending-attempt shutdown ordering, initial application connection failure, and SIGINT/SIGTERM clean application shutdown;
- `EreliaIntegrationTestSuite` adds a real `ClientRuntime -> Router` transport-boundary fixture while preserving the ST-001-09 raw-`spk::Client` Chunk fixtures whose send/message coordination belongs to ST-001-11;
- `EreliaClientServerProcessSmoke` launches real `EreliaServer` and `EreliaClient` executables with temporary explicit configurations, proves the Client reaches the Router endpoint, then verifies unexpected Server loss terminates the Client with failure;
- the process fixture uses a production-valid Router configuration containing the required `terrain` route but does not extend ST-001-10 into terrain request semantics.

Validation:
- CI run `36276188328` passed clang-format, Linux Core/Server Debug+Release, Windows Core/Server Debug+Release, Client Windows Debug+Release, and Integration Windows Debug+Release, including the separate-process smoke, before the final application-signal tests were added;
- latest-head CI validation including the SIGINT/SIGTERM and initial-connect-failure application tests is required before review completion.

Human validation still pending:
- project-owner code review/approval of PR #18;
- only after that approval and green latest-head CI may this ticket be marked **Done**.

ST-001-11 remains the next dependency-ordered implementation area, but it is still **Blocked** on its own Client cache/retry/recycle policy specification.
