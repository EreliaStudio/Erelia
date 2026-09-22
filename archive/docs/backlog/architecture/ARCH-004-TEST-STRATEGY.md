# ARCH-004 — Test Strategy

## Levels

| Level | Purpose |
|---|---|
| Story acceptance | unit/component behavior of one deliverable |
| Epic integration | boundaries between components/services |
| Meta-epic/system | capability and cross-feature invariants |
| Vertical slice | end-to-end player/server workflow |

## Current-renderer regression gate

Before replacing `Chunk::Baker`, capture deterministic fixtures for representative current content:

- solid cube neighborhood;
- adjacent Chunk boundary;
- slab against cube/empty neighbor;
- slope orientations and vertical flip;
- stair geometry;
- crossed vegetation plane;
- material/texture-slot assignment;
- generated debug chunk or stable World fixture.

For structural refactor stages, compare semantic mesh output rather than byte identity when vertex ordering is not part of the contract: exposed polygon topology, transformed positions, normals, material slots and bounds must remain equivalent.

## Textured Chunk image-regression gate

Before changing Chunk storage/access or extracting the mesher, capture and manually approve PNG references from the **current textured renderer**. Semantic mesh checks alone do not validate the final appearance. EP-000 supplies the GPU harness; ST-001-01 owns the initial references, and EP-001/EP-032 must preserve them throughout structural refactoring.

Use Sparkle Version0.1.1's [SparkleTestLibrary](https://github.com/EreliaStudio/Sparkle/tree/Version0.1.1/tests/TU/SparkleTestLibrary) capture/comparison workflow. Its `OpenGLTestContext` supplies a hidden Windows/WGL context, framebuffer capture and PNG saving; `compareImages(actual, expected, difference, options)` reports dimensions and differing-pixel count and writes a red mismatch image on failure. Passing comparisons remove actual/difference files. Keep approved references separate from result paths. ST-000-05 exports `Sparkle::TestLibrary` as an optional installed component and provides configurable consumer-owned resource/result paths; complete that story before integrating the Playground GPU harness. See [Sparkle export PR](https://github.com/EreliaStudio/Sparkle/pull/1).

### Fixtures and repeatability

- Capture the actual Chunk → textured mesh → shader/atlas → framebuffer path, using current checked-in textures, Shape/Definition bindings and UVs, not a mock renderer or flat-color substitute.
- Include cubes, slabs, slopes, stairs, crossed vegetation planes, horizontal orientations, vertical flips, partial occlusion and adjacent-Chunk boundaries. Use fixed views that expose geometry and texture placement, plus one representative seeded multi-Chunk scene.
- Freeze fixture seed/data, camera/projection, framebuffer size, lighting (where applicable), animation/time, shader/atlas revisions and sampling/render state. Wait deterministically for generation, baking and upload completion before capture.
- Store PNG references and fixture metadata in version control, recording the pre-refactor Playground/Sparkle revisions and canonical GPU/driver environment. Review the initial images visually before accepting them as the baseline.
- Use explicit per-fixture comparison options. Sparkle defaults are RGB tolerance 4, alpha tolerance 8 and transparent-alpha threshold 4; `matches` requires zero differing pixels after those rules. Calibrate on the canonical environment and never loosen tolerances to hide texture/style changes.
- Publish expected, actual and difference images, dimensions, differing-pixel count and comparison settings on failure. Missing references must fail, never auto-create or overwrite themselves during ordinary test runs.
- Run these presentation tests on a supported Windows/OpenGL runner separately from CPU/headless tests. An unavailable GPU gate is reported as not run/blocked, not accepted as visual parity.
- Prove test sensitivity with a controlled texture/UV alteration: the unchanged renderer passes, the altered output fails, and restoring it passes again.

For the initial Playground baseline, [OD-024](../open-decisions/OD-024-current-chunk-golden-fixtures-and-runner.md) resolves the fixture family and canonical runner and records why the installed component's fixed `640 x 480` framebuffer supersedes the generic `512 x 512` convention for ST-001-01.

### Later non-textured rendering migration

The textured references remain unchanged through EP-001 and EP-032. In EP-003/EP-033, switching the same fixtures to non-textured/palette rendering must produce visible comparison failures against those old references. Do not silently regenerate the expected images, disable the tests, mark arbitrary mismatches as expected failures, or broaden tolerance to make that migration green.

Review the expected/actual/difference artifacts and record that the appearance change is intentional. Only then introduce a separately versioned non-textured baseline in an explicit reviewed change. Preserve the original textured references and provenance; keep them as an active compatibility gate while the atlas path remains supported. If that path is later retired, record the suite's retirement explicitly and retain its references as historical evidence. The new active baseline must pass before the material migration is Done; ongoing CI is not left permanently failing. Headless geometry/occlusion checks remain required across both visual versions.

## Cross-cutting resilience catalogue

Every relevant subsystem should cover:

1. nominal behavior;
2. exact boundaries;
3. invalid/corrupt input;
4. create/use/unload/recreate lifecycle;
5. determinism where expected;
6. persistence/version migration;
7. retry/idempotency where relevant;
8. concurrency and cancellation where relevant;
9. authority/trust-boundary validation;
10. dependency failure and rollback/fail-closed behavior;
11. headless execution for simulation/data paths;
12. regression and performance fixtures.

## Unified voxel property candidates

- every valid coordinate maps to exactly one valid cell index;
- out-of-bounds coordinates never alias valid cells;
- volume local extent equals dimensions × uniform voxel size;
- changing voxel scale changes positions/bounds but not grid neighbor relationships;
- same cell arrangement at two scales yields equivalent visibility/topology;
- current `Voxel::Cell` pack/unpack round-trips exactly;
- orientation/flip preserve current Shape transform semantics;
- standalone outside-volume neighbor resolves empty;
- Chunk neighbor resolution matches `Chunk::Collection::worldCell` semantics;
- meshing never requires Window/Surface/OpenGL;
- transform animation never dirties voxel mesh;
- cell edit dirties only the owning volume/affected chunk neighbors required by the chosen scheduler.

## Material visual-regression fixtures

Render fixed camera/light scenes for:

- palette-only terrain across multiple Chunk boundaries;
- same Material on moving object using local sampling;
- world-space terrain variation with no chunk seam;
- camera movement with no pattern swimming;
- emissive prototype when that property is introduced.

Image comparison should allow explicit tolerances for GPU/driver differences while preserving obvious seam/swimming regressions.

## Fuzz/property targets

- volume coordinate/index conversion;
- cell orientation/flip transforms;
- Shape parser/content validation;
- imported voxel volume parser/converter;
- VoxelMesher neighbor visibility decisions;
- formula/content parsers;
- network protocol decoding.

## Performance methodology

Use fixed fixtures and distributions, not one stopwatch sample. Measure separately:

- retained pre-change Baker semantic/count evidence where available;
- pre/post-change generalized VoxelMesher on equivalent deterministic 16³ fixtures;
- small/medium/large VoxelModel meshing;
- mesh upload/cache hit/miss;
- material rendering;
- world generation and simulation.

The generic abstraction is acceptable only if it does not introduce an unexplained terrain regression. Vertex-count reduction alone is not a performance claim, and hosted CI timings are not treated as reliable benchmarks. Optimize storage/specialization behind the shared contract before splitting the meshing architecture.
