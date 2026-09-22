# ARCH-001 — Product boundaries and authority model

**Status:** Approved
**Applies to:** Core, Server, Client, all authoritative gameplay systems
**Established by decisions:** ../DECISIONS/DR-001-PRODUCT-BOUNDARIES.md, ../DECISIONS/DR-002-SPARKLE-CORE-DEPENDENCY.md, ../DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md

## Invariant

Erelia uses three deliberate long-term product boundaries:

1. **Core** — shared reusable library for code and representations used by both Server and Client.
2. **Server** — separate dedicated process that owns all authoritative gameplay decisions and authoritative state transitions.
3. **Client** — separate player-facing process that owns input and presentation and executes/presents Server-authoritative results.

Core is shared implementation infrastructure, not a third authority domain.

## Why it exists

The final game requires a dedicated multiplayer Server and a Client while many data types, algorithms, and representations are useful on both sides. A common library avoids duplication, while explicit Server authority prevents trust and ownership from becoming ambiguous.

## Allows

- Server -> Core dependencies.
- Client -> Core dependencies.
- Core -> Sparkle Core dependencies suitable for shared/headless use.
- Shared request/result/event/data representations in Core.
- Shared generic algorithms in Core.
- Client-side prediction/speculation for responsiveness when a system explicitly defines reconciliation.
- Server-side use of the same Core helpers used by Client.

## Forbids

- Core -> Server dependencies.
- Core -> Client dependencies.
- Server -> Client dependencies.
- Client-authoritative persistent/shared gameplay outcomes.
- Core committing authoritative state changes independently of Server orchestration.
- Using Core calls as a hidden in-process path that bypasses the Client/Server boundary.
- Presentation/graphics-only dependencies leaking into Core when they prevent headless Server consumption.

## Boundary

The authority rule applies to every shared-world or player-owned state transition whose result matters beyond local presentation, including inventory, roster/loadout, crafting, economy, world progression, resources, encounters, and movement authority.

Purely local presentation state is Client-owned and does not require Server authority.

Core may contain deterministic/shared calculations, validation helpers, data structures, contracts, and algorithms. Whether a callable function exists in Core does not grant the caller authority to commit its result.

## Ownership and dependencies

### Core

Owns shared reusable foundations and representations.

Typical examples:

- math-facing game types;
- identifiers and value types used by both sides;
- request/result/event structures;
- serialization helpers shared by Client and Server;
- generic algorithms;
- deterministic calculations useful to both sides;
- data definitions shared across the process boundary.

### Server

Owns:

- authoritative validation;
- authoritative state mutation;
- persistence;
- transactions;
- ordering of concurrent authoritative operations;
- world/shared progression;
- authoritative simulation outcomes;
- trust-boundary enforcement.

### Client

Owns:

- input collection;
- rendering;
- audio;
- UI;
- presentation;
- local interpolation;
- explicitly approved prediction/speculation;
- reconciliation and correction to Server state.

## Failure / invalid-state rules

- A Client prediction that disagrees with Server state must resolve in favor of Server state.
- Invalid or unauthorized state-changing requests must not mutate authoritative state.
- Core helpers must not silently perform Server-only side effects.
- Failure to contact Server must be surfaced as a Client connectivity/runtime state rather than replaced by local authority.

## Enforcement

- CMake target dependencies must preserve the permitted dependency direction.
- New cross-boundary features must state which side owns the decision and which shared Core contracts they use.
- Networking semantics must be specified independently of transport before implementation tickets are Ready.
- CI must retain headless Core/Server coverage.

## Tests

- Build/dependency tests for target direction.
- Headless Core/Server CI.
- Server-authority integration tests per state-changing feature.
- Shared contract serialization tests.
- Prediction/reconciliation tests for every feature that permits Client speculation.
- Cross-process integration tests for the dedicated Server topology.

## Affected backlog

All future Epics and implementation tickets that involve shared data or authoritative gameplay.
