# Voxel Engine Reimplementation — Feature Design

> Historical terrain-engine plan. The current implementation direction is maintained in [docs/backlog](docs/backlog/README.md), including the unified volume/mesher architecture for terrain and models. Where plans differ, use the current backlog; retain this document as context for the existing terrain implementation.

## 1. Document purpose

This document describes the voxel feature we want to build for Playground. It is intended
to play the same role as a small game-design document: it explains what the feature is for,
how it should behave, the concepts it contains, and the quality and performance expected
from it.

It deliberately avoids prescribing detailed C++ signatures, exact header contents, or a
line-by-line implementation. Those decisions should follow the feature boundaries defined
here.

The old voxel implementation is inspiration, not an implementation to restore. The new
system must fit the current Sparkle engine, remain understandable, and establish a clean
base for a future multiplayer world.

## 2. Vision

We want a small, data-driven voxel engine capable of displaying and maintaining a world
made of independently loadable chunks.

The engine should let the game describe voxel geometry and appearance in data, request
parts of the world by chunk coordinates, generate those parts procedurally, convert their
contents into efficient render meshes, and display them without making every voxel an
individual engine entity.

The first implementation is local and single-player, but the world must already behave as
if chunk contents come from an external source. The chunk collection asks for a coordinate;
a generator responds to that request. Later, a multiplayer client will be able to replace
the local response with a request to a server without changing the collection, baker, or
renderer.

The desired character of the system is:

- simple to read;
- strongly separated by responsibility;
- deterministic;
- data-driven;
- efficient enough for a streamed voxel world;
- usable without rendering on a server;
- easy to extend without rewriting its foundation.

## 3. What we want to build

### 3.1 A reusable textured 3D rendering path

The current engine has 3D entities and transforms but does not yet have the complete
textured 3D mesh path needed by voxels.

The voxel feature therefore includes a general `TextureMesh3D`, a perspective `Camera3D`,
and a `TextureMeshRenderer3D`. These are not voxel-specific objects. They should be usable
later for characters, props, buildings, or any other textured 3D geometry.

The rendering path must provide 3D positions, normals, texture coordinates, indexed
triangles, model transforms, perspective projection, depth testing, and nearest-neighbor
texture sampling suitable for a voxel atlas.

The first version does not need a complete lighting system. It may be unlit or use a small
fixed lighting term, but normals must already be present so lighting can evolve without
changing the voxel mesh format.

### 3.2 Data-driven voxel shapes

A voxel shape describes geometry inside one logical voxel cell. Examples include a cube,
slab, slope, stair, crossed vegetation planes, posts, rocks, or future custom geometry.

Shapes must be loaded from JSON. A shape contains polygons, local texture coordinates, and
named material slots such as `top`, `side`, `bottom`, or `plane`.

Shape variety belongs to content rather than C++ inheritance. Adding a stair or a new rock
shape should mean adding JSON data, not adding another baker branch or concrete shape class.

Shapes are authored in normalized cell space. The engine analyses them once when loading:

- polygons on an outward-facing cell boundary become outer faces;
- polygons inside the cell become inner geometry;
- outer faces record which side of the cell they occupy;
- shapes record whether a side completely covers a cell boundary.

This preprocessing lets the baker work with a uniform representation regardless of the
authored shape.

### 3.3 Data-driven voxel definitions

A voxel definition describes a semantic voxel material such as grass, dirt, stone, water,
or a flower.

It selects a shape and maps that shape's named material slots to cells in a texture atlas.
Two voxel definitions can reuse the same geometry with different textures. One definition
may also gain gameplay properties later, such as solidity, tags, emitted light, or fluid
behavior.

For the first milestone, definitions only need identity, shape, and textures. The format
must catch missing texture slots, extra slots, invalid atlas positions, unknown shapes, and
duplicate voxel identifiers during startup.

### 3.4 A central voxel catalog

The loaded shapes and voxel definitions form one immutable catalog.

The catalog translates human-readable content identifiers into compact runtime identifiers.
JSON and generator setup may use names such as `grass`, while every placed cell stores only
a small numeric value.

