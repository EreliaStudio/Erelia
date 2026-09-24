# OD-023 — Whether Cell orientation and flip must be expanded

## Status

Resolved

## Problem

The project requires an explicit choice for whether cell orientation and flip must be expanded. Leaving it implicit would force an implementation ticket to invent design, balance, content, or technology policy.

## Affected epics/tickets

[EP-001](../epics/EP-001-unified-voxel-volume/EP-001-unified-voxel-volume.md), [EP-029](../epics/EP-029-content-contracts/EP-029-content-contracts.md), [EP-030](../epics/EP-030-external-voxel-import/EP-030-external-voxel-import.md), [EP-031](../epics/EP-031-articulated-voxel-characters/EP-031-articulated-voxel-characters.md). Each affected ticket links back to this record.

## Known constraints

- Do not contradict the current GDD or the fixed contracts stated in affected tickets.
- Work independent of this choice may proceed behind configuration or an interface.
- No arbitrary value or technology is selected merely to unblock implementation.

## Options considered

- Preserve the current four horizontal orientations plus vertical flip and their packed bit layout.
- Expand the representation now without a demonstrated asset or transform requirement.

## Decision

Preserve the existing `Voxel::Cell` orientation and vertical flip representation byte-for-byte. Do not widen or redesign it during EP-001. If authored content later proves the representation insufficient, open a separate evidence-backed decision before changing the packed contract.

## Rationale

The current representation supports the existing Chunk renderer, Shapes, slopes, stairs, and approved regression fixtures. No current VoxelVolume requirement demonstrates that more orientations or flips are needed, while changing the packed layout would create avoidable compatibility and regression risk.

## Consequences

- EP-001 reuses `Voxel::Cell` without changing its size, masks, packing, or round-trip behavior.
- EP-029/EP-030/EP-031 must not invent additional orientation states while consuming the current contract.
- A future expansion requires its own Open Decision, compatibility plan, and regression evidence.

## Validation / evidence

- `tests/voxel_cell_tests.cpp` proves the current `0xE000002A` ID/orientation/flip round trip.
- The 20 approved current-Chunk golden fixtures cover current orientation and vertical-flip rendering.

## Resolution provenance

Resolved from the project owner's explicit ST-001-02 instruction on 19 September 2026 to preserve the current packed representation unless a real requirement demonstrates otherwise.
