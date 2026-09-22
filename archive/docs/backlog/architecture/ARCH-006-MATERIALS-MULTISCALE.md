# ARCH-006 — Materials for Multi-Scale Voxels

## Goal

Replace texture-atlas dependence incrementally with a reusable `Material` abstraction while preserving semantic Shape material slots and supporting both world-scale Chunks and small movable VoxelModels.

## Migration, not flag day

The current Shape vertices contain UVs and current Definitions bind slots to atlas coordinates. Keep that path while extracting the generic mesher. Add Material binding alongside it, prove visual fixtures, migrate content, then retire atlas assumptions only if the new direction is accepted.

## Target contract

```text
Shape::Polygon.materialSlot
        ↓
Voxel::Definition slot binding
        ↓
Material
```

Initial Material responsibility may be only base palette/color selection. Extension points can later include emissive, metallic/roughness, transparency, animated/procedural parameters, outline/dissolve or water-like behavior.

## Stable palette variation

Palette variation must be deterministic and stable as the camera moves. Candidate techniques include deterministic position hashing, quantized procedural coordinates, per-face/per-voxel palette selection or shader-side palette lookup. Exact algorithm is chosen by early visual prototype, not architecture speculation.

## Sampling space is explicit

At minimum distinguish intent equivalent to:

- **World/continuous:** terrain pattern can continue across Chunk boundaries without visible seams.
- **Object/local:** a sword/barrel/Hero part carries its pattern as its transform moves.
- **Voxel/local** if later useful for a pattern deliberately restarting per cell.

A single hard-coded world-space implementation is invalid because patterns would slide across moving entities. A single chunk-local implementation is invalid because terrain could reveal seams at Chunk boundaries.

## Mesher/render separation

Material changes do not alter Shape geometry or cell visibility. The mesh preserves material references/slots; rendering resolves current Material data. Ordinary palette/tint/effect changes must not remesh a volume.