The catalog must own every loaded shape and definition, validate their relationships,
assign runtime identifiers, provide fast baking lookup, report useful loading errors, and
become immutable before chunks are generated.

The catalog is shared by generators, bakers, tools, and future gameplay systems. It must
outlive every chunk that uses its identifiers.

### 3.5 Compact placed voxel cells

A placed voxel is represented by a packed `VoxelCell` value occupying exactly one 32-bit
word. It contains the voxel runtime identifier and the complete placement orientation; it
must not store these as separate C++ data members when they can share the same word.

The placement orientation has two independent parts.

**Horizontal orientation** is a rotation around the vertical Y axis, centered inside the
voxel cell. Shapes are authored with one canonical forward direction, local positive Z, and
may be placed in four cardinal orientations:

- positive Z: no horizontal rotation;
- positive X: one quarter-turn;
- negative Z: one half-turn;
- negative X: three quarter-turns.

This changes which direction a slope rises, which way stairs face, or which direction an
asymmetric shape points. It does not move the voxel into another cell. Cubes may look
identical in every orientation, but they use the same representation as every other shape.

**Vertical orientation** has two possibilities. A positive-Y placement uses the authored
shape normally. A negative-Y placement mirrors the shape vertically inside its own cell:
local height `y` becomes `1 - y`. A slab can therefore attach to the ceiling, and a slope
can become its upside-down counterpart. Baking must also correct polygon winding and normals
after this mirror so the flipped shape remains visible and lit correctly.

The packed 32-bit layout is:

| Bits | Meaning | Capacity |
|---|---|---|
| 0–28 | Voxel runtime identifier | 29 bits, including the empty value |
| 29–30 | Horizontal orientation | 2 bits, four cardinal values |
| 31 | Vertical orientation | 1 bit, positive Y or negative Y |

In hexadecimal terms, the conceptual masks are:

- voxel identifier: `0x1FFFFFFF`;
- horizontal orientation: `0x60000000`;
- vertical orientation: `0x80000000`.

Identifier `0` represents an empty cell. The remaining identifier space allows
536,870,911 non-empty runtime voxel identifiers, far beyond the expected catalog size while
keeping every cell four bytes.

The orientation enums should use the same already-shifted bit patterns stored in the packed
word. Combining an ID, a horizontal value, and a vertical value is therefore a mask-and-OR
operation. Reading them is a mask operation followed by conversion back to the corresponding
enum. The enum values and masks are part of the packed-cell contract and must not overlap.

The intended stored enum values are:

| Horizontal value | Stored bits | Meaning |
|---|---|---|
| Positive Z | `0x00000000` | Authored orientation; no quarter-turn |
| Positive X | `0x20000000` | One quarter-turn from positive Z |
| Negative Z | `0x40000000` | Two quarter-turns from positive Z |
| Negative X | `0x60000000` | Three quarter-turns from positive Z |

| Vertical value | Stored bits | Meaning |
|---|---|---|
| Positive Y | `0x00000000` | Use the authored vertical orientation |
| Negative Y | `0x80000000` | Mirror the shape vertically inside the cell |

The zero values deliberately represent the default authored placement. A default-created
cell therefore needs no extra orientation initialization, and an ID can be combined directly
with non-default enum bit patterns. Public cell operations should hide the masking details
so other systems cannot accidentally corrupt the identifier or combine overlapping values.

Although the feature is called `VoxelCell`, it should be a small semantic wrapper around an
unsigned 32-bit value rather than a struct with three independent fields. Unsigned storage
is preferred to `int32_t` because shifts, masks, and the highest bit have well-defined
behavior; it still occupies exactly the requested four bytes. The type should remain
trivially copyable, cheaply comparable, and have no padding or size overhead.

An empty cell has the all-zero packed value. Empty cells should normally have the default
orientation bits as well, keeping zero-initialized chunk memory valid. Empty checks only
need to inspect the identifier portion, so defensive loading may still tolerate orientation
bits on an empty identifier and normalize them when appropriate.

Cells must not contain strings, pointers, render meshes, callbacks, optional objects, or
gameplay instances. All meaning is resolved through the immutable voxel catalog.

