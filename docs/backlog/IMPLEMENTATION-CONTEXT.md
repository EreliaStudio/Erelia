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

Prefer typed enums for closed semantic domains such as protocol states, message kinds, result states, and error codes. Use `enum class` with an explicit underlying integer type when storage or serialization width is part of the contract (for example `enum class State : std::uint8_t`). Do not use macros or untyped integer constants for values that have a meaningful finite type domain.

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

EP-001 uses a separate terrain Server-node process from its first implementation. `EreliaServer` owns the Client-facing `spk::NodeRouter`, which reaches the terrain process through `spk::RemoteNode` / `spk::RemoteNode::Endpoint` as fixed by DR-021.

Client messages express intent/requests. Server code validates and produces canonical results.

For terrain streaming:

- Client requests Chunks by Chunk coordinate.
- One message may request a batch of Chunk coordinates.
- Server returns canonical Chunk data, never render meshes.
- Client owns its own loading/view region policy. The Server does not choose the Client's render radius.

## 6. Voxel data/API taste

The active direction intentionally keeps the voxel data representation small and domain-shaped.

### `Voxel::Definition`

- `Voxel::Definition` is the semantic owner of voxel Definition identity;
- `Voxel::Definition::ID` is an alias of `std::uint32_t`;
- ST-001-04 / DR-018 establish the first shared Definition/Shape/Catalog contract:
  - Shape IDs are strings owned by the Shape catalog; Definition IDs remain `std::uint32_t` owned by the Definition catalog;
  - Shape/Definition objects do not store their own catalog IDs;
  - `Voxel::Vertex` is the semantic Shape-vertex type and aliases `spk::Vector3Int` (`std::int32_t` components); Shape polygons store `std::vector<Voxel::Vertex>`, a semantic `Voxel::Material::SlotID`, and a derived cached `spk::Vector3` floating normal;
  - JSON vertices remain normalized floats in `[0,1]`, authored using Sparkle's `TVector3` JSON representation as three-element arrays `[x, y, z]`, and quantized with `Voxel::Shape::VertexPrecision = 0.001f`;
  - Use wider integer vectors only for exact intermediate geometry arithmetic that can overflow 32-bit products/dot products; the current Shape validator uses source-local `spk::TVector3<std::int64_t>` intermediates while retaining 32-bit stored `Voxel::Vertex` values;
  - base Shape polygons are convex, planar, non-degenerate and authored CCW;
  - every Shape is authored as `PositiveX + PositiveY` and lazily caches the other seven Orientation/Flip polygon arrays;
  - `NegativeY` mirrors around `Y=0.5`, reverses polygon order, and recomputes the final normal;
  - semantic slots move with their polygons through transforms;
  - `Voxel::Material::ID` and `Voxel::Material::SlotID` are semantic string aliases; `Voxel::Material::InvalidID` is `"InvalidID"`;
  - missing required Definition slots warn and bind InvalidID; extra slots throw;
  - every Definition stores a non-owning `const Voxel::Shape&` to a Shape owned by the Shape catalog and must not outlive that catalog; Definition ID 0 is catalog-created Air referencing a private catalog-owned empty Shape sentinel with zero polygons and no slots;
  - the owning aggregate is `Voxel::Catalog`, loaded from filesystem JSON resources;
  - generic `spk::JSON::Catalog<TElement>::load(path)` accepts either an aggregate root `{"elements":[...]}` or a direct single `{"id":...,"data":...}` root; both forms share one internal element parse/insert path and retain identical duplicate/error behavior;
  - ST-001-06 intentionally exercises both forms: cube+slab remain in shared aggregate files while slope/stair use individual files for both Shapes and Definitions; this mixed layout is validation, not a final one-file policy;
  - JSON/resource validation errors with source location use the single shared `spk::JSON::throwAt` helper; do not duplicate file/path exception formatting in loaders;
  - Shape, Definition, and aggregate Catalog implementations are split by class into `shape_catalog.cpp`, `definition_catalog.cpp`, and `catalog.cpp`;
  - shared Shape/Definition catalog machinery uses public inheritance from Sparkle Version-0.1.3 `spk::JSON::Catalog<TElement>`: the base owns JSON envelope parsing, iteration, duplicate detection, direct `std::unordered_map<TElement::ID, TElement>` value storage, and lookup, while derived catalogs implement only `_parseKey(const spk::JSON::Reader&) -> TElement::ID` and `_parseElement(const spk::JSON::Reader&) -> TElement` pure virtual hooks; parsing returns values and the base stores them directly in `std::unordered_map<TElement::ID, TElement>` with no shared ownership wrapper; catalog elements must be move-constructible, and lookup references/pointers remain stable across later insertions because the catalog exposes no erase operation; the base public `load`/lookup API is inherited directly without forwarding wrappers; derived voxel catalogs should contain only their parsing overrides and genuinely required domain state/constructors; the generic catalog and shared JSON error helper are now Sparkle-owned after upstreaming from Erelia;
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
- DR-019 supersedes the original deep-copy/direct-Lease value model: a Builder owns mutable pooled storage, then build publishes it through shared immutable Volume backing content;
- Volume copy construction/assignment shares immutable Cell backing content instead of copying the Cell array; copies are cheap snapshots and keep the same content alive independently;
- Volume move still leaves the source in the canonical default-empty state;
- any `Builder(std::move(volume))` path must preserve immutability: it may reuse backing storage only when exclusive and must otherwise obtain/copy mutable storage before edits;
- `Voxel::Volume` owns pooled Cell-buffer acquisition; `volume_buffer_pool.cpp` retains the source-private `CellArrayPool` / `CellArrayCollection` implementation;
- one source-private `CellArrayCollection` owns deterministic power-of-two capacity classes for every non-empty Volume, including Chunks;
- pooled Buffers retain capacity while logical size is reset on obtain;
- generic Message decode never overwrites a destination's existing backing Buffer in place, because another copied Volume may observe it; successful decode publishes fresh immutable content and swaps the destination only after validation;
- no `VersionedTrait` inheritance or mutable Editor remains in the Volume contract.

