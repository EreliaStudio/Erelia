# Feat Requirement List

All entries follow the same pattern:
- **Event emitted by**: where in the code the event is produced
- **Accumulation**: `Additive` (progress sums across events) or `Maximum` (progress takes the best single event)
- **Status**: `Implemented` / `Planned`

---

## Damage Dealing

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `DealDamageRequirement` | Deal a total of X damage across the whole battle | `DamageTargetEffect` | Additive | Implemented |
| `MaxSingleHitDamageRequirement` | Deal at least X damage in a single hit | `DamageTargetEffect` | Maximum | Implemented |
| `DealDamageInOneTurnRequirement` | Deal at least X total damage within a single turn | Turn start/end pair | Additive per turn, reset each turn | Planned |
| `KillCountRequirement` | Defeat at least X enemies during the battle | `BattleContext.DefeatUnit` | Additive | Planned |
| `KillWithOneHitRequirement` | Kill an enemy that was at full HP with a single hit | `DamageTargetEffect` | Maximum | Planned |
| `OverkillRequirement` | Deal at least X excess damage beyond the target's remaining HP in one hit | `DamageTargetEffect` | Maximum | Planned |

---

## Damage Taking

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `TakeDamageRequirement` | Take a total of X damage across the whole battle | `DamageTargetEffect` | Additive | Implemented |
| `MaxSingleHitTakenRequirement` | Take at least X damage in a single hit | `DamageTargetEffect` | Maximum | Planned |
| `SurviveHitRequirement` | Survive a hit that would have dealt at least X damage | `DamageTargetEffect` | Maximum | Planned |
| `TakeDamageAndSurviveRequirement` | Take at least X total damage but finish the battle alive | End phase | Maximum | Planned |

---

## Healing

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `HealHealthRequirement` | Heal a total of X HP across the whole battle | `HealTargetEffect` | Additive | Implemented |
| `MaxSingleHealRequirement` | Restore at least X HP in a single heal | `HealTargetEffect` | Maximum | Planned |
| `HealFromBelowThresholdRequirement` | Heal an ally that was below X% of their max HP | `HealTargetEffect` | Maximum | Planned |
| `FullyHealAllyRequirement` | Bring an ally from less than X HP back to full HP | `HealTargetEffect` | Maximum | Planned |

---

## Positioning — Turn Start

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `TurnStartPositionRequirement` (Within / Enemy) | Start a turn within X cells of an enemy | `BattleTurnRules.BeginTurn` | Maximum | Implemented |
| `TurnStartPositionRequirement` (AtLeast / Enemy) | Start a turn at least X cells from every enemy | `BattleTurnRules.BeginTurn` | Maximum | Implemented |
| `TurnStartPositionRequirement` (Within / Ally) | Start a turn within X cells of an ally | `BattleTurnRules.BeginTurn` | Maximum | Implemented |
| `TurnStartPositionRequirement` (AtLeast / Ally) | Start a turn at least X cells from every ally | `BattleTurnRules.BeginTurn` | Maximum | Implemented |
| `TurnStartPositionRequirement` (Within / AnyUnit) | Start a turn within X cells of any other unit | `BattleTurnRules.BeginTurn` | Maximum | Implemented |
| `TurnStartPositionRequirement` (AtLeast / AnyUnit) | Start a turn at least X cells from every other unit | `BattleTurnRules.BeginTurn` | Maximum | Implemented |

---

## Positioning — Turn End

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `TurnEndPositionRequirement` (Within / Enemy) | End a turn within X cells of an enemy | `BattleTurnRules.EndTurn` | Maximum | Implemented |
| `TurnEndPositionRequirement` (AtLeast / Enemy) | End a turn at least X cells from every enemy | `BattleTurnRules.EndTurn` | Maximum | Implemented |
| `TurnEndPositionRequirement` (Within / Ally) | End a turn within X cells of an ally | `BattleTurnRules.EndTurn` | Maximum | Implemented |
| `TurnEndPositionRequirement` (AtLeast / Ally) | End a turn at least X cells from every ally | `BattleTurnRules.EndTurn` | Maximum | Implemented |
| `TurnEndPositionRequirement` (Within / AnyUnit) | End a turn within X cells of any other unit | `BattleTurnRules.EndTurn` | Maximum | Implemented |
| `TurnEndPositionRequirement` (AtLeast / AnyUnit) | End a turn at least X cells from every other unit | `BattleTurnRules.EndTurn` | Maximum | Implemented |

---

## Movement

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `TotalDistanceTravelledRequirement` | Travel a cumulative total of X cells during the battle | `BattleActionResolver.ResolveMove` | Additive | Planned |
| `MaxDistanceInOneTurnRequirement` | Travel at least X cells in a single turn | `BattleActionResolver.ResolveMove` | Maximum | Planned |
| `MoveTowardEnemyRequirement` | End a move closer to the nearest enemy than you started | `BattleActionResolver.ResolveMove` | Maximum | Planned |
| `MoveAwayFromEnemyRequirement` | End a move farther from the nearest enemy than you started | `BattleActionResolver.ResolveMove` | Maximum | Planned |
| `SpendAllMovementPointsRequirement` | Use all movement points in a single turn | `BattleTurnRules.EndTurn` | Maximum | Planned |

