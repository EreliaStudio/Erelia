# EP-029 — Unified Content Schemas and Voxel Asset Contract

**Dependencies:** EP-000  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Treat official content as versioned validated data, including the unified VoxelVolume model assets, Definition/Shape catalogs, Materials, articulation metadata and import manifests, so runtime and external conversion tools share supported contracts without privileged shortcuts.

## Starting state

- Contracts supplied by `EP-000` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- Chunk generated cells and imported VoxelModel cells use the same stable Voxel::Cell/Definition identity semantics.
- Do not standardize one external authoring file as the runtime data model; importers are adapters.
- Derived mesh cache data is reproducible and may be invalidated by source/definition/mesher version.
- Current JSON Shape/Definition content remains readable during migration.
- Schema evolution is explicit and deterministic; no silent reinterpretation of old cells.
- Gameplay content cannot smuggle executable arbitrary code through Material/formula schemas.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Versioned voxel volume/model schema
- Definition/Shape and Material schema migration
- Articulation/import manifests
- Derived mesh-cache contract

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
- Chunk generated cells and imported VoxelModel cells use the same stable Voxel::Cell/Definition identity semantics.
- Do not standardize one external authoring file as the runtime data model; importers are adapters.
- Derived mesh cache data is reproducible and may be invalidated by source/definition/mesher version.
- Current JSON Shape/Definition content remains readable during migration.
- Schema evolution is explicit and deterministic; no silent reinterpretation of old cells.
- Gameplay content cannot smuggle executable arbitrary code through Material/formula schemas.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Versioned voxel volume/model schema | [ST-029-01](tickets/ST-029-01-versioned-voxel-volume-model-schema.md) | [ST-029-01](tickets/ST-029-01-versioned-voxel-volume-model-schema.md) |
| Definition/Shape and Material schema migration | [ST-029-02](tickets/ST-029-02-definition-shape-and-material-schema-migration.md) | [ST-029-02](tickets/ST-029-02-definition-shape-and-material-schema-migration.md) |
| Articulation/import manifests | [ST-029-03](tickets/ST-029-03-articulation-import-manifests.md) | [ST-029-03](tickets/ST-029-03-articulation-import-manifests.md) |
| Derived mesh-cache contract | [ST-029-04](tickets/ST-029-04-derived-mesh-cache-contract.md) | [ST-029-04](tickets/ST-029-04-derived-mesh-cache-contract.md) |

## Ticket index

- [ST-029-01 — Versioned voxel volume/model schema](tickets/ST-029-01-versioned-voxel-volume-model-schema.md) — Define versioned runtime content for arbitrary dimensions, uniform voxelSize and packed Voxel::Cell data.
- [ST-029-02 — Definition/Shape and Material schema migration](tickets/ST-029-02-definition-shape-and-material-schema-migration.md) — Evolve current atlas-bound definitions toward Material bindings without breaking current content fixtures.
- [ST-029-03 — Articulation/import manifests](tickets/ST-029-03-articulation-import-manifests.md) — Version imported asset mapping, assembly anchor hierarchy, attachments, model references, and source provenance.
- [ST-029-04 — Derived mesh-cache contract](tickets/ST-029-04-derived-mesh-cache-contract.md) — Allow optional serialized/cacheable mesher output without making it the authoritative model representation.

## Epic integration acceptance tests

1. Existing Shape/Definition fixtures validate under the revised content pipeline.
2. A runtime-sized model volume serializes/reloads dimensions, voxelSize and packed cells without semantic change.
3. Stable Definition IDs referenced by imported model cells resolve identically after content reload.
4. Unknown schema version or missing Definition fails before partial asset publication.
5. Derived mesh cache is rejected/rebuilt when its source/Definition/mesher version is incompatible.
6. Articulation/attachment metadata detects cycles/missing part IDs before runtime use.
7. Material sampling-space/configuration fields round-trip deterministically.
8. Headless server can load authoritative gameplay content without loading presentation-only model/material GPU data.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-017](../../open-decisions/OD-017-rich-shape-mapping-metadata-for-imported-assets.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
- [OD-021](../../open-decisions/OD-021-serialized-derivative-mesh-cache-versus-rebuild-on-load.md) — Status at ticket authoring: Open. The fixed contracts in this ticket may proceed; behavior requiring the final choice remains blocked.
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
