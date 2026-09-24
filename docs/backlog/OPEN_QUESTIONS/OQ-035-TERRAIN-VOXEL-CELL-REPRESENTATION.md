# OQ-035 — What is the first terrain voxel / cell representation?

**Status:** Partially resolved
**Decision records:** [DR-012](../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md), [DR-017](../DECISIONS/DR-017-VOXEL-VOLUME-MESSAGE-SERIALIZATION.md)
**Affected areas:** EP-001, Core voxel data, networking

## Question

What is the first terrain voxel / cell representation?

## Problem / context

EP-001 needs a compact shared cell value and a reusable owning Volume that both Server and Client understand. The archived prototype is a useful reference but must not be copied blindly.

## Known constraints

- `Voxel::Cell` should fit in one 32-bit value.
- ID 0 denotes empty.
- `Voxel::Volume` owns contiguous Cell storage plus dimensions and voxel size.
- Volume serialization uses friend `spk::Message` operators.

## Possible solutions

1. Keep the archived-style 32-bit packed Cell and generic owning Volume, while re-deciding questionable details.
2. Store Shape/material directly in every terrain cell.
3. Use a terrain-specific Chunk array without a generic Volume abstraction.

## Remaining ambiguity

Canonical empty representation, exact contiguous storage/index order, Editor/versioning behavior and any remaining validation details still need explicit decisions.

## Chosen solution

Use a 32-bit packed `Voxel::Cell` carrying Definition ID + Orientation + Flip, and a generic `Voxel::Volume` owning dimensions, voxel size and contiguous cells. `Voxel::Volume` declares friend `spk::Message` insertion/extraction operators for direct `message << volume` / `message >> volume` use. Remaining representation details are still open.