A terrain Chunk is a semantic specialization of Volume. DR-019 fixes:

- `Chunk::Builder : Voxel::Volume::Builder`, always 16×16×16 at unit size 1.0f, with `build()` returning `Chunk`;
- a public checked `Chunk(Voxel::Volume&&)` promotion path that rejects incompatible dimensions/unit size;
- Chunk does not store its own `Chunk::Coordinate`;
- `Chunk::Collection` owns coordinate identity and a nested abstract `Chunk::Collection::Provider`;
- Collection exclusively owns its Provider through a private `std::unique_ptr<Provider>`; its public constructor is a constrained template taking only a concrete Provider rvalue derived from `Provider`, moving that concrete object into the owned polymorphic allocation; lvalue Provider construction is rejected and null/absent Provider state is unrepresentable;
- Collection uses explicit semantic `Absent / Pending / Available` coordinate state;
- Pending means one unique in-flight `spk::Task<Chunk>::Answer` exists for that coordinate; overlapping requests reuse/subscribe to that Answer instead of regenerating;
- stale asynchronous work must not overwrite newer authoritative Collection state; the exact private generation/identity mechanism remains an implementation detail;
- `tryGet(coordinate)` returns `std::optional<Chunk>`; Available values are copied under a short `spk::ProtectedData` Reader and remain valid after the lock is released;
- published Chunks are immutable; whole-value replacement is used instead of Cell mutation; copied Chunks keep old immutable content alive across Collection replacement;
- no Collection lock is held during expensive generation work.

Sparkle Version-0.1.3 owns the generic headless facilities first prototyped by Erelia. In particular:

- `spk::Task<TResult>` is a generic asynchronous result state with explicit `validate(TResult)` / `fail(std::exception_ptr)` settlement and no execution lambda;
- `Task<TResult>::Answer` is the shared observation handle and exposes `subscribeToCompletion(...)` through thread-safe `spk::ContractProvider`;
- `spk::WorkerPool::submit(callable)` executes worker work through its internal type-erased Job / TaskJob layer and returns `Task<TResult>::Answer`;
- `spk::TaskGroup<TResult>` groups arbitrary Task Answers, including manually-settled and WorkerPool-produced Tasks, without occupying a worker merely to wait.

