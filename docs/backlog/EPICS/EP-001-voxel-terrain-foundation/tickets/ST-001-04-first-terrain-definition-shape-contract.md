# ST-001-04 — First terrain Definition and Shape contract

**Status:** In Progress
**Epic:** EP-001
**Production target(s):** Core
**Test suite(s):** EreliaCoreTestSuite

## Intent

Implement the first shared, immutable voxel Shape/Definition resource contract needed by both Server simulation and Client meshing, including JSON loading, catalog lookup, discrete polygon geometry, deterministic Orientation/Flip variants, semantic material slots, and the first cube/slab/slope/stair Shape resources.

This ticket deliberately stops before occlusion algorithms, material rendering resources, terrain generation, and Client meshing.

## User / system value

A packed `Voxel::Cell` Definition ID and Orientation/Flip bits must resolve to the same immutable semantic voxel geometry and material-slot bindings everywhere in the project.

Server and Client must be able to load the same Shape/Definition JSON schema without duplicating or independently redefining voxel meaning.

## Starting state / prerequisites

- ST-001-01, ST-001-02, and ST-001-03 are Done and merged into `master`.
- `Voxel::Definition::ID` already aliases `std::uint32_t`.
- `Voxel::Cell` already reserves lower 29 bits for Definition ID, bits 29-30 for Orientation, and bit 31 for FlipOrientation.
- ST-001-04 intentionally revises the existing Orientation enum value/name ordering according to DR-018 while preserving the packed bit layout.
- Sparkle 0.1.3 provides `spk::JSON::Reader`, `spk::UUID`, `spk::Logger`, vector math, and `spk::Exception`.
- The archived resources at `archive/resources/voxels/shapes.json` are approved as geometry/topology reference for the first cube/slab/slope/stair resources, with the adaptations stated below.
- OQ-039 remains partially unresolved for the later terrain-generator scene. Its remaining world coordinates, scene Definition IDs, and material choices do **not** block this ticket.

## Product ownership

Core owns the shared semantic representation and resource loading:

- `Voxel::Shape`;
- `Voxel::Definition`;
- `Voxel::Material::ID` / invalid sentinel;
- `Voxel::Catalog`;
- immutable transformed Shape variants.

Server and Client consume the same semantic data. GPU resources, shaders, textures, palette realization, and render-material objects remain Client-owned later work.

## Allowed dependencies

C++ standard library and headless-safe Sparkle 0.1.3 facilities.

## Forbidden dependencies

- Client graphics/GPU types;
- Server authority/state;
- production editor/import pipeline;
- production Material catalog/render realization;
- occlusion algorithm/cache;
- terrain generator implementation;
- Client mesher implementation.

## Public contract

### `Voxel::Material`

Introduce the minimal shared material identity contract:

```cpp
Voxel::Material::ID
```

is a string identifier.

Reserve:

```cpp
Voxel::Material::InvalidID == "InvalidID"
```

for unresolved/invalid material bindings.

ST-001-04 does not implement a Material catalog or validate whether a non-empty material ID names a real material resource.

### `Voxel::Shape`

`Voxel::Shape` is immutable semantic geometry.

`Voxel::Shape::ID` is a string identifier used by the Shape catalog. The Shape object itself does **not** store its own catalog ID.

The Shape exposes:

```cpp
static constexpr float VertexPrecision = 0.001f;
```

A polygon semantically contains:

```cpp
struct Polygon
{
    std::vector<spk::Vector3Int> vertices;
    std::string slot;
    spk::Vector3 normal;
};
```

The exact member visibility/accessor arrangement may follow project style, but the observable data above is required and immutable after construction.

A Shape contains one or more polygons.

### JSON vertex conversion

Shape JSON expresses each vertex in normalized voxel-local floating coordinates in `[0.0, 1.0]`.

Derive the integer scale as:

```cpp
round(1.0f / Voxel::Shape::VertexPrecision)
```

which is 1000 for the current precision.

For each JSON component:

1. validate the authored numeric value is within `[0.0, 1.0]`;
2. multiply by the derived scale;
3. convert to `std::int32_t` using normal C++ conversion.

Because valid input is non-negative, conversion intentionally rounds down/truncates toward zero.

Examples with the current precision:

- `0.0 -> 0`;
- `0.25 -> 250`;
- `0.5 -> 500`;
- `0.9999 -> 999`;
- `1.0 -> 1000`.

