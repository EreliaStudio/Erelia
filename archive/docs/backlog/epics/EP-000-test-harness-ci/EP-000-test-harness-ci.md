# EP-000 — Test Harness and CI Quality Gates

**Dependencies:** None  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Create the executable quality infrastructure used by every later Erelia feature: unit/component tests, integration fixtures, deterministic replay, headless simulation, asset-validation tests, and performance regression fixtures.

## Starting state

- Contracts supplied by `None` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Unit and component test baseline
- Headless integration harness
- Replay and state hashing
- Performance and soak fixtures
- Export Sparkle test utilities for downstream projects

## Out of scope

- Responsibilities explicitly assigned to neighboring epics.
- Final choices tracked by unresolved Open Decisions.
- Unapproved balance values, content, serialization syntax, or technology selections.

## Architecture / behavioral overview

- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Unit and component test baseline | [ST-000-01](tickets/ST-000-01-unit-and-component-test-baseline.md) | [ST-000-01](tickets/ST-000-01-unit-and-component-test-baseline.md) |
| Headless integration harness | [ST-000-02](tickets/ST-000-02-headless-integration-harness.md) | [ST-000-02](tickets/ST-000-02-headless-integration-harness.md) |
| Replay and state hashing | [ST-000-03](tickets/ST-000-03-replay-and-state-hashing.md) | [ST-000-03](tickets/ST-000-03-replay-and-state-hashing.md) |
| Performance and soak fixtures | [ST-000-04](tickets/ST-000-04-performance-and-soak-fixtures.md) | [ST-000-04](tickets/ST-000-04-performance-and-soak-fixtures.md) |
| Export Sparkle test utilities for downstream projects | [ST-000-05](tickets/ST-000-05-export-sparkle-test-utilities-for-downstream-projects.md) | [ST-000-05](tickets/ST-000-05-export-sparkle-test-utilities-for-downstream-projects.md) |

## Ticket index

- [ST-000-01 — Unit and component test baseline](tickets/ST-000-01-unit-and-component-test-baseline.md) — Standardize GoogleTest fixtures, naming, deterministic seeds, temporary directories, and failure diagnostics.
- [ST-000-02 — Headless integration harness](tickets/ST-000-02-headless-integration-harness.md) — Provide a reusable application/runtime harness that instantiates domain services without presentation services.
- [ST-000-03 — Replay and state hashing](tickets/ST-000-03-replay-and-state-hashing.md) — Add accepted-command recording, deterministic replay, and semantic state hashing for regression tests.
- [ST-000-04 — Performance and soak fixtures](tickets/ST-000-04-performance-and-soak-fixtures.md) — Provide fixed workloads and reporting for regressions, leaks, lifecycle loops, and long-running simulations.
- [ST-000-05 — Export Sparkle test utilities for downstream projects](tickets/ST-000-05-export-sparkle-test-utilities-for-downstream-projects.md) — **Implementation repository/branch:** Sparkle `Version0.1.1`  
**Sparkle-side status:** [Sparkle #1](https://github.com/EreliaStudio/Sparkle/pull/1) is merged into `Version0.1.1`; its final head CI succeeded. The optional exported `Sparkle::TestLibrary`, installed-package consumer support, reusable image comparison, and Sparkle CI validation are complete.  
**Blocks:** ST-001-01 textured Chunk reference-image gate.

Publish the existing image-comparison, path and framebuffer helpers as an optional installed `Sparkle::TestLibrary` target. Downstream projects must consume the installed package without building SparkleTestSuite, depending on GTest, or accessing Sparkle's checkout. Keep existing GPU capture's Windows/WGL requirement explicit.

## Epic integration acceptance tests

1. A clean checkout can configure, build, and execute the complete automated test suite from one documented command path.
2. Headless tests execute without creating a Sparkle Window, Surface, or OpenGL context.
3. A deterministic fixture can record an accepted command stream and replay it to the same terminal state hash.
4. Voxel-volume/mesher/import fixtures can compare semantic outputs against stable expectations rather than raw byte layout where layout is allowed to evolve.
5. A deliberately malformed content fixture fails validation with source-identifying diagnostics and no crash.
6. Performance fixtures report distributions for current Chunk bake, generalized VoxelMesher, imported-model load/mesh-cache, encounter step, and representative render preparation workloads.
7. Test data has stable seeds and versions so failures are reproducible locally and in CI.
8. A failing integration test prevents the owning epic from being considered Done.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

None.

## Rendering validation

No epic-owned visible output; integration acceptance is semantic/headless.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
