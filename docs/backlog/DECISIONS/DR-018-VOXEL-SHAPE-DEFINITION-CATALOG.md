# DR-018 — Shared voxel Shape, Definition, and Catalog contract

**Status:** Resolved
**Date opened:** 2026-09-23
**Date resolved:** 2026-09-23
**Applies to:** Core voxel resources, EP-001 Definition/Shape loading, Cell Orientation/Flip semantics

## Context

ST-001-04 needs one shared, headless-safe representation that both Server and Client can load from JSON. The archived prototype is useful reference material, but its runtime IDs, float geometry, automatic polygon reorientation, UV ownership, and canonical +Z orientation are not the new contract.

The project owner clarified that Shape slots and Definition material bindings are semantic voxel data rather than Client-only rendering data. Server may not use all of that information, but both products load the same shared Shape/Definition resources. Only the eventual realization of a material into palette/texture/shader/GPU resources is Client-owned.

## Decision

### Shared Shape semantics

`Voxel::Shape` is immutable semantic geometry shared by Core, Server, and Client.

- `Voxel::Shape::ID` is a string identifier owned by the Shape catalog, not stored by the Shape object.
- A Shape contains polygons.
- Each polygon contains:
  - `std::vector<spk::Vector3Int>` vertices;
  - a non-empty semantic slot name;
  - a cached `spk::Vector3` normal derived from the final stored vertices.
- Shape JSON is authored in normalized voxel-local floating coordinates in `[0.0, 1.0]`.
- `Voxel::Shape::VertexPrecision` is `0.001f`.
- Parsing derives the integer scale by rounding `1.0f / VertexPrecision`; the current scale is therefore 1000.
- Each valid JSON coordinate is multiplied by that scale and converted to `std::int32_t` by normal C++ conversion, intentionally truncating toward zero. Because valid authored coordinates are non-negative, this is equivalent to rounding down.
- Runtime Shape vertices are therefore discrete and deterministic after parsing.
- A polygon requires at least three vertices, must be non-degenerate, planar, and convex.
- Concave surfaces are authored as multiple convex polygons.
- JSON vertex order is authoritative and is authored counter-clockwise for the intended face direction.
- The loader never silently reverses the authored base polygon.
- Normals are never authored in JSON; they are computed during Shape parsing from the discrete CCW geometry.

This ticket does not define occlusion algorithms, partial polygon subtraction, side-coverage metadata, or an occlusion-result cache.

### Orientation and vertical flip

Every authored Shape is canonical `PositiveX + PositiveY`.

`Voxel::Cell::Orientation` is revised so the numeric value is directly the number of counter-clockwise quarter-turns around +Y, viewed from +Y toward the voxel:

- `PositiveX = 0`;
- `NegativeZ = 1`;
- `NegativeX = 2`;
- `PositiveZ = 3`.

The packed Cell bit allocation remains unchanged: bits 29-30 still store the two-bit Orientation value. This ordering supersedes only the previous Orientation value/name mapping in DR-012.

`Voxel::Cell::FlipOrientation` remains:

- `PositiveY = 0`;
- `NegativeY = 1`.

Let `M = round(1.0f / Voxel::Shape::VertexPrecision)`, currently 1000. Horizontal transforms around the voxel center are exact integer transforms:

- `PositiveX` / 0°: `x' = x`, `z' = z`;
- `NegativeZ` / 90° CCW: `x' = z`, `z' = M - x`;
- `NegativeX` / 180°: `x' = M - x`, `z' = M - z`;
- `PositiveZ` / 270° CCW: `x' = M - z`, `z' = x`.

`NegativeY` mirrors around the plane `Y = 0.5` using `y' = M - y`. Mirroring reverses handedness, so a mirrored polygon's vertex order is reversed before its final normal is recomputed. Slots stay attached to their original polygons through every rotation and flip.

### Oriented polygon cache