After parsing, canonical Shape geometry uses `spk::Vector3Int`; floating-point epsilon is not used to represent the stored vertices.

### Polygon rules

Every polygon:

- has at least three vertices;
- has a non-empty semantic `slot`;
- is non-degenerate;
- is planar;
- is convex;
- has no duplicate adjacent vertex, including an explicit closing vertex equal to the first;
- is authored in authoritative counter-clockwise order for its intended face direction.

Concave surfaces must be authored as multiple convex polygons.

The base JSON loader never silently rewinds/reverses an authored polygon.

The polygon normal is **not** authored in JSON. Compute and normalize it from the final discrete polygon vertices during construction and store it in the immutable polygon.

All malformed/invalid cases throw `spk::Exception` with useful JSON file/path context.

### Orientation enum revision

Revise `Voxel::Cell::Orientation` to:

```cpp
enum class Orientation : std::uint8_t
{
    PositiveX = 0,
    NegativeZ = 1,
    NegativeX = 2,
    PositiveZ = 3
};
```

The enum value is directly the number of 90° counter-clockwise quarter-turns around +Y from canonical +X, viewed from +Y toward the voxel.

The packed bit layout remains unchanged. Update the existing Cell tests and durable documentation accordingly.

`Voxel::Cell::FlipOrientation` remains:

```cpp
PositiveY = 0,
NegativeY = 1
```

### Oriented polygon arrays

Every Shape is authored as `PositiveX + PositiveY`.

Define the oriented cached representation:

```cpp
struct OrientedPolygonArray
{
    std::atomic<spk::UUID> uuid{spk::UUID::null()};
    std::vector<Polygon> polygons;
};
```

A Shape stores:

- a mutable fixed array of exactly eight `OrientedPolygonArray` entries;
- an internal mutable mutex protecting first-time cache population.

The eight indices represent the Cartesian product of four `Orientation` values and two `FlipOrientation` values. The implementation may derive the index directly from the enum values, e.g. orientation quarter-turn index plus four times the flip value.

The canonical `PositiveX + PositiveY` entry is created during Shape JSON construction and receives a non-null generated UUID.

The other seven entries start with:

- null UUID;
- empty polygon vector.

Expose:

```cpp
const OrientedPolygonArray& orientedPolygons(
    Voxel::Cell::Orientation orientation,
    Voxel::Cell::FlipOrientation flipOrientation) const;
```

The exact qualification/nesting may follow the final header organization, but this observable API is required.

#### Lazy cache synchronization

On access:

1. load the selected entry UUID with acquire semantics;
2. when non-null, immediately return the already-immutable entry without taking the mutex;
3. when null, lock the Shape cache mutex;
4. re-check the UUID after acquiring the mutex;
5. if still null, completely generate the transformed polygon vector;
6. move/store the completed polygons into the entry;
7. generate and store a new non-null `spk::UUID` **last**, with release semantics;
8. return the entry.

Once published, neither the UUID nor polygons of that oriented entry may change.

Require at compile time that `spk::UUID` satisfies the requirements for `std::atomic<spk::UUID>`, including trivial copyability.

Repeated access to the same variant returns the same cached entry/UUID for the remaining lifetime of the Shape.

The returned reference is non-owning. It is valid only while the originating Shape remains alive; callers must not retain it beyond a lifetime they can guarantee.

### Exact transforms

Let:

```cpp
M = round(1.0f / Voxel::Shape::VertexPrecision)
```

currently `M == 1000`.

Generate every non-canonical variant from canonical `PositiveX + PositiveY` geometry.

Horizontal transforms around the center of the voxel are:

| Orientation | Quarter turns | Transform |
| --- | ---: | --- |
| `PositiveX` | 0 | `x' = x`, `z' = z` |
| `NegativeZ` | 1 / 90° CCW | `x' = z`, `z' = M - x` |
| `NegativeX` | 2 / 180° | `x' = M - x`, `z' = M - z` |
| `PositiveZ` | 3 / 270° CCW | `x' = M - z`, `z' = x` |

`PositiveY` leaves Y unchanged.

`NegativeY` mirrors around the plane `Y = 0.5`:

```cpp
y' = M - y;
```

