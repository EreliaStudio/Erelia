# DR-020 — Headless asynchronous task infrastructure prototype

**Status:** Resolved
**Date opened:** 2026-09-24
**Date resolved:** 2026-09-24
**Applies to:** Core asynchronous work, Server worker execution, Sparkle task/worker primitives consumed by Erelia
**Supersedes:** —

## Context

ST-001-06 requires Chunk generation to stop blocking the requesting thread. The same requirement will recur for other engine work, so Chunk-specific threads or direct `std::promise` / `std::future` plumbing would create the wrong ownership boundary.

Sparkle Version-0.1.3 already provides headless-safe synchronization/container primitives such as `spk::ThreadSafeFIFO` and `spk::ProtectedData`, but it does not yet provide the generic task/worker abstractions required here.

The project owner selected a reusable Sparkle-shaped API, implemented temporarily inside Erelia until it is mature enough to propose upstream.

## Decision

### Current placement

DR-020 originally introduced temporary Erelia-local Sparkle-shaped prototypes so ST-001-06 could proceed before the reusable contracts were mature.

Those reusable facilities have since been upstreamed and merged into Sparkle Version-0.1.3. Erelia must consume the Sparkle-owned implementations directly and must not retain duplicate local implementations.

The relevant Sparkle-owned primitives are:

- `spk::ThreadSafeSet<T>`;
- `spk::ThreadSafeQueue<T>`;
- `spk::Task<TResult>`;
- `spk::WorkerPool`;
- `spk::TaskGroup<TResult>`;
- `spk::Singleton<T>`;
- the thread-safe `spk::ContractProvider` completion infrastructure.

They remain headless/Core-compatible and do not depend on Window, graphics, OpenGL, input, UI, or presentation APIs.

### `spk::ThreadSafeSet<T>`

`ThreadSafeSet` mirrors the ownership shape of Sparkle's existing `ThreadSafeFIFO`:

- one shared internal `State`;
- nested `Producer`;
- nested move-only `Consumer`;
- `Endpoints`;
- `create()`, `producer()`, and `consumer()`;
- stop-token-aware `wait()`;
- reusable `drain()`.

Unlike FIFO, insertion is set-based and therefore deduplicates equal values atomically. `publish()` / `emplace()` return whether a new value was inserted.

### `spk::ThreadSafeQueue<T>`

`ThreadSafeQueue` mirrors the same shared `State` / `Producer` / move-only `Consumer` / `Endpoints` structure as Sparkle's `ThreadSafeFIFO`.

It owns FIFO-ordered values and is designed for worker-style individual consumption rather than batch draining. `waitPop(stopToken)` blocks while empty, atomically removes and returns exactly one value when work is available, and returns `std::nullopt` when stop is requested while no value remains. Multiple Consumers may wait on the same shared State; every queued value is removed by exactly one Consumer.

This primitive is headless and generic. It is not specific to `WorkerPool`.

### `spk::Task<TResult>`

Sparkle Version-0.1.3 now defines `Task<TResult>` as a generic asynchronous result state rather than as an executable callable.

A Task owns:

- one shared Pending / Completed / Failed state;
- an optional completed `TResult`;
- an `std::exception_ptr` failure;
- a lightweight shared `Task<TResult>::Answer`;
- completion subscriptions through `ContractProvider`.

The only lifecycle states are:

- `Pending`: no result or failure is available yet;
- `Completed`: a fully valid `TResult` exists;
- `Failed`: no valid result exists and an exception is stored.

The producer of the asynchronous work settles the Task explicitly:

```cpp
task.validate(result);
task.fail(exception);
```

A Task has no execution lambda and no worker-execution method. Settlement is one-shot: repeated terminal transitions are programming errors, and `fail(nullptr)` is rejected.

`Answer::result()` is valid only for `Completed`. `Answer::failure()` is valid only for `Failed`. Invalid access throws `spk::Exception`.

`Answer::subscribeToCompletion(...)` is race-safe with settlement. A Pending Answer registers the callback; a terminal Answer invokes a late subscriber immediately. Callback exceptions are isolated from Task settlement and from other subscribers.

### `spk::WorkerPool`