Each Shape owns exactly eight potential oriented polygon arrays: four horizontal orientations × two vertical flip states.

The public concept is:

```cpp
struct OrientedPolygonArray
{
    std::atomic<spk::UUID> uuid{spk::UUID::null()};
    std::vector<Polygon> polygons;
};
```

and the Shape stores a mutable fixed array of eight of these entries plus an internal mutex.

- `PositiveX + PositiveY` is materialized during Shape construction.
- The other seven entries are generated lazily on first request.
- `spk::UUID::null()` means that oriented entry has not yet been published.
- On a cache miss, the Shape locks its orientation-cache mutex, checks the UUID again, completely builds the polygon vector, stores the completed polygons, then publishes a newly-generated UUID last with release semantics.
- Readers load the UUID with acquire semantics.
- Once a UUID is non-null, that oriented polygon array is immutable and subsequent access does not take the mutex.
- `std::atomic<spk::UUID>` requires `spk::UUID` to be trivially copyable; the implementation must enforce that expectation at compile time.
- `Shape::orientedPolygons(orientation, flip)` returns `const OrientedPolygonArray&`.
- The returned reference is non-owning and is valid only while the originating Shape remains alive; callers must not retain it beyond a lifetime they can guarantee.
- The UUID is stable for the lifetime of the materialized oriented entry and may later be used as identity by separate caches. ST-001-04 does not implement those later caches.

### Definition and material semantics

`Voxel::Definition::ID` remains `std::uint32_t` and is owned by the Definition catalog rather than stored by the Definition object.

A non-air Definition:

- resolves a Shape string ID once during loading;
- retains a fast immutable shared reference to that Shape;
- maps Shape slot names to semantic `Voxel::Material::ID` values.

`Voxel::Material::ID` is a string identifier. The first shared material sentinel is:

```cpp
Voxel::Material::InvalidID == "InvalidID"
```

Actual material-catalog/resource resolution is later work.

For Definition slot validation:

- multiple polygons may use the same Shape slot;
- a Shape slot missing from the Definition does not invalidate loading: log a warning through `spk::Logger` and bind that slot to `Material::InvalidID`;
- a Definition slot that does not exist on its Shape is invalid and throws `spk::Exception`;
- empty slot names are invalid;
- whether a non-empty material ID exists in a future Material catalog is not validated by ST-001-04; later material resolution logs an error and substitutes `Material::InvalidID`.

Definition ID 0 is reserved for Air.

- The Definition catalog creates ID 0 automatically on construction.
- Air has no Shape and no slot bindings.
- `at(0)`, `operator[](0)`, `contains(0)`, and `tryGet(0)` treat Air as a valid existing Definition.
- Authored JSON may not declare Definition ID 0.

### Voxel::Catalog and resource loading

The owning aggregate is `Voxel::Catalog`. It owns the Shape and Definition catalogs and keeps the Shape catalog alive for every Definition that references a Shape.

The required usage includes:

```cpp
myVoxelCatalog.load(shapePath, definitionPath);
myVoxelCatalog.loadShape(shapePath);
myVoxelCatalog.loadDefinition(definitionPath);

myVoxelCatalog.shapes().at(shapeId);
myVoxelCatalog.definitions().at(definitionId);
```

Public resource paths use `std::filesystem::path`.

The two typed subcatalogs share generic catalog behavior. Exact internal helper-template naming is not a public contract. Their observable read API is:

- checked `at(ID)` returning `const T&`;
- checked `operator[](ID)` returning `const T&`;
- `contains(ID)`;
- `tryGet(ID)` returning `const T*` and `nullptr` when missing;
- repeated JSON loading that appends new IDs.

Missing checked lookup and duplicate IDs throw `spk::Exception`.

Loading is intentionally incremental rather than transaction-wide:

- successfully inserted earlier elements remain present if a later element in the same file fails;
- the currently failing element is not inserted;
- loading stops immediately and throws;
- values loaded before the current call also remain untouched.

