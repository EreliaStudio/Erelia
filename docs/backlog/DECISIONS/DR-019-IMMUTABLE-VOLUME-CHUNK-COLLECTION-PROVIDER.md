# DR-019 — Immutable shared Volume content and Chunk Collection/Provider contract

**Status:** Resolved
**Date opened:** 2026-09-24
**Date resolved:** 2026-09-24
**Applies to:** Core voxel ownership, Chunk construction/lifetime, Chunk acquisition/storage, Server terrain providers, future Client Chunk requests
**Supersedes:** DR-012 Volume deep-copy/direct-Lease ownership details; DR-017 destination-buffer reuse detail

## Context

ST-001-03 originally implemented each `Voxel::Volume` as the direct owner of one pooled `Buffer::Lease`. Copying a Volume therefore deep-copied its Cells into another pooled Buffer.

While specifying ST-001-06, the project owner clarified a stronger immutable-value model:

- built Volumes and Chunks should remain immutable;
- copying a Volume/Chunk should cheaply keep the same immutable Cell content alive;
- an update/render thread may continue using an older Chunk while another thread replaces the Collection entry with a newer complete Chunk;
- the Collection should not need to expose `shared_ptr` merely to obtain that lifetime behavior;
- Chunk acquisition needs one shared abstraction usable by procedural Server generation and later Client request-driven acquisition.

## Decision

### Immutable shared Volume content

A built `Voxel::Volume` owns its Cell storage through shared immutable backing content.

The exact private representation may evolve, but the semantic contract is:

- mutable construction still happens only through `Voxel::Volume::Builder`;
- Builder owns one mutable pooled `Voxel::Volume::Buffer::Lease`;
- `std::move(builder).build()` publishes that Lease as immutable Volume content;
- copying a built Volume shares the immutable backing Cell content instead of deep-copying all Cells;
- copying/assigning a Volume therefore preserves the same Cell-storage lifetime and is intended to be cheap;
- no public API permits Cell mutation after build;
- destroying/replacing the final owner of the shared content destroys its Lease and returns the Buffer to its originating Sparkle Pool;
- dimensions and unit size remain ordinary immutable Volume value metadata and may be copied directly;
- Volume move semantics still leave the moved-from source in the valid default-empty state.

A `Builder(Volume&&)` path, if retained, must never permit mutation of Cell storage still observed by another copied Volume. It may directly reuse storage only when that is provably exclusive; otherwise it must obtain/copy mutable storage before modification. This is an implementation optimization, not permission to violate built-value immutability.

### Pool and networking consequence

The existing power-of-two pooled Buffer classes remain valid.

Because a destination Volume may now share its current Cell storage with other Volume copies, generic Message extraction must not overwrite that existing storage in place. Decode instead reconstructs fresh valid content and replaces the destination's shared content only after full decode validation succeeds.

Therefore the ST-001-05 same-pool destination-buffer reuse optimization is superseded. Destination preservation on failed decode remains required.

### Derived Chunk construction

`Chunk` remains a semantic specialization of `Voxel::Volume`.

The reusable implementation state/constructors in `Voxel::Volume` and `Voxel::Volume::Builder` that are legitimately required by derived semantic Volume types become `protected` rather than `private`. Unrelated implementation helpers should remain private.

`Voxel::Volume::Builder` is no longer `final`.

`Chunk` exposes a nested `Chunk::Builder` derived from `Voxel::Volume::Builder`.

`Chunk::Builder` fixes the Chunk invariants structurally:

- dimensions are exactly `16 x 16 x 16`;
- unit size is exactly `1.0f`;
- inherited Builder Cell mutation is available during construction;
- `std::move(builder).build()` returns a `Chunk`, not a generic `Voxel::Volume`;
- the trusted build path transfers the Builder's pooled Cell storage directly into the new Chunk without constructing an intermediate Volume and without re-validating dimensions/unit size.

`Chunk` additionally keeps a public checked conversion constructor from `Voxel::Volume&&`. That path validates exactly `16 x 16 x 16` and unit size `1.0f` before accepting an arbitrary generic Volume as a Chunk.

