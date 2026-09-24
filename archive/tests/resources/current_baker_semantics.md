# Current `Chunk::Baker` semantic fixtures

These headless fixtures freeze the pre-`VoxelMesher` triangle semantics independently of rendered PNGs. Snapshot rows contain position, normal, and atlas UV values quantized to one millionth. Triangles are canonicalized by cyclic rotation and sorted, so vertex-buffer and triangle emission order are ignored while winding and visible topology remain significant.

The catalog is `resources/catalog_config.json`: atlas grid `4 × 4`; runtime IDs `grass=1`, `stone=3`, `plant=4`, `debug_slab=5`, `debug_slope=6`, and `debug_stair=7`.

| Snapshot | Chunk/cell fixture | Transform | Expected topology/material semantics |
|---|---|---|---|
| `cube.mesh` | Chunk `(0,0,0)`, grass at `(1,1,1)` | `positive-z`, `positive-y` | 12 triangles; grass top/side/bottom atlas UVs. |
| `slab_flipped.mesh` | Chunk `(0,0,0)`, debug slab at `(2,1,1)` | `positive-z`, `negative-y` | 12 triangles; flipped positions/normals/winding; debug-slab UVs. |
| `slope_positive_x.mesh` | Chunk `(0,0,0)`, debug slope at `(3,1,1)` | `positive-x`, `positive-y` | 8 triangles; rotated positions/normals; debug-slope UVs. |
| `stair_negative_x_flipped.mesh` | Chunk `(0,0,0)`, debug stair at `(4,1,1)` | `negative-x`, `negative-y` | 20 triangles; rotated/flipped positions, normals and winding; debug-stair UVs. |
| `cross_negative_z.mesh` | Chunk `(0,0,0)`, plant at `(5,1,1)` | `negative-z`, `positive-y` | 8 double-sided triangles; vegetation-plane UVs. |
| `boundary_unloaded_neighbor.mesh` | Chunk `(0,0,0)`, stone at `(15,2,3)`; Chunk `(1,0,0)` absent | default | 12 triangles: outside the available grid is empty. |
| `boundary_occluded_neighbor.mesh` | The preceding cell plus stone at local `(0,2,3)` in Chunk `(1,0,0)` | default | 10 triangles: the shared positive-X face is absent. |

The existing 20 `640 × 480` textured Chunk references remain the rendering gate. This ticket adds no replacement image and does not change its approved camera, atlas, tolerance, or runner.
