# Battle.Phases README

## Purpose
Phases contains the concrete battle flow phases. Each phase inherits from `BattlePhase`
and is invoked by `BattleManager` through the registry.

## Contents
- `InitializePhase`: prepares battle data and placement centers.
- `PlacementPhase`: handles unit placement and placement masks.
- `PlayerTurnPhase`: runs player turn logic.
- `EnemyTurnPhase`: runs enemy turn logic.
- `ResolveActionPhase`: resolves queued actions or effects.
- `VictoryPhase`: handles end-of-battle victory flow.
- `DefeatPhase`: handles end-of-battle defeat flow.
- `CleanupPhase`: resets state and exits the battle scene.

## Adding Or Extending
1. Implement a new phase that derives from `BattlePhase`.
2. Add its id to `BattlePhaseId` and register it in `BattlePhaseRegistry`.
3. Request transitions from existing phases when appropriate.
