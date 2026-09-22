# OD-024 — Current Chunk golden fixtures, capture size, and canonical runner

## Status

Resolved

## Problem

ST-001-01 required deterministic current-render scenes, cameras, a framebuffer size, and a canonical Windows/OpenGL environment, but the repository did not define those inputs. The generic backlog text requested `512 x 512`, while the installed Sparkle `OpenGLTestContext` owns a fixed `640 x 480` framebuffer. Its `setGeometry` operation does not resize that framebuffer, so a `512 x 512` capture would attempt to read beyond its 480-row attachment.

## Affected epics/tickets

[ST-000-05](../epics/EP-000-test-harness-ci/tickets/ST-000-05-export-sparkle-test-utilities-for-downstream-projects.md), [ST-001-01](../epics/EP-001-unified-voxel-volume/tickets/ST-001-01-current-chunk-regression-characterization.md), and [ARCH-004](../architecture/ARCH-004-TEST-STRATEGY.md).

## Known constraints

- Consume the installed `Sparkle::TestLibrary`; do not change or duplicate it in Playground.
- Exercise the current textured Chunk production path without approving generated images automatically.
- Preserve CPU/headless execution without creating an OpenGL context.
- Keep each visual scene and camera deterministic and explicitly documented.

## Options considered

- Use `512 x 512` by expanding this checkpoint to add a resizable Sparkle test framebuffer.
- Use the exported context's supported fixed `640 x 480` framebuffer.
- Use one generated debug scene only.
- Create several focused scenes plus one deterministic generated multi-Chunk scene.
- Target GitHub Actions `windows-2025`, `windows-2022`, or defer CI pending a self-hosted GPU contract.

## Decision

- Use several focused, explicitly declared scenes, each with a fixed camera, plus one seeded four-Chunk debug scene.
- Use `640 x 480`, matching the installed TestLibrary's actual framebuffer contract. This is the explicit ST-001-01 exception to generic `512 x 512` backlog wording.
- Use GitHub Actions `windows-2025` with the same software OpenGL approach and OpenGL 4.6/GLSL 460 overrides established by Sparkle CI.
- Treat all first captures as candidates. They remain result artifacts until the project owner explicitly approves them.

## Rationale

The selected resolution is the only safe capture size available through the installed component without reopening Sparkle-side scope. Multiple focused scenes make failures attributable to cube/boundary occlusion, partial geometry, transforms, or material/vegetation behavior, while the seeded scene protects their current composition in generated content.

## Consequences

- Fixture cell data, cameras, projection, state, atlas, seed, synchronization, and tolerance metadata are versioned with the tests.
- Missing approved references intentionally fail the GPU gate while preserving candidates.
- The canonical environment uses software OpenGL; CI records the actual vendor, renderer, and version.
- Any later move to `512 x 512` requires a separately reviewed Sparkle TestLibrary framebuffer change and new approved references.

## Validation / evidence

- `tests/chunk_golden_tests.cpp`
- `tests/resources/current_chunk_fixtures.md`
- Playground `Windows/OpenGL golden candidates` CI job
- Candidate/reference approval remains pending.

## Resolution provenance

Resolved by the project owner during ST-001-01 execution on 19 September 2026: multiple scene-specific cameras, `640 x 480` after inspection of the fixed exported framebuffer, and GitHub Actions `windows-2025` were approved.
