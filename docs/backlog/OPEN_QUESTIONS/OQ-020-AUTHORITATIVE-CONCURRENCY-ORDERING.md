# OQ-020 — How are concurrent authoritative operations ordered?

**Status:** Resolved
**Decision records:** [DR-008](../DECISIONS/DR-008-AUTHORITATIVE-OPERATION-ORDERING.md)
**Affected areas:** Concurrency, shared resources, economy

## Question

How are concurrent authoritative operations ordered?

## Problem / context

Multiple Clients can submit apparently valid requests against the same authoritative state nearly simultaneously. Mutually exclusive operations cannot both commit.

## Known constraints

- Server is authoritative.
- Later requests are validated against the state produced by already accepted operations.

## Possible solutions

1. Last Client packet wins.
2. Serialize conflicting mutations through the owning Server authority.
3. Allow optimistic parallel commits and repair conflicts afterward.

## Chosen solution

Conflicting authoritative mutations are serialized by the Server/owning authority. A later request may be rejected because an earlier accepted operation changed the state.
