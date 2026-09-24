# DR-020 — Headless asynchronous task infrastructure prototype

**Status:** Resolved
**Date opened:** 2026-09-24
**Date resolved:** 2026-09-24
**Applies to:** Core asynchronous work, Server worker execution, temporary Erelia-local Sparkle prototypes
**Supersedes:** —

## Context

ST-001-06 requires Chunk generation to stop blocking the requesting thread. The same requirement will recur for other engine work, so Chunk-specific threads or direct `std::promise` / `std::future` plumbing would create the wrong ownership boundary.

Sparkle Version-0.1.3 already provides headless-safe synchronization/container primitives such as `spk::ThreadSafeFIFO` and `spk::ProtectedData`, but it does not yet provide the generic task/worker abstractions required here.

The project owner selected a reusable Sparkle-shaped API, implemented temporarily inside Erelia until it is mature enough to propose upstream.

## Decision

### Temporary placement

The following types live in Erelia Core headers but in namespace `spk`, alongside the existing Erelia-local `spk::JSON::Catalog` prototype:

- `spk::ThreadSafeSet<T>`;
- `spk::ThreadSafeQueue<T>`;
- `spk::Task<TResult>`;
- `spk::WorkerPool`;
- `spk::Singleton<T>`.

They must remain headless/Core-compatible. They may depend on the C++ standard library and Sparkle Core, but never on Window, graphics, OpenGL, input, UI, or platform-specific presentation APIs.

Once their contracts have been exercised in Erelia, they may be proposed to Sparkle. Until then, Erelia owns the prototype implementation and tests.

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

A Task owns one callable representing worker-side work and exposes a lightweight shared `Task<TResult>::Answer`.

The only lifecycle states are:

- `Pending`: no result or failure is available yet;
- `Completed`: a fully valid `TResult` exists;
- `Failed`: no valid result exists and an exception is stored.

There is no `Completed + expected-error` state. Domain work that fails does so through the Task failure path.

`Answer::result()` is valid only for `Completed`. `Answer::failure()` is valid only for `Failed`. Invalid access throws `spk::Exception`.

### `spk::WorkerPool`

`WorkerPool` is a headless generic executor.

It owns:

- a pool of standard C++ worker threads;
- a `spk::ThreadSafeQueue<std::unique_ptr<WorkerPool::Job>>` of polymorphic jobs;
- an internal `TaskJob<TResult> : WorkerPool::Job` adapter for typed `spk::Task<TResult>`.

Submitting a Task returns its `Answer`. Workers execute jobs without knowing their result type.

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

Server systems may then submit generic `spk::Task<TResult>` work through:

```cpp
spk::Singleton<spk::WorkerPool>::instance()
```

The WorkerPool remains generic and has no Chunk, Server-authority, rendering, or networking semantics.

## Chunk-provider consequence

`PrototypeChunkProvider` buffers generation requests, submits `spk::Task<Chunk>` values to the shared WorkerPool, retains their Answers, and consumes completed/failed Answers from its update-thread `update()` pass.

The final mutation of `Chunk::Collection` therefore remains on the update thread rather than occurring directly inside WorkerPool threads.

## Consequences

- asynchronous execution becomes reusable outside terrain generation;
- worker threads stay independent of engine presentation APIs;
- Chunk generation does not require direct `std::future` ownership in gameplay systems;
- typed Task results coexist on one polymorphic Job queue;
- WorkerPool does not duplicate mutex/condition-variable/queue synchronization already owned by `ThreadSafeQueue`;
- request producers can deduplicate batches through `ThreadSafeSet`;
- Erelia gains a concrete proving ground before proposing these APIs to Sparkle.

## Required tests

Because these Erelia-local `spk` types are intended as candidates for later direct integration into Sparkle, their test depth is intentionally larger than ordinary ticket scaffolding.

Core tests cover at least:

- ThreadSafeSet direct and endpoint APIs, duplicate insertion reporting, contains/erase, drain/reset behavior, re-request after drain, move-only values, endpoint lifetime, wait/publish wakeup, stop-token wakeup, high-contention duplicate suppression, multiple producers, and concurrent Consumers draining shared state exactly once;
- ThreadSafeQueue direct and endpoint FIFO behavior, move-only values, endpoint lifetime, blocking wakeup, empty stop-token behavior, queued-value behavior with an already-requested stop, multiple producers, and multiple Consumers removing every value exactly once;
- Task initial Pending state, invalid result/failure access, shared Answer observation, move-only callable captures, move-only results, successful completion, spk and standard exception preservation, and Failed/Completed access invariants;
- WorkerPool invalid/explicit/default worker counts, single-worker FIFO execution, real multi-worker concurrency, high-contention exactly-once execution, failure isolation, heterogeneous Task result types, queued-work draining during destruction, and Answer lifetime after pool destruction;
- Singleton compile-time construction/overload constraints, uninstantiated failure, movable and move-only value instantiation, non-movable pointer instantiation, stable mutable reference behavior, re-instantiation replacement, owned-pointer destruction, null rejection, and preservation of an existing instance after rejected null input.

Concurrency tests must prefer deterministic coordination and count/set invariants over arbitrary sleep-based timing.
