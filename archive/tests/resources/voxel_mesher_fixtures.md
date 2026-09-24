# `VoxelMesher` and `Chunk::Mesher` fixtures

All GPU captures use the Sparkle TestLibrary's fixed `640 × 480` OpenGL framebuffer, the pinned `windows-2025` software-OpenGL runner, and the existing atlas/catalog. Candidate references are published under the `voxel_mesher` category; they are not approved merely because they were generated.

## JSON cross statue and procedural volume

`voxel_models/mesher_cross_statue.json` uses the provisional ST-001-04 schema with dimensions `8 × 8 × 16` and voxel size `0.1`. Ten stone cube cells form a connected cross statue: a six-cell vertical column at `(3, 0..5, 3)` and four horizontal arm cells at `(1,4,3)`, `(2,4,3)`, `(4,4,3)`, and `(5,4,3)`. Its nine internal cube adjacencies remove eighteen faces.

Five isolated cells exercise the remaining current catalog Shapes and transforms:

| Cell | Runtime ID | Definition | Transform |
|---|---:|---|---|
| `(0,0,0)` | 1 | `grass` cube | `positive-z`, `positive-y` |
| `(7,0,0)` | 4 | `plant` cross | `negative-z`, `positive-y` |
| `(0,0,15)` | 5 | `debug_slab` | `positive-z`, `negative-y` |
| `(7,0,15)` | 6 | `debug_slope` | `positive-x`, `positive-y` |
| `(7,3,15)` | 7 | `debug_stair` | `negative-x`, `negative-y` |

The checked-in JSON model and a raw `VoxelVolume` filled through `VoxelVolume::Editor` describe the same fifteen cells. Before ST-032-04 both emitted `290` vertices and `432` indices. Deterministic exact-compatible reuse shares the existing stair Shape's two repeated position/normal/UV vertices, so both now emit `288` vertices and the same `432` indices. Four orthogonal captures are produced for each input, and a ninth capture renders both volumes beside one another.

## Filled adjacent Chunks

Chunk `(0,0,0)` contains a `2 × 2 × 2` stone section at local `x=14..15`, `y=2..3`, `z=4..5`. Chunk `(1,0,0)` contains the matching section at local `x=0..1`. The left section emits `144` indices without its neighbor and `120` when the right Chunk is available. The right section also emits `120`; together they emit `240` without either duplicated shared boundary.

Three captures use the same camera looking toward the shared positive-X face: left without a neighbor, left while the available right neighbor is deliberately not drawn, and both Chunks drawn. Separate headless cases exercise the corresponding one-cell lookup across all six Chunk boundary directions.
