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

The original synchronous `provide(coordinate) -> Chunk` shape was superseded during ST-001-06 implementation by an explicit asynchronous state machine.

Collection state for one coordinate is exactly:

```text
Absent
Pending(generation)
Available(Chunk)
```

`request(coordinate)` performs the atomic `Absent -> Pending` transition under the Collection's protected storage and allocates a monotonically increasing request generation. If the coordinate is already Pending or Available, the request is rejected and the Provider is not invoked again.

The Provider receives the immutable request identity and is update-driven:

```cpp
class Chunk::Collection::Provider
{
public:
    virtual ~Provider() = default;

    virtual void request(
        const Chunk::Collection::Request& request) = 0;

    virtual void update(
        Chunk::Collection& collection) = 0;
};
```

The Collection owns coordinate state. The Provider owns scheduling/execution policy.

A Provider may buffer requests from arbitrary producer threads, submit asynchronous work, and later call `publish(request, chunk)` or `fail(request)` from its update pass.

Publication/failure succeeds only if the coordinate is still Pending with the exact same generation. A stale result from an older generation cannot overwrite a newer request or replacement.

Available Chunk lookup uses `tryGet(coordinate) -> std::optional<Chunk>`. The Chunk is copied while a short shared `spk::ProtectedData` Reader is held; the lock is released immediately and the immutable shared-backed Chunk snapshot remains independently valid.

Collection mutable coordinate state uses `spk::ProtectedData`; shared immutable Chunk content is the lifetime mechanism, not a substitute for synchronizing the coordinate map.

### Replacement rather than mutation

A Chunk already published through a Collection is never edited in place.

A later result for the same coordinate replaces the complete stored Chunk value. Existing copies obtained by other threads continue to reference the previous immutable Cell content until those copies are destroyed.

Replacement is an upsert owned by ST-001-06: if the coordinate is absent, the supplied complete Chunk is inserted immediately. Replacement/upsert never invokes the Provider.

This rule is intentionally suitable for the future Client without inventing an "empty Chunk means loading" sentinel:

- an absent requested coordinate becomes explicit Pending state;
- repeated requests are suppressed while Pending;
- canonical Server data can publish the complete immutable Chunk for the matching generation;
- whole-value replacement remains available for later canonical refresh/replacement;
- the render thread may safely finish work against an older copied Chunk value.

The exact network batching, retry, eviction, unsolicited-response and partial-response policy remains owned by OQ-038 / ST-001-11. The generic Collection already owns duplicate Pending suppression and stale-generation rejection.

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
- Server procedural generation and future Client request-driven acquisition share one asynchronous Core Collection/Provider state machine;
- Absent/Pending/Available is explicit; no empty-Chunk loading sentinel is required;
- request generations prevent stale asynchronous results from overwriting newer state;
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
- Collection performs Absent -> Pending -> Available and does not enqueue the Provider again while Pending or Available;
- stale request generations cannot publish over a newer request;
- Collection exclusively owns a concrete moved Provider through its private polymorphic allocation and rejects lvalue Provider construction;
- replacement/upsert of an absent coordinate inserts the supplied Chunk without invoking the Provider;
- a copied Chunk remains valid after the Collection entry is replaced;
- concurrent Collection lookup/replacement follows the implemented synchronization contract without exposing mutable Chunk Cells;
- generic Volume Message decode replaces content rather than overwriting Cell storage shared with an existing copy.

Future ST-001-08/ST-001-11 tests own dedicated Chunk wire encoding and Client network retry/cache/response semantics. The generic Pending state and stale-generation rejection are already fixed here.

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


## ST-001-06 asynchronous refinement

On 24 September 2026 the project owner explicitly replaced the synchronous Provider portion of this record with the asynchronous contract above and approved:

- explicit Collection `Absent / Pending / Available` state;
- `spk::ProtectedData` for the Collection coordinate container;
- short read sections that copy immutable Chunks by value before releasing the Reader;
- no Collection lock held during generation work;
- a monotonically increasing request generation used to reject stale asynchronous results;
- Provider-side deduplicated request buffering;
- update-thread publication of completed asynchronous results;
- the local headless `spk::Task`, `spk::WorkerPool`, `spk::ThreadSafeSet`, and `spk::Singleton` prototype direction captured by DR-020.

