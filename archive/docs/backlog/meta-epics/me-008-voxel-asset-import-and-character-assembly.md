# ME-008 — Voxel Asset Import and Character Assembly

## Goal

Provide a practical content path for small-scale voxel objects and articulated characters **without building a custom modeling program**, converting mature external-tool output into the same VoxelVolume representation and mesher used by terrain.

## Epics

- [EP-030 — External Voxel Asset Import and Volume Conversion](../epics/EP-030-external-voxel-import/EP-030-external-voxel-import.md)
- [EP-031 — Articulated Voxel Character Assembly and Transform Animation](../epics/EP-031-articulated-voxel-characters/EP-031-articulated-voxel-characters.md)

The shared mesher and runtime assembly contracts live in ME-001 (EP-032 and EP-002). This tooling meta-epic authors/imports data into those contracts; it must not invent parallel runtime model, anchor, animation, equipment, mesh, or Palette types.

## Meta-epic invariants

- Imported models become normal VoxelVolumes; they do not create a second cell format.
- Imported VoxelModel files remain pure volume data. Assembly-only anchors, parent relationships, part references, animation, and equipment attachment metadata are authored separately.
- The importer is replaceable if authoring tools change.
- No custom MagicaVoxel/Blender replacement is required for H0/H1.
- Rigid-part animation updates transforms only.
- Equipment uses independent VoxelModel assets and attachment transforms.
- Import/runtime schemas remain versioned and validate atomically.