A vertical mirror reverses handedness, so reverse the transformed polygon's vertex order before finalizing the mirrored polygon.

For every generated variant:

- the semantic slot follows its original polygon;
- recompute the final normal from the final transformed/rewound vertices;
- do not reuse an authored normal because no normal exists in JSON.

### `Voxel::Definition`

`Voxel::Definition::ID` remains `std::uint32_t`.

A Definition object does **not** store its own catalog ID.

A non-Air Definition contains:

- a non-owning immutable `const Voxel::Shape&` to its resolved Shape;
- slot-to-`Voxel::Material::ID` bindings.

Definition loading resolves its JSON `shape` string against the Shape catalog once. Unknown Shape IDs throw `spk::Exception`.

The Shape catalog owns every referenced Shape. Definitions do not extend Shape lifetime independently and must not outlive the owning `Voxel::Catalog` / Shape catalog.

Slot validation:

- multiple Shape polygons may share the same slot;
- every non-empty slot key in Definition JSON must exist on the Shape;
- an extra Definition slot not used by the Shape throws `spk::Exception`;
- if a Shape slot is absent from the Definition JSON, log a warning through `spk::Logger` and bind that missing slot to `Voxel::Material::InvalidID`;
- actual existence of a non-empty Material ID is deferred to later Material work.

### Air

Definition ID `0` is reserved Air.

On catalog construction, the Shape catalog creates one private empty Shape sentinel with zero polygons. The Definition catalog automatically creates ID 0 Air referencing that empty Shape, with:

- a valid `const Voxel::Shape&` whose `polygons()` is empty;
- no slot bindings.

The empty Shape sentinel has no authored/catalog Shape ID and is not exposed as a normal Shape-catalog entry.

Air is a valid catalog entry:

- `at(0)` succeeds;
- `operator[](0)` succeeds;
- `contains(0)` is true;
- `tryGet(0)` returns a non-null pointer to Air.

Authored Definition JSON declaring ID 0 throws `spk::Exception`.

### `Voxel::Catalog`

The aggregate owner is named exactly:

```cpp
Voxel::Catalog
```

It owns the Shape and Definition subcatalogs. The Definition subcatalog keeps access to the Shape subcatalog it resolves from.

Required usage:

```cpp
myVoxelCatalog.load(shapePath, definitionPath);
myVoxelCatalog.loadShape(shapePath);
myVoxelCatalog.loadDefinition(definitionPath);

myVoxelCatalog.shapes().at(shapeId);
myVoxelCatalog.definitions().at(definitionId);
```

Paths are `std::filesystem::path`.

The Shape and Definition subcatalogs derive privately from an Erelia-local prototype named `spk::JSON::Catalog<TElement>`. This prototype deliberately lives under the Sparkle namespace while it is exercised in Erelia; promotion into the Sparkle repository is deferred until the API has proven useful.

`spk::JSON::Catalog<TElement>` owns the common JSON catalog machinery: file/root parsing, the `elements` array envelope, wrapper validation, iteration order, duplicate detection, immutable shared storage, incremental failure behavior, and lookup. `TElement` provides `TElement::ID`; the base does not require Sparkle's `json_readable` concept. Derived parsing code produces plain `TElement` values and does not depend on the catalog's internal ownership/storage representation; `TElement` must therefore be move-constructible.

The base exposes exactly two protected pure-virtual parsing hooks:

```cpp
virtual TElement::ID _parseKey(const spk::JSON::Reader& reader) const = 0;
virtual TElement _parseElement(const spk::JSON::Reader& reader) const = 0;
```

The key hook receives the element wrapper reader so domain code can parse/validate `id` with normal Reader diagnostics. The element hook receives only the corresponding `data` reader and returns a movable value. The base catalog moves that value into its internal immutable storage, keeping storage/ownership policy invisible to derived catalogs. `Voxel::Shape::Catalog` and `Voxel::Definition::Catalog` implement those two hooks and contain only their domain-specific parsing rules. `Voxel::Shape` is move-constructible for this purpose, remains non-copyable, and remains non-move-assignable. Do not introduce a `detail` / `details` namespace for this machinery.

The observable subcatalog API includes:

```cpp
const T& at(const ID&) const;
const T& operator[](const ID&) const;
bool contains(const ID&) const noexcept;
const T* tryGet(const ID&) const noexcept;
```

Required behavior:

- `at` and `operator[]` both throw `spk::Exception` when absent;
- `tryGet` returns `nullptr` when absent;
- duplicate IDs throw `spk::Exception`;
- repeated load calls may append new unique IDs;
- existing successfully-loaded entries are immutable and remain present.

### Loading atomicity

A load is intentionally **incremental**, not transaction-wide.

When an element fails:

- elements successfully inserted before it, including earlier elements from the same call, remain in the catalog;
- the failing element itself is not inserted;
- no later element from that file is attempted;
- loading throws immediately.

`Voxel::Catalog::load(shapePath, definitionPath)` loads the Shape path first and only begins Definition loading if Shape loading completes without throwing.

### JSON catalog envelope

Both resource kinds use the same outer envelope:

```json
{
  "elements": [
    {
      "id": "...",
      "data": {
      }
    }
  ]
}
```

The shared JSON catalog base owns envelope validation and iteration, dispatches key parsing to the derived catalog with the element wrapper reader, and dispatches element parsing with only the corresponding `data` reader.

Shape example:

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

Definition example:

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

### JSON failures

All of the following throw `spk::Exception` with useful file/path diagnostics:

- unknown fields at any validated schema level;
- missing `elements`;
- `elements` of the wrong type;
- malformed element wrapper / missing `id` or `data`;
- empty Shape ID;
- duplicate Shape/Definition ID in the current or a previous load;
- Definition ID 0 in authored JSON;
- Definition ID above `0x1FFFFFFF`;
- unknown Shape reference;
- malformed `slots`;
- empty polygon slot;
- missing/wrong-type polygon list;
- missing/wrong-type vertex list;
- vertex missing x/y/z;
- vertex coordinate outside `[0.0, 1.0]`;
- fewer than three vertices;
- duplicate adjacent vertices / explicit repeated closing vertex;
- zero-area or otherwise degenerate polygon;
- non-planar polygon;
- concave polygon.

Use Sparkle JSON readers/loaders rather than adding another JSON dependency.

## First Shape resources / exact fixtures

Create active first Shape resources using the validated geometry/topology from:

`archive/resources/voxels/shapes.json`

Only these archived Shapes are brought forward by this ticket:

- `cube`;
- `slab`;
- `slope`;
- `stair`.

Do **not** bring forward `cross` yet.

Adaptations from the archive are required:

1. remove UV data; UVs are not part of this shared Shape contract;
2. preserve the archived coordinates/topology and semantic slot grouping;
3. correct JSON vertex ordering where the archived loader relied on automatic boundary-face reversal, because the new loader treats authored CCW order as authoritative;
4. the archived slope/stair are canonical +Z; rotate those fixture coordinates once into canonical +X for the new resource;
5. keep the archived slab at half-height;
6. keep the archived stair as the validated two-step shape.

These active Shape resources are semantic/test fixtures. This ticket does **not** choose the final ST-001-06 terrain-scene Definition IDs, material choices, wall coordinates, or elevated fixture coordinates.

## Invariants

- Shape/Definition/catalog data is immutable after successful loading except for lazy internal oriented-Shape cache population.
- Shape/Definition objects do not store their own catalog IDs.
- Shape IDs are strings.
- Definition IDs are `Voxel::Definition::ID`.
- Definition ID 0 always means catalog-provided Air referencing the catalog-owned empty Shape sentinel.
- runtime polygon vertices are discrete `spk::Vector3Int`.
- material slots remain attached to the same semantic polygon through transform.
- published oriented polygon arrays never change.
- Core stays headless safe.
- no occlusion semantics are frozen by this ticket.

## State transitions

Catalog construction creates Definition ID 0 Air.

Resource loading appends immutable valid entries in file order. Failure preserves already-inserted entries and aborts at the first failing element.

An oriented Shape cache entry transitions exactly once:

`unpublished (null UUID, empty polygons) -> generated -> published non-null UUID`.

There is no transition back and no replacement after publication.

## Failure behavior

All invalid public/resource operations described above throw `spk::Exception`, except:

- missing `tryGet` returns `nullptr`;
- missing required material slot logs a warning and substitutes `Material::InvalidID`.

No partially-constructed failing Shape/Definition element is inserted into its catalog.

## Determinism / ordering