A chunk is logically a three-dimensional grid, but its storage should be one contiguous,
flattened array of these packed four-byte cells. Three-dimensional coordinates are converted
to an array index by the chunk. This avoids nested-array overhead, improves iteration and
cache locality, and makes future serialization straightforward.

With the currently planned `16 x 16 x 16` chunk size, the raw cell payload is exactly
4,096 cells x 4 bytes = 16,384 bytes before the small amount of chunk-level metadata. This
fixed and predictable cost makes memory budgets and future chunk caches easier to reason
about.

The packed layout is an internal runtime contract. A future network or save serializer
should encode the 32-bit value explicitly with a defined byte order rather than assuming
that native process memory is automatically a portable wire format.

### 3.6 Independently owned chunks

The world is divided into fixed-size `Chunk` objects. A chunk owns a dense three-dimensional
array of voxel cells and knows its chunk coordinates.

Chunks are headless world data. They are not entities and do not render themselves.

A chunk supports safe local lookup, local/chunk/world coordinate conversion, correct
negative coordinates, grouped edits, a content revision, committed-edit notification, and
knowledge of which boundaries changed.

Grouped edits are important. A generator may write thousands of cells, but the operation
must produce one completed change notification rather than thousands of rebake requests.

### 3.7 A chunk collection with exclusive ownership

`ChunkCollection` is the single owner of the world's loaded chunks.

It knows which coordinates are absent, pending, available, or removed. It provides
world-cell lookup across chunk boundaries and announces lifecycle events to interested
systems.

The collection must not contain a concrete generator and must not generate terrain itself.
When a missing coordinate is requested, it announces a chunk request. A source that knows
how to supply chunks subscribes to that request.

Repeated requests for the same pending or available coordinate must be harmless. This is
essential for streaming and for multiplayer clients, where several systems may express
interest in the same area.

### 3.8 A subscribed generator hierarchy

`ChunkGenerator` is the abstract base for local procedural chunk generation.

It subscribes to requests emitted by the chunk collection. It knows how to generate a
requested coordinate and return the completed chunk to the collection. The collection does
not know which concrete generator answered.

The first concrete implementation is `DebugChunkGenerator`. It creates recognizable,
deterministic terrain using a seed and world coordinates. It should produce enough height
variation and material layering to validate chunk borders and visibility without trying to
be the final game generator.

Later generators may introduce biomes, caves, structures, or authored world plans. They
build ordinary chunks and publish them through the same collection.

Generation depends on the seed and requested coordinate, not on request order or previously
generated neighbors. The same chunk must be reproducible independently.

### 3.9 A single-purpose chunk baker

`ChunkBaker` converts one chunk's voxel cells into a textured 3D mesh.

It is an algorithmic service. It does not own chunks, subscribe to events, decide what to
load, create entities, or render anything.

The baker resolves cells through the catalog, applies orientation and flip, maps local UVs
into the atlas, handles normals, and emits triangles.

It removes invisible faces by inspecting neighboring cells inside the same chunk and across
loaded chunk boundaries. A missing neighboring chunk is treated as empty for rendering so
the current chunk remains visually complete. When a neighbor later arrives, the shared
border is baked again.

### 3.10 A controlled baking scheduler

`ChunkBakeScheduler` coordinates when baking occurs.

It listens to chunk availability, edits, and removal, then records which chunk coordinates
are dirty. A coordinate may be invalidated many times during one update but should be baked
only once.

This prevents cascading duplicate work when several adjacent chunks arrive together. It
also provides the natural location for a future per-frame baking budget while keeping the
baker itself simple.

### 3.11 Separate chunk presentation

A loaded chunk has a `ChunkView` only in a graphical client.

The view owns the engine entity and `TextureMeshRenderer3D` associated with one coordinate.
Its transform places a chunk-local mesh at the correct world origin.

`ChunkViewCollection` owns the active views. It creates a view when data becomes available,
replaces its mesh after baking, destroys it when data is removed, and keeps neighboring
borders visually synchronized.

