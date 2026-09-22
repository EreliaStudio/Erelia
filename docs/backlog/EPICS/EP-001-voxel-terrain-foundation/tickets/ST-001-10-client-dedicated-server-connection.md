# ST-001-10 — Client dedicated-Server connection

**Status:** Draft
**Epic:** EP-001
**Production target(s):** Client
**Test suite(s):** EreliaClientTestSuite; cross-process integration fixture

## Intent

Replace the Client smoke-only runtime with the minimal Sparkle Client connection lifecycle required to reach the dedicated EP-001 Server.

## User / system value

Terrain retrieval can exercise the real process/network boundary required from the first playable.

## Starting state / prerequisites

- Depends on ST-001-07 for a real Server endpoint.
- Current Client executable only returns smoke status.
- DR-003 and DR-016 require a separate Client using `spk::Client`.
- Exact endpoint configuration, connection startup/shutdown, unavailable-Server behavior, and reconnect policy are not yet specified.

## Product ownership

Client owns connection lifecycle/presentation-side connectivity state. Server remains authoritative.

## Allowed dependencies

EreliaClientLibrary, EreliaCore, Sparkle Version-0.1.3 Client/network APIs, standard library.

## Forbidden dependencies

In-process Server authority, raw socket wrappers, extra networking libraries, Client-side authoritative terrain generation.

## Owned behavior

The eventual ticket establishes a Client connection to the configured dedicated Server and exposes enough connection state for later Chunk request coordination.

## Explicitly not owned

Chunk protocol payloads, cache policy, automatic reconnect unless explicitly approved, terrain generation, meshing/rendering, production session/account identity.

## Public contract

Draft: exact endpoint source (hard-coded development value vs startup config), connection state model, startup timeout/failure presentation, reconnect/retry behavior, and shutdown semantics are not specified.

## Invariants

- Client and Server remain separate processes.
- Client uses Sparkle `spk::Client`.
- Loss of Server connectivity never grants local authority.

## State transitions

Expected states include disconnected -> connecting -> connected -> disconnected/failure, but exact observable state/event API is Draft.

## Failure behavior

Draft until unavailable Server, refused connection, connection loss, and shutdown behavior are explicit.

## Determinism / ordering

Not applicable beyond ordered connection-state transitions.

## Lifecycle / ownership

Client owns the network connection object and must shut it down before dependent runtime resources disappear. Exact ownership API remains Draft.

## Serialization / persistence

Not owned.

## Networking / authority

This ticket establishes transport only. Server remains canonical and Client does not synthesize successful authoritative results while disconnected.

## Implementation constraints

- Use Sparkle Version-0.1.3 networking.
- No hidden in-process Server shortcut.
- Keep production movement/session scope out of EP-001.

## Exact test fixtures

Need an explicitly approved loopback endpoint/configuration and lifecycle fixture before Ready.

## Acceptance tests

### Nominal

Separate Client connects to separately running Server at the approved endpoint.

### Boundaries

Repeated clean start/stop if the chosen lifecycle supports it.

### Invalid / rejected operations

Unavailable/refused endpoint behavior per final contract.

### Failure atomicity

Failed connection leaves Client in a well-defined non-connected state.

### Determinism

Not applicable.

### Lifecycle / ownership

Clean shutdown and connection object destruction.

### Serialization / persistence

Not applicable.

### Retry / idempotency

Reconnect/retry is Draft; do not invent.

### Concurrency / cancellation

Connection shutdown/cancel behavior must match the final lifecycle contract.

### Authority / trust boundary

Disconnected Client cannot replace Server terrain with local canonical state.

### Dependency failure

Server unavailable / network connection failure.

### Cross-system integration

Real separate-process connection fixture required.

### Performance

No timing budget.

### Client-visible / golden-image validation

Not applicable.

## Decisions / unresolved questions

- [DR-003](../../../DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md)
- [DR-016](../../../DECISIONS/DR-016-SPARKLE-NETWORK-NODE-ROUTER.md)

Specification still needed before Ready: EP-001 endpoint/configuration, connection-state lifecycle, unavailable-Server behavior, and retry/reconnect policy.

## Completion evidence

Ready/Done evidence must include a real separate-process connection test using the approved deterministic local endpoint fixture.
