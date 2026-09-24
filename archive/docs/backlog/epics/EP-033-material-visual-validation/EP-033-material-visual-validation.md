# EP-033 — Multi-Scale Material Prototype and Visual Validation

**Dependencies:** EP-003, EP-004, EP-030, EP-031, EP-032  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Validate the unified visual architecture early with one representative scene using generated Chunks, current Shapes, palette Materials, an imported prop, an articulated Hero and attached weapon—all flowing through the same VoxelVolume → VoxelMesher → render path.

## Starting state

- Contracts supplied by `EP-003, EP-004, EP-030, EP-031, EP-032` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- This is a validation milestone, not a material-graph/editor project.
- First implementation can use simple color palettes and deterministic variation.
- Compare world/object sampling approaches visually before freezing Material schema details.
- Scene must keep current cube/slope/slab/stair behavior visible.
- Imported model/character content must not require custom authoring software.
- If the visual result is rejected, revisit art/material choices before gameplay backlog accumulates content debt.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- WorldPalette and model Palette visual validation
- Current-palette binding per render command
- Chunk per-slot/per-side material resolution
- Reviewed intentional golden-baseline migration

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
- This is a validation milestone, not a material-graph/editor project.
- First implementation can use simple color palettes and deterministic variation.
- Compare world/object sampling approaches visually before freezing Material schema details.
- Scene must keep current cube/slope/slab/stair behavior visible.
- Imported model/character content must not require custom authoring software.
- If the visual result is rejected, revisit art/material choices before gameplay backlog accumulates content debt.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| WorldPalette and model Palette visual validation | [ST-033-01](tickets/ST-033-01-palette-material-terrain-prototype.md) | [ST-033-01](tickets/ST-033-01-palette-material-terrain-prototype.md) |
| Current-palette binding per render command | [ST-033-02](tickets/ST-033-02-object-local-material-prototype.md) | [ST-033-02](tickets/ST-033-02-object-local-material-prototype.md) |
| Chunk per-slot/per-side material resolution | [ST-033-03](tickets/ST-033-03-unified-scene-assembly.md) | [ST-033-03](tickets/ST-033-03-unified-scene-assembly.md) |
| Reviewed intentional golden-baseline migration | [ST-033-04](tickets/ST-033-04-visual-performance-decision-record.md) | [ST-033-04](tickets/ST-033-04-visual-performance-decision-record.md) |

## Ticket index

- [ST-033-01 — Palette Material terrain prototype](tickets/ST-033-01-palette-material-terrain-prototype.md) — Render terrain with a minimal palette-based Material while retaining semantic Shape slots.
- [ST-033-02 — Object-local Material prototype](tickets/ST-033-02-object-local-material-prototype.md) — Render imported barrel/weapon with stable local palette pattern/variation.
- [ST-033-03 — Unified scene assembly](tickets/ST-033-03-unified-scene-assembly.md) — Build the required terrain + barrel + articulated Hero + weapon scene using the common volume/mesher path.
- [ST-033-04 — Visual/performance decision record](tickets/ST-033-04-visual-performance-decision-record.md) — Capture images/profiling and decide whether the multi-scale voxel/Material direction is acceptable before broad content production.

## Epic integration acceptance tests

1. Scene renders multiple generated 16³ terrain Chunks using generalized mesher with cube/slope/slab/stair cells.
2. Palette Material terrain remains stable while camera moves and has no unintended Chunk-boundary seam.
3. Imported small-scale barrel meshes through the same mesher and uses object-local Material sampling.
4. Articulated Hero renders from multiple small-scale VoxelModels and animates part transforms without remesh.
5. Attached voxel weapon follows hand transform and reuses the same Material/mesh infrastructure.
6. Changing Hero/weapon transforms and Material palette leaves mesher invocation count unchanged.
7. A deliberate voxel edit to barrel or test Chunk invalidates/remeshes only required volume(s)/neighbors.
8. Profiling captures mesh counts, triangles, draw calls, build times and cache reuse for the complete scene.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-018](../../open-decisions/OD-018-first-material-effects-beyond-palette-color.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-019](../../open-decisions/OD-019-palette-variation-algorithm-and-sampling-declaration.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

Graphical tickets use deterministic `512 × 512` Sparkle TestLibrary comparisons. Structural refactors preserve reviewed baselines. Intentional visual migrations retain old references until actual/difference images receive human approval, then create a versioned baseline.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