For ST-001-09 the selected ownership is:

- TerrainNode splits one Client Chunk request into smaller internal coordinate batches;
- each batch is passed to `Chunk::Collection::request(vector<Coordinate>)`, which returns one `Task<BatchResult>::Answer`;
- on Available coordinates, Collection shallow-copies the Chunk into the batch result;
- on Pending coordinates, Collection subscribes to the existing coordinate Answer;
- on Absent coordinates, Collection asks Provider for exactly one coordinate Task Answer, stores/reuses it as Pending, and subscribes;
- Provider is single-coordinate and WorkerPool-backed; it no longer owns batch buffering or `update(Collection&)` polling;
- Collection creates its BatchResult Task directly and never submits that aggregation Task to WorkerPool;
- the Collection batch stays Pending until every coordinate dependency is terminal;
- each coordinate contributes either `Chunk::Collection::BatchResult::Acquired { coordinate, chunk }` or `Chunk::Collection::BatchResult::Failed { coordinate, std::exception_ptr exception }`;
- after all coordinate dependencies are terminal, Collection validates one complete BatchResult containing every coordinate outcome;
- ordinary per-coordinate acquisition failure does not fail the batch Task; the batch Task fails only when aggregation itself cannot produce a valid BatchResult;
- TerrainNode groups the Collection batch Answers in one `spk::TaskGroup<BatchResult>` and handles one terminal protocol outcome using the original RequestID.

The exact private Collection state structs remain implementation details. The public BatchResult shape is fixed as nested `Acquired` / `Failed` types with `std::vector<Acquired> acquired` and `std::vector<Failed> failed`; `Failed` preserves the originating `std::exception_ptr`. TerrainNode translates these acquisition-domain outcomes into `Chunk::Protocol::Response::Success` / `Response::Failure` entries. On 26 September 2026 the project owner explicitly selected this per-coordinate failure-as-data contract so successful coordinates are preserved independently of internal batch partitioning.

Future Client request acquisition uses the same Collection/Provider state machine, but ST-001-11 still owns Client network retry/cache/response policy.

## 7. Serialization/API ergonomics

Prefer domain-level operator syntax for `Voxel::Volume` serialization:

```cpp
message << volume;
message >> volume;
```

`Voxel::Volume` also exposes `explicit Volume(const spk::Message &message)`, which delegates to the same extraction operator and therefore uses the exact same validation and cursor semantics.

The operators are declared as friends directly on `Voxel::Volume` and implemented as free functions in namespace `Voxel`:

```cpp
friend spk::Message &operator<<(spk::Message &message, const Volume &volume);
friend const spk::Message &operator>>(const spk::Message &message, Volume &volume);
```

The operators serialize the logical Volume contents—dimensions, unit size, and contiguous Cell data. They must never raw-copy the C++ object representation of `Voxel::Volume`, because it owns a `std::vector`.

`volume.hpp` includes Sparkle's `network/message.hpp` directly because Message is an explicit part of the public Volume API. Networking-specific implementation lives in `core/src/voxel/volume_networking.cpp`, keeping ordinary Volume behavior in `volume.cpp`. Network decoding reconstructs fresh immutable Volume content directly and does not use `Voxel::Volume::Builder`; it must not mutate previously published shared backing storage.

ST-001-08 / DR-022 implemented the first dedicated Chunk protocol codec, but ST-001-09 planning later refined the terminal Response target. `Chunk::Protocol::Request` and `Chunk::Protocol::Response` remain Message-backed domain types. `Response` now owns nested semantic terminal entries: `Response::Success { coordinate, chunk }` and `Response::Failure { coordinate, Failure::Code, message }`. `Failure::Code` belongs under `Failure`; ST-001-09 fixes `Response::Failure::Code::AcquisitionFailed = 0` as the initial and currently only code. Builders may own temporary semantic containers during construction, but finalized protocol objects retain no mirrored semantic vectors: the inherited `spk::Message` payload remains the single persistent representation, read through protocol accessors. Success entries still transfer only the fixed contiguous 4096-Cell block because Chunk is always 16×16×16 at unit size 1.0f; dimensions/unit size are not serialized. The exact byte encoding for the variable-length Failure string remains unresolved. The old Chunk-specific `Chunk::Protocol::Error` message is not the preferred long-term target; non-terminal technical diagnostics are expected to move to a generic diagnostic message whose final wire contract is still unresolved.

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

