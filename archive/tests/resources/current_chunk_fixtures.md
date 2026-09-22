# Current textured Chunk fixtures

These fixtures characterize the pre-refactor `Chunk -> Chunk::Baker -> TextureMesh3DRenderCommand -> atlas` path. Candidate images are written beneath the configured result root. Nothing in this directory is generated or overwritten by normal test execution.

## Shared render contract

- Pre-characterization Playground revision: `65df17799dbd855add50ba38861dc0d174e1fc20`.
- Sparkle: `Version0.1.1` revision `c1158dc228c8f8cbe674ac4510ad148f02b519d6`, installed `TestLibrary` component.
- Framebuffer/viewport: `640 x 480`, the fixed size supported by the exported `OpenGLTestContext`.
- Projection: perspective; each fixture records field of view and clipping planes in `chunk_golden_tests.cpp`.
- Camera orientation: `spk::Quaternion::lookAt(position, target, {0, 1, 0})`. Each fixture is captured from four deterministic viewpoints separated by 90-degree rotations around the same target; the original reviewed camera remains view 1.
- Background: RGBA `(0.30, 0.48, 0.72, 1.0)`.
- Render state: the state established by `OpenGLTestContext::reset`; depth test `LEQUAL`, back-face culling with counter-clockwise fronts, alpha blending, full color/depth writes.
- Lighting: the unchanged hard-coded directional-light calculation in `resources/shaders/texture_mesh/vertex.glsl`.
- Atlas: `resources/textures/meshSpriteSheet.png`, grid `4 x 4`, SHA-256 `0a2026037578eecfcf04f7254ecf36c25e98d817623500b708cdf24563294194`.
- Shapes revision: `resources/voxels/shapes.json`, SHA-256 `b2e354e2d8b5c3172a152fa4249ce52f3c13cff9b8e85ad448f1d0e74d01f763`.
- Definitions revision: `resources/voxels/definitions.json`, SHA-256 `c592c53fbd59f7ceae9ae9c1ceec545f8c96ffa89ccf26e4b49e9e98f11a18bc`.
- Shader revisions: vertex SHA-256 `8c61ba957b553f642b0ad132b2228a0b68f9ec1bd706279cf215048911f13cd7`; fragment SHA-256 `fe103998ea0ddd42665b2bceda2ad5c734c5093fe335ee13f10853298c2dd13b`.
- Definition runtime IDs: empty `0`, grass `1`, dirt `2`, stone `3`, plant `4`, debug_slab `5`, debug_slope `6`, debug_stair `7`. Shape IDs are cube, cross, slab, slope, and stair as declared in the checked-in catalog sources.
- Animation/time: none.
- Completion condition: baking is synchronous; each production render command performs resource validation/upload synchronously; `OpenGLTestContext::save` calls `glFinish` before reading pixels.
- Comparison: Sparkle defaults—RGB tolerance `4`, alpha tolerance `8`, transparent-alpha threshold `4`, and zero differing pixels after those rules.
- Canonical CI environment: GitHub Actions `windows-2025`, clang-cl, `x64-windows-static`, and Sparkle's pinned software OpenGL setup. Exact renderer/driver strings are recorded by the CI job.
- Test framework ownership: Playground owns and links GoogleTest explicitly. `GTest::gtest_main` supplies the executable entry point; `Sparkle::TestLibrary` remains framework-independent and supplies only paths, framebuffer capture, and image comparison utilities.

## Fixtures

Exact cells, Definition IDs by stable catalog name, transforms, and primary camera values live in the declarative `Scene` records in `tests/library/current_chunk_golden_scenes.cpp`. The primary image keeps the fixture's base name; additional viewpoints use `_view_2`, `_view_3`, and `_view_4` suffixes.

- `solid_cube_boundary`: solid-neighborhood face occlusion across the boundary between Chunk `(0,0,0)` and `(1,0,0)`.
- `slab_partial_occlusion`: slab against a cube, an empty neighbor, and a vertically flipped slab.
- `slope_stair_transforms`: all four horizontal orientations and both vertical-flip states across slope and stair geometry.
- `materials_and_vegetation`: grass/dirt/stone atlas slots and crossed vegetation planes, including horizontal orientation.
- `seeded_debug_chunks`: four generated Chunks at `(-1,0,-1)`, `(0,0,-1)`, `(-1,0,0)`, `(0,0,0)` using seed `0xE7E11A`.

The sensitivity sequence uses view 1 of `solid_cube_boundary`, collapses all UVs of the first baked Chunk mesh to `(0,0)` in a test-owned mesh copy, verifies a mismatch and preserved actual/difference artifacts, then renders the unmodified production output again and verifies a match.
