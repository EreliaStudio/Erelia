# OQ-011 — Which concepts need runtime-only identifiers?

**Status:** Open
**Decision records:** None.
**Affected areas:** Runtime identity, encounters, entities, requests

## Question

Which concepts need runtime-only identifiers?

## Problem / context

Transient entities such as encounters, temporary combat entities, dungeon instances, status instances and requests need correlation while alive, but persistence may be unnecessary.

## Known constraints

- Runtime IDs must not be confused with persistent identity.
- Scope affects lookup cost, stale-reference detection and network payloads.

## Possible solutions

1. Use Server-global runtime IDs.
2. Use IDs scoped to an owning aggregate such as Encounter/World/connection.
3. Use a mixed scheme depending on whether cross-owner references are required.

## Chosen solution

None yet. Runtime ID categories, scopes and reuse rules remain open.
