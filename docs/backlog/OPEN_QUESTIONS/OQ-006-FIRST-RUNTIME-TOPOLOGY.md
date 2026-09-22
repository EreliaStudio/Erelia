# OQ-006 — What is the first authoritative runtime topology?

**Status:** Resolved
**Decision records:** [DR-003](../DECISIONS/DR-003-DEDICATED-SERVER-FIRST.md)
**Affected areas:** Runtime topology, local development, networking boundary

## Question

What is the first authoritative runtime topology?

## Problem / context

The GDD permitted a local authoritative host as an implementation stage, but starting in-process could hide the real process/network boundary and later force architectural changes.

## Known constraints

- Final product uses a dedicated authoritative Server.
- The user prefers correct separation from the start.
- Local development still needs convenient simultaneous Server/Client launch.

## Possible solutions

1. Start with an in-process local Server and split later.
2. Start immediately with separate dedicated Server and Client processes.
3. Support both topologies equally from the first milestone.

## Chosen solution

Use a real dedicated Server process from the first playable. Local development may launch Server and Client together, but they remain distinct processes connected through the same network boundary.