---

## Resource Usage (Action Points / Movement Points)

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `SpendAllActionPointsRequirement` | Use all action points in a single turn | `BattleTurnRules.EndTurn` | Maximum | Planned |
| `EndTurnWithFullMovementRequirement` | End a turn without spending any movement points | `BattleTurnRules.EndTurn` | Maximum | Planned |
| `EndTurnWithNoResourcesRequirement` | End a turn with 0 AP and 0 MP remaining | `BattleTurnRules.EndTurn` | Maximum | Planned |
| `CastAbilityCountRequirement` | Cast a total of X abilities during the battle | `BattleActionResolver.ResolveAbility` | Additive | Planned |
| `CastMultipleAbilitiesInOneTurnRequirement` | Cast at least X abilities in a single turn | `BattleTurnRules.EndTurn` | Maximum | Planned |

---

## Status Effects

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `ApplyStatusCountRequirement` | Apply a status effect X times (any status) | `ApplyStatusEffect` | Additive | Planned |
| `ApplySpecificStatusCountRequirement` | Apply a specific named status X times | `ApplyStatusEffect` | Additive | Planned |
| `HaveStatusAtTurnStartRequirement` | Begin a turn while affected by at least one status | `BattleTurnRules.BeginTurn` | Maximum | Planned |
| `HaveNoStatusAtTurnStartRequirement` | Begin a turn with no active status effects | `BattleTurnRules.BeginTurn` | Maximum | Planned |
| `CleansedStatusRequirement` | Remove at least X stacks of any status in one action | `CleanseEffect` / `RemoveStatusEffect` | Maximum | Planned |

---

## HP Thresholds (Self)

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `StartTurnBelowHPThresholdRequirement` | Begin a turn with HP below X% of max HP | `BattleTurnRules.BeginTurn` | Maximum | Planned |
| `StartTurnAboveHPThresholdRequirement` | Begin a turn with HP above X% of max HP | `BattleTurnRules.BeginTurn` | Maximum | Planned |
| `EndTurnBelowHPThresholdRequirement` | End a turn with HP below X% of max HP | `BattleTurnRules.EndTurn` | Maximum | Planned |
| `EndTurnAboveHPThresholdRequirement` | End a turn with HP above X% of max HP | `BattleTurnRules.EndTurn` | Maximum | Planned |
| `ReachCriticalHPRequirement` | Reach X HP or fewer at any point and survive the battle | `DamageTargetEffect` + end phase | Maximum | Planned |

---

## Turn Bar / Stamina

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `TakeTurnCountRequirement` | Take at least X turns during the battle | `BattleTurnRules.BeginTurn` | Additive | Planned |
| `ActBeforeAnyEnemyRequirement` | Be the first unit to take a turn in the battle | `BattleTurnRules.BeginTurn` | Maximum | Planned |
| `StealTurnBarTimeRequirement` | Steal a cumulative total of X turn-bar time from enemies | `StealResourceEffect` | Additive | Planned |
| `AdjustEnemyTurnBarRequirement` | Delay or advance an enemy's turn bar by at least X total | `AdjustTurnBarTimeEffect` / `AdjustTurnBarDurationEffect` | Additive | Planned |

---

## Displacement / Forced Movement

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `PushEnemyRequirement` | Force-move an enemy at least X times | `MoveStatus` | Additive | Planned |
| `MaxForceMoveDistanceRequirement` | Force-move an enemy at least X cells in a single push | `MoveStatus` | Maximum | Planned |
| `SwapPositionRequirement` | Successfully swap positions at least X times | `SwapPositionEffect` | Additive | Planned |
| `TeleportRequirement` | Use a teleport effect at least X times | `TeleportEffect` | Additive | Planned |

---

## Battle Outcome

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `WinBattleWithFullHPRequirement` | Win the battle with 100% of max HP remaining | End phase | Maximum | Planned |
| `WinBattleWithoutTakingDamageRequirement` | Win the battle without taking any damage | End phase | Maximum | Planned |
| `WinBattleWithinXTurnsRequirement` | Win the battle in X turns or fewer | End phase | Maximum | Planned |
| `AllAlliesSurviveRequirement` | Win the battle with no ally defeated | End phase | Maximum | Planned |
| `WinWithLowHPRequirement` | Win the battle with X HP or fewer remaining | End phase | Maximum | Planned |

---

## Interactive Objects

| Name | Description | Event emitted by | Accumulation | Status |
|---|---|---|---|---|
| `PlaceObjectCountRequirement` | Place at least X interactive objects during the battle | `PlaceInteractiveObjectEffect` | Additive | Planned |
| `RemoveObjectCountRequirement` | Remove at least X interactive objects during the battle | `RemoveInteractiveObjectEffect` | Additive | Planned |

---

## Notes

- **Additive** requirements are naturally completed by accumulation over a whole battle (total damage, total heals, kill count…).
- **Maximum** requirements need a single exceptional moment (best hit, closest approach, full-HP win…); multiple small events must not combine to satisfy them.
- Requirements listed as a single row with multiple `TargetKind`/`DistanceKind` variants (e.g. `TurnStartPositionRequirement`) are one class with configurable fields, not separate classes.
- "Planned" entries have no implementation yet; events and requirement classes still need to be created.
