# OQ-022 — What is the authoritative gameplay collision representation for articulated voxel entities?

**Status:** Open
**Decision records:** None.
**Affected areas:** Collision, Heroes, enemies, props

## Question

What is the authoritative gameplay collision representation for articulated voxel entities?

## Problem / context

The GDD explicitly separates visual voxel parts from authoritative gameplay collision. Characters can be articulated voxel assemblies, but rendered geometry should not automatically become collision/damage geometry.

## Known constraints

- Visual animation/mesh does not determine gameplay results.
- Collision must be suitable for authoritative Server simulation.

## Possible solutions

1. Use simple authored primitives such as capsules/boxes.
2. Use dedicated authored voxel/shape collision volumes.
3. Use a hybrid: simple movement collider plus additional authored hit/interaction volumes.

## Chosen solution

None yet. Collision primitives, ownership and authoring rules remain open.
