# ST-001-04 — Runtime-sized VoxelModel storage

## Intent

Introduce the semantic `VoxelModel` volume type for imported props and rigid assembly parts, reusing the owning vector storage, checked read contract, and batched editor/version behavior supplied by ST-001-02.

## Execution decision policy

- Do not invent unspecified asset, serialization, editing, or ownership behavior.
- Pause and record any missing observable policy as an Open Decision.
- Preserve `Voxel::Cell` byte-for-byte under resolved OD-023.

## Starting state / prerequisites

- ST-001-02 owning VoxelVolume/read contract is complete.
- The model dimensions and voxel size are runtime data, not template parameters.

## Owned behavior

- A `VoxelModel` is a semantic volume type with arbitrary valid runtime dimensions and uniform scale.
- Its cells use the same `Voxel::Cell` representation and storage/access rules as VoxelVolume and Chunk.
- It inherits the generic controlled editor/version behavior without adding a model-specific mutation API.
- A provisional `spk::JSON::Reader` constructor loads dimensions, uniform scale, and sparse cell entries for early model fixtures.
- Sparse entries contain integer `position`, numeric `value`, required named `orientation`, and optional named `flip` defaulting to `positive-y`.
- Loading rejects unknown fields and enum names, duplicate/out-of-range positions, and values outside packed Cell capacity with file/path diagnostics.

### Provisional JSON prototype

```json
{
  "dimensions": [8, 8, 16],
  "voxelSize": 0.1,
  "voxels": [
    {
      "position": [2, 3, 4],
      "value": 17,
      "orientation": "negative-x",
      "flip": "negative-y"
    }
  ]
}
```

Supported orientations are `positive-z`, `positive-x`, `negative-z`, and `negative-x`. Supported flips are `positive-y` and `negative-y`; `flip` may be omitted. Empty positions are absent from `voxels`. This prototype deliberately uses numeric runtime values and does not yet map stable Definition names through a Catalog.

## Explicitly not owned

- Stable file format/serialization compatibility, direct file loading, external authoring tool, importer, stable asset identity, or Definition-name/catalog mapping.
- Anchors, pivots, hierarchy, animation, equipment slots, world transform, rendering, Palette ownership, GPU resources, or meshing.
- Chunk world/streaming/generation behavior.

## Behavioral acceptance

- [x] Models with dimensions `10×10×14`, `12×7×14`, and `32×32×22` use the same C++ type.
- [x] An `8×8×16` model at scale `0.1` reports local bounds `0.8×0.8×1.6` within floating tolerance.
- [x] Inherited model editing stores and retrieves the same packed `Voxel::Cell` values through the public read contract and publishes one invalidation per changed session.
- [x] Invalid dimensions, scale, and coordinates follow the validation policy inherited from VoxelVolume.
- [x] Model access and bounds run headlessly without a Definition catalog, Window, OpenGL context, or GPU ownership.
- [x] No model-specific concept enters Chunk, VoxelVolume, or world lookup APIs.
- [x] The provisional Reader constructor loads sparse numeric values and all four named horizontal orientations without changing packed Cell semantics.
- [x] Optional `flip` accepts `positive-y`/`negative-y` and defaults to `positive-y` when absent.
- [x] Sparse omissions remain empty, while duplicate/out-of-range positions, unknown enum names, and packed-capacity overflow are rejected with the failing JSON path.

## Rendering impact

None. This ticket creates headless model data only and does not require a golden image.

## Open decisions

- [OD-011](../../../open-decisions/OD-011-c-ownership-and-view-design-for-voxelvolume.md) — Resolved: VoxelModel uses the concrete vector-owning VoxelVolume base.
- [OD-023](../../../open-decisions/OD-023-whether-cell-orientation-and-flip-must-be-expanded.md) — Resolved: preserve current packing.
- Any importer/serialization/editing policy beyond the smallest required in-memory behavior remains a separate decision for its owning ticket.
- The project owner approved the provisional sparse Reader schema and named orientation/flip encoding on 20 September 2026. It is a prototype input contract, not the stable asset format owned by EP-029/EP-030.

## Completion evidence

- Four `VoxelModelTest` cases cover representative runtime dimensions, scale/bounds, packed cell values with one invalidation, and inherited rejection cases.
- Five `VoxelModelJSONTest` cases cover sparse loading, all orientation/flip names and defaults, duplicate/out-of-range positions, unknown fields/enum names, and packed-capacity overflow.
- Implementation head: [`6d413e6`](https://github.com/EreliaStudio/Playground/commit/6d413e651b6f137ad17406692ed0f508500ab8b7), delivered by [PR #5](https://github.com/EreliaStudio/Playground/pull/5).
- [CI run 35497085545](https://github.com/EreliaStudio/Playground/actions/runs/35497085545) passed the CPU/headless regression suite and the unchanged Windows/OpenGL golden suite.