`WorkerPool` is a headless generic executor and is one producer of Sparkle Tasks; it does not define the Task abstraction itself.

It owns:

- a pool of standard C++ worker threads;
- a `spk::ThreadSafeQueue<std::unique_ptr<WorkerPool::Job>>` of polymorphic jobs;
- the abstract `WorkerPool::Job` type-erasure boundary required to store jobs with heterogeneous result types in one queue;
- an internal `TaskJob<TResult>` that derives publicly from `Job` and privately from `Task<TResult>`.

`TaskJob<TResult>` owns the executable callable. Its worker execution performs the equivalent of:

```cpp
try
{
    validate(operation());
}
catch (...)
{
    fail(std::current_exception());
}
```

Callers submit the callable directly:

```cpp
auto answer = workerPool.submit(
    []() -> TResult {
        return produceResult();
    });
```

`submit(...)` returns the same `Task<TResult>::Answer` type that can also observe a manually settled generic Task. Workers execute type-erased Jobs without knowing their result types.

The default pool size is the standard-library hardware concurrency when available, with one worker as fallback. Explicit construction with zero workers is rejected.

Workers consume jobs through separate `ThreadSafeQueue::Consumer` handles and block through `waitPop(stop_token)`. `std::jthread` stop tokens provide shutdown signaling; queued work already visible to the queue remains consumable before workers exit.

### `spk::Singleton<T>`

`Singleton<T>` owns one `inline static std::unique_ptr<T>`.

Approved API:

```cpp
spk::Singleton<T>::instanciate(T value);  // when T is movable
spk::Singleton<T>::instanciate(T* value); // ownership transfer, also supports non-movable T
spk::Singleton<T>::instance();            // T&
spk::Singleton<T>::isInstanciated();      // bool
```

The pointer overload rejects `nullptr`. `instance()` throws `spk::Exception` when no value is installed.

Singleton initialization is startup/lifecycle configuration, not a concurrently mutated runtime operation.

### Server WorkerPool lifetime

The Server instantiates `spk::Singleton<spk::WorkerPool>` at startup.

Server systems submit executable work directly through:

```cpp
spk::Singleton<spk::WorkerPool>::instance().submit(
    []() -> TResult {
        return produceResult();
    });
```

The returned value is `spk::Task<TResult>::Answer`.

The WorkerPool remains generic and has no Chunk, Server-authority, rendering, or networking semantics.

## Chunk-provider consequence

ST-001-09 supersedes the earlier Provider polling shape introduced during ST-001-06.

`Chunk::Collection::Provider` is a single-coordinate generation driver. For one missing coordinate it submits one callable to the shared WorkerPool and returns the resulting `spk::Task<Chunk>::Answer`.

Conceptually:

```cpp
virtual spk::Task<Chunk>::Answer request(
    const Chunk::Coordinate& coordinate) = 0;
```

The Provider no longer buffers Collection batches and no longer owns an `update(Collection&)` polling phase.

`Chunk::Collection` owns batching and aggregation. A batched Collection request creates a generic `spk::Task<BatchResult>` that is **not** submitted to the WorkerPool. The Collection:

- copies already-Available Chunks directly into the batch result;
- reuses and subscribes to the existing `Task<Chunk>::Answer` for already-Pending coordinates;
- asks the Provider for exactly one `Task<Chunk>::Answer` for each Absent coordinate;
- subscribes to every asynchronous coordinate Answer;
- settles its batch Task only from those completion callbacks.

No worker thread is consumed merely to wait for child Chunk generation.

If any coordinate acquisition/generation Task fails, the Collection batch Task is Failed rather than validating a partial `BatchResult`. The exact Server wire-level mapping of that failed batch/request remains owned by ST-001-09 because DR-022 currently defines no request-level failure state or generic generation-error `ChunkError` code.

## ST-001-09 TaskGroup refinement

Sparkle Version-0.1.3 owns Task completion contracts, the generic manually-settled Task contract described above, thread-safe `spk::ContractProvider`, and `spk::TaskGroup<TResult>`.

The authoritative composition contract is:

