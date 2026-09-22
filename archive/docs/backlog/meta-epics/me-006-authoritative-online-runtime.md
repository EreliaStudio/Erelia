# ME-006 — Authoritative Online Runtime

## Goal

Provide authoritative durable state first locally/headlessly and then through remote networking/replication.

## Children

- [EP-024 — Authoritative Domain Server and Persistence](../epics/EP-024-server-authority-persistence/EP-024-server-authority-persistence.md)
- [EP-028 — Networking, Replication, Interest Management, and Client Reconciliation](../epics/EP-028-network-replication/EP-028-network-replication.md)

## Capability-level Done

- All child epics required by the active milestone satisfy their integration catalogues.
- Cross-epic invariants are covered by executable system tests.
- No child capability makes presentation or editor state authoritative gameplay truth.
- Open design values remain explicit decision gates rather than accidental constants.
