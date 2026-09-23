# Implementation Context  Taste, Conventions, and Working Knowledge

**Purpose:** give a future implementation/planning session a high-density view of how Erelia should be built, not just what the game should do.
**Authority:** summary only. The user's latest explicit direction, resolved Decision Records, Architecture documents, and the GDD remain authoritative.
**Companion:** read PROJECT-CONTEXT.md for product/gameplay context and OPEN_QUESTIONS/README.md before treating an unresolved detail as fixed.

## 1. General working rule

Do not invent implementation contracts when the project has not chosen them.

If a detail materially affects public behavior, ownership, lifetime, serialization, authority, networking, persistence, ordering, coordinates, testing, or another durable contract, inspect the relevant OQ/DR/architecture first. If it is still genuinely ambiguous, ask rather than silently choosing.

Historical code under `archive/` may be inspected as inspiration or evidence of what previously worked, but it is not an active requirement. When reusing an archived idea, review it critically. If something looks strange, unnecessarily coupled, unsafe, or like a code smell, surface the concern instead of copying it blindly.

## 2. Naming and type organization taste

Prefer semantic namespace/type composition over repeating a domain prefix in every class name.

Do not add a redundant top-level `erelia` C++ namespace inside the Erelia project. Start from the relevant semantic/module namespace instead, for example `core::terrain` or `Voxel`.

Preferred:

```cpp
Voxel::Cell
Voxel::Volume
```

rather than:

```cpp
VoxelCell
VoxelVolume
```

The intent is to let namespaces communicate the domain and keep individual type names short and precise.

Do not introduce a prefixed name merely because an archived implementation used one if a clean nested/domain-scoped name expresses the concept better.

Do not introduce `detail` / `details` namespaces in public Erelia code. Prefer explicit private members/types or source-local anonymous namespaces instead. A `detail`-style namespace is acceptable only as a last resort inside a private header or source-only implementation area, and should still be avoided when a clearer structure is available.

Coordinate vocabulary follows the same semantic ownership:

```cpp
Voxel::Cell::Coordinate
Voxel::Volume::LocalCoordinate
Voxel::Volume::UnitSize
Chunk::Coordinate
```

`Voxel::Cell::Coordinate`, `Voxel::Volume::LocalCoordinate`, and `Chunk::Coordinate` are aliases of `spk::Vector3Int`. `Voxel::Volume::UnitSize` is the scalar type used for a Volume instance's unit size and is currently `float`. Terrain `Chunk` inherits `Voxel::Volume` and owns the Chunk coordinate conversion behavior/constants. The actual per-instance unit-size storage/accessor belongs to the Volume implementation ticket; ST-001-01 does not introduce premature Volume instance state. A Volume validates its own local-coordinate bounds once the Volume contract is implemented; do not encode those runtime bounds by narrowing `LocalCoordinate` to `std::uint8_t`.

Use established project terminology consistently: Core, Server, Client, Chunk, World, Hero, Encounter, Definition, Shape, `Voxel::Cell`, and `Voxel::Volume`.

## 3. Product/module ownership taste

Core / Server / Client are deliberate long-term boundaries.

- **Core** contains code and representations genuinely shared by both Server and Client.
- **Server** owns authoritative validation, decisions, ordering, simulation results, and shared/persistent state mutation.
- **Client** owns input, presentation, rendering, UI/audio, and explicitly non-authoritative prediction/speculation.
- Core should contain as little authoritative decision-making as possible. It may expose reusable calculations, data structures, algorithms, serializers, and result descriptions that Server and Client both need.

A useful shorthand is:

> Server decides; Client executes/presents; Core provides shared tools and representations.

## 4. Dependency taste

Prefer the C++ standard library and Sparkle. Do not add another third-party runtime dependency unless the project owner explicitly changes this policy.

Core may directly use headless-safe Sparkle Core facilities such as math/vector types, common algorithms, and networking primitives that are valid for both Server and Client.