- `Task<TResult>::Answer::subscribeToCompletion(...)` returns a normal `ContractProvider<>::Contract`;
- subscribing while a Task is Pending registers the completion callback, while subscribing after it is already terminal invokes the callback immediately;
- `ContractProvider` synchronizes cross-thread subscription, resignation, invalidation, validity checks, and dispatch;
- `spk::TaskGroup<TResult>` groups `Task<TResult>::Answer` values regardless of whether they came from WorkerPool execution or manual Task settlement;
- TaskGroup does not submit child Tasks and does not consume another WorkerPool thread merely to wait;
- the grouped Answer stays Pending while any child is Pending;
- it becomes Completed when every child completed successfully;
- it becomes Failed only after every child is terminal when at least one child failed;
- child Answers remain individually available in insertion order;
- TaskGroup exposes the same completion-subscription model.

ST-001-09 uses these primitives directly from Sparkle. The temporary Erelia-local TaskGroup prototype is obsolete and must be removed from the active feature branch.

The intended Erelia ownership is now:

- TerrainNode splits one Client protocol request into smaller internal coordinate batches;
- `Chunk::Collection::request(vector<Coordinate>)` returns one `Task<BatchResult>::Answer` per internal batch;
- Collection owns authoritative Available/Pending/Absent state, pending-work reuse, deduplication, aggregation, and batch Task settlement;
- Provider produces one WorkerPool-backed `Task<Chunk>::Answer` per Absent coordinate;
- Collection batch Tasks are manually settled generic Tasks and never occupy workers merely to wait;
- any failed coordinate Task makes its Collection batch Task Failed;
- TerrainNode groups the Collection batch Answers in one Sparkle TaskGroup and subscribes once to grouped completion;
- grouped completion is the point at which the Server may produce the one terminal protocol outcome correlated with the original Client RequestID.

The exact private Collection state representation remains an implementation detail. The final wire-level mapping for a failed Collection batch/request, plus Server disconnect/reply/shutdown behavior, remain ST-001-09 decisions.

## Consequences

- asynchronous execution becomes reusable outside terrain generation;
- worker threads stay independent of engine presentation APIs;
- Chunk generation does not require direct `std::future` ownership in gameplay systems;
- typed Task results coexist on one polymorphic Job queue;
- WorkerPool does not duplicate mutex/condition-variable/queue synchronization already owned by `ThreadSafeQueue`;
- request producers can deduplicate batches through `ThreadSafeSet`;
- the asynchronous primitives proven in Erelia are now Sparkle-owned and consumed directly by Erelia.

## Required tests

Because these Erelia-local `spk` types are intended as candidates for later direct integration into Sparkle, their test depth is intentionally larger than ordinary ticket scaffolding.

Core tests cover at least:

- ThreadSafeSet direct and endpoint APIs, duplicate insertion reporting, contains/erase, drain/reset behavior, re-request after drain, move-only values, endpoint lifetime, wait/publish wakeup, stop-token wakeup, high-contention duplicate suppression, multiple producers, and concurrent Consumers draining shared state exactly once;
- ThreadSafeQueue direct and endpoint FIFO behavior, move-only values, endpoint lifetime, blocking wakeup, empty stop-token behavior, queued-value behavior with an already-requested stop, multiple producers, and multiple Consumers removing every value exactly once;
- Task initial Pending state, explicit validate/fail settlement, invalid result/failure access, shared Answer observation, move-only results, successful/failing completion, repeated-settlement rejection, null-failure rejection, completion-subscription races, and Failed/Completed access invariants;
- WorkerPool invalid/explicit/default worker counts, direct callable submission, move-only callable captures/results, single-worker FIFO execution, real multi-worker concurrency, high-contention exactly-once execution, failure isolation, heterogeneous result types, queued-work draining during destruction, and Answer lifetime after pool destruction;
- TaskGroup aggregation of manually settled Task Answers, WorkerPool-produced Task Answers, mixed sources, concurrent completion, failure aggregation, and passive no-wait behavior;
- Singleton compile-time construction/overload constraints, uninstantiated failure, movable and move-only value instantiation, non-movable pointer instantiation, stable mutable reference behavior, re-instantiation replacement, owned-pointer destruction, null rejection, and preservation of an existing instance after rejected null input.

Concurrency tests must prefer deterministic coordination and count/set invariants over arbitrary sleep-based timing.
