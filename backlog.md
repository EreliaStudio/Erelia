# Erelia — Battle Backend Backlog

This file tracks what remains to complete the battle backend.
The goal is a backend where every player-facing interaction is exposed as a clean phase API,
and controllers can be written without containing any game logic.

## What is done

- `BattleContext` owns battle-wide runtime state (units, board, placement style, stats).
- `TurnContext` owns per-turn runtime state (active unit, pending action, resolved count).
- All `BattleAttributes` are observable (health, AP, MP, turn bar, combat stats).
- `BattleUnit` tracks board position, statuses, defeat state, and turn readiness.
- `BattleStatuses` is a fully observable collection with hook-point-aware iteration.
- The full phase pipeline exists: Setup → Placement → Idle → PlayerTurn / EnemyTurn → Resolution → End.
- `SetupPhase` initializes turn bars with randomized starting positions before entering Placement.
- `IdlePhase` calls `TryBeginNextTurn` on Enter, which advances turn bars and selects the next unit.
- `PlacementPhase` exposes a complete placement API for controllers.
- `PlayerTurnPhase` exposes a complete action submission API including `TryGetPathTo`.
- `EnemyTurnPhase` prefers abilities targeting an enemy unit, then moves toward the nearest enemy.
- `ResolutionPhase` resolves the pending action and drives turn/battle flow.
- `EndPhase` computes `BattleOutcome` via `BattleOutcomeRules` and emits it with `EventCenter.EmitBattleEnded`.
- `BattleActionValidator` validates movement (BFS reachability + path reconstruction), ability legality, and end-turn.
- `BattleActionResolver` resolves Move (with BeforeMove/AfterMove hooks), Ability (with defeat handling and stat tracking), and EndTurn.
- Defeated units are removed from the board via `BattleContext.DefeatUnit` immediately after ability resolution.
- `BattleStats` tracks per-unit moves made, abilities cast, damage dealt, and healing done.
- `BattleOutcome` carries winner, surviving units per side, and `BattleStats`.
- `BattleOutcomeRules` computes the outcome from `BattleContext` state.
- `BattleTargetingRules` expands AoE cells (Square, Cross, Circle shapes).
- `BattleLineOfSightRules` implements 3D DDA raycasting.
- `BattleTurnRules` implements stamina-based initiative with deterministic tiebreaking and board object duration advancement.
- `BattleStatusRules` applies status hooks at 9 hook points including `BeforeMove` and `AfterMove`.
- Hook execution semantics are documented in `BattleStatusRules.cs`.
- `BattlePlacementRules` builds HalfBoard zones, validates cells, and auto-places enemies.
- Interactive object durations are decremented each turn end; expired objects are removed from the board.
- `PlacementStyle` is decoupled from `BoardData` and lives on `BattleContext`.
- 17 effect types exist, all using `BattleAbilityExecutionContext`.
- `EventCenter.BattleEnded` carries a `BattleOutcome` payload.

---

## Remaining Work

### Phase controllers (shell only)

All seven phase controllers (`SetupPhaseController`, `PlacementPhaseController`, `IdlePhaseController`,
`PlayerTurnPhaseController`, `EnemyTurnPhaseController`, `ResolutionPhaseController`, `EndPhaseController`)
are empty stubs. These are the controller/view layer and are intentionally next.

### Battle HUD / view layer

Still missing:
- HP / AP / MP display updates
- Active unit display
- Ability list binding
- Target and area previews (driven by `GetAffectedCells`, `GetValidTargetCells`, `TryGetPathTo`)
- Damage and healing popups
- Status icons
- Turn order / initiative display

### Board overlays

Still missing:
- Reachable-cell overlay
- Target-cell overlay
- AoE preview
- LoS preview
- Placement-zone preview

All of the above should be driven exclusively by phase API calls — no rules logic in controllers.

---

## Completion Criteria

The backend is done. Controllers can now be written using only phase API methods:

- `PlacementPhase`: `GetPlayerPlacementCells`, `GetValidPlacementCells`, `TryPlaceUnit`, `CanCompletePlacement`, `TryCompletePlacement`
- `PlayerTurnPhase`: `GetReachableCells`, `TryGetPathTo`, `GetValidTargetCells`, `GetAffectedCells`, `GetAffectedObjects`, `TrySubmitMove`, `TrySubmitAbility`, `TrySubmitEndTurn`
- `EnemyTurnPhase`: automatic (no controller API needed)
- `ResolutionPhase`: lock inputs, observe `BattleContext.UnitDefeated` for death feedback
- `EndPhase`: read `BattleOutcome` from `EventCenter.BattleEnded` event

No game logic lives in any `MonoBehaviour` other than input forwarding and event subscriptions.
