# Epics

An Epic represents a coherent system capability with explicit GDD requirements, architecture boundaries, implementation ownership, integration scenarios, and exit criteria.

An Epic must not claim a capability that has no implementation/test owner.

## Current state

- [EP-001 — Voxel Terrain Delivery and Visual Validation](EP-001-voxel-terrain-foundation/EP-001-voxel-terrain-foundation.md) — Draft

EP-001 is the first approved implementation direction. Its detailed tickets remain gated by voxel/network/visual contracts.

## Future layout

When an Epic is approved, use a folder such as:

EP-XXX-short-name/
- EP-XXX-short-name.md
- tickets/
- optional supporting diagrams or fixtures only when needed

Implementation tickets live directly under each Epic's `tickets/` folder using `ST-XXX-YY-<implementation-goal>.md`.

There is no mandatory Story layer. Around 50 tickets is a deliberate Epic split-review threshold; see DR-010.