This refinement supersedes every earlier sentence in this record that described `Provider::provide()` as synchronous or an empty Chunk as the generic Pending sentinel.

## ST-001-09 batched acquisition refinement

On 26 September 2026 the project owner refined the asynchronous Collection/Provider contract again to use Sparkle Version-0.1.3's generic manually-settled `spk::Task<TResult>` model.

This refinement supersedes the ST-001-06 Provider buffering / `update(Collection&)` polling portion above for the target ST-001-09 architecture.

The semantic coordinate states remain:

```text
Absent
Pending(asynchronous Chunk work)
Available(Chunk)
```

The Pending state is backed by the `spk::Task<Chunk>::Answer` for the unique in-flight generation/acquisition of that coordinate. The exact private state representation remains an implementation detail. Existing stale-work protection remains required; a stale asynchronous result must not overwrite newer authoritative state.

`Chunk::Collection::Provider` now owns only single-coordinate generation execution. Its target contract is conceptually:

```cpp
virtual spk::Task<Chunk>::Answer request(
    const Chunk::Coordinate& coordinate) = 0;
```

For an Absent coordinate, the Provider submits one callable to the shared WorkerPool and returns its `Task<Chunk>::Answer`. The Provider no longer receives Collection batches and no longer owns `update(Collection&)`.

`Chunk::Collection` owns batching. Its acquisition operation accepts a vector of coordinates and returns one `spk::Task<BatchResult>::Answer` representing the whole requested batch. The public result shape is fixed as:

```cpp
struct Chunk::Collection::BatchResult
{
    struct Acquired
    {
        Chunk::Coordinate coordinate;
        Chunk chunk;
    };

    struct Failed
    {
        Chunk::Coordinate coordinate;
        std::exception_ptr exception;
    };

    std::vector<Acquired> acquired;
    std::vector<Failed> failed;
};
```

Every requested coordinate appears exactly once across `acquired` and `failed`. These types are acquisition-domain types only and have no dependency on `Chunk::Protocol::Response`.

For each coordinate in one Collection batch:

- Available -> copy the existing Chunk directly into the batch result;
- Pending -> reuse the already-existing `Task<Chunk>::Answer` and subscribe to its completion;
- Absent -> create the coordinate's Pending entry, ask the Provider for one `Task<Chunk>::Answer`, store/reuse that Answer, and subscribe to its completion.

The Collection creates its batch `spk::Task<BatchResult>` directly and does **not** submit it to the WorkerPool. It settles that Task from child completion callbacks, so no worker is occupied merely waiting for other workers.

Batch settlement waits for every coordinate dependency and preserves per-coordinate outcomes:

- successful coordinate acquisition contributes the coordinate and its shallow-copied immutable Chunk;
- failed coordinate acquisition contributes a networking-agnostic failure outcome for that coordinate;
- after every requested coordinate is terminal, the Collection calls `validate(BatchResult)` once with the complete set of outcomes;
- the Collection batch Task becomes `Failed` only if a batch/aggregation-level failure prevents production of a valid `BatchResult`.

Multiple overlapping Collection requests that include the same Pending coordinate must subscribe to the same in-flight coordinate Answer and must not invoke the Provider a second time for that coordinate.

TerrainNode may group several Collection batch Answers in one `spk::TaskGroup<BatchResult>`. Because TaskGroup accepts arbitrary `Task<TResult>::Answer` values, these manually-settled Collection Tasks compose with the same API as WorkerPool-produced Tasks.

ST-001-09 later refined the terminal protocol representation so `Chunk::Protocol::Response` owns nested `Response::Success { coordinate, chunk }` and `Response::Failure { coordinate, Failure::Code, message }` entries. Collection remains networking-agnostic and must not return those protocol types directly. TerrainNode owns the translation from acquisition outcomes into protocol entries. On 26 September 2026 the project owner explicitly selected per-coordinate failure-as-data semantics and fixed the Collection result names as `Chunk::Collection::BatchResult::Acquired` and `Chunk::Collection::BatchResult::Failed`; `Failed` preserves the originating `std::exception_ptr`. Ordinary coordinate generation/acquisition failure completes the Collection batch with a `Failed` entry; it does not fail the batch Task.