`Chunk` itself does **not** store its `Chunk::Coordinate`. Coordinate is collection/addressing metadata external to the immutable Chunk value.

### Chunk Collection and Provider

Core introduces `Chunk::Collection` as the shared coordinate-to-Chunk ownership abstraction.

The Provider concept is nested because it exists specifically to satisfy a Collection:

```cpp
class Chunk::Collection
{
public:
    class Provider;
};
```

The Collection exclusively owns its Provider through a private `std::unique_ptr<Provider>`, but pointer ownership is not exposed at the public call site.

Construction accepts only a concrete Provider rvalue and moves that concrete object into the Collection-owned polymorphic allocation:

```cpp
template<typename TProvider>
    requires
        std::derived_from<
            std::remove_cvref_t<TProvider>,
            Provider> &&
        (!std::is_lvalue_reference_v<TProvider>)
explicit Collection(TProvider&& provider)
    : _provider(
        std::make_unique<std::remove_cvref_t<TProvider>>(
            std::forward<TProvider>(provider)))
{
}
```

This keeps Provider polymorphism and exclusive ownership while allowing call sites such as `Collection(PrototypeChunkProvider{})` instead of requiring them to construct a `std::unique_ptr`. Lvalue construction is intentionally rejected: transfer of the Provider into the Collection must be explicit through an rvalue. The concrete Provider must therefore be movable.

A successfully constructed Collection always owns one Provider. Null/absent Provider state is not part of the Collection contract.

The Provider contract is synchronous from the Collection's point of view:

```cpp
class Chunk::Collection::Provider
{
public:
    virtual ~Provider() = default;

    [[nodiscard]] virtual Chunk provide(
        const Chunk::Coordinate& coordinate) = 0;
};
```

A Collection lookup behaves conceptually as:

1. if the coordinate is already stored, return that Chunk value;
2. otherwise call the attached Provider;
3. store the provided Chunk under the requested coordinate;
4. return a Chunk value.

The Collection owns the coordinate identity. Chunk values remain unaware of their coordinate.

The Collection stores immutable published Chunk values. Callers obtain Chunks by value rather than mutable references or exposed `shared_ptr` handles. Copying the Chunk keeps the same immutable Volume content alive, so a consumer can continue using an old Chunk after the Collection replaces its entry.

Collection lookup/replacement must be safe for the intended update-thread/render-thread concurrency. The exact standard synchronization primitive is an implementation detail; shared immutable Chunk content is the lifetime mechanism, not a substitute for synchronizing the Collection's coordinate container.

### Replacement rather than mutation

A Chunk already published through a Collection is never edited in place.

A later result for the same coordinate replaces the complete stored Chunk value. Existing copies obtained by other threads continue to reference the previous immutable Cell content until those copies are destroyed.

Replacement is an upsert owned by ST-001-06: if the coordinate is absent, the supplied complete Chunk is inserted immediately. Replacement/upsert never invokes the Provider.

This rule is intentionally suitable for the future Client:

- a Client-side Provider may send a Server request and immediately provide an empty valid 16x16x16 Chunk placeholder;
- that placeholder prevents repeated missing lookups from requiring mutation of one Chunk object;
- when canonical Server data arrives, the Client replaces the complete Collection entry with the decoded canonical Chunk;
- the render thread may safely finish work against an older copied Chunk value.

The exact Client outstanding-request/retry/stale-response policy remains owned by OQ-038 / ST-001-11. The generic Collection behavior for replacement of an absent coordinate is resolved here as direct insertion/upsert.

### Server implementation

ST-001-06 introduces the Core abstractions above and a Server-owned concrete provider for the temporary deterministic validation terrain.

The intended concrete name is `PrototypeChunkProvider`, deriving from `Chunk::Collection::Provider`.

It has no Client authority and does not implement production world generation.

### Future Chunk wire format

Generic `Voxel::Volume` serialization from DR-017 remains available for genuinely runtime-sized Volumes.

