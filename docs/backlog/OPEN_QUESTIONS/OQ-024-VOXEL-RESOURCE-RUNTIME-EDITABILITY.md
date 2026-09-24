# OQ-024 — What is runtime editability of voxel / model resources?

**Status:** Open
**Decision records:** None.
**Affected areas:** Resource ownership, caching, invalidation

## Question

What is runtime editability of voxel / model resources?

## Problem / context

Meshes/caches and versioning depend on whether Definitions, Shapes, Volumes and imported models are immutable resources or can mutate after load.

## Known constraints

- Ordinary gameplay does not modify permanent terrain.
- Runtime/generated Volumes may still need controlled editing during construction or specific systems.

## Possible solutions

1. Treat loaded resource definitions/models as immutable.
2. Allow versioned mutable resources with cache invalidation.
3. Separate immutable definitions/assets from mutable runtime Volume instances.

## Chosen solution

None yet. Resource mutability and invalidation rules remain open.