Graphics/presentation-only dependencies must not leak into headless Core or Server code.

## 5. Server/networking taste

Use Sparkle Version-0.1.3 networking.

The Server starts with `spk::NodeRouter`; do not first build a monolithic bare-`spk::Server` game-message loop and plan to migrate later.

EP-001 begins with one in-process `spk::LocalNode` owning the terrain/Chunk message family. Add further nodes only when a coherent ownership boundary appears. A local node may become a `spk::RemoteNode` later when actual process separation is justified.

Client messages express intent/requests. Server code validates and produces canonical results.

For terrain streaming:

- Client requests Chunks by Chunk coordinate.
- One message may request a batch of Chunk coordinates.
- Server returns canonical voxel/Volume data, never render meshes.
- Client owns its own loading/view region policy. The Server does not choose the Client's render radius.

## 6. Voxel data/API taste

The active direction intentionally keeps the voxel data representation small and domain-shaped.

### `Voxel::Definition`

- `Voxel::Definition` is the semantic owner of voxel Definition identity;
- `Voxel::Definition::ID` is an alias of `std::uint32_t`;
- ST-001-04 / DR-018 establish the first shared Definition/Shape/Catalog contract:
  - Shape IDs are strings owned by the Shape catalog; Definition IDs remain `std::uint32_t` owned by the Definition catalog;
  - Shape/Definition objects do not store their own catalog IDs;
  - Shape polygons store discrete `spk::Vector3Int` vertices, a semantic string slot, and a derived cached normal;
  - JSON vertices remain normalized floats in `[0,1]`, quantized with `Voxel::Shape::VertexPrecision = 0.001f`;
  - base Shape polygons are convex, planar, non-degenerate and authored CCW;
  - every Shape is authored as `PositiveX + PositiveY` and lazily caches the other seven Orientation/Flip polygon arrays;
  - `NegativeY` mirrors around `Y=0.5`, reverses polygon order, and recomputes the final normal;
  - semantic slots move with their polygons through transforms;
  - `Voxel::Material::ID` is a string and `Voxel::Material::InvalidID` is `"InvalidID"`;
  - missing required Definition slots warn and bind InvalidID; extra slots throw;
  - every Definition stores a non-owning `const Voxel::Shape&` to a Shape owned by the Shape catalog and must not outlive that catalog; Definition ID 0 is catalog-created Air referencing a private catalog-owned empty Shape sentinel with zero polygons and no slots;
  - the owning aggregate is `Voxel::Catalog`, loaded from filesystem JSON resources;
  - shared Shape/Definition catalog machinery currently uses an Erelia-local prototype `spk::JSON::Catalog<TElement>`: the base owns JSON envelope parsing, iteration, duplicate detection, direct `std::unordered_map<TElement::ID, TElement>` value storage, and lookup, while derived catalogs implement only `_parseKey(const spk::JSON::Reader&) -> TElement::ID` and `_parseElement(const spk::JSON::Reader&) -> TElement` pure virtual hooks; parsing returns values and the base stores them directly in `std::unordered_map<TElement::ID, TElement>` with no shared ownership wrapper; catalog elements must be move-constructible, and lookup references/pointers remain stable across later insertions because the catalog exposes no erase operation; this prototype may be proposed to Sparkle after it has been exercised in Erelia;
  - occlusion algorithms/metadata are deliberately not part of ST-001-04.

### `Voxel::Cell`

