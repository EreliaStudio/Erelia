# ST-032-02 — Unified VoxelMesher and Chunk::Mesher implementation

## Intent

Implement one inheritable, scale-aware `VoxelMesher` for `VoxelVolume`/`VoxelModel` and its narrow `Chunk::Mesher` specialization, including cached occlusion, cross-Chunk lookup, exhaustive semantic evidence, and reviewed rendering fixtures.

## Execution decision policy

- Do not invent unspecified behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology.
- Treat every unspecified observable choice as an Open Decision and ask the project owner before implementing it.
- Deliberate failures originating in Playground throw `spk::Exception`.

## Starting state / prerequisites

- ST-032-01 locks the current `Chunk::Baker` semantics and textured output.
- ST-001-02/03/04 provide `VoxelVolume`, Chunk inheritance, `VoxelModel`, editing, scale, and the provisional sparse JSON Reader schema.
- OD-025 resolves one shared cached algorithm with only outside-volume lookup specialized for Chunks.
- OD-024 fixes the supported golden runner, tolerance policy, and `640 × 480` framebuffer.

## Approved consolidation

The project owner consolidated the former ST-032-02 Shape/iteration work, ST-032-03 boundary/cache work, and ST-004-02 Chunk specialization into this ticket on 20 September 2026. `Chunk::Baker` remains intact as the independent parity oracle. ST-004-01 separately owns the later migration of runtime consumers from `Chunk::Baker` to `Chunk::Mesher`.

## Owned behavior

- `VoxelMesher` owns deterministic volume iteration, Definition/Shape expansion, orientation/flip transforms, uniform-scale position mapping, material/UV propagation, visibility evaluation, mesh emission, and the reusable occlusion-result cache.
- In-bounds neighbors use `VoxelVolume::at`. Only out-of-volume coordinates reach a protected virtual hook; the base returns `Voxel::Cell{}`.
- `Chunk::Mesher` derives from `VoxelMesher`, inherits the entire algorithm/cache, and overrides only outside-volume lookup through the source Chunk coordinate and `Chunk::Collection`.
- Invalid runtime Definition IDs fail validation before mesh/cache mutation.
- The existing textured `spk::TextureMesh3D` representation is preserved; OD-020 continues to own later merging/indexing/hard-normal policy.

## Explicitly not owned

- Redirecting `Chunk::BakeScheduler`, views, application code, or other runtime consumers away from `Chunk::Baker`; ST-004-01 owns that transition.
- Removing `Chunk::Baker` or its characterization fixtures before the later transition/parity gate.
- Palette/material-SSBO migration, final mesh optimization, import formats, collision, anchors, animation, or gameplay behavior.

## Approved fixtures

### JSON model and raw volume

- Checked-in descriptor: `tests/resources/voxel_models/mesher_cross_statue.json`.
- Provisional ST-001-04 schema: dimensions `8 × 8 × 16`, voxel size `0.1`, numeric current runtime IDs.
- Ten stone cubes form a connected cross statue with nine internal adjacencies.
- Five isolated cells exercise grass cube, cross/plant, flipped slab, rotated slope, and rotated/flipped stair Shapes.
- A raw `VoxelVolume` is filled procedurally with the identical fifteen packed cells.
- Both inputs must produce identical semantic output: exactly `290` vertices and `432` indices.

### Filled adjacent Chunks

- Chunk `(0,0,0)` contains stone at local `x=14..15`, `y=2..3`, `z=4..5`.
- Chunk `(1,0,0)` contains stone at local `x=0..1`, `y=2..3`, `z=4..5`.
- The left section emits `144` indices without its neighbor and `120` with the neighbor available; the right emits `120` and the combined meshes emit `240`.
- Independent one-cell cases exercise correct lookup across all six Chunk boundary directions.

## Acceptance tests

### Structure and shared algorithm

- [x] `VoxelMesher` works directly with `VoxelVolume` and `VoxelModel` and contains no Chunk/world dependency or concrete-volume branch.
- [x] `Chunk::Mesher` derives from `VoxelMesher` and overrides only protected outside-volume neighbor resolution.
- [x] No second iteration, Shape transform, visibility, cache, or mesh-emission loop exists in `Chunk::Mesher`.
- [x] `Chunk::Baker` remains present and exercised as a separate production parity oracle.

### Semantic and numerical behavior

- [x] The JSON-loaded model and procedurally filled raw volume produce byte-identical canonical semantic meshes.
- [x] The cross statue emits exactly `290` vertices and `432` indices, proving internal face removal alongside every current Shape family.
- [x] Identical arrangements at scale `1.0` and `0.1` preserve topology, indices, normals, UVs, and materials while positions scale by exactly ten.
- [x] Standalone out-of-volume lookup returns empty and exposes all six faces of a boundary cube.
- [x] Cache misses/entry count stabilize after the first bake while cache hits increase on an identical repeated bake.
- [x] Cube, slab, slope, stair, cross, unloaded-neighbor, and loaded-neighbor outputs match all ST-032-01 semantic snapshots.
- [x] Filled adjacent Chunk sections produce the approved `144 → 120 + 120` index counts and match `Chunk::Baker` semantics.
- [x] All six Chunk boundary directions resolve the exact adjacent cell and remove only the shared face.
- [x] An invalid Definition ID throws `spk::Exception`, leaves cache statistics unchanged, and does not poison a later valid bake.
- [x] CPU/headless tests require no Window or OpenGL context.

### Golden rendering

- [x] All captures use the OD-024 `640 × 480` framebuffer and existing atlas/shader/tolerances on the canonical Windows software-OpenGL runner.
- [x] Four orthogonal JSON-model views and four procedural-volume views are generated and pairwise pixel-identical.
- [x] One scene renders the JSON model and procedural volume beside one another.
- [x] Chunk captures show left without neighbor, left with an available-but-undrawn neighbor, and both adjacent Chunks rendered.
- [x] Existing 20 current-Chunk golden references remain unchanged and passing.
- [x] The twelve new candidate references were reviewed and approved by the project owner, then checked in byte-for-byte from artifact `10606095311`.

## Open decisions

- [OD-020](../../../open-decisions/OD-020-mesher-merging-indexing-and-hard-normal-policy.md) remains open and is not selected by this structural implementation.
- [OD-024](../../../open-decisions/OD-024-current-chunk-golden-fixtures-and-runner.md) supplies the authoritative visual runner/size/tolerance contract.
- [OD-025](../../../open-decisions/OD-025-voxelmesher-chunk-specialization-and-occlusion-cache.md) supplies the implemented inheritance and cache-ownership contract.

## Completion evidence

- `VoxelMesherTest`, `VoxelMesherSemanticGoldenTest`, and `Chunk::MesherTest` provide headless numerical, semantic, cache, scale, failure, and six-direction boundary evidence.
- `CurrentChunkGoldenTest.VoxelMesherJsonAndProceduralVolumes` and `CurrentChunkGoldenTest.Chunk::MesherCrossChunkOcclusion` cover the twelve approved references.
- Candidate artifact `10606095311` from CI run `35510619947`, SHA-256 `91bb25488ec4a2c0f5c4c47184559ae1f6e9b44922e82e4a1f7abff36ec447d4`, was explicitly approved by the project owner on 20 September 2026.
- Commit `ae1deac` checks in those exact twelve `640 × 480` PNGs without changing existing references, tolerances, cameras, atlas data, or shaders.
- Commit `6e0054f` protects JSON/procedural parity inputs from Sparkle's successful-comparison artifact cleanup without changing any reference.
- CI run `35513348937` passes both the complete CPU/headless and Windows/OpenGL lanes with all 32 approved references.