A future dedicated Chunk codec should exploit the fixed Chunk invariants:

- serialize only the 4096 packed Cells in existing Y-fastest, then X, then Z order;
- do not transmit `16 x 16 x 16` dimensions;
- do not transmit unit size `1.0f`;
- decode into a new complete immutable Chunk;
- higher-level Chunk message identifiers and coordinate association remain owned by ST-001-08.

The dedicated Chunk codec is **not** implemented by ST-001-06.

## Consequences

- Volume/Chunk copies become cheap immutable snapshots rather than deep Cell copies.
- old Chunk snapshots remain alive naturally across Collection replacement;
- the Collection API does not need to expose shared-pointer ownership to consumers;
- Server procedural generation and future Client request-driven acquisition share one Core Collection/Provider shape;
- Client placeholder data remains explicitly non-authoritative and is replaced by Server-canonical data;
- generic Volume decode loses the previous in-place same-pool reuse optimization;
- Chunk-specific serialization can later omit redundant fixed metadata.

## Required tests

ST-001-06 implementation must update/add Core coverage proving at least:

- Volume copy construction/assignment share immutable Cell storage rather than deep-copying it;
- destroying/replacing one copy does not invalidate another copy;
- moving a Volume preserves the existing moved-from-empty contract;
- any retained Volume-to-Builder path cannot mutate another Volume copy's content;
- Chunk::Builder always produces `16 x 16 x 16`, unit size `1.0f`;
- Chunk::Builder returns `Chunk`;
- checked `Chunk(Voxel::Volume&&)` accepts the exact Chunk invariants and rejects incompatible dimensions/unit size;
- Chunk does not require/stash a coordinate;
- Collection caches a provided Chunk by coordinate and does not call the Provider again for an already stored coordinate;
- Collection exclusively owns a concrete moved Provider through its private polymorphic allocation and rejects lvalue Provider construction;
- replacement/upsert of an absent coordinate inserts the supplied Chunk without invoking the Provider;
- a copied Chunk remains valid after the Collection entry is replaced;
- concurrent Collection lookup/replacement follows the implemented synchronization contract without exposing mutable Chunk Cells;
- generic Volume Message decode replaces content rather than overwriting Cell storage shared with an existing copy.

Future ST-001-08/ST-001-11 tests own dedicated Chunk wire encoding and Client request/placeholder/replacement state transitions.

## Resolution provenance

Resolved directly with the project owner during ST-001-06 specification on 24 September 2026.

The owner explicitly approved:

- `Chunk::Builder` as the semantic Builder exposed for Chunk construction;
- protected reusable Volume/Builder internals for derived semantic Volume types;
- public checked `Chunk(Voxel::Volume&&)`;
- Chunk values not storing their own coordinates;
- `Chunk::Collection::Provider` as the nested acquisition abstraction;
- Collection ownership of its Provider through a private `std::unique_ptr<Provider>`, with the public API taking only a concrete Provider rvalue through the constrained templated constructor rather than exposing pointer ownership;
- null/absent Provider construction being unrepresentable and lvalue Provider construction being rejected;
- complete-Chunk replacement rather than Cell mutation, with absent-coordinate replacement defined as direct insertion/upsert without Provider invocation;
- shared immutable Volume content so copied Chunk values keep older content alive across replacement;
- removal of same-destination-Lease network decode reuse as the accepted consequence;
- future dedicated fixed-size Chunk serialization that omits dimensions/unit size.

## Supersession

This record supersedes only the following earlier details:

- DR-012 / ST-001-03: direct per-Volume Lease ownership and deep-copy Volume copy semantics;
- DR-012 / ST-001-03: unconditional moved-Volume-to-Builder Lease reuse when another copy could observe the same immutable content;
- DR-017 / ST-001-05: in-place destination Buffer reuse during Message decode.

All other Cell packing, Volume indexing, pooling, generic Volume validation/wire order, destination-on-failure, and semantic determinism rules remain active.
