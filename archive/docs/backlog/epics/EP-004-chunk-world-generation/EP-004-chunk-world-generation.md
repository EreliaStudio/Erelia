# EP-004 — Deterministic Chunk World Runtime

**Dependencies:** EP-001, EP-032  
**Dependency semantics:** only the named contracts are required; this is not a mandatory waterfall requiring every earlier epic to be globally complete.

## Purpose

Preserve the current headless 16³ Chunk world runtime while transitioning production consumers from `Chunk::Baker` to the validated `Chunk::Mesher` supplied by consolidated ST-032-02.

## Starting state

- Contracts supplied by `EP-001, EP-032` exist at the narrowest story/API level needed by this epic.
- Authoritative state is valid, deterministic test fixtures are available, and unresolved choices remain explicit decision gates.
- Simulation/domain code remains executable headlessly without a Window, Surface, OpenGL context, or GPU.
- Presentation is a view of authoritative state and must never create gameplay outcomes.
- Stable content IDs are distinct from display names and file-system paths.
- Invalid commands, content, or assets fail before partial authoritative mutation.
- Open GDD values remain configuration or explicit decision gates rather than silently invented constants.
- Chunk remains a semantic world specialization with 16³ cells and voxelSize 1.0 for the baseline world.
- Generation, streaming, collection membership and world/local coordinate conversion remain outside generic volume code.
- Same ServerSeed + WorldID + generation/content version yields the same immutable base cells.
- `Chunk::Collection` remains the source of cross-Chunk world cell availability; only the `Chunk::Mesher` subclass depends on it, while base VoxelMesher remains world-agnostic.
- World runtime remains headless and does not own GPU meshes.
- Current Chunk editing/version/bake scheduling behavior is preserved or migrated under explicit tests.

## Target state

- Every subject in the coverage matrix has an implementation owner and executable acceptance evidence.
- Invalid operations fail before partial mutation; deterministic inputs reproduce state, events, and visible output where applicable.
- Ticket-specific user inputs and decisions are recorded rather than invented by implementers.

## Scope

- Chunk::Mesher outside-neighbor override and cross-Chunk lookup
- World/local coordinate conversion
- Bake scheduling and dirty/version propagation
- Streaming and generation integration without duplicate VoxelVolume adaptation

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
- Chunk remains a semantic world specialization with 16³ cells and voxelSize 1.0 for the baseline world.
- Generation, streaming, collection membership and world/local coordinate conversion remain outside generic volume code.
- Same ServerSeed + WorldID + generation/content version yields the same immutable base cells.
- `Chunk::Collection` remains the source of cross-Chunk world cell availability; only the `Chunk::Mesher` subclass depends on it, while base VoxelMesher remains world-agnostic.
- World runtime remains headless and does not own GPU meshes.
- Current Chunk editing/version/bake scheduling behavior is preserved or migrated under explicit tests.

## Subjects / capabilities covered

| Subject / capability | Implemented by tickets | Tested by tickets |
|---|---|---|
| Transition current Chunk mesh consumers from Baker to Chunk::Mesher | [ST-004-01](tickets/ST-004-01-chunk-mesher-integration.md) | [ST-004-01](tickets/ST-004-01-chunk-mesher-integration.md) |
| Chunk::Mesher external-neighbor override, cross-Chunk lookup, and world/local conversion | [ST-032-02](../EP-032-unified-voxel-mesher/tickets/ST-032-02-generic-cell-iteration-and-scale-aware-shape-transform.md) | [ST-032-02](../EP-032-unified-voxel-mesher/tickets/ST-032-02-generic-cell-iteration-and-scale-aware-shape-transform.md) |
| Deterministic generation and streaming | [ST-004-03](tickets/ST-004-03-deterministic-world-generation-streaming.md) | [ST-004-03](tickets/ST-004-03-deterministic-world-generation-streaming.md) |
| Bake scheduling, dirty/version propagation, and neighbor invalidation | [ST-004-04](tickets/ST-004-04-bake-scheduling-after-unified-mesher.md) | [ST-004-04](tickets/ST-004-04-bake-scheduling-after-unified-mesher.md) |

## Ticket index

- [ST-004-01 — Transition Chunk consumers from Baker to Chunk::Mesher](tickets/ST-004-01-chunk-mesher-integration.md) — Migrate scheduler/application/view consumers only after consolidated mesher parity is approved.
- [ST-004-02 — Superseded by consolidated ST-032-02](tickets/ST-004-02-chunk-occlusion-resolver.md) — Stable historical link; owns no remaining implementation.
- [ST-004-03 — Deterministic World generation/streaming](tickets/ST-004-03-deterministic-world-generation-streaming.md) — Keep seeded generation, request/publish/remove and regeneration behavior independent of rendering.
- [ST-004-04 — Bake scheduling after unified mesher](tickets/ST-004-04-bake-scheduling-after-unified-mesher.md) — Route Chunk mesh requests through EP-032 while preserving dirty/version and neighbor invalidation semantics.

## Epic integration acceptance tests

1. Existing stable Chunk generation fixture is unchanged by adapting Chunk to the volume contract.
2. Chunk dimensions remain exactly 16³ and baseline voxelSize is exactly 1 world unit.
3. Chunk resolver maps an out-of-bounds local neighbor to the correct adjacent Chunk cell when available.
4. Missing/unavailable neighboring Chunk follows the documented current/fallback visibility policy deterministically.
5. Replacing `Chunk::Baker` use with VoxelMesher + Chunk resolver preserves representative current mesh semantics.
6. Same seed/content/generation version regenerates the same base Chunk cells after unload/reload.
7. World generation and Chunk collection run without Material/GPU/model-import dependencies.
8. Editing one Chunk increments/version-dirties it and required neighbor bake scheduling without mutating immutable generated source rules.

Additional integration invariants:

- Invalid input is rejected without partial state, event, resource, or scheduling mutation.
- Exact supported boundaries and one-step-outside values are exercised using explicit fixture configuration.
- Same seed/configuration/snapshot/commands produce the same result and ordering.
- Create → use → unload → restore/recreate is free of stale registrations and duplicate effects.
- Persistent state, where owned, preserves semantics and stable IDs across save/load and retry.
- Rendering comparisons supplement and never replace semantic/value assertions.

## Decision gates

- [OD-025](../../open-decisions/OD-025-voxelmesher-chunk-specialization-and-occlusion-cache.md) — Resolved: Chunk::Mesher overrides only outside-volume lookup and inherits the generic meshing/cache algorithm.

## Rendering validation

Graphical tickets use OD-024's deterministic `640 × 480` Sparkle TestLibrary comparisons. Structural refactors preserve reviewed baselines; no transition difference creates a replacement reference.

## Required user-provided inputs

- None beyond linked decisions and ticket-specific gates.

## Epic exit criteria

- [ ] Every ticket's behavioral, boundary, rejection, determinism, lifecycle, integration, rendering, and evidence sections pass.
- [ ] Every coverage-matrix row has implementation and test evidence.
- [ ] Applicable decisions are resolved in their persistent OD files, with provenance.
- [ ] No graphical baseline was replaced solely because a comparison failed.
