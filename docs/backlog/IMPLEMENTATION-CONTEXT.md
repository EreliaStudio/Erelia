# Implementation Context — Taste, Conventions, and Working Knowledge

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

Coordinate vocabulary follows the same semantic ownership:

```cpp
Voxel::Cell::Coordinate
Voxel::Volume::LocalCoordinate
Voxel::Volume::VoxelSize
Chunk::Coordinate
```

`Voxel::Cell::Coordinate`, `Voxel::Volume::LocalCoordinate`, and `Chunk::Coordinate` are aliases of `spk::Vector3Int`. `Voxel::Volume::VoxelSize` is the scalar voxel-size type and is currently `float`. Terrain `Chunk` inherits `Voxel::Volume`, exposes its fixed one-world-unit voxel size as `Chunk::UnitSize`, and owns the Chunk coordinate conversion behavior/constants. A Volume validates its own local-coordinate bounds once the Volume contract is implemented; do not encode those runtime bounds by narrowing `LocalCoordinate` to `std::uint8_t`.

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

### `Voxel::Cell`

- one Cell fits in exactly one `std::uint32_t`;
- it remains trivially copyable;
- packed concepts are Definition ID + horizontal Orientation + vertical Flip;
- Definition ID 0 means empty;
- the packed representation is useful for compact storage/network transfer.

### `Voxel::Volume`

`Voxel::Volume` is a generic owning container for groups of voxel cells, not inherently a terrain Chunk.

Its intended shape includes:

- runtime dimensions;
- a uniform voxel size;
- contiguous owning `std::vector<Voxel::Cell>` storage;
- checked cell access;
- read-only contiguous cell access;
- controlled editing rather than arbitrary external writable storage.

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

The operators serialize the logical Volume contents—dimensions, voxel size, and contiguous Cell data. They must never raw-copy the C++ object representation of `Voxel::Volume`, because it owns a `std::vector`.

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
- elevated stairs, slabs, and slopes around Y ≈ 3;
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

The active near-term Epic is EP-001 — Voxel Terrain Delivery and Visual Validation.

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

- OQ-035 — remaining `Voxel::Cell` / `Voxel::Volume` details such as canonical empty/storage/editor behavior;
- OQ-036 — missing-neighbor/remesh policy for terrain meshing;
- OQ-037 — remaining scalar wire portability policy;
- OQ-038 — request/cache/eviction/partial-response details;
- OQ-039 — exact terrain-generator fixture coordinates/Definitions;
- OQ-029 through OQ-031 — golden-image and performance-validation policy.

Do not hide one of these unresolved choices inside a coding ticket.
