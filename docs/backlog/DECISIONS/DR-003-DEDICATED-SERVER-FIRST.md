# DR-003 — Dedicated authoritative server from the first playable

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Server runtime, Client/Server topology, networking boundary, first playable

## Context

The GDD permits a local authoritative host as an implementation stage, but the initial runtime topology was not selected.

The project owner prefers to establish the real production boundary immediately rather than first implementing an in-process host.

## Already-fixed constraints

- Server owns authoritative decisions.
- Client is a separate player-facing program.
- Core is shared by both programs but is not an authority layer.
- The final product requires a dedicated authoritative server process.

## Question

Should the first playable use an in-process/local authoritative host, or start with a real dedicated Server process?

## Options

### Option A — In-process authoritative host first

Run authoritative Server logic locally/in-process while preserving a conceptual command boundary, then split into a dedicated process later.

### Option B — Dedicated Server process from the start

Implement the actual process/network boundary from the first playable architecture.

## Decision

Use **Option B**.

The first playable architecture will use a **real dedicated Server process** and a separate Client process.

The Server/Client separation must therefore be real at runtime, not merely represented by library boundaries inside one process.

## Consequences

- Initial gameplay features that cross authority boundaries must define explicit Client -> Server requests/commands and Server -> Client results/events/snapshots.
- Tests must include actual cross-process or transport-boundary integration coverage where relevant.
- Core remains a shared library but is not used to bypass the Server/Client communication boundary.
- Local development conveniences may launch both executables together, but they must remain separate authoritative/client processes.
- A future private-server experience can reuse the same dedicated Server executable or equivalent authoritative process model rather than requiring an in-process authority design.

## Required tests

- Smoke integration proving Client and Server can start separately and establish the chosen transport connection once networking is implemented.
- Authority tests proving Client requests cannot directly mutate authoritative state.
- Contract compatibility tests for shared messages crossing the process boundary.
- Failure tests for disconnect, rejected request, malformed/invalid request, and unavailable Server as each capability is introduced.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-006.

## Supersession

None.