OQ-039 is resolved and DR-015 contains the exact fixture:

- Definition 1 cube baseline at world Y=0 for every X/Z;
- Definition 1 cube walls on X=0 and Z=0 for Y=1..3;
- Definition 2 slope fixture in Chunk (1,0,1);
- Definition 3 stair fixture in Chunk (2,0,1);
- Definition 4 slab fixture in Chunk (1,0,2);
- identical local ground/elevated placement matrices for all three Shapes;
- all eight Orientation/Flip combinations, horizontal adjacency and vertical stacking/contact;
- elevated fixture bottoms at Y=4;
- every non-authored Cell is Air/Empty, including all world space below Y=0;
- exact material IDs follow `<shape-id>-<slot-id>`;
- exact positive/negative validation Chunk set is recorded in DR-015.

ST-001-06 Provider/Collection readiness decisions are resolved. The reusable Provider/Collection contract is tested with a purpose-built Core test Provider rather than by freezing the temporary `PrototypeChunkProvider` terrain layout. The prototype still implements the resolved DR-015 validation scene for temporary integration/visual use.

## 11. Temporary inspection controls

EP-001 may use a temporary free-flight 3D inspection controller rather than production Hero movement.

The explicitly requested keyboard movement convention is **ZQSD**. Do not silently substitute WASD.

Production third-person movement, collision, prediction/reconciliation, followers, and traversal behavior are separate later work.

## 12. Test and implementation taste

Prefer small, focused implementation slices with strong tests over large feature dumps.

Prefer named source-local helper functions in an anonymous namespace over lambdas declared inside a function when the logic is independently describable and does not materially benefit from captures. Keep lambdas for genuinely local callback/capture behavior rather than using them as a substitute for ordinary helper functions.

`OPEN_REQUESTS/` tracks external dependency fixes that should trigger later Erelia cleanup. Use one `OR-XXX-[name].md` file per request. Its first line is the external issue link, its second line is `Status : Open`, `Status : Treated`, or `Status : Rejected`, and its `# Edition` section lists every `[file:line]` location that must change when a treated request is integrated.

An ST ticket should ideally own one coherent implementation goal and be small enough to review, test, and revert independently. Do not combine several architectural layers into one giant ticket merely because they contribute to the same Epic.

Avoid tickets that imply thousands of lines across Core + Server + Client + rendering + networking at once when the work can be separated behind explicit contracts.

Testing should live at the lowest layer that owns the behavior:

- Core tests shared data/algorithms/serialization;
- Server tests authority/generation/routing/network-facing Server behavior;
- Client tests meshing/rendering/input/presentation;
- integration tests cover real boundaries between those layers.

For behavior with important boundary/failure semantics, cover nominal behavior, boundaries, invalid input, atomic failure, determinism, lifetime/ownership, and integration where relevant. For polymorphic acquisition abstractions such as `Chunk::Collection::Provider`, prefer a purpose-built test implementation with controlled outputs/call observation when the concrete production implementation is temporary scaffolding whose exact output should not become a durable unit-test contract.

Visual golden references require explicit human approval and must not be silently regenerated/overwritten.

Core test resources are copied by CMake from the source `resources/` tree into `${CMAKE_BINARY_DIR}/resources` when `EreliaCoreTestSuite` is built. CTest runs that suite with `${CMAKE_BINARY_DIR}` as its working directory, so tests must resolve project resources through relative paths such as `resources/voxels/...` rather than through a source-tree compile definition.

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
- OQ-037 — resolved generic Volume native-representation contract;
- OQ-038 — resolved ST-001-08 Chunk wire contract; Client retry/cache/recycle-threshold policy remains in ST-001-11;
- OQ-039 — resolved exact terrain-generator fixture coordinates/Definitions;
- OQ-029 through OQ-031  golden-image and performance-validation policy.

Do not hide one of these unresolved choices inside a coding ticket.