This separation allows the same catalog, chunks, collection, and generation code to run on
a server without a graphics engine.

### 3.12 A chunk requester for streaming policy

`ChunkRequester` decides which coordinates the player or camera currently needs.

It does not own chunks and does not generate or bake them. It only expresses interest to
the collection.

The initial requester uses a small fixed radius around a focus. Later it may gain separate
load/unload distances, vertical ranges, movement prediction, distance priorities, cache
budgets, and multiple player interests.

Keeping this policy separate prevents movement, networking, generation, and rendering from
becoming one streaming class.

### 3.13 A lightweight world composition root

The scene needs one place that creates these services in a safe order and exposes them to
the game. This may be called `VoxelWorldRuntime`.

It is a composition root, not a god object. It wires together the catalog, collection,
generator, baker, scheduler, views, and requester without absorbing their behavior.

A headless server composition omits views, camera, renderer, and GPU resources. A client
composition may omit the procedural generator and use a future network source instead.

## 4. Conceptual class roster

Exact naming may be adjusted to current Sparkle conventions, but these responsibilities
should remain distinct.

| Concept | Purpose |
|---|---|
| `TextureMesh3D` | Generic indexed textured geometry with positions, normals, and UVs. |
| `Camera3D` | Supplies perspective projection and a view transform. |
| `TextureMeshRenderer3D` | Presents a textured 3D mesh attached to an entity. |
| `TextureMesh3DRenderCommand` | Performs the textured 3D draw. |
| `VoxelID` | Compact runtime identity stored in cells. |
| `VoxelOrientation` | Describes four rotations around the vertical axis. |
| `VoxelFlip` | Describes normal or vertically flipped placement. |
| `VoxelShape` | Immutable, preprocessed polygon geometry loaded from JSON. |
| `VoxelDefinition` | Semantic voxel type binding a shape to atlas textures. |
| `VoxelCatalog` | Loads, validates, owns, and resolves shapes and definitions. |
| `VoxelCell` | Compact value for one placed voxel. |
| `Chunk` | Headless fixed-size dense voxel storage with batched edits. |
| `Chunk::Editor` | Short-lived controlled mutation surface. |
| `ChunkCollection` | Sole owner and lookup service for requested and available chunks. |
| `ChunkGenerator` | Abstract subscribed source for locally generated chunks. |
| `DebugChunkGenerator` | Deterministic first generator used to validate the engine. |
| `ChunkBaker` | Stateless conversion from chunk data to render geometry. |
| `ChunkBakeScheduler` | Deduplicates dirty chunks and controls rebake timing. |
| `ChunkView` | Graphical entity and renderer representing one chunk. |
| `ChunkViewCollection` | Owns and synchronizes graphical chunk views. |
| `ChunkRequester` | Chooses which coordinates to request around a focus. |
| `VoxelWorldRuntime` | Small composition root wiring the services together. |

Likely future concepts, deliberately not required initially:

| Future concept | Purpose |
|---|---|
| `NetworkChunkSource` | Turns collection requests into client/server messages and publishes replies. |
| `ChunkSerializer` | Encodes chunk snapshots and later chunk deltas. |
| `ChunkCache` | Retains recently unused chunks under a memory budget. |
| `ChunkGenerationPipeline` | Coordinates biome, terrain, cave, and structure stages. |
| `VoxelSimulation` | Hosts fluid or other cell simulations without rendering coupling. |
| `VoxelNavigation` | Builds traversal information from loaded voxel data. |

## 5. Expected world behavior

### 5.1 Startup

The game loads the atlas, shapes, and voxel definitions before creating the world. Invalid
content stops startup with a useful error. A partially loaded catalog is never used.

World services are then created, subscriptions are established, and the requester asks for
the first visible coordinates.

### 5.2 Chunk request and arrival

When an absent coordinate becomes interesting:

1. The requester asks the collection for it.
2. The collection records that it is pending.
3. The collection announces the request once.
4. The subscribed generator creates the chunk.
5. The generator publishes the completed chunk.
6. The collection takes exclusive ownership and announces availability.
7. A view is created and the chunk is marked for baking.
8. Available neighbors are marked dirty so shared borders are corrected.

