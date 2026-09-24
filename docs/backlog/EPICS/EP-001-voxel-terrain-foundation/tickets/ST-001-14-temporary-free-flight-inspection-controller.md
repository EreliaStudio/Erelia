# ST-001-14 — Temporary free-flight inspection controller

**Status:** Draft
**Epic:** EP-001
**Production target(s):** Client
**Test suite(s):** EreliaClientTestSuite; manual Client inspection

## Intent

Provide the temporary non-production 3D inspection controls needed to move through the EP-001 terrain rendering scene.

## User / system value

A developer can inspect terrain from above, below, and across Chunk boundaries before production Hero movement exists.

## Starting state / prerequisites

- Depends on ST-001-13 for a visible terrain scene.
- DR-009 identifies this as temporary validation tooling.
- IMPLEMENTATION-CONTEXT fixes ZQSD as the keyboard convention.
- Exact translation speed, vertical movement keys, camera-look input, pitch/yaw limits, time-step behavior, focus/cursor capture, and startup pose are not yet specified.

## Product ownership

Client owns temporary inspection input/camera state.

## Allowed dependencies

EreliaClientLibrary, Sparkle input/camera/presentation facilities, standard library.

## Forbidden dependencies

Server authority, production Hero locomotion, collision/gravity, movement prediction/reconciliation, follower/combat movement logic.

## Owned behavior

The eventual ticket should own free 3D translation and camera orientation sufficient to inspect the terrain scene, using ZQSD for horizontal keyboard movement.

## Explicitly not owned

Production exploration controls, character physics/collision, Server movement commands, gameplay camera behavior, combat targeting.

## Public contract

Draft until exact input mapping and numeric movement/camera behavior are approved.

## Invariants

- Controller is explicitly non-authoritative validation tooling.
- ZQSD is used for horizontal movement; WASD must not silently replace it.
- Movement is free-flight and is not constrained by terrain collision/gravity.

## State transitions

Input sampled -> inspection pose changes. Exact per-frame/time-step semantics are Draft.

## Failure behavior

Focus loss/cursor capture/window input failure behavior is not yet specified.

## Determinism / ordering

Interactive input itself need not be deterministic across real-time runs, but testable synthetic input should produce exact approved pose deltas.

## Lifecycle / ownership

Client scene/camera owns inspection controller lifetime. Input subscriptions must be removed safely on teardown.

## Serialization / persistence

Not applicable.

## Networking / authority

Not networked. Inspection pose does not become Server/player authoritative state.

## Implementation constraints

- Temporary validation controller only.
- ZQSD horizontal convention.
- No collision, gravity, Hero entity, prediction, or reconciliation.

## Exact test fixtures

Not yet fixed. Ready fixtures must define startup position/orientation, Z/Q/S/D directions, vertical bindings, movement speed/modifiers, camera-look input/sensitivity, pitch/yaw behavior, delta-time semantics, and focus/cursor behavior required for tests.

## Acceptance tests

### Nominal

Synthetic exact input sequences produce exact approved pose changes.

### Boundaries

Pitch/yaw or speed modifier boundaries if the approved contract includes them.

### Invalid / rejected operations

Unsupported/unfocused input behavior according to final contract.

### Failure atomicity

Not applicable for pure pose updates unless input/camera subsystem can fail.

### Determinism

Synthetic input + fixed delta fixture produces exact pose output.

### Lifecycle / ownership

Input subscription/cursor capture is released on teardown.

### Serialization / persistence

Not applicable.

### Retry / idempotency

No input produces no pose change.

### Concurrency / cancellation

Focus/capture cancellation if applicable.

### Authority / trust boundary

Inspection movement never sends authoritative movement state to Server.

### Dependency failure

Input/window unavailability according to final contract.

### Cross-system integration

Manual test can traverse and inspect the ST-001-13 terrain scene.

### Performance

No numeric budget.

### Client-visible / golden-image validation

Not owned; controller supports manual viewpoints for ST-001-16.

## Decisions / unresolved questions

- [DR-009](../../../DECISIONS/DR-009-FIRST-VOXEL-TERRAIN-MILESTONE.md)
- Project implementation convention: ZQSD.

Specification still needed before Ready: full input map, numeric movement/look semantics, startup pose, and focus/cursor behavior.

## Completion evidence

Automated synthetic-input tests plus manual evidence show the developer can freely inspect above/below/across rendered Chunk boundaries without introducing production movement systems.
