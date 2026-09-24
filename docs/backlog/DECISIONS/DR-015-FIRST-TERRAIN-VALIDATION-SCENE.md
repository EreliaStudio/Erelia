# DR-015 — First deterministic terrain generator is a visual validation scene

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** EP-001 Server Chunk generation, Client terrain rendering, golden/manual validation

## Context

The first generator must remain simple enough to debug coordinate/network/meshing issues while exercising more geometry than a featureless plane.

## Decision

The first deterministic generator produces an intentionally artificial validation terrain containing:

- a flat terrain baseline;
- vertical wall-like terrain aligned around the world planes/axes **X = 0** and **Z = 0**, so vertical faces and cross-Chunk vertical geometry can be inspected;
- a separated collection of **stairs, slabs, and slopes** placed above the ground, approximately around **Y = 3**, leaving visible space beneath them;
- multiple approved Orientation/Flip combinations so transformed Shapes are visually exercised.

This is a technical validation fixture, not production procedural world generation.

## Consequences

- the generator is designed for deterministic semantic/golden validation rather than natural-looking terrain;
- the scene should exercise horizontal surfaces, vertical surfaces, undersides, transformed Shapes, and Chunk boundaries;
- exact coordinates, Definition IDs/materials, and the complete Orientation/Flip fixture table still need to be fixed before the generator ticket becomes Ready.

## Required tests

Once the exact fixture table is approved:

- exact cell values at all authored fixture coordinates;
- exact emptiness at selected negative-space coordinates;
- generation across positive and negative Chunk coordinates;
- repeat generation equality;
- semantic mesher expectations;
- human-approved golden views covering floor, walls, undersides, stairs, slabs, slopes, orientations, and flips.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-039.

## Supersession

None.
