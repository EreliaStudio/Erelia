# EP-003 — Material Rendering, Mesh Runtime, and Caching

**Dependencies:** EP-001, EP-032  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Render meshes produced by the generalized VoxelMesher through one ordinary Sparkle mesh/material path, while introducing the Material abstraction incrementally and preserving current atlas rendering until parity is proven.

## Starting state

- Contracts supplied by `EP-001, EP-032` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- Renderer consumes meshes/material references, not Chunk or Hero-specific geometry formats.
- Avoid one draw call per voxel; volumes are meshed into exterior/indexed geometry.
- Keep current `TextureMesh3D`/atlas path available during mesher extraction so terrain regression is attributable.
- Material/palette changes do not dirty geometry when slot topology is unchanged.
- Material sampling space is explicit and stable for moving objects versus world terrain.
- Dynamic visual effects remain presentation-only.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Shared Chunk/model VoxelMesh upload and cache
- Independently bindable Palette SSBO resources
- VoxelRenderCommand palette binding
- Same shader contract without GPU paletteId

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
- Renderer consumes meshes/material references, not Chunk or Hero-specific geometry formats.
- Avoid one draw call per voxel; volumes are meshed into exterior/indexed geometry.
- Keep current `TextureMesh3D`/atlas path available during mesher extraction so terrain regression is attributable.
- Material/palette changes do not dirty geometry when slot topology is unchanged.
- Material sampling space is explicit and stable for moving objects versus world terrain.
- Dynamic visual effects remain presentation-only.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Shared Chunk/model VoxelMesh upload and cache | [ST-003-01](tickets/ST-003-01-common-mesh-upload-cache-path.md) | [ST-003-01](tickets/ST-003-01-common-mesh-upload-cache-path.md) |
| Independently bindable Palette SSBO resources | [ST-003-02](tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) | [ST-003-02](tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) |
| VoxelRenderCommand Palette binding | [ST-003-02](tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) | [ST-003-02](tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) |
| Shared Chunk/model shader without GPU palette ID | [ST-003-02](tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) | [ST-003-02](tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) |
| Explicit sampling spaces and Palette variation | [ST-003-03](tickets/ST-003-03-sampling-spaces-and-palette-variation.md) | [ST-003-03](tickets/ST-003-03-sampling-spaces-and-palette-variation.md) |
| Presentation-only advanced Material effects | [ST-003-04](tickets/ST-003-04-presentation-material-effects.md) | [ST-003-04](tickets/ST-003-04-presentation-material-effects.md) |

## Ticket index

- [ST-003-01 — Common mesh upload/cache path](tickets/ST-003-01-common-mesh-upload-cache-path.md) — Upload/index/cache VoxelMesher output independent of whether the source was Chunk or VoxelModel.
- [ST-003-02 — Palette Resource Binding and Unified Voxel Shader Contract](tickets/ST-003-02-palette-resource-binding-and-voxel-shader-contract.md) — Bind one Palette SSBO per renderable command, feed flat palette-element indices through the shared Chunk/model shader, and preserve mesh reuse across Palette changes.
- [ST-003-03 — Sampling spaces and palette variation](tickets/ST-003-03-sampling-spaces-and-palette-variation.md) — Prototype deterministic palette variation with explicit world/object/local sampling intent.
- [ST-003-04 — Presentation material effects](tickets/ST-003-04-presentation-material-effects.md) — Support per-instance tint/effect parameters and leave room for emissive/metallic/transparency later.

## Epic integration acceptance tests

1. Current textured chunk fixture renders through generalized mesher with equivalent Shape transforms/material-slot/UV semantics.
2. One generalized mesh representation can be rendered for both a Chunk-derived mesh and a VoxelModel-derived mesh.
3. Same VoxelModel mesh is reused by multiple transforms/entities.
4. Palette/material parameter changes do not call the VoxelMesher.
5. World-space terrain variation remains continuous across Chunk boundaries where configured.
6. Object-local material variation moves with a barrel/sword and does not swim as the object moves through world space.
7. Moving the camera alone cannot change deterministic palette-cell assignment/pattern phase.
8. Cache unload/reload releases GPU resources without invalidating headless volume/model data.

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
- [OD-021](../../open-decisions/OD-021-serialized-derivative-mesh-cache-versus-rebuild-on-load.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.

## Rendering validation

Graphical tickets use deterministic `512 × 512` Sparkle TestLibrary comparisons. Structural refactors preserve reviewed baselines. Intentional visual migrations retain old references until actual/difference images receive human approval, then create a versioned baseline.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