### 5.3 Cell edits

Systems edit one or several cells through a controlled chunk edit. Once committed:

- the chunk revision changes once;
- the collection announces one change;
- the edited chunk becomes dirty;
- only neighbors touching changed boundaries become dirty;
- the scheduler prevents repeated work before the next bake.

Assigning identical values produces no revision or rebake.

### 5.4 Chunk removal

When a chunk leaves the retained area, the collection removes its data, its view is
destroyed, and neighboring chunks become dirty because their border faces are visible again.

Removal must not leave an entity, subscription, or pointer referring to destroyed data.

### 5.5 Missing data

Rendering never requests a neighboring chunk merely to decide whether a face is visible.
Missing data is rendered as empty.

Gameplay queries should still distinguish an unloaded cell from loaded empty space whenever
that difference matters.

## 6. Required optimizations

The engine should be efficient by design while avoiding complex optimization before it is
justified.

### 6.1 Compact contiguous chunk storage

Cells use one dense allocation per chunk, with exactly one four-byte packed word per cell,
no per-cell heap allocations, and no voxel entities. The logically three-dimensional chunk
is physically a flat contiguous array. This benefits memory, cache locality, iteration,
generation, baking, serialization, and networking.

### 6.2 Dense runtime identifiers

Cells devote 29 of their 32 bits to numeric runtime IDs, reserve two bits for cardinal
orientation, and reserve one bit for vertical flip. String lookup happens during loading
and generator setup, not in the baker's inner loop. The implementation should explicitly
verify that `VoxelCell` remains four bytes and reject catalog IDs that exceed the packed
identifier mask.

### 6.3 Immutable preprocessed definitions

Normals, boundary classification, boundary coverage, and slot validation are calculated
once when content loads rather than rediscovered for every cell in every bake.

### 6.4 One render mesh per chunk

Visible geometry is combined into chunk meshes. The world never issues one draw call per
voxel or face. Transparency may later introduce a separate transparent product.

### 6.5 Hidden-face removal

Completely hidden faces are not emitted, including across chunk boundaries. Partial shapes
use conservative occlusion: remove a face only when full coverage is proven. Drawing an
extra hidden polygon is preferable to creating a visible hole.

### 6.6 Batched mutation

Generation and gameplay edits update revisions, events, and dirty state once per meaningful
batch.

### 6.7 Dirty-chunk deduplication

Dirty work is represented by unique coordinates. Many invalidations before a bake collapse
into one job per affected chunk.

### 6.8 Boundary-aware invalidation

An interior edit dirties only its chunk. A boundary edit dirties that chunk and the
specific touching neighbors, never the entire loaded world.

### 6.9 Stable request state

Duplicate requests do not duplicate generation. Failed requests can be retried. Available
chunks are looked up rather than regenerated.

### 6.10 Deterministic independent generation

Randomness derives from the seed and world coordinates rather than a mutable global random
sequence. Generation is reproducible and request-order independent.

### 6.11 Bounded work per frame

The first demonstration may bake synchronously, but the scheduler must allow a future limit
on the number or cost of bakes processed in one frame.

### 6.12 Cheap mesh handoff

Meshes use Sparkle's shared resource behavior so handing a result to a renderer does not
repeatedly copy all vertices and indices.

### 6.13 No automatic neighbor generation during baking

The baker only observes available data. It cannot call the requester or generator, avoiding
recursive generation and unpredictable stalls.

### 6.14 Measure before advanced meshing

Greedy meshing, vertex deduplication, mesh pools, background workers, levels of detail, and
GPU-driven rendering are not initial requirements. The design must leave room for them, but
they should follow profiling rather than complicate the foundation.

## 7. Extensibility expectations

### New shapes and materials

New geometry is added through JSON. New materials bind existing shapes to atlas cells.
Neither requires baker branches.

### New generators

A concrete generator responds through the established subscription and publishes ordinary
chunks. Storage and presentation remain unaware of its algorithm.

### Multiplayer client

