# EP-005 — Voxel Traversal Graph and Surface Extraction

**Dependencies:** EP-001, EP-004  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Derive navigation/traversable surfaces from authoritative world Chunk voxel geometry while ensuring the new small-scale VoxelModel representation does not accidentally become navigation or collision truth.

## Starting state

- Contracts supplied by `EP-001, EP-004` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- Traversal operates on world/Chunk semantics, not every VoxelVolume instance.
- Entity model voxels, equipment and visual animation do not automatically create world traversal nodes.
- Existing Shape geometry/coverage/orientation/flip can inform surface extraction at world scale.
- Material/color/effect changes never alter traversal.
- Multiple traversable surfaces in one X/Z column remain supported where world geometry permits.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- World surface extraction parity
- Cardinal connectivity graph
- Explicit dynamic obstacle boundary
- Combat-cell projection from traversal

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
- Traversal operates on world/Chunk semantics, not every VoxelVolume instance.
- Entity model voxels, equipment and visual animation do not automatically create world traversal nodes.
- Existing Shape geometry/coverage/orientation/flip can inform surface extraction at world scale.
- Material/color/effect changes never alter traversal.
- Multiple traversable surfaces in one X/Z column remain supported where world geometry permits.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| World surface extraction parity | [ST-005-01](tickets/ST-005-01-world-surface-extraction-parity.md) | [ST-005-01](tickets/ST-005-01-world-surface-extraction-parity.md) |
| Cardinal connectivity graph | [ST-005-02](tickets/ST-005-02-cardinal-connectivity-graph.md) | [ST-005-02](tickets/ST-005-02-cardinal-connectivity-graph.md) |
| Explicit dynamic obstacle boundary | [ST-005-03](tickets/ST-005-03-explicit-dynamic-obstacle-boundary.md) | [ST-005-03](tickets/ST-005-03-explicit-dynamic-obstacle-boundary.md) |
| Combat-cell projection from traversal | [ST-005-04](tickets/ST-005-04-combat-cell-projection-from-traversal.md) | [ST-005-04](tickets/ST-005-04-combat-cell-projection-from-traversal.md) |

## Ticket index

- [ST-005-01 — World surface extraction parity](tickets/ST-005-01-world-surface-extraction-parity.md) — Protect current/expected shape-based traversable-surface extraction through the voxel refactor.
- [ST-005-02 — Cardinal connectivity graph](tickets/ST-005-02-cardinal-connectivity-graph.md) — Connect compatible neighboring world surfaces with height/action metadata needed by exploration/combat.
- [ST-005-03 — Explicit dynamic obstacle boundary](tickets/ST-005-03-explicit-dynamic-obstacle-boundary.md) — Keep movable/model entities separate from static terrain graph while allowing explicit blockers later.
- [ST-005-04 — Combat-cell projection from traversal](tickets/ST-005-04-combat-cell-projection-from-traversal.md) — Expose valid world surfaces to encounter combat-cell construction without using render meshes as authority.

## Epic integration acceptance tests

1. Existing cube/slab/slope/stair world fixtures yield expected traversable surfaces after unified volume refactor.
2. Rotated/flipped world cells produce correspondingly transformed traversal geometry.
3. Two surfaces at different Y positions in one X/Z column remain distinct.
4. A visually identical Chunk at a different hypothetical voxel scale changes world dimensions consistently only when explicitly used as world geometry.
5. Adding a decorative barrel VoxelModel does not create traversal cells unless an explicit world collision/obstacle system registers it.
6. Hero limb/equipment VoxelModels never alter terrain navigation during animation.
7. Material palette changes have zero traversal effect.
8. Chunk unload/reload regenerates equivalent traversal for identical authoritative cells.

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
