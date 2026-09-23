# ST-001-07 — Server NodeRouter terrain-node bootstrap

**Status:** Draft
**Epic:** EP-001
**Production target(s):** Server
**Test suite(s):** EreliaServerTestSuite; EreliaServerSmoke

## Intent

Replace the Server smoke-only entry point with the approved router-first runtime shape: one Client-facing `spk::NodeRouter` and one in-process terrain `spk::LocalNode`.

## User / system value

EP-001 begins on the intended Server ownership topology instead of introducing a temporary monolithic message loop that would immediately need migration.

## Starting state / prerequisites

- Current Server executable only returns the smoke status.
- DR-016 and OQ-040 fix router-first topology.
- No terrain payload handler is required to be implemented by this ticket.
- Listen endpoint/startup/lifecycle behavior is not yet specified enough for Ready acceptance tests.

## Product ownership

Server owns router/node composition and runtime lifecycle.

## Allowed dependencies

EreliaServerLibrary, EreliaCore, Sparkle Version-0.1.3 Core networking, standard library.

## Forbidden dependencies

Client/graphics code, raw socket wrappers, an alternate networking library, a temporary bare-`spk::Server` game loop.

## Owned behavior

- EreliaServer owns/starts a `spk::NodeRouter`.
- One in-process terrain `spk::LocalNode` exists as the owner of the EP-001 Chunk message family.
- Future Chunk message types are routed to that node rather than handled in the executable entry point.

## Explicitly not owned

Chunk request payload semantics, generation, Client connection policy, remote-node deployment, future gameplay node families.

## Public contract

Draft: exact Server endpoint configuration, startup/shutdown API, message-type registration values, and behavior when no terrain handler is yet installed must be fixed before Ready.

## Invariants

- There is exactly one public Client-facing router for EP-001.
- Terrain logic does not accumulate in `main.cpp`.
- Initial terrain node is local/in-process.
- Server remains headless.

## State transitions

Expected runtime: construct/configure -> start/listen -> route messages -> stop/shutdown. Exact states and shutdown trigger are not yet explicit.

## Failure behavior

Draft until bind/listen/startup failure and shutdown behavior are specified.

## Determinism / ordering

Node routing by message type follows Sparkle semantics; no gameplay ordering behavior is owned here.

## Lifecycle / ownership

Server must own router and local-node lifetimes so registered routing cannot outlive its target. Exact construction/destruction ordering must be documented before Ready.

## Serialization / persistence

Not applicable.

## Networking / authority

Server is the Client-facing endpoint. This ticket establishes topology only; it does not accept Client data as authoritative.

## Implementation constraints

- Use `spk::NodeRouter` from the first networked Server runtime.
- Do not add raw WinSock/BSD wrappers.
- Do not introduce `RemoteNode` in EP-001 without a later decision.

## Exact test fixtures

Not fixed yet. Ready fixtures need an explicit local endpoint/configuration and lifecycle trigger that can run in tests without ambiguous port ownership.

## Acceptance tests

### Nominal

Router and terrain LocalNode start under the approved fixture and can register the terrain message family.

### Boundaries

Repeated start/stop and resource cleanup only if supported by the chosen runtime contract.

### Invalid / rejected operations

Bind/listen/configuration failures once exact endpoint semantics are fixed.

### Failure atomicity

Failed startup leaves no partially running Server resources.

### Determinism

Not applicable.

### Lifecycle / ownership

Router/node lifetime and shutdown are observable and leak-free under the final fixture.

### Serialization / persistence

Not applicable.

### Retry / idempotency

Startup retry behavior must be explicit if supported.

### Concurrency / cancellation

Shutdown while idle must be covered; request cancellation belongs to later handler tickets.

### Authority / trust boundary

Unknown/unregistered Client messages must not invoke terrain behavior; exact handling belongs to final router/protocol contract.

### Dependency failure

Network bind/start failure once fixture is fixed.

### Cross-system integration

A later Client connection ticket depends on this runtime.

### Performance

No timing budget.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-016](../../../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)
- [OQ-040](../../../OPEN_QUESTIONS/OQ-040-EP001-SERVER-NODE-ROUTER.md)

Specification still needed before Ready: exact EP-001 Server endpoint configuration and start/stop/failure contract.

## Completion evidence

Server tests and smoke coverage prove the router/local-node runtime shape once endpoint/lifecycle fixtures are explicit.