- catalog files are processed in their authored `elements` order;
- oriented transforms are exact over discrete integer vertices;
- enum numeric values define deterministic quarter-turn count;
- slot association follows polygon identity, not world-facing side;
- repeated requests for a materialized orientation return the same cached UUID and geometry.

Random UUID values are identity only; their exact generated bytes are not semantic deterministic output.

## Lifecycle / ownership

- `Voxel::Catalog` owns its Shape and Definition catalog state.
- Definitions hold non-owning `const Shape&` references to Shapes owned by the Shape catalog; Shape lookup is not repeated.
- oriented polygon arrays live inside their Shape.
- Definitions, references returned by `orientedPolygons()`, subcatalog `at`/operator[] references, and `tryGet` pointers are non-owning views and may not outlive their owning catalog/Shape.

## Serialization / persistence

Definition/Shape catalogs are loaded from JSON resources. They are not serialized through `spk::Message` in EP-001.

The Server sends Cell/Volume data containing Definition IDs, not Shape polygons or render meshes.

## Networking / authority

Server and Client are expected to have compatible shared Shape/Definition resources available locally and interpret the same Cell IDs consistently.

Runtime catalog synchronization/version negotiation over the network is out of scope for ST-001-04.

## Implementation constraints

- Extend the existing `Voxel::Definition` type; do not replace `Definition::ID`.
- Update the existing Cell Orientation enum and its tests to the DR-018 order.
- Prefer semantic nested/domain naming already established by the project.
- Keep production code under Core/headless-safe dependencies.
- Use `spk::JSON`, `spk::UUID`, `spk::Logger`, and `spk::Exception` as appropriate.
- Do not implement occlusion, meshing, GPU materials, production asset loading/editor infrastructure, or generator scene placement.
- Archive code is inspiration/source-fixture material only; the active contract in this ticket wins where it differs.

## Exact test fixtures

Tests must use checked-in JSON resources and/or inline JSON readers sufficient to prove the exact public behavior.

At minimum include:

- active `cube`, `slab`, `slope`, and `stair` Shape resources derived from the archive under the adaptations above;
- an asymmetric polygon/Shape fixture that makes all four horizontal orientations visibly distinct;
- a vertically asymmetric fixture that proves `NegativeY` mirroring and rewinding;
- Definitions with complete slots, a missing required slot, an extra invalid slot, Air ID 0, duplicate IDs, unknown Shape reference, and maximum/overflow Definition IDs.

Test-local nonzero Definition IDs/material strings are fixtures only and do not establish the later OQ-039 generator-scene IDs/material choices.

## Acceptance tests

### Nominal

- Load the four active Shapes through `std::filesystem::path`.
- Verify Shape IDs resolve through the Shape subcatalog.
- Verify JSON normalized values become the expected discrete integer vertices.
- Verify polygon slots and derived normals.
- Load valid Definitions and resolve their Shape once.
- Verify Definition slot bindings.
- Verify `Voxel::Catalog::load`, `loadShape`, `loadDefinition`, `shapes()`, and `definitions()`.
- Verify Air exists immediately at Definition ID 0.

### Orientation / flip

- Assert exact enum numeric values:
  - `PositiveX=0`;
  - `NegativeZ=1`;
  - `NegativeX=2`;
  - `PositiveZ=3`.
- Update packed Cell fixtures so bits 29-30 round-trip the revised semantic ordering.
- For an asymmetric Shape, verify all four exact integer horizontal transform formulas.
- Verify `PositiveY` leaves Y unchanged.
- Verify `NegativeY` uses `y' = M-y`.
- Verify mirrored polygon order is reversed and its final cached normal matches the transformed/re-wound geometry.
- Verify slots stay attached to their original polygons through rotation and mirroring.

### Lazy oriented cache / concurrency

- Canonical `PositiveX + PositiveY` begins materialized with non-null UUID.
- Other entries begin unpublished/null.
- First request generates one entry.
- Repeated request returns the same entry/UUID and does not regenerate it.
- Concurrent first requests for the same missing orientation publish exactly one final immutable entry and all callers observe the same UUID/geometry.
- After publication, normal reads take no mutex-dependent slow path observable through behavior.
- Compile-time evidence confirms `spk::UUID` can be used with `std::atomic<spk::UUID>`.

### Catalog lookup

For both Shape and Definition subcatalogs:

- existing `at` succeeds;
- existing `operator[]` succeeds;
- existing `contains` is true;
- existing `tryGet` returns a valid `const T*`;
- missing `contains` is false;
- missing `tryGet` returns `nullptr`;
- missing `at` and `operator[]` throw `spk::Exception`.

Definition ID 0 is the exception to "missing": it always resolves to Air.

### Incremental loading / duplicates

- second load with new IDs appends them;
- duplicate ID in the same file throws;
- duplicate ID against a previous load throws;
- if element N fails, elements before N remain inserted, element N is absent, elements after N are not attempted;
- `load(shapePath, definitionPath)` does not start Definition loading when Shape loading throws.

### Definition/material slots

- complete bindings load unchanged;
- missing required Shape slot emits a `spk::Logger` warning and stores `Material::InvalidID`;
- extra Definition slot not present on Shape throws;
- repeated Shape slot across multiple polygons needs only one Definition binding;
- Air references the catalog-owned empty Shape with zero polygons and has no slots;
- authored ID 0 throws;
- max 29-bit Definition ID is accepted;
- ID above `0x1FFFFFFF` throws.

### Invalid Shape/schema data

Each documented malformed schema/geometry case throws `spk::Exception`, including unknown fields, bad wrapper/data types, empty IDs/slots, out-of-range coordinates, too few vertices, duplicate adjacent/closing vertices, degenerate, non-planar, and concave polygons.

Verify the failing element is not inserted.

### Determinism

Repeated loading of equivalent resource content yields equivalent semantic Shape geometry/Definition bindings.

Repeated oriented lookup yields identical geometry and the same UUID within one Shape lifetime.

UUID byte values themselves are not compared across independent Shape instances/process runs.

### Lifecycle / ownership

- Definition references the exact catalog-owned Shape resolved during loading and does not own/extend its lifetime.
- Air references the private catalog-owned empty Shape sentinel.
- Definitions and other non-owning catalog/oriented-array references are used only while their owner is alive.
- destroying the aggregate Catalog safely releases Definitions/Shapes/cache contents without graphics/Server dependencies.

### Serialization / persistence

No `spk::Message` serialization is added for Shape/Definition resources.

### Retry / idempotency

Repeated lookup is read-only. Re-loading a file containing an existing ID is not idempotent; it throws as a duplicate by contract.

### Authority / trust boundary

Core owns the semantic data. Neither Server nor Client may reinterpret Orientation/Flip/slot meaning independently.

### Cross-system integration

No Server/Client production integration is required in this ticket. Tests prove the data is headless-safe and ready for later Server generation and Client meshing.

### Performance

No timing budget.

The required structural optimization is the lazy eight-way oriented polygon cache with lock-free published reads and mutex-protected first construction.

### Client-visible / golden-image validation

Not owned by this ticket.

## Decisions / unresolved questions

- [DR-012](../../../DECISIONS/DR-012-PACKED-CELL-AND-VOLUME-DIRECTION.md)
- [DR-013](../../../DECISIONS/DR-013-CLIENT-TERRAIN-MESHING.md)
- [DR-018](../../../DECISIONS/DR-018-VOXEL-SHAPE-DEFINITION-CATALOG.md)
- [OQ-039](../../../OPEN_QUESTIONS/OQ-039-FIRST-TERRAIN-GENERATOR-FIXTURE.md) — **not blocking ST-001-04**; remaining exact generator-scene coordinates/Definition IDs/material choices belong to ST-001-06.

## Definition of Ready result

**Ready.**

The public behavior, ownership, lifecycle, loading/error behavior, deterministic transforms, concurrency/publication semantics, resource schema, fixture source, and acceptance-test categories required by `DEFINITION-OF-READY.md` are specified without requiring the implementation agent to invent public behavior.

## Completion evidence

**Implementation state:** Technically complete; project-owner approval pending.

Implementation remains on:

