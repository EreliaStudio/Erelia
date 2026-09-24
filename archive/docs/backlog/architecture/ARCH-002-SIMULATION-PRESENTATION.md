# ARCH-002 — Simulation / Presentation Boundary

## Rule

Authoritative simulation decides what happens. Presentation visualizes it.

```text
authoritative event at simulation tick T
        ↓
presentation selects pose/animation/material effect
```

Never invert this relationship:

```text
animation reaches keyframe N
        ↓
apply gameplay damage   ← prohibited authority flow
```

## Unified voxel implication

Chunks and VoxelModels share graphical cell/meshing technology, but that does **not** make every model voxel a simulation entity.

- Chunk cells can feed explicit world collision/navigation systems.
- Hero/weapon/prop visual cells remain presentation data unless a gameplay subsystem explicitly consumes a separate authoritative representation.
- Part animation changes transforms without changing authoritative traversal/combat cells merely because the visual geometry moved.
- Material effects, palette variation, emissive pulses and damage flashes are presentation-only.
- GPU meshes are derivative and never authoritative state.

## Headless invariant

World generation, VoxelVolume data, import conversion where relevant, meshing, navigation and gameplay tests must be executable without a Window/Surface/OpenGL context unless the behavior under test is specifically rendering.
