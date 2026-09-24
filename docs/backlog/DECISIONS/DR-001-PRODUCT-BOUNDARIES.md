# DR-001 — Long-term Core / Server / Client boundaries

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Core, Server, Client, shared gameplay contracts, authority

## Context

The repository already contains Core, Server, and Client targets. The project needs a dedicated authoritative server program and a separate client program, while avoiding duplicated code for concepts that both sides need.

The unresolved questions were whether these targets were merely bootstrap folders, what Core should own, what Server should own, and what Client should own.

## Already-fixed constraints

- The final product requires a server program that hosts authoritative operations.
- The client connects to that server.
- Server authority is a GDD requirement for persistent multiplayer state and gameplay outcomes.
- Shared code should not be duplicated between Server and Client.

## Question

Should Core, Server, and Client remain long-term architectural boundaries, and what is each boundary responsible for?

## Options

### Option A — Treat the current split as temporary

Allow the boundaries to change substantially as implementation grows, including duplicating or relocating shared behavior as needed.

### Option B — Keep three deliberate long-term boundaries

Keep Core as the common reusable library, Server as the authoritative decision maker, and Client as the player-facing execution/presentation application.

## Decision

Use **Option B**.

Core, Server, and Client are deliberate long-term product boundaries.

- **Core** contains code intended to be shared by Server and Client, especially generic reusable types, utilities, algorithms, data representations, protocol/domain descriptions, and non-authoritative helpers. Generic functionality that both sides need should live in Core rather than be duplicated.
- **Server** owns authoritative decisions and authoritative state transitions. Gameplay outcomes that matter to the shared world are decided by Server.
- **Client** owns input, rendering, audio, UI, presentation, local responsiveness, and execution/presentation of authoritative results received from Server.
- Client may run explicitly speculative or predictive logic for responsiveness, but speculative state is never authoritative and must yield to Server results.
- Core is not an authority layer. It may describe requests, results, reactions, shared data, and reusable calculations, but it must not become an alternate source of authoritative game decisions.

The guiding rule is: **Server decides; Client presents/executes; Core provides shared tools and shared representations used by both.**

## Consequences

Required:

- Server and Client may both depend on Core.
- Core must not depend on Server or Client.
- Authoritative state-changing rules are entered through Server-owned orchestration.
- Client-side predicted/speculative results must be replaceable or reconcilable with Server state.
- Shared request/result/event/data contracts should live in Core when both sides require them.
- Shared algorithms may live in Core when they do not confer authority merely by being callable.

Forbidden:

- Client becoming authoritative for persistent/shared gameplay outcomes.
- Core independently committing authoritative decisions.
- Duplicating common Server/Client code when a reusable Core representation is appropriate.
- Server implementation depending on Client presentation code.

## Required tests

- Dependency/build tests proving Core is independently consumable by both Server and Client.
- Architectural dependency checks preventing Core -> Server/Client and Server -> Client dependencies.
- Server-authority integration tests for state-changing commands.
- Client reconciliation tests for any system that introduces prediction/speculation.
- Contract serialization/round-trip tests when shared request/result/event representations are introduced.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-001, Q-002, Q-004, and Q-005.

## Supersession

None.