- `Voxel::Cell::PackedType` aliases `std::uint32_t`;
- one Cell stores exactly one private `PackedType` and remains exactly 32 bits;
- it remains trivially copyable and immutable after construction;
- lower 29 bits are `Voxel::Definition::ID`, bits 29-30 are `Orientation`, and bit 31 is `FlipOrientation`;
- `Orientation` is exactly `PositiveX = 0`, `NegativeZ = 1`, `NegativeX = 2`, `PositiveZ = 3`; the numeric value is the counter-clockwise quarter-turn count around +Y from canonical +X;
- `FlipOrientation` is exactly `PositiveY = 0`, `NegativeY = 1`;
- Definition ID 0 means semantically empty, but its Orientation/FlipOrientation bits remain valid and are not canonicalized away;
- default construction and `Voxel::Cell::Empty` use packed zero;
- every raw `Voxel::Cell::PackedType` is a valid packed Cell and is preserved exactly;
- logical-field construction rejects Definition IDs above `0x1FFFFFFF` or invalid enum-domain values with `spk::Exception`;
- expose logical fields through mask/shift getters rather than C++ bitfields so the packed layout is explicit and portable;
- the packed representation is useful for compact storage/network transfer.

### `Voxel::Volume`

`Voxel::Volume` is a generic immutable built value for groups of voxel cells, not inherently a terrain Chunk.

Its approved first contract includes:

- `spk::Vector3UInt` runtime dimensions and `Voxel::Volume::UnitSize` (`float`);
- default construction as the valid empty Volume (`{0,0,0}`, `0.0f`, zero Cells);
- read-only dimensions, unit size, checked Cell access, and contiguous `std::span<const Voxel::Cell>`;
- `contains(LocalCoordinate)` for a pure bounds query and `tryGet(LocalCoordinate)` returning `std::optional<Voxel::Cell>` for non-throwing lookup;
- Y-fastest, then X, then Z storage order: `y + sizeY * (x + sizeX * z)`;
- a nested mutable `Voxel::Volume::Builder`, declared separately in `volume_builder.hpp`;
- Builder construction from positive dimensions + finite positive unit size;
- `Builder::set()` for checked mutation before build;
- `std::move(builder).build()` to produce an immutable Volume;
- a nested `Voxel::Volume::Buffer` semantic wrapper over `std::vector<Voxel::Cell>`, exposing `Buffer::Pool` and `Buffer::Lease`;
- each Volume directly owns dimensions, unit size, and one `Buffer::Lease`; no shared backing Content object is used;
- Volume copy construction/assignment deep-copies Cell contents through the Sparkle Pool Lease copy semantics, producing independent pooled storage;
- Volume move transfers the existing Lease and leaves the source in the default-empty state;
- `Builder(std::move(volume))` destructively consumes a Volume and directly reuses/transfers its existing Lease without copying;
- pool instances are implementation details in `volume_builder.cpp`: one dedicated `Buffer::Pool` is used only for exact 16×16×16 Chunk dimensions, while other sizes use a source-local ordered `std::map<std::size_t, Buffer::Pool>`;
- general pool lookup uses `lower_bound(expectedCellCount)`, selecting the exact size class or the smallest existing higher class; when none exists, a new pool is created for the requested size;
- pooled Buffers retain capacity while their logical size is reset through the Pool per-obtain callback;
- no `VersionedTrait` inheritance or mutable Editor remains in the Volume contract.

A terrain Chunk is one semantic use of a Volume. Terrain Chunks are fixed at 16×16×16 cells and one world unit per cell.


## 7. Serialization/API ergonomics

Prefer domain-level operator syntax for `Voxel::Volume` serialization:

```cpp
message << volume;
message >> volume;
```

The operators are declared as friends directly on `Voxel::Volume` and implemented as free functions in namespace `Voxel`:

```cpp
friend spk::Message &operator<<(spk::Message &message, const Volume &volume);
friend const spk::Message &operator>>(const spk::Message &message, Volume &volume);
```

The operators serialize the logical Volume contentsdimensions, voxel size, and contiguous Cell data. They must never raw-copy the C++ object representation of `Voxel::Volume`, because it owns a `std::vector`.

Do not expose otherwise-unnecessary mutable internals merely to make serialization possible.

## 8. Voxel/rendering ownership

The Server never emits terrain render meshes.

The Server produces/canonicalizes voxel terrain. The Client receives canonical voxel data and owns terrain meshing, rendering, Client-side mesh/cache state, and remeshing when relevant data changes.

