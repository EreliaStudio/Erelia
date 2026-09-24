# DR-011 — Voxel and Chunk coordinate conventions

**Status:** Resolved
**Date opened:** 2026-09-22
**Date resolved:** 2026-09-22
**Applies to:** Core voxel data, Server terrain generation, Client terrain requests/rendering

## Context

EP-001 requires one coordinate convention shared by Core, Server, Client, generation, networking, and rendering.

Sparkle's 3D convention uses +Y as up and local -Z as forward. The project owner explicitly approved following that convention directly rather than introducing an Erelia-specific transform convention.

## Decision

- World up is **+Y**.
- Forward follows Sparkle's **-Z** convention.
- Terrain cell coordinates are integer 3D coordinates represented with `spk::Vector3Int`.
- One terrain cell spans exactly **1 world unit** on each axis.
- Terrain Chunks are exactly **16 × 16 × 16** cells.
- Chunk coordinates are themselves integer 3D coordinates and may be represented with `spk::Vector3Int`.
- Global-cell -> Chunk conversion uses mathematical **floor division by 16** independently on X/Y/Z.
- Global-cell -> local-Chunk conversion uses the matching floor modulo and always yields coordinates in **[0, 15]** on each axis.
- The convention must work identically for positive and negative world coordinates.

Examples:

| Global cell | Chunk coordinate | Local coordinate |
| --- | --- | --- |
| (0,0,0) | (0,0,0) | (0,0,0) |
| (15,15,15) | (0,0,0) | (15,15,15) |
| (16,16,16) | (1,1,1) | (0,0,0) |
| (-1,-1,-1) | (-1,-1,-1) | (15,15,15) |
| (-16,-16,-16) | (-1,-1,-1) | (0,0,0) |
| (-17,-17,-17) | (-2,-2,-2) | (15,15,15) |

## Consequences

- Server generation, Chunk addressing, Client requests, and rendering must use the same coordinate conversion semantics.
- Negative positions must never rely on C++ truncating integer division directly.
- No separate Erelia axis-remapping layer is introduced between voxel world coordinates and Sparkle rendering coordinates.
- The archived project's floor-division/floor-modulo behavior is a useful implementation reference, but its old `Chunk::Coordinate` wrapper is not automatically restored because the new design explicitly permits `spk::Vector3Int` Chunk coordinates.

## Required tests

- exact conversion fixtures above;
- reconstruction invariant: `chunk * 16 + local == global`;
- local result remains in [0,15] for every tested negative/positive boundary;
- tests at -17, -16, -15, -1, 0, 1, 15, 16, 17 on each axis.

## Resolution provenance

Resolved directly by the project owner on 2026-09-22 while answering Q-021.

## Supersession

None.
