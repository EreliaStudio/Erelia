# ST-003-02 — Palette Resource Binding and Unified Voxel Shader Contract

## Intent

Introduce the accepted per-Palette GPU binding architecture for both Chunk and VoxelModel rendering while retaining the current atlas path until structural mesh parity is proven.

## Execution decision policy

- Do not invent, infer, or silently select behavior, data, assets, values, formulas, schemas, ownership, error policy, architecture, or technology that this ticket or an already-resolved linked decision does not define.
- Treat every unspecified choice that can affect public behavior, compatibility, persistence, rendering, tests, or later tickets as an Open Decision.
- Stop before implementing the affected portion, ask the user a focused clarification question, and record the approved answer in the applicable OD file—or create/link a new OD record when none exists.
- Continue only with work that is independent of the missing decision.
- Select an implementation detail without clarification only when this ticket explicitly delegates it and the choice cannot alter an observable contract.

## Starting state / prerequisites

- The dependency contracts `EP-001, EP-032` used by this story are available; an epic dependency denotes a required contract, not global waterfall completion.
- The test begins from an explicitly constructed, valid state with no pending operation unless the case says otherwise.
- Rendering, authoring data, and display names are not authoritative gameplay state.

## Owned behavior

- Represent each `Palette` as an independent CPU-side resource associated with its own GPU SSBO.
- Make `VoxelRenderCommand` reference and bind the Palette required by the current renderable.
- Feed a flat integer `paletteElementIndex` from `VoxelMesh` to the shared voxel shader.
- Bind a required Palette reference without rescanning prepared mesh data; OD-027 makes compatibility a caller precondition.
- Keep Palette selection independent from pure VoxelModel volume data and cached mesh geometry.
- Preserve the current textured/atlas rendering path until the deliberate palette visual migration.

## Explicitly not owned

- Behavior assigned to sibling tickets or later epics.
- Resolution of linked Open Decisions.
- New balance values, content, schemas, or platform choices not approved by the user.
- CPU ownership syntax such as `shared_ptr` versus another safe resource handle; this remains an implementation choice only if lifetime and observable behavior stay identical.
- Exact SSBO wrapper type, binding number, packed vertex layout, Palette sorting, and redundant-bind optimization.
- Palette variation/sampling policy, advanced Material effects, and serialized derivative mesh-cache policy.
- Shape-to-WorldPalette content-schema precedence, which requires its own approved example and ticket contract.

## Implementation inputs

### Already defined values/data

- Story intent: bind the Palette required by each voxel render command and use one shared Chunk/VoxelModel shader contract.
- Required dependency contracts: EP-001, EP-032.
- Test fixture rule: Use the smallest deterministic fixture that exposes the owned behavior. Record exact dimensions, cell coordinates, palette indices, transforms, expected vertices/state, and stable asset IDs in the test source; do not substitute an unspecified representative asset.

### Confirmed architecture decisions

These choices are closed for this ticket and must not be redesigned during implementation:

1. `PaletteCollection`, if present, is CPU-side resource management only. The GPU does **not** receive one global PaletteCollection SSBO.
2. Every `Palette` is independently bindable and owns or is associated with its own SSBO.
3. A `VoxelRenderCommand` references the Palette required by the current renderable. Execution is conceptually: bind voxel program → bind mesh vertex/index buffers → bind required Palette SSBO → bind transform/render data → draw.
4. `VoxelMesh` semantically carries at least `position`, `normal`, and integer `paletteElementIndex`. Exact packing is deliberately not fixed here.
5. `paletteElementIndex` addresses the currently bound Palette and is transported as a flat/non-interpolated integer. It is not a Voxel ID.
6. The initial shader contract has no GPU `paletteId` and performs no PaletteCollection lookup.
7. Chunk and VoxelModel rendering use the same VoxelMesh vertex semantics and the same voxel shader.
8. Palette selection belongs to the renderable/part instance/render command, never to the pure `.vxm` volume asset.
9. The same cached mesh may be rendered with multiple compatible Palettes. Changing the Palette does not change geometry, invalidate the mesh, or invoke `VoxelMesher`.
10. Chunks normally share one relatively large `WorldPalette`; models/equipment may use smaller Palettes. This does not create a different shader path.
11. Every Palette used with a mesh must contain all referenced element indices. As superseded by OD-027, this is a caller precondition rather than a render-command validation pass.
12. `MaterialResolver` selects an element per emitted polygon. For Chunks it may use both semantic `materialSlot` and spatial `outerSide`; these concepts remain distinct.
13. All vertices of one emitted polygon normally carry the same resolved element index.
14. Palette grouping or redundant-bind avoidance may be added later as a measured optimization without changing this resource model.
15. OD-026 selects one RGBA base color as the initial Palette element; effects beyond it remain blocked by OD-018.
16. `MaterialResolver` receives immutable volume, Definition, transformed-polygon, coordinate, and Cell context and returns only the Palette element index without defining content precedence.
17. `VoxelRenderCommand` stores a Palette value copy; Sparkle's shared GPU-resource copy semantics retain the exact SSBO through deferred execution. OD-027 removes the original validation step and changes input from a pointer to `const Palette&`.
18. `Palette` owns `ShaderStorageBuffer` by composition and retains its typed `View<void, Palette::Data>`. `Palette` is default-constructible; `resize(count)` establishes its runtime number of `Data` elements, after which entries are edited through `data(index)`. Neither `resize()` nor mutable access implicitly validates the GPU resource; the caller explicitly invokes `Palette::validate()` once the desired CPU-side state is ready. Sparkle inheritance remains separate [issue #5](https://github.com/EreliaStudio/Sparkle/issues/5).

### User validation required before implementation

No user clarification is required for the confirmed choices above. Ask the user before implementing any unspecified behavior, especially Palette serialization, ownership that changes lifetime semantics, or Shape/material mapping precedence. Before accepting a changed image baseline, present old, produced, and difference images for human approval.

## Behavioral contract

For every draw, the command receives a required Palette reference, retains its shared-resource value copy, binds that Palette's SSBO, and draws through the common voxel shader. It trusts the prepared mesh/Palette pair and performs no compatibility scan; valid element indices are a caller precondition. Palette changes affect material output only; mesh bytes, mesh-cache identity, source volume, and mesher invocation count remain unchanged.

## Acceptance tests

### Nominal behavior and integration interactions

- [x] Given the declared fixture and valid dependency state, when the public operation is performed, `One generalized mesh representation can be rendered for both a Chunk-derived mesh and a VoxelModel-derived mesh.` is observed exactly; no private-state shortcut is used.
- [ ] Given the declared fixture and valid dependency state, when the public operation is performed, `Object-local material variation moves with a barrel/sword and does not swim as the object moves through world space.` is observed exactly; no private-state shortcut is used.
- [x] Existing atlas-backed Definition fixture continues to render during migration.
- [x] A base-color/palette Material can render a new definition without changing mesher logic.
- [x] Required Palette input is represented by `const Palette&`; a missing binding is not representable through the command API.
- [x] Per OD-027, the command accepts prepared render data without a mesh scan, fallback, clamping, or compatibility rejection path.
- [x] Identical deterministic inputs produce identical observable results.
- [x] Valid lower and upper referenced Palette elements render through the same unchecked command path.
- [x] Command construction only copies the mesh, Palette resource handle, and transform; it does not mutate or validate caller-owned state.
- [x] Repeating the same seed, configuration, starting snapshot, and ordered commands produces the same result and event order.
- [ ] Create → use → serialize where applicable → unload → restore/recreate → retry preserves stable IDs and does not duplicate the operation.
- [x] The nearest upstream and downstream contracts named in prerequisites are exercised together; dependency failure follows the documented fail-closed or rollback behavior.

- [x] `VoxelRenderCommand` binds one independently owned Palette SSBO for the current renderable; the shader uses flat integer `paletteElementIndex` and has no global PaletteCollection buffer or GPU `paletteId` lookup.
- [x] The same cached mesh rendered with `HumanPalette` and `OrcPalette` produces two reviewed images while mesh bytes and mesh-cache identity remain identical.
- [x] `VoxelRenderCommand` accepts a Palette reference and performs no empty-Palette or element-index validation; compatible prepared inputs are required by OD-027.
- [x] Chunk and VoxelModel commands use the same vertex semantics and shader; only their bound Palette resources and transforms differ.
- [x] A command trace proves the binding order: voxel program → vertex/index buffers → selected Palette SSBO → transform/render data → draw.
- [x] Two consecutive commands using different Palettes observe their own Palette values; command B cannot sample command A's buffer.
- [x] Two consecutive commands sharing `WorldPalette` may avoid a redundant bind only if the observable command result is unchanged.
- [x] A polygon with element `2` carries integer `2` on all of its vertices and reaches fragment processing without interpolation.
- [x] Model and Chunk meshes using element `2` both read entry `2` from their currently bound, potentially different, Palettes.
- [x] Changing a renderable from `HumanPalette` to compatible `OrcPalette` leaves positions, normals, indices, cache key, source model, and mesher-call counter unchanged.
- [x] Mesh/Palette compatibility is not recomputed in the command, mesher, or shader; the caller must not submit index `3` against a three-entry Palette.
- [x] A caller-side Palette object may be destroyed after command construction because the command's value copy retains the shared GPU resource.

### Boundaries and invalid/rejected operations

- [x] Missing Palette input is excluded by the reference API; other malformed-content, ID, ownership, version, and enum policies are outside this render-command contract.
- [x] Minimum and maximum valid referenced elements are exercised; empty/incompatible Palette relationships are caller errors and are deliberately not revalidated here.

### Determinism and lifecycle / retry / persistence

- [x] Deterministic iteration never depends on pointer values, hash-table accident, render frame rate, or wall-clock timing.
- [x] A duplicate/retried operation is either idempotent or rejected as already applied, according to the story contract, without duplicating state or events.

### Rendering / golden images

- [x] Render the deterministic Palette fixtures at their approved fixed `640 × 480` viewport; compare against their reviewed PNGs with the tolerance recorded by the Sparkle TestLibrary fixture. On failure, retain the expected images and publish actual/difference images for review.
- [x] Assert semantic geometry/material/transform values independently of the PNG comparison.

## Rendering impact

Yes. The controlled fixture uses a fixed camera, viewport, asset set, lighting, and supported GPU runner. Structural refactors must match the reviewed current baseline; intentional migrations require human review before a new versioned baseline is accepted.

## Open decisions

- [OD-018](../../../open-decisions/OD-018-first-material-effects-beyond-palette-color.md) — Does not block base Palette binding; blocks only effects beyond the agreed Palette material lookup.
- [OD-019](../../../open-decisions/OD-019-palette-variation-algorithm-and-sampling-declaration.md) — Does not block direct element lookup; blocks procedural/variation sampling behavior.
- [OD-021](../../../open-decisions/OD-021-serialized-derivative-mesh-cache-versus-rebuild-on-load.md) — Does not block runtime Palette binding; blocks only serialized derivative mesh-cache policy.
- [OD-026](../../../open-decisions/OD-026-initial-palette-element-resolver-and-command-lifetime.md) — Resolved: RGBA base element, full immutable resolver context, and value-copy command lifetime.
- [OD-027](../../../open-decisions/OD-027-render-command-palette-trust-boundary.md) — Resolved: the render command accepts `const Palette&` and trusts prepared compatibility without rescanning mesh vertices.

## Completion evidence

- Implementation commits [`408d379`](https://github.com/EreliaStudio/Playground/commit/408d379a2dbea6358a6496c1da48855cbaac2c01), [`3538309`](https://github.com/EreliaStudio/Playground/commit/35383096d7b3e25f69bd084fe55c8be6d21f4278), and [`b5cbcdc`](https://github.com/EreliaStudio/Playground/commit/b5cbcdc814a608cf493b0cefe4eb9b2592997bf1) are delivered by [PR #9](https://github.com/EreliaStudio/Playground/pull/9).
- [CI run 35538640066](https://github.com/EreliaStudio/Playground/actions/runs/35538640066) passes both `CPU/headless tests` and `Windows/OpenGL golden candidates` against the pinned Sparkle `0.1.2` package before baseline promotion.
- Focused tests cover typed Palette storage/editing and lifetime, integer vertex attributes, resolver context, material-safe compatible-vertex reuse, deterministic Chunk/VoxelModel parity, and no-remesh Palette/transform swaps. OD-027 deliberately removes render-command compatibility-rejection tests.
- The GPU lane compiles the shared shader, draws Human and Orc Palettes sequentially from one mesh, draws a Chunk with WorldPalette, and retains the intentional textured-to-Palette difference image.
- Existing 32 approved textured references and tolerances are unchanged. Final-run artifact `10613866642` contains actual/difference evidence; artifact `10614355465` contains the four proposed Palette expected images.
- On 21 September 2026, the project owner approved the four exact `640 × 480` Palette candidates from CI run `35538640066`. They are now checked in under `tests/resources/expectedImages/palette_migration/`, and the candidate-only capture test now compares fresh renders against those immutable references while preserving actual/difference evidence on failure.
- Approved SHA-256 values are `6cf479148d4cba5748add39f43fada8b926c10c55b6e1dd60138f9211eb5ed7d` (Human), `b3a814cefa25313bb964446eb3e854195309fd6b855a6e55478709869af8809d` (Orc), `da063bdd24ee4da0bc5ce05e6317476a6fd15ac636a30fbd4967f593e230315c` (sequential pair), and `400d2dd0cde766122d865ec9dc62b2af3a827004d0e77243f6365276796f03c0` (WorldPalette Chunk).
- [CI run 35572632309](https://github.com/EreliaStudio/Playground/actions/runs/35572632309) passes both complete lanes after OD-027 removes command-side compatibility scans and the four approved Palette images become enforced golden comparisons.
- The barrel/sword wording belongs to later ST-033-02 object-local material content and remains open; ST-003-02 proves the underlying per-draw transform/Palette contract with the deterministic model fixture.
