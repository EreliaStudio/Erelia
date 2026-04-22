# Erelia - Battle Backend Backlog

This backlog reflects a fresh review of the current battle backend and EditMode tests.
It is intentionally split into:

- confirmed backend strengths
- logic issues found during review
- test coverage gaps
- controller / view work that still depends on the backend

The goal remains the same: thin controllers, explicit legality APIs, and battle flow owned by logic.

## Confirmed Working

- `BattleContext` owns runtime battle state, unit collections, board access, placement style, and turn context.
- `TurnContext` cleanly models active unit plus one pending action at a time.
- The phase pipeline is present and wired through `BattleOrchestrator`:
  `Setup -> Placement -> Idle -> PlayerTurn / EnemyTurn -> Resolution -> End`
- `PlayerTurnPhase` already exposes thin-controller APIs:
  `CanMoveTo`, `GetReachableCells`, `TryGetPathTo`, `CanUseAbility`, `GetCastLegality`,
  `GetValidTargets`, `GetValidTargetCells`, `GetAffectedCells`, `GetAffectedObjects`,
  `CanTarget`, `CanTargetCell`, `CanCastAtCell`, `CanEndTurn`,
  `TrySubmitMove`, `TrySubmitAbility`, `TrySubmitEndTurn`
- `EnemyTurnPhase` already chooses a legal action automatically.
- `BattleActionValidator` owns movement legality, pathfinding, cast legality, target legality, and end-turn legality.
- `BattleActionResolver` already resolves move / ability / end-turn actions and updates stats.
- `BattleTargetingRules` separates:
  `CanCastAtCell` via validator
  `GetAffectedCells`
  `GetAffectedObjects`
- `BattleLineOfSightRules` exists and uses a 3D traversal.
- `BattleTurnRules` already owns readiness progression, turn begin, turn end, and deterministic tie-breaking.
- `BattleStatusRules` already owns hook dispatch for battle logic.
- `BattlePlacementRules` already computes zones and supports enemy auto-placement.
- `EndPhase` emits a `BattleOutcome` through `EventCenter.BattleEnded`.
- The EditMode suite already covers many happy-path phase APIs and a full-combat victory flow.
- `LineOfSightTestFixture` uses Walkable voxels at standing level (y=1) and Obstacle walls via `WithWall`; nav nodes are injected at y=1 so unit placement and ability LoS tests work correctly.
- All targeting and legality cases are covered, including LoS blocking, AoE shapes, and all four target profiles.
- `NavGraphTests` covers `VoxelTraversalGraphBuilder`: node creation, wall blocking, Walkable voxel transparency, neighbour connections, symmetry, manual `CreateNode` injection, and `BoardNavigationLayer` integration.
- `ResolutionPhase` checks the resolver return value and reverts to the active turn phase on failure.
- `BattleStatusRules.ApplyHook()` uses an explicit `anchorSet` bool so a caster at `(0,0,0)` is handled correctly.
- Recent TU cleanup aligned tests with current backend semantics:
  defeated units free their board cell without requiring battle end, lower `Recovery` means faster turn-bar readiness, raised obstacle voxels can be standable elevated floors when there is clearance above, and line / diagonal targeting tests now use deterministic target placement.

## Current Verification Status

- `dotnet build .\Erelia.Tests.EditMode.csproj` passes with 0 errors.
- The latest inspected Unity XML reported 177 passed, 1 failed, and 2 inconclusive before the final TU cleanup.
- The remaining failure and inconclusive tests were patched in:
  `BackendBehaviorTests.DefeatUnit_DefeatedUnitNoLongerBlocksMovement`,
  `TargetingAndLegalityTests.LineRange_ValidatesAxisAlignedCell`,
  `TargetingAndLegalityTests.DiagonalRange_RejectsAxisAlignedCell`
- Next verification step: rerun Unity EditMode tests and confirm the XML has 0 failed / 0 inconclusive tests.

## Test Coverage Gaps

The EditMode suite now covers the main backend rules, but coverage is still light around integration-style edge cases.

- Add tests for battle-end stats immutability once `BattleOutcome` snapshots are implemented.
- Add tests for enemy placement strategy once auto-placement grows beyond random valid-cell selection.
- Add controller/view tests only after runtime controllers start binding to the backend APIs.

## Backend Refinements Still Worth Doing

These are not immediate bugs, but they are still good backend work before heavy controller implementation.

### Snapshot `BattleOutcome`

Priority: low

Problem:
- `BattleOutcome` currently holds the live `BattleStats` reference.

Risk:
- If anything mutates the same stats object after battle end, listeners are not looking at an immutable snapshot.

Possible fix:
- Copy stats into an immutable outcome payload.

### Enemy auto-placement should be strategy-ready

Priority: low

Problem:
- `TryAutoPlaceUnitsRandomly()` currently assumes one generic placement probe and random distribution only.

Future need:
- AI placement preferences such as front row / back row.

Possible fix:
- Introduce a placement strategy helper that scores valid cells per enemy unit.

## Controller / View Backlog

These remain intentionally after backend stabilization.

### Phase controllers

Still shell-only by design:

- `SetupPhaseController`
- `PlacementPhaseController`
- `IdlePhaseController`
- `PlayerTurnPhaseController`
- `EnemyTurnPhaseController`
- `ResolutionPhaseController`
- `EndPhaseController`

### HUD / overlays

Still missing:

- HP / AP / MP live binding
- active unit panel binding
- ability list binding
- turn order display
- reachable-cell overlay
- valid-target-cell overlay
- AoE preview overlay
- line-of-sight preview overlay
- placement-zone overlay
- damage / healing / status feedback visuals

These should continue to consume backend APIs only, with no legality or battle rules in controllers.

## Recommended Order

1. Rerun Unity EditMode tests and archive the clean XML result.
2. Address the low-priority backend refinements only if they block controller work.
3. Start implementing controllers and visual bindings on top of the stabilized APIs.
