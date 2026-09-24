# EP-030 — External Voxel Asset Import and Volume Conversion

**Dependencies:** EP-001, EP-029  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Choose a practical mature voxel/3D authoring workflow and implement the smallest headless importer/converter that maps authored cells into the unified VoxelModel representation without creating a custom modeling program.

## Starting state

- Contracts supplied by `EP-001, EP-029` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- Tool/format selection is evidence-driven and replaceable; no editor becomes an engine dependency.
- Import output is VoxelVolume cell data, not a special pre-baked model mesh format.
- Cube-centric external formats are acceptable initially; Erelia Shapes remain richer than the source format.
- Definition/material mapping is explicit and deterministic.
- Coordinate axes/origin, palette mapping and scale are never guessed silently.
- Importer can run headlessly in CI/build tooling.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Authoring tool/format evaluation
- Basic VoxelModel importer
- Coordinate and Definition mapping
- Headless conversion pipeline

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
- Tool/format selection is evidence-driven and replaceable; no editor becomes an engine dependency.
- Import output is VoxelVolume cell data, not a special pre-baked model mesh format.
- Cube-centric external formats are acceptable initially; Erelia Shapes remain richer than the source format.
- Definition/material mapping is explicit and deterministic.
- Coordinate axes/origin, palette mapping and scale are never guessed silently.
- Importer can run headlessly in CI/build tooling.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Authoring tool/format evaluation | [ST-030-01](tickets/ST-030-01-authoring-tool-format-evaluation.md) | [ST-030-01](tickets/ST-030-01-authoring-tool-format-evaluation.md) |
| Basic VoxelModel importer | [ST-030-02](tickets/ST-030-02-basic-voxelmodel-importer.md) | [ST-030-02](tickets/ST-030-02-basic-voxelmodel-importer.md) |
| Coordinate and Definition mapping | [ST-030-03](tickets/ST-030-03-coordinate-and-definition-mapping.md) | [ST-030-03](tickets/ST-030-03-coordinate-and-definition-mapping.md) |
| Headless conversion pipeline | [ST-030-04](tickets/ST-030-04-headless-conversion-pipeline.md) | [ST-030-04](tickets/ST-030-04-headless-conversion-pipeline.md) |

## Ticket index

- [ST-030-01 — Authoring tool/format evaluation](tickets/ST-030-01-authoring-tool-format-evaluation.md) — Evaluate mature existing voxel/3D tools and common formats for Erelia model production.
- [ST-030-02 — Basic VoxelModel importer](tickets/ST-030-02-basic-voxelmodel-importer.md) — Import dimensions/cells/palette mapping into EP-001 runtime-sized VoxelModel.
- [ST-030-03 — Coordinate and Definition mapping](tickets/ST-030-03-coordinate-and-definition-mapping.md) — Make axis/origin/palette/Definition conversion explicit and testable.
- [ST-030-04 — Headless conversion pipeline](tickets/ST-030-04-headless-conversion-pipeline.md) — Integrate import/validation into build/content workflow with stable diagnostics and atomic output.

## Epic integration acceptance tests

1. Tool/format spike compares at least viable candidates against documented requirements and records the selected first path.
2. A small barrel authored externally imports to expected dimensions, voxelSize and Cell coordinates.
3. Imported palette/color entries map deterministically to configured Voxel::Definition IDs.
4. Axis/origin conversion fixture produces expected orientation with no mirror/rotation surprise.
5. Invalid/oversized source or unknown mapping fails without replacing last valid asset.
6. Re-import of identical source/mapping/converter version produces semantically identical VoxelModel.
7. Imported VoxelModel meshes through EP-032 with no model-specific mesher.
8. Importer executes without Sparkle Window/Surface/OpenGL and does not require a custom editor executable.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-012](../../open-decisions/OD-012-first-external-voxel-authoring-tool-and-interchange-format.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-017](../../open-decisions/OD-017-rich-shape-mapping-metadata-for-imported-assets.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-023](../../open-decisions/OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

Graphical tickets use deterministic `512 × 512` Sparkle TestLibrary comparisons. Structural refactors preserve reviewed baselines. Intentional visual migrations retain old references until actual/difference images receive human approval, then create a versioned baseline.

## Required user-provided inputs

- User approval of proposed sample schemas and authored model/assembly/animation/equipment fixtures before those formats are frozen.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
