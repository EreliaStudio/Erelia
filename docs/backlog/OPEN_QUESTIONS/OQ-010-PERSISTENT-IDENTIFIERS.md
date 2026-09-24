# OQ-010 — Which concepts need stable persistent identifiers?

**Status:** Open
**Decision records:** None.
**Affected areas:** Identity, persistence, serialization, references

## Question

Which concepts need stable persistent identifiers?

## Problem / context

Persistent entities such as accounts, Heroes, crafted items, Worlds, buildings or resource sites may be referenced across restarts and serialized data. Their identity scope must be known before durable schemas are finalized.

## Known constraints

- Identifiers that survive restarts must remain stable under serialization/deserialization.
- Not every runtime object necessarily needs a globally unique persistent identity.

## Possible solutions

1. Give every persistent concept a globally unique identifier.
2. Scope identifiers by owning aggregate such as World/account plus a local ID.
3. Use a mixed policy: global IDs only where cross-aggregate references require them, scoped IDs elsewhere.

## Chosen solution

None yet. The persistent concepts that need IDs, their scopes and generation rules remain open.
