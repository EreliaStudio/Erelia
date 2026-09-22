# OQ-002 — What is Core allowed to own?

**Status:** Resolved
**Decision records:** [DR-001](../DECISIONS/DR-001-PRODUCT-BOUNDARIES.md)
**Affected areas:** Core ownership, shared domain/data/tooling

## Question

What is Core allowed to own?

## Problem / context

A shared library can easily become either too small to be useful or too broad and start owning application behavior. The project needs a durable rule for what belongs in Core versus Server or Client.

## Known constraints

- Core is used by both Server and Client.
- Server remains the authority.
- Generic reusable code should not be duplicated.

## Possible solutions

1. Core contains only pure mathematical/domain primitives.
2. Core contains reusable types, algorithms, protocol/data descriptions, serialization and non-authoritative helpers shared by Client and Server.
3. Core becomes a general application layer that can also make authoritative decisions.

## Chosen solution

Core contains functionality genuinely reusable by both Server and Client: shared types, algorithms, protocol/data representations, serialization and non-authoritative helpers. It does not own authoritative gameplay decisions.
