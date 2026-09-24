# EP-026 — Marketplace, Gold Economy, Bank Transactions, and PvE Procurement

**Dependencies:** EP-018, EP-024  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Implement a server-wide marketplace and gold ledger, then allow PvE vaults/camps to buy eligible player-crafted listings within a reward budget without minting finished items.

## Starting state

- Contracts supplied by `EP-018, EP-024` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Gold ledger
- Marketplace listing lifecycle
- Reward budget
- PvE procurement

## Out of scope

- Responsibilities explicitly assigned to neighboring epics.
- Final choices tracked by unresolved Open Decisions.
- Unapproved balance values, content, serialization syntax, or technology selections.

## Architecture / behavioral overview

- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Gold ledger | [ST-026-01](tickets/ST-026-01-gold-ledger.md) | [ST-026-01](tickets/ST-026-01-gold-ledger.md) |
| Marketplace listing lifecycle | [ST-026-02](tickets/ST-026-02-marketplace-listing-lifecycle.md) | [ST-026-02](tickets/ST-026-02-marketplace-listing-lifecycle.md) |
| Reward budget | [ST-026-03](tickets/ST-026-03-reward-budget.md) | [ST-026-03](tickets/ST-026-03-reward-budget.md) |
| PvE procurement | [ST-026-04](tickets/ST-026-04-pve-procurement.md) | [ST-026-04](tickets/ST-026-04-pve-procurement.md) |

## Ticket index

- [ST-026-01 — Gold ledger](tickets/ST-026-01-gold-ledger.md) — Track universal currency through auditable authoritative transactions.
- [ST-026-02 — Marketplace listing lifecycle](tickets/ST-026-02-marketplace-listing-lifecycle.md) — Create/cancel/purchase player listings.
- [ST-026-03 — Reward budget](tickets/ST-026-03-reward-budget.md) — Represent funded PvE reward-source budget.
- [ST-026-04 — PvE procurement](tickets/ST-026-04-pve-procurement.md) — Select and buy eligible crafted listings for reward placement.

## Epic integration acceptance tests

1. Gold changes only through authoritative ledger operations.
2. Listing transfers/reserves item without duplicating ownership.
3. Purchase transfers item and gold atomically.
4. Cancelled/expired listing returns ownership correctly.
5. PvE reward source cannot exceed configured budget.
6. Procured reward corresponds to a real eligible listing and seller receives gold.
7. Pricing-abuse policy remains a decision gate but interface supports eligibility/fair-price filter.
8. Concurrent purchase/procurement of same listing resolves at most once.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-009](../../open-decisions/OD-009-marketplace-fair-price-and-outlier-procurement-policy.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
