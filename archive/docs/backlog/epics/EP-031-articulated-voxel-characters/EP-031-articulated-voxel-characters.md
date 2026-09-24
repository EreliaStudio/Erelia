# EP-031 — Articulated Voxel Character Assembly and Transform Animation

**Dependencies:** EP-002, EP-030  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Use externally authored rigid VoxelModel parts, anchor hierarchies and transform animation to produce the first practical Hero/enemy workflow without skeletal vertex skinning or custom animation software.

## Starting state

- Contracts supplied by `EP-002, EP-030` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- Each part remains an independent VoxelModel using the shared mesher.
- Hierarchy/animation metadata may come from Blender or a simple sidecar; exact tool is a decision gate.
- Runtime animation changes transforms, not cell geometry.
- Equipment uses attachment points and independent VoxelModels.
- Gameplay collision is explicit and not recomputed from animated visual voxels.
- Do not require every armor slot to have unique visible geometry in H0.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- External authoring/import into EP-002 contracts
- Assembly anchor hierarchy source
- Animation and equipment attachment metadata
- First production Hero, enemy, and equipment fixtures

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
- Each part remains an independent VoxelModel using the shared mesher.
- Hierarchy/animation metadata may come from Blender or a simple sidecar; exact tool is a decision gate.
- Runtime animation changes transforms, not cell geometry.
- Equipment uses attachment points and independent VoxelModels.
- Gameplay collision is explicit and not recomputed from animated visual voxels.
- Do not require every armor slot to have unique visible geometry in H0.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| External authoring/import into EP-002 contracts | [ST-031-01](tickets/ST-031-01-character-part-hierarchy-metadata.md) | [ST-031-01](tickets/ST-031-01-character-part-hierarchy-metadata.md) |
| Assembly anchor hierarchy source | [ST-031-02](tickets/ST-031-02-transform-animation-authoring-import.md) | [ST-031-02](tickets/ST-031-02-transform-animation-authoring-import.md) |
| Animation and equipment attachment metadata | [ST-031-03](tickets/ST-031-03-equipment-attachments.md) | [ST-031-03](tickets/ST-031-03-equipment-attachments.md) |
| First production Hero, enemy, and equipment fixtures | [ST-031-04](tickets/ST-031-04-first-hero-enemy-production-fixture.md) | [ST-031-04](tickets/ST-031-04-first-hero-enemy-production-fixture.md) |

## Ticket index

- [ST-031-01 — Character part hierarchy metadata](tickets/ST-031-01-character-part-hierarchy-metadata.md) — Define named assembly anchors, parent/child relationships, and local transforms independent of pure VoxelModel cell storage.
- [ST-031-02 — Transform animation authoring/import](tickets/ST-031-02-transform-animation-authoring-import.md) — Use mature tooling or declarative clips to author rigid-part transform keyframes.
- [ST-031-03 — Equipment attachments](tickets/ST-031-03-equipment-attachments.md) — Attach weapon/shield/helmet or other visible equipment to named hierarchy points.
- [ST-031-04 — First Hero/enemy production fixture](tickets/ST-031-04-first-hero-enemy-production-fixture.md) — Produce a representative Hero and enemy using the chosen external workflow and shared runtime.

## Epic integration acceptance tests

1. Torso/head/upper+lower limbs import and assemble into one Hero hierarchy.
2. Idle/walk clip transforms parts without any VoxelMesher call after meshes are ready.
3. Attack/spell pose can rotate hierarchy while weapon remains attached to expected point.
4. Swapping sword/shield assets does not rebuild Hero body meshes.
5. Two Heroes can share part/model meshes while having independent poses/transforms.
6. Animation interruption/cross-clip switch never creates a cyclic/invalid hierarchy.
7. Visual pose change does not change authoritative collision unless explicit gameplay logic updates it.
8. Missing part/anchor/attachment metadata fails at validation/load boundary with useful diagnostics.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-022](../../open-decisions/OD-022-assembly-anchor-and-animation-authoring-source.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
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