- branch: `feat/st-001-04-definition-shape-contract`;
- pull request: PR #11;
- main implementation commit: `c85f7e82360d79a79ac25d39cba6df71b0b1ea59`;
- authored-winding/correctness follow-up: `6423e789b69770f8f15df1e16bacda23f54a17fb`;
- Linux atomic-link fix: `5adc37e319fe2725d4d153fed9f5186ff9562457`;
- final formatting corrections before the catalog redesign: `0407a8081499acc4b6380702b066a8b0480dce36`, `a67fbf89c88322d2e6e0417a78d7a62da14d94d4`, and `44aaf1d509c231bb5f69ad9775a97349af959ba3`;
- virtual JSON-catalog foundation: `19d2efe8768bef36c01da07877e448afb13153af`, `1fd28eeaf84d67242fc263c600e62b3af3b6642c`, and `33768efcd45427bd182e7bbc8094e8af2bd3c061`;
- value-returning catalog parsing / movable Shape: `b2f8cc97b95c8f4842c2c67830e069f3fd1b053f` through `97151c24c550205d46ea5a6f6a70df97d0ba2cfd`;
- Definition `const Shape&` ownership and Air empty-Shape sentinel: `afb350ca5cce99d1d206d301002df4d0ceb1feec` through `0ced9a3c0d84ce055e1341e00734b9f56ae9709c`.

The implementation preserves authored JSON polygon vertex order, derives normals from that order, validates the required structural polygon properties, and reverses transformed vertex order only for the specified `NegativeY` mirror. The lazy eight-way cache retains the approved acquire-load / mutex re-check / complete construction / release-store UUID publication contract. Linux links `libatomic` transitively through `EreliaCore` because `std::atomic<spk::UUID>` requires the platform atomic runtime there. The catalog implementation now uses an Erelia-local `spk::JSON::Catalog<TElement>` abstract base with two pure virtual Reader-based parsing hooks returning plain values and no `detail` namespace. `Voxel::Definition` holds a non-owning `const Shape&`; Air references a private Shape-catalog sentinel with zero polygons.

Active Shape resources are checked in at `resources/voxels/shapes.json` and contain only `cube`, `slab`, `slope`, and `stair`.

### Tests and validation

The focused Core tests cover:

- direct `spk::JSON::Catalog<TElement>` unit coverage for move-only elements, lookup APIs, protected insertion, duplicate IDs, incremental failure, and malformed envelope diagnostics;
- direct `Voxel::Definition` unit coverage for Air's empty Shape, exact Shape-reference identity, copy/move construction, and non-assignable reference semantics;
- active Shape loading and normalized JSON-to-discrete conversion;
- polygon slots and normals;
- exact four-way Orientation transforms and both Flip orientations;
- `NegativeY` mirroring, winding reversal, and final-normal recomputation;
- slot preservation through transforms;
- canonical cache publication, lazy reuse, and concurrent first access;
- Definition-to-Shape reference identity and catalog lifetime semantics;
- Air, catalog lookup behavior, complete/missing/extra slots, duplicate IDs, repeated loads, and incremental failure semantics;
- aggregate Shape-before-Definition loading;
- malformed JSON/schema and malformed polygon geometry;
- maximum/overflowing Definition IDs;
- deterministic semantic loading;
- revised packed Cell Orientation fixtures.

CI run #127 validated implementation head `44aaf1d509c231bb5f69ad9775a97349af959ba3` successfully:

- clang-format: passed;
- Linux Core/Server Debug: passed; CTest 3/3 (`EreliaCoreTestSuite`, `EreliaServerTestSuite`, `EreliaServerSmoke`);
- Linux Core/Server Release: passed; CTest 3/3;
- Windows Core/Server Debug: passed; CTest 3/3;
- Windows Core/Server Release: passed; CTest 3/3;
- Windows Client Debug: passed; CTest 5/5 including `EreliaCoreTestSuite`, `EreliaServerTestSuite`, `EreliaServerSmoke`, `EreliaClientTestSuite`, and `EreliaClientSmoke`;
- Windows Client Release: passed; CTest 5/5.

The concurrent cache test uses 12 threads racing the same previously unmaterialized Orientation/Flip entry and verifies that all callers observe the same entry, UUID, and immutable geometry after publication.

CI run #182 (run ID `35901607987`) validated complete code head `0ced9a3c0d84ce055e1341e00734b9f56ae9709c` successfully across clang-format, Linux Core/Server Debug + Release, Windows Core/Server Debug + Release, and Windows Client Debug + Release. Subsequent commits only synchronize ticket/decision/status documentation with that validated code. The ticket remains **In Progress**, not Done, solely because explicit project-owner approval required by `DEFINITION-OF-DONE.md` has not yet been recorded. PR #11 must not be merged until separately authorized.
