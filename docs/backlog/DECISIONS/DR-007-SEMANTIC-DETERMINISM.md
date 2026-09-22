# DR-007 — Require semantic rather than bit-for-bit runtime determinism

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Core algorithms, Server simulation, procedural generation, Windows/Linux behavior

## Context

Erelia runs across platforms and uses procedural generation and authoritative simulation. Requiring every runtime floating-point operation to produce bit-identical state across platforms would substantially constrain implementation.

## Question

Must runtime simulation be bit-for-bit deterministic across supported platforms?

## Decision

No. Require **semantic determinism** where determinism matters.

- Seeded procedural generation must reproduce the same intended generated content for the same approved inputs/algorithm version.
- PRNG and seed-derivation behavior used for reproducible content must be deliberately specified.
- Authoritative Server execution determines runtime truth.
- Controlled gameplay logic should produce equivalent observable outcomes for equivalent inputs.
- General Client/Server floating-point simulation does not need to remain bit-identical across Windows and Linux when authority/reconciliation already defines truth.

## Consequences

- Deterministic subsystems must define their observable deterministic contract.
- Tests should assert semantic/generated outputs rather than raw floating-point bit identity unless bit identity is specifically required by that subsystem.
- Versioning/migration may be required before changing procedural algorithms whose generated outputs are expected to remain stable.

## Required tests

- exact seeded fixtures for deterministic generators;
- repeated generation produces the same approved semantic output;
- cross-platform CI verifies deterministic fixtures where supported;
- runtime authority tests verify that minor Client numerical divergence cannot become authoritative.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-019.

## Supersession

None.