The local generator is absent. A `NetworkChunkSource` subscribes to requests, asks the
server, validates replies, and publishes chunks on the client update thread.

### Multiplayer server

The server owns the authoritative collection and generator without views or GPU resources.
It can serialize requested chunks and later broadcast cell deltas.

### Gameplay metadata

Solidity, traversal, tags, light, and fluid properties can be added to immutable voxel
definitions without changing cell ownership or rendering boundaries.

### Alternative presentation

Tools, minimaps, and debug viewers may create their own presentation of the same headless
chunks without creating another world-data implementation.

## 8. First milestone content

The initial demonstration includes:

- cube, slab, slope, stair, and crossed-plane shapes;
- grass, dirt, stone, and a plant/flower definition;
- a small nearest-filtered atlas;
- deterministic layered terrain;
- several chunks around a focus;
- at least one negative chunk coordinate;
- visible orientation and flip;
- cross-chunk face removal;
- runtime edits causing localized rebakes;
- chunk removal restoring exposed neighbor faces.

This proves a functioning engine rather than a static voxel model.

## 9. Quality expectations

### Clarity

Each class has one primary reason to change. Ownership is visible from the structure, not
explained only in comments.

### Headless operation

Catalog loading, generation, chunk ownership, coordinate lookup, edits, and baking logic are
testable without a window. Only GPU validation and presentation require rendering.

### Failure behavior

Invalid content, unknown voxel names, duplicate publication, and contract violations fail
clearly rather than silently substituting unrelated content.

### Lifetime safety

Subscriptions use RAII contracts. Views unregister entities before the engine dies. The
catalog outlives chunks, and the collection outlives subscribed providers and views.

### Determinism

Given the same content, seed, and coordinate, generation and baked topology produce the
same result regardless of request order.

### Visual correctness

There are no cracks caused by negative coordinates, chunk transforms, missing-neighbor
updates, orientation, flip, or face winding.

## 10. Delivery stages

### Stage 1 — Textured 3D foundation

Display a manually constructed textured object. Validate camera, perspective, transforms,
texture sampling, and depth.

### Stage 2 — Content model

Load and validate JSON shapes and voxel definitions. Inspect every initial shape before
procedural generation is introduced.

### Stage 3 — Headless chunk world

Create cells, chunks, collection ownership, request state, edits, revisions, and coordinate
conversion. Validate negative coordinates.

### Stage 4 — Subscribed generation

Connect `DebugChunkGenerator` through request subscriptions and prove deterministic,
idempotent generation.

### Stage 5 — Baking and occlusion

Build meshes, remove hidden faces, handle generic shapes, and validate shared chunk borders.

### Stage 6 — Views and streaming

Add views, dirty scheduling, requester policy, localized rebakes, and removal. Display the
complete multi-chunk demonstration.

### Stage 7 — Hardening

Test malformed content, failed generation and retry, repeated load/unload, all boundaries,
shutdown order, and bursts of arriving chunks.

## 11. Features deliberately deferred

The first milestone does not need final biome generation, caves, structures, fluids,
transparency sorting, prefabs, navigation, collision, ray casting, persistence, networking,
background workers, greedy meshing, ambient occlusion, shadows, complete lighting, or
levels of detail.

These are future features, not hidden requirements for the foundation.

## 12. Feature definition of done

The foundation is complete when:

- Sparkle can display textured 3D chunk meshes with perspective and depth;
- shapes and materials load from validated JSON;
- shape variety does not require C++ subclasses;
- cells are compact and chunks use contiguous storage;
- `ChunkCollection` exclusively owns chunk data;
- it requests chunks without knowing a concrete generator;
- an abstract generator responds through a retained subscription;
- the first generator creates deterministic terrain;
- `ChunkBaker` is the only voxel-to-mesh component;
- hidden faces are removed inside and across chunks without holes;
- dirty work is deduplicated and boundary-aware;
- graphical views are separate from headless data;
- several chunks stream around a focus, including negative coordinates;
- edits and removal update only necessary views;
- the system can be composed without graphics for a future server;
- networking can later provide client chunks without redesigning ownership or baking.