The exact missing-neighbor behavior at Chunk boundaries remains an open question; do not freeze it in an implementation ticket until its OQ is resolved.

## 9. Coordinate taste

Follow Sparkle's 3D convention directly:

- +Y is up;
- -Z is forward;
- terrain cells use integer coordinates;
- Chunk coordinates use integer coordinates;
- one terrain cell equals one world unit;
- Chunks are 16×16×16;
- global-cell to Chunk conversion uses mathematical floor division/modulo so negative coordinates work correctly.

Do not introduce an unnecessary Erelia-specific axis-remapping layer.

## 10. First terrain validation taste

The first Server terrain generator is intentionally a technical validation fixture, not production procedural generation.

The approved direction is:

- flat baseline;
- vertical wall-like geometry around X = 0 and Z = 0;
- elevated stairs, slabs, and slopes around Y H 3;
- varied Orientation/Flip combinations;
- enough empty space to inspect geometry from above and below.

Exact fixture coordinates/Definitions remain open until explicitly resolved.

## 11. Temporary inspection controls

EP-001 may use a temporary free-flight 3D inspection controller rather than production Hero movement.

The explicitly requested keyboard movement convention is **ZQSD**. Do not silently substitute WASD.

Production third-person movement, collision, prediction/reconciliation, followers, and traversal behavior are separate later work.

## 12. Test and implementation taste

Prefer small, focused implementation slices with strong tests over large feature dumps.

An ST ticket should ideally own one coherent implementation goal and be small enough to review, test, and revert independently. Do not combine several architectural layers into one giant ticket merely because they contribute to the same Epic.

Avoid tickets that imply thousands of lines across Core + Server + Client + rendering + networking at once when the work can be separated behind explicit contracts.

Testing should live at the lowest layer that owns the behavior:

- Core tests shared data/algorithms/serialization;
- Server tests authority/generation/routing/network-facing Server behavior;
- Client tests meshing/rendering/input/presentation;
- integration tests cover real boundaries between those layers.

For behavior with important boundary/failure semantics, cover nominal behavior, boundaries, invalid input, atomic failure, determinism, lifetime/ownership, and integration where relevant.

Visual golden references require explicit human approval and must not be silently regenerated/overwritten.

## 13. Backlog granularity taste

Epics directly contain `ST-XXX-YY` implementation tickets. There is no mandatory separate Story layer despite the `ST` prefix.

Keep Epics focused and useful. If an Epic approaches roughly 50 implementation tickets, explicitly discuss whether it should be split.

Prefer near-term, just-in-time detailed planning instead of materializing a huge speculative backlog for systems that are still far away.

When an OQ still blocks a public contract, do not mark the corresponding ticket Ready.

## 14. Current implementation focus

The active near-term Epic is EP-001  Voxel Terrain Delivery and Visual Validation.

The intended progressive path is roughly:

1. shared voxel/Chunk data contracts;
2. deterministic basic Server Chunk generation;
3. Server node routing + Client connection;
4. batched Chunk request/response;
5. Client Chunk storage/request coordination;
6. Client terrain meshing;
7. Client terrain rendering;
8. temporary ZQSD/free-flight inspection;
9. end-to-end adjacent-Chunk visual/integration validation.

This list describes implementation direction, not a pre-approved ticket decomposition. Ticket boundaries must remain small and should be adjusted around actual contracts/dependencies.

## 15. Important unresolved implementation details

Before implementation code assumes an answer, check the corresponding files under OPEN_QUESTIONS/.

For EP-001 in particular, the still-partial questions include:

- OQ-036  missing-neighbor/remesh policy for terrain meshing;
- OQ-037  remaining scalar wire portability policy;
- OQ-038  request/cache/eviction/partial-response details;
- OQ-039  exact terrain-generator fixture coordinates/Definitions;
- OQ-029 through OQ-031  golden-image and performance-validation policy.

Do not hide one of these unresolved choices inside a coding ticket.
