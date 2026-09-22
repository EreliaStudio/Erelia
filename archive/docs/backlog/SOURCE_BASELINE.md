# Source Baseline

## Repositories re-read for this revision

- Playground: `https://github.com/EreliaStudio/Playground`
- Original source/baseline GDD at commit `ed75feb5e9e9a748c8bb4df95e341a19648d0091`: revision **17 September 2026**.
- Authoritative integrated GDD: `docs/gdd/Erelia_GDD_Current.md` — revision **18 September 2026**, integrated by commit `d0e8bf6`.
- Playground implementation baseline inspected for this revision: `ed75feb5e9e9a748c8bb4df95e341a19648d0091`.
- Sparkle: `https://github.com/EreliaStudio/Sparkle/tree/Version0.1.1`
- Sparkle `Version0.1.1` commit: `9784377d41509234d43e4adec16505382eef178f`

## Current voxel implementation verified

The revised plan is based on the current code rather than the earlier speculative modeling architecture:

- `includes/voxel/voxel_shape.hpp`: normalized polygonal Shapes with position, UV, material slot, normal, outer side, full-side coverage and `coversSide`.
- `includes/voxel/voxel_definition.hpp`: Definition resolves one Shape and texture-slot assignments.
- `includes/voxel/voxel_cell.hpp`: trivially copyable 32-bit cell packing runtime Definition ID, horizontal Orientation and vertical Flip.
- `includes/voxel/chunk.hpp`: headless fixed 16×16×16 cell storage plus chunk/world coordinate helpers and versioning.
- `includes/voxel/chunk_collection.hpp`: chunk ownership/availability and `worldCell()` cross-chunk access.
- `includes/voxel/chunk_baker.hpp`: current meshing entry point, coupled to `Chunk::Collection` and `TextureMesh3D`.
- current shape content includes cube, slab, slope, stair and crossed vegetation planes.

This backlog therefore evolves the working implementation toward one multi-scale `VoxelVolume → VoxelMesher → Mesh/Materials` architecture with the smallest safe refactor.