`Voxel::Catalog::load(shapePath, definitionPath)` loads Shapes first, then Definitions. If Shape loading throws, Definition loading is not started.

### JSON schema

Shape files use:

```json
{
  "elements": [
    {
      "id": "cube",
      "data": {
        "polygons": [
          {
            "slot": "side",
            "vertices": [
              {"x": 0.0, "y": 0.0, "z": 0.0},
              {"x": 0.0, "y": 1.0, "z": 0.0},
              {"x": 0.0, "y": 1.0, "z": 1.0}
            ]
          }
        ]
      }
    }
  ]
}
```

Definition files use:

```json
{
  "elements": [
    {
      "id": 1,
      "data": {
        "shape": "cube",
        "slots": {
          "top": "grass",
          "side": "dirt",
          "bottom": "stone"
        }
      }
    }
  ]
}
```

The outer catalog layer owns `id` parsing and passes only the `data` reader to the final object constructor.

Malformed schema/content throws `spk::Exception` with useful source/path context. This includes unknown fields, missing/wrong-type `elements`, empty Shape IDs, Definition ID 0, Definition IDs above the Cell 29-bit capacity, duplicate IDs, unknown Shape references, empty polygon slots, missing/wrong-type vertices, missing x/y/z components, coordinates outside `[0,1]`, fewer than three vertices, duplicate adjacent vertices, zero-area/degenerate polygons, non-planar polygons, and concave polygons.

### First Shape resources

ST-001-04 reintroduces the validated archived `cube`, `slab`, `slope`, and `stair` geometry from `archive/resources/voxels/shapes.json` as the first active Shape fixtures.

- `cross` is intentionally excluded.
- UV data is not part of the new shared Shape contract.
- Preserve the archived geometry/topology and slot grouping, but adapt polygon winding to the new authoritative CCW rule where the archived loader previously corrected it automatically.
- The archived slope and stair were authored in the archived canonical +Z direction. Rotate their fixture geometry once into the new canonical +X direction before using them as active resources.
- The archived slab remains half-height and the archived stair remains the validated two-step shape.

Exact generator-world coordinates, Definition IDs used by the validation scene, and material choices for ST-001-06 remain in OQ-039 and are not fixed by this decision.

## Consequences

- Server and Client consume the same semantic Shape/Definition resource schema.
- Shape geometry is exact integer data after parsing while authoring remains normalized and human-readable.
- Cell Orientation values directly encode CCW quarter-turn count.
- Shape variants are transformed once and reused.
- Material slots move with their polygons.
- Definition lookup remains compact through Cell Definition IDs while Shape lookup remains author-friendly through string IDs.
- Resource loading is simple and additive, with visible partial progress if a later element fails.
- Occlusion policy/algorithms remain deliberately deferred.

## Required tests

ST-001-04 must cover:

- revised Cell Orientation numeric values and packed round trips;
- JSON float-to-discrete vertex conversion at `VertexPrecision = 0.001f`;
- canonical cube/slab/slope/stair loading;
- all eight oriented variants for an asymmetric Shape;
- mirror rewinding and final normal correctness;
- slot preservation through every transform;
- lazy-cache UUID publication, stable repeated UUID/reference behavior, and concurrent first access;
- valid/invalid Shape polygon rules;
- Air Definition ID 0;
- Definition Shape resolution and retained lifetime;
- missing slot warning + `Material::InvalidID`;
- extra slot rejection;
- checked/missing/optional catalog lookup;
- duplicate IDs across one or multiple loads;
- incremental load failure semantics;
- malformed JSON/schema failures with source/path diagnostics.

## Resolution provenance

Resolved directly with the project owner during ST-001-04 contract review on 23 September 2026.

## Supersession

This record supersedes only the Orientation enum value/name ordering previously recorded in DR-012. DR-012's packed bit allocation, Definition-ID capacity, Cell raw-value semantics, and Volume contract remain active.
