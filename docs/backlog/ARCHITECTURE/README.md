# Architecture Documents

Architecture documents are created only for durable cross-cutting rules that have been explicitly approved.

Examples of appropriate topics:

- product/module boundaries;
- identity conventions;
- authority/trust model;
- coordinate/time models;
- ownership and lifetime;
- serialization/persistence;
- networking semantics independent of transport;
- resource/content ownership;
- test strategy.

Each architecture document should explain the invariant, why it exists, what it allows, what it forbids, where the boundary is, how it is enforced/tested, and which decisions establish it.

Do not create an architecture document merely because one implementation ticket is complicated.

## Approved architecture documents

- [ARCH-001 — Product boundaries and authority model](ARCH-001-PRODUCT-BOUNDARIES.md)
- [ARCH-002 — Client/Server authoritative protocol semantics](ARCH-002-AUTHORITATIVE-PROTOCOL.md)
- [ARCH-003 — Time, determinism, and authoritative ordering](ARCH-003-TIME-DETERMINISM-ORDERING.md)
- [ARCH-004 — Server node-routing topology](ARCH-004-SERVER-NODE-ROUTING.md)
