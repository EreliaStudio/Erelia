# EP-002 — Voxel Models, Assemblies, Animation, and Mesh Reuse

**Dependencies:** EP-001, EP-030, EP-032  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Use runtime-sized VoxelModel volumes as reusable visual assets and compose articulated characters from rigid model parts whose cached meshes are transformed rather than rebuilt during animation.

## Starting state

- Contracts supplied by `EP-001, EP-030, EP-032` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- A VoxelModel is cell data + scale/metadata, not a second geometry/meshing format.
- Anchor hierarchy, attachment, and clip metadata belong only to VoxelAssembly/animation assets and remain separate from pure VoxelModel cell data.
- Animation moves rigid parts; it does not normally mutate voxel cells.
- Equipment is an independent VoxelModel attached to hierarchy points.
- Detailed model voxels are not automatically authoritative collision or tactical cells.
- Mesh caching keys must include all geometry-affecting inputs and exclude transform/material-only state.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Pure VoxelModel runtime references
- VoxelAssembly anchor hierarchy and part transforms
- Anchor-targeted animation without remeshing
- Equipment attachment and palette association at render-instance level

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
- A VoxelModel is cell data + scale/metadata, not a second geometry/meshing format.
- Anchor hierarchy, attachment, and clip metadata belong only to VoxelAssembly/animation assets and remain separate from pure VoxelModel cell data.
- Animation moves rigid parts; it does not normally mutate voxel cells.
- Equipment is an independent VoxelModel attached to hierarchy points.
- Detailed model voxels are not automatically authoritative collision or tactical cells.
- Mesh caching keys must include all geometry-affecting inputs and exclude transform/material-only state.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Pure VoxelModel runtime references | [ST-002-01](tickets/ST-002-01-voxelmodel-asset-contract.md) | [ST-002-01](tickets/ST-002-01-voxelmodel-asset-contract.md) |
| VoxelAssembly anchor hierarchy and part transforms | [ST-002-02](tickets/ST-002-02-rigid-articulated-assembly.md) | [ST-002-02](tickets/ST-002-02-rigid-articulated-assembly.md) |
| Anchor-targeted animation without remeshing | [ST-002-03](tickets/ST-002-03-transform-animation-clips.md) | [ST-002-03](tickets/ST-002-03-transform-animation-clips.md) |
| Equipment attachment and palette association at render-instance level | [ST-002-04](tickets/ST-002-04-equipment-and-mesh-reuse.md) | [ST-002-04](tickets/ST-002-04-equipment-and-mesh-reuse.md) |

## Ticket index

- [ST-002-01 — VoxelModel asset contract](tickets/ST-002-01-voxelmodel-asset-contract.md) — Define stable identity and pure VoxelVolume data for imported/runtime model assets; anchors, parent relationships, animation, equipment slots, and world transforms are forbidden.
- [ST-002-02 — Rigid articulated assembly](tickets/ST-002-02-rigid-articulated-assembly.md) — Represent parent/child part hierarchy for torso/head/limbs using transforms over independent VoxelModels.
- [ST-002-03 — Transform animation clips](tickets/ST-002-03-transform-animation-clips.md) — Define clips over articulated transforms with explicit duration/loop and optional presentation-only markers.
- [ST-002-04 — Equipment and mesh reuse](tickets/ST-002-04-equipment-and-mesh-reuse.md) — Attach independent equipment VoxelModels and cache meshes across entities/poses.

## Epic integration acceptance tests

1. A barrel model and one Chunk both mesh through EP-032 with the same Definition/Shape resolution path.
2. Reusing one VoxelModel asset across many entities reuses cached mesh data rather than duplicating geometry per transform.
3. Changing entity/world transform never invokes the VoxelMesher.
4. Playing a walk/attack clip over rigid-part transforms never invokes the VoxelMesher.
5. Editing one part's voxel cells invalidates/remeshes that part without rebuilding unrelated Hero parts.
6. Sword/shield/helmet model attachments follow named parent transforms and can be swapped independently.
7. Visual part hierarchy changes cannot modify authoritative HP/AP/collision/spell validity.
8. Missing part, cyclic hierarchy or missing attachment point fails predictably before publishing the assembly.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-022](../../open-decisions/OD-022-assembly-anchor-and-animation-authoring-source.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

Graphical tickets use deterministic `512 × 512` Sparkle TestLibrary comparisons. Structural refactors preserve reviewed baselines. Intentional visual migrations retain old references until actual/difference images receive human approval, then create a versioned baseline.

## Required user-provided inputs

- User approval of proposed sample schemas and authored model/assembly/animation/equipment fixtures before those formats are frozen.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
