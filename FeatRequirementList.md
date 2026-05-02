# Expanded Feat Requirement List

This document expands the current combat feat requirement catalogue with additional requirement ideas, event suggestions, and implementation priorities.

All entries follow the same pattern:

* **Event emitted by**: where in the code the event is produced
* **Scope**:

  * `Ability`: evaluate the requirement inside one ability resolution or hit
  * `Turn`: evaluate the requirement inside one unit turn
  * `Fight`: evaluate the requirement across one battle, then reset if incomplete
  * `Game`: persist progress across multiple fights
* **Status**: `Implemented` / `Planned`
* **Priority**:

  * `Priority`: implement now; core, reusable, easy to test, or already partially implemented
  * `High`: implement soon; valuable, but depends on more context or tracking
  * `Low`: implement later; niche, complex, or mainly useful for special challenges

## Core Design Distinction

A major distinction in the system is the requirement scope window. The requirement class describes the measured behavior once; `FeatRequirement.Scope` decides which time window makes that behavior valid.

```text
Ability scope = progress must fit inside one ability resolution or hit.
Turn scope    = progress must fit inside one unit turn.
Fight scope   = progress must fit inside one battle.
Game scope    = progress persists across multiple fights.
```

Examples:

```text
Cast Fireball 3 times in one battle.
Cast Fireball 100 times across all battles.
```

Both use `CastAbilityCountRequirement`; the first uses `Scope.Fight`, the second uses `Scope.Game`. This avoids creating separate lifetime/fight/turn classes for the same measured behavior.

Events should remain the combat log: what happened, when, from whom, to whom, and with which ability/effect. Scope-specific behavior belongs in requirement parsing, not in separate event emission paths for every scope variant.

`FeatRequirement` also owns the repeat count:

```text
RequiredRepeatCount = how many scope windows must satisfy this requirement.
FeatRequirementProgress.CompletedRepeatCount = how many windows have already satisfied it.
```

Examples:

```text
Deal 100 damage with Scope.Fight and RequiredRepeatCount = 3
= deal at least 100 damage in a fight three separate times.

Deal 50 damage with Scope.Ability and RequiredRepeatCount = 2
= have two ability windows that each dealt at least 50 damage.
```

Children should only answer "how much progress does this typed event contribute?" Base requirement code groups events into ability, turn, fight, or game windows, sorts those windows by progress, and counts repeat completions.

## Requirement Category Value Order

The requirement sections are sorted by implementation value:

1. Core combat behavior and broad progression hooks
2. Common support, status, kill, and battle-outcome hooks
3. Resource and movement hooks
4. More specialized positioning, object, mitigation, and composite hooks

This keeps the first implementation wave focused on requirements that are reusable across many creatures and easy to validate with tests.

## Ability Casting

| Name                                        | Description                                                            | Event emitted by                                      | Status | Priority |
| ------------------------------------------- | ---------------------------------------------------------------------- | ----------------------------------------------------- | ----------- | --- |
| `CastAbilityCountRequirement`               | Cast any or a specific ability X times                                 | `BattleActionResolver.ResolveAbility`                 | Implemented | Priority |
| `CastDifferentAbilitiesRequirement`         | Cast X different abilities                                             | `BattleActionResolver.ResolveAbility`                 | Planned | Priority |
| `CastSameAbilityConsecutivelyRequirement`   | Cast the same ability X turns in a row                                 | `BattleActionResolver.ResolveAbility` + turn tracking | Planned | High |
| `CastAbilityWithoutMovingRequirement`       | Cast an ability during a turn where the unit did not move              | `BattleTurnRules.EndTurn`                             | Planned | High |
| `CastAbilityAfterMovingRequirement`         | Cast an ability after moving in the same turn                          | `BattleActionResolver.ResolveAbility`                 | Planned | High |
| `CastAbilityBeforeMovingRequirement`        | Cast an ability before moving in the same turn                         | `BattleActionResolver.ResolveMove`                    | Planned | High |
| `CastAbilityAtMaxRangeRequirement`          | Hit or cast an ability at its maximum possible range                   | `BattleActionResolver.ResolveAbility`                 | Planned | High |
| `CastAbilityAtMinimumRangeRequirement`      | Cast an ability adjacent to the target or at its minimum allowed range | `BattleActionResolver.ResolveAbility`                 | Planned | High |
| `CastAbilityOnFirstTurnRequirement`         | Cast an ability on this unit's first turn                              | `BattleActionResolver.ResolveAbility`                 | Planned | High |
| `CastAbilityEveryTurnRequirement`           | Cast at least one ability on every turn this unit takes                | End phase                                             | Planned | Low |
| `WinAfterCastingSpecificAbilityRequirement` | Win a battle after casting a specific ability at least once            | End phase                                             | Planned | Priority |
| `CastAbilityWithNoAPRemainingRequirement`   | Cast an ability that leaves the unit with 0 AP                         | `BattleActionResolver.ResolveAbility`                 | Planned | High |
| `CastAbilityWhileLowHPRequirement`          | Cast an ability while below X% HP                                      | `BattleActionResolver.ResolveAbility`                 | Planned | High |
| `CastAbilityWhileStatusedRequirement`       | Cast an ability while affected by a specific status/tag                | `BattleActionResolver.ResolveAbility`                 | Planned | High |
| `CastAbilityFromSpecificFormRequirement`    | Cast an ability while in a specific form                               | `BattleActionResolver.ResolveAbility`                 | Planned | High |

## Damage Dealing

| Name                                          | Description                                              | Event emitted by                             | Status | Priority |
| --------------------------------------------- | -------------------------------------------------------- | -------------------------------------------- | ----------- | --- |
| `DealDamageRequirement`                       | Deal X damage                                            | `DamageTargetEffect`                         | Implemented | Priority |
| `DealDamageWithSpecificAbilityRequirement`    | Deal X damage using a specific ability                   | `DamageTargetEffect`                         | Planned | Priority |
| `DealDamageWithSpecificDamageKindRequirement` | Deal X physical/magical/etc. damage                      | `DamageTargetEffect`                         | Planned | Priority |
| `DealDamageToStatusedTargetRequirement`       | Deal X damage to targets with a specific status/tag      | `DamageTargetEffect`                         | Planned | High |
| `DealDamageToUnstatusedTargetRequirement`     | Deal X damage to targets with no statuses                | `DamageTargetEffect`                         | Planned | High |
| `DealDamageWhileStatusedRequirement`          | Deal X damage while the source has a specific status/tag | `DamageTargetEffect`                         | Planned | High |
| `DealDamageWhileLowHPRequirement`             | Deal X damage while below a HP threshold                 | `DamageTargetEffect`                         | Planned | High |
| `DealDamageAtFullHPRequirement`               | Deal X damage while at full HP                           | `DamageTargetEffect`                         | Planned | High |
| `DealDamageFromRangeRequirement`              | Deal damage from at least X cells away                   | `DamageTargetEffect` + cast context          | Planned | High |
| `DealDamageAdjacentRequirement`               | Deal damage while adjacent to the target                 | `DamageTargetEffect` + cast context          | Planned | High |
| `DealDamageWithoutTakingDamageRequirement`    | Deal X damage before taking any damage                   | `DamageTargetEffect` + damage taken tracking | Planned | Low |
| `DealDamageAfterTakingDamageRequirement`      | Deal X damage after being damaged this turn/battle       | `DamageTargetEffect`                         | Planned | High |
| `DamageEveryEnemyRequirement`                 | Damage every enemy at least once                         | End phase                                    | Planned | Low |
| `DamageSameEnemyMultipleTimesRequirement`     | Damage the same enemy X times                            | `DamageTargetEffect`                         | Planned | High |
| `DamageDifferentEnemiesRequirement`           | Damage X different enemies                               | `DamageTargetEffect`                         | Planned | High |
| `NoDamageTakenWhileDealingDamageRequirement`  | Win after dealing X damage and taking 0 damage           | End phase                                    | Planned | Low |

## Damage Taking

| Name                                       | Description                                         | Event emitted by     | Status | Priority |
| ------------------------------------------ | --------------------------------------------------- | -------------------- | ----------- | --- |
| `TakeDamageRequirement`                    | Take X total damage                                 | `DamageTargetEffect` | Implemented | Priority |
| `SurviveHitRequirement`                    | Survive a hit that dealt at least X damage          | `DamageTargetEffect` | Planned | Priority |
| `TakeDamageAndSurviveRequirement`          | Take X total damage and finish battle alive         | End phase            | Planned | High |
| `TakeDamageFromSpecificKindRequirement`    | Take X physical/magical/etc. damage                 | `DamageTargetEffect` | Planned | High |
| `TakeDamageFromSpecificAbilityRequirement` | Take X damage from a specific ability               | `DamageTargetEffect` | Planned | High |
| `TakeDamageWhileShieldedRequirement`       | Take damage while having at least one active shield | `DamageTargetEffect` | Planned | High |
| `TakeDamageWhileStatusedRequirement`       | Take damage while affected by a specific status/tag | `DamageTargetEffect` | Planned | High |
| `TakeDamageWhileLowHPRequirement`          | Take damage while already below X% HP               | `DamageTargetEffect` | Planned | High |
| `SurviveAtOneHPRequirement`                | Survive damage and remain at exactly 1 HP           | `DamageTargetEffect` | Planned | Low |
| `LoseHalfHPInOneHitRequirement`            | Lose at least X% max HP in one hit                  | `DamageTargetEffect` | Planned | High |
| `TakeNoDamageForTurnsRequirement`          | Avoid damage for X own turns                        | Turn tracking        | Planned | Low |
| `TakeNoDamageRequirement`                  | Finish battle without taking damage                 | End phase            | Planned | High |
| `TakeDamageEveryTurnRequirement`           | Take damage on every turn this creature acts        | End phase            | Planned | Low |
| `TankDamageForAlliesRequirement`           | Take X damage while allies remain alive             | `DamageTargetEffect` | Planned | Low |

## Healing

| Name                                | Description                                                  | Event emitted by                | Status | Priority |
| ----------------------------------- | ------------------------------------------------------------ | ------------------------------- | ----------- | --- |
| `HealHealthRequirement`             | Heal X total HP                                              | `HealTargetEffect`              | Implemented | Priority |
| `HealSpecificAllyRequirement`       | Heal a specific ally X total HP                              | `HealTargetEffect`              | Planned | High |
| `HealSelfRequirement`               | Heal self X total HP                                         | `HealTargetEffect`              | Planned | Priority |
| `HealOtherAllyRequirement`          | Heal allies other than self X total HP                       | `HealTargetEffect`              | Planned | Priority |
| `HealFromBelowThresholdRequirement` | Heal a unit that was below X% HP                             | `HealTargetEffect`              | Planned | High |
| `FullyHealAllyRequirement`          | Bring an ally from below X HP/% back to full HP              | `HealTargetEffect`              | Planned | High |
| `OverhealRequirement`               | Attempt to heal X amount beyond missing HP                   | `HealTargetEffect`              | Planned | Low |
| `HealAfterTakingDamageRequirement`  | Heal after the target took damage this turn                  | `HealTargetEffect` + turn state | Planned | High |
| `HealBeforeTakingDamageRequirement` | Heal a target before they take damage later in the same turn | Turn summary                    | Planned | Low |
| `HealStatusedAllyRequirement`       | Heal an ally affected by a specific status/tag               | `HealTargetEffect`              | Planned | High |
| `HealUnstatusedAllyRequirement`     | Heal an ally with no statuses                                | `HealTargetEffect`              | Planned | Low |
| `HealAllAlliesRequirement`          | Heal every ally at least once during battle                  | End phase                       | Planned | Low |
| `WinAfterHealingRequirement`        | Win after healing at least X HP                              | End phase                       | Planned | Priority |
| `HealWithoutDamagingRequirement`    | Heal X HP in a battle where this creature deals no damage    | End phase                       | Planned | High |

## Shields

| Name                                       | Description                                                | Event emitted by                              | Status | Priority |
| ------------------------------------------ | ---------------------------------------------------------- | --------------------------------------------- | --------------------- | --- |
| `ApplyShieldRequirement`                   | Apply X total shield amount                                | `ApplyShieldEffect`                           | Implemented | Priority |
| `ApplySpecificShieldRequirement`           | Apply X shield amount of a specific kind                   | `ApplyShieldEffect`                           | Implemented / Planned | Priority |
| `ApplyShieldCountRequirement`              | Apply shield effects X times                               | `ApplyShieldEffect`                           | Planned | Priority |
| `ApplyShieldToAllyRequirement`             | Apply X shield amount to allies                            | `ApplyShieldEffect`                           | Planned | High |
| `ApplyShieldToSelfRequirement`             | Apply X shield amount to self                              | `ApplyShieldEffect`                           | Planned | High |
| `ApplyShieldWhileLowHPRequirement`         | Apply shield while below X% HP                             | `ApplyShieldEffect`                           | Planned | High |
| `AbsorbDamageWithShieldRequirement`        | Absorb X total damage using shields                        | Shield absorption in `BattleAttributes`       | Implemented / Planned | Priority |
| `ShieldBrokenRequirement`                  | Have X shields broken                                      | Shield absorption                             | Implemented / Planned | High |
| `BreakEnemyShieldRequirement`              | Break X enemy shields                                      | Shield absorption + damage source attribution | Planned | Low |
| `EndTurnWithShieldRequirement`             | End a turn with at least X shield amount active            | `BattleTurnRules.EndTurn`                     | Planned | High |
| `WinWithShieldRemainingRequirement`        | Win while still having shield active                       | End phase                                     | Planned | High |
| `PreventLethalDamageWithShieldRequirement` | Shield absorbs damage that would otherwise defeat the unit | Shield absorption + HP check                  | Planned | Low |
| `MaintainShieldForTurnsRequirement`        | Keep any shield active for X turns                         | Turn tracking                                 | Planned | Low |

## Status Effects

| Name                                      | Description                                      | Event emitted by                       | Status | Priority |
| ----------------------------------------- | ------------------------------------------------ | -------------------------------------- | ------- | --- |
| `ApplyStatusCountRequirement`             | Apply any status X times                         | `ApplyStatusEffect`                    | Planned | Priority |
| `ApplySpecificStatusCountRequirement`     | Apply a specific status X times                  | `ApplyStatusEffect`                    | Planned | Priority |
| `ApplyStatusWithTagRequirement`           | Apply statuses with a specific tag X times       | `ApplyStatusEffect`                    | Planned | High |
| `ApplyMultipleStatusesOneTurnRequirement` | Apply X statuses in one turn                     | Turn summary                           | Planned | Low |
| `ApplyMultipleStatusesOneCastRequirement` | Apply X statuses with one ability resolution     | Ability resolution summary             | Planned | High |
| `ApplyStatusToMultipleEnemiesRequirement` | Apply a status to X enemies in one cast          | `ApplyStatusEffect`                    | Planned | High |
| `ConsumeStatusRequirement`                | Consume X total stacks of statuses               | `ConsumeStatus`                        | Planned | High |
| `ConsumeSpecificStatusRequirement`        | Consume X stacks of a specific status            | `ConsumeStatus`                        | Planned | High |
| `RemoveStatusStackRequirement`            | Remove X total status stacks                     | `RemoveStatusEffect` / `CleanseEffect` | Planned | High |
| `CleanseStatusRequirement`                | Cleanse X statuses or stacks                     | `CleanseEffect`                        | Planned | High |
| `CleanseAllyRequirement`                  | Cleanse statuses from allies X times             | `CleanseEffect`                        | Planned | High |
| `CleanseSelfRequirement`                  | Cleanse self X times                             | `CleanseEffect`                        | Planned | High |
| `HaveStatusAtTurnStartRequirement`        | Start turn with a specific status/tag            | `BattleTurnRules.BeginTurn`            | Planned | High |
| `HaveNoStatusAtTurnStartRequirement`      | Start turn with no active statuses               | `BattleTurnRules.BeginTurn`            | Planned | Low |
| `EndTurnWithStatusRequirement`            | End turn with a specific status/tag              | `BattleTurnRules.EndTurn`              | Planned | High |
| `EndTurnWithNoStatusRequirement`          | End turn with no active statuses                 | `BattleTurnRules.EndTurn`              | Planned | Low |
| `MaintainStatusForTurnsRequirement`       | Keep a status active for X turns                 | Turn tracking                          | Planned | Low |
| `ReachStatusStackRequirement`             | Reach X stacks of a specific status              | `ApplyStatusEffect` + turn check       | Planned | High |
| `WinWithStatusRequirement`                | Win while affected by a specific status/tag      | End phase                              | Planned | High |
| `WinWithoutStatusRequirement`             | Win without ever receiving a specific status/tag | End phase                              | Planned | Low |

## Critical / Finisher / Kill Requirements

| Name                                    | Description                                           | Event emitted by                    | Status | Priority |
| --------------------------------------- | ----------------------------------------------------- | ----------------------------------- | ------- | --- |
| `KillCountRequirement`                  | Defeat at least X enemies                             | `BattleContext.DefeatUnit`          | Planned | Priority |
| `KillWithSpecificAbilityRequirement`    | Defeat an enemy using a specific ability              | `DamageTargetEffect` + `DefeatUnit` | Planned | Priority |
| `KillWithSpecificDamageKindRequirement` | Defeat an enemy using physical/magical/etc. damage    | `DamageTargetEffect` + `DefeatUnit` | Planned | High |
| `KillWithOneHitRequirement`             | Kill an enemy that was at full HP with one hit        | `DamageTargetEffect`                | Planned | High |
| `OverkillRequirement`                   | Deal at least X excess damage beyond remaining HP     | `DamageTargetEffect`                | Planned | High |
| `ExecuteLowHPEnemyRequirement`          | Kill an enemy that was below X% HP                    | `DamageTargetEffect` + `DefeatUnit` | Planned | High |
| `KillWhileLowHPRequirement`             | Kill an enemy while source is below X% HP             | `DefeatUnit`                        | Planned | High |
| `KillWhileAtFullHPRequirement`          | Kill an enemy while source is at full HP              | `DefeatUnit`                        | Planned | High |
| `KillStatusedEnemyRequirement`          | Kill an enemy affected by a specific status/tag       | `DefeatUnit`                        | Planned | High |
| `KillWithoutMovingRequirement`          | Kill an enemy during a turn where source did not move | `DefeatUnit` + turn summary         | Planned | Low |
| `KillAfterMovingRequirement`            | Kill an enemy after moving this turn                  | `DefeatUnit` + turn state           | Planned | High |
| `KillMultipleEnemiesOneTurnRequirement` | Kill X enemies in one turn                            | Turn summary                        | Planned | High |
| `KillMultipleEnemiesOneCastRequirement` | Kill X enemies with one ability cast resolution       | Ability resolution summary          | Planned | High |
| `FirstBloodRequirement`                 | Be the first unit to defeat an enemy in battle        | `DefeatUnit`                        | Planned | High |
| `LastHitRequirement`                    | Land the killing blow on X enemies                    | `DefeatUnit`                        | Planned | Priority |
| `SoloKillRequirement`                   | Kill an enemy that only this creature damaged         | `DefeatUnit` + damage attribution   | Planned | Low |

## Battle Outcome

| Name                                      | Description                                   | Event emitted by | Status | Priority |
| ----------------------------------------- | --------------------------------------------- | ---------------- | ------- | --- |
| `WinBattleCountRequirement`               | Win X battles with this creature present      | End phase        | Planned | Priority |
| `SurviveBattleCountRequirement`           | Survive X battles                             | End phase        | Planned | Priority |
| `WinBattleWithFullHPRequirement`          | Win with full HP                              | End phase        | Planned | High |
| `WinBattleWithoutTakingDamageRequirement` | Win without taking damage                     | End phase        | Planned | High |
| `WinBattleWithinXTurnsRequirement`        | Win in X turns or fewer                       | End phase        | Planned | High |
| `WinBattleAfterXTurnsRequirement`         | Win after at least X turns                    | End phase        | Planned | Low |
| `AllAlliesSurviveRequirement`             | Win with no ally defeated                     | End phase        | Planned | High |
| `OnlyThisUnitSurvivesRequirement`         | Win with this unit as the only surviving ally | End phase        | Planned | Low |
| `WinWithLowHPRequirement`                 | Win with X HP or fewer                        | End phase        | Planned | High |
| `WinWithoutHealingRequirement`            | Win without receiving healing                 | End phase        | Planned | Low |
| `WinWithoutShieldRequirement`             | Win without receiving shields                 | End phase        | Planned | Low |
| `WinWithoutMovingRequirement`             | Win without this unit moving                  | End phase        | Planned | Low |
| `WinWithoutCastingRequirement`            | Win without this unit casting abilities       | End phase        | Planned | Low |
| `WinAfterTakingDamageRequirement`         | Win after taking at least X damage            | End phase        | Planned | High |
| `WinAfterDealingDamageRequirement`        | Win after dealing at least X damage           | End phase        | Planned | Priority |
| `WinAfterApplyingStatusRequirement`       | Win after applying a specific status/tag      | End phase        | Planned | High |
| `WinAgainstEnemyCountRequirement`         | Win a battle with at least X enemies          | End phase        | Planned | High |
| `WinWithTeamSizeRequirement`              | Win with X or fewer allies                    | End phase        | Planned | Low |

## Resource Usage

| Name                                     | Description                            | Event emitted by                               | Status | Priority |
| ---------------------------------------- | -------------------------------------- | ---------------------------------------------- | ------- | --- |
| `SpendActionPointsRequirement`           | Spend X total AP                       | AP decrease event / ability resolution         | Planned | Priority |
| `SpendMovementPointsRequirement`         | Spend X total MP                       | MP decrease event / move resolution            | Planned | Priority |
| `SpendAllActionPointsRequirement`        | End a turn with 0 AP after spending AP | `BattleTurnRules.EndTurn`                      | Planned | High |
| `SpendAllMovementPointsRequirement`      | End a turn with 0 MP after moving      | `BattleTurnRules.EndTurn`                      | Planned | High |
| `EndTurnWithNoResourcesRequirement`      | End a turn with 0 AP and 0 MP          | `BattleTurnRules.EndTurn`                      | Planned | High |
| `EndTurnWithFullActionPointsRequirement` | End turn without spending AP           | `BattleTurnRules.EndTurn`                      | Planned | High |
| `EndTurnWithFullMovementRequirement`     | End turn without spending MP           | `BattleTurnRules.EndTurn`                      | Planned | High |
| `GainActionPointsRequirement`            | Gain X AP from effects                 | `ResourceChangeEffect` / `StealResourceEffect` | Planned | High |
| `GainMovementPointsRequirement`          | Gain X MP from effects                 | `ResourceChangeEffect` / `StealResourceEffect` | Planned | High |
| `ReduceEnemyActionPointsRequirement`     | Reduce enemy AP by X                   | `ResourceChangeEffect` / `StealResourceEffect` | Planned | High |
| `ReduceEnemyMovementPointsRequirement`   | Reduce enemy MP by X                   | `ResourceChangeEffect` / `StealResourceEffect` | Planned | High |
| `StealHealthRequirement`                 | Steal X health                         | `StealResourceEffect`                          | Planned | High |
| `StealActionPointsRequirement`           | Steal X AP                             | `StealResourceEffect`                          | Planned | High |
| `StealMovementPointsRequirement`         | Steal X MP                             | `StealResourceEffect`                          | Planned | High |
| `StealRangeRequirement`                  | Steal X bonus range                    | `StealResourceEffect`                          | Planned | Low |
| `EndTurnWithExactResourceRequirement`    | End turn with exactly X AP/MP          | `BattleTurnRules.EndTurn`                      | Planned | Low |
| `UseNoResourcesTurnRequirement`          | Take a turn without spending AP or MP  | `BattleTurnRules.EndTurn`                      | Planned | Low |

## Movement

| Name                                    | Description                                    | Event emitted by                   | Status | Priority |
| --------------------------------------- | ---------------------------------------------- | ---------------------------------- | ------- | --- |
| `TotalDistanceTravelledRequirement`     | Travel X total cells                           | `BattleActionResolver.ResolveMove` | Planned | Priority |
| `MaxDistanceInOneMoveRequirement`       | Move X cells in one move action                | `BattleActionResolver.ResolveMove` | Planned | Priority |
| `MoveCountRequirement`                  | Perform X move actions                         | `BattleActionResolver.ResolveMove` | Planned | Priority |
| `MoveTowardEnemyRequirement`            | End move closer to nearest enemy               | `BattleActionResolver.ResolveMove` | Planned | High |
| `MoveAwayFromEnemyRequirement`          | End move farther from nearest enemy            | `BattleActionResolver.ResolveMove` | Planned | High |
| `MoveTowardAllyRequirement`             | End move closer to nearest ally                | `BattleActionResolver.ResolveMove` | Planned | High |
| `MoveAwayFromAllyRequirement`           | End move farther from nearest ally             | `BattleActionResolver.ResolveMove` | Planned | Low |
| `MoveAdjacentToEnemyRequirement`        | Move and end adjacent to an enemy              | `BattleActionResolver.ResolveMove` | Planned | High |
| `MoveAdjacentToAllyRequirement`         | Move and end adjacent to an ally               | `BattleActionResolver.ResolveMove` | Planned | High |
| `MoveFromAdjacentToEnemyRequirement`    | Start adjacent to enemy and move away          | `BattleActionResolver.ResolveMove` | Planned | Low |
| `MoveWithoutEndingNearEnemyRequirement` | Move and end at least X cells from every enemy | `BattleActionResolver.ResolveMove` | Planned | Low |
| `MoveThroughSpecificCellRequirement`    | Move through a specific type/tagged cell       | `BattleActionResolver.ResolveMove` | Planned | Low |
| `VisitCellsRequirement`                 | Visit X different cells during battle          | Move tracking                      | Planned | High |
| `EndTurnOnSpecificCellRequirement`      | End a turn on a specific board cell/tag/zone   | `BattleTurnRules.EndTurn`          | Planned | High |
| `NeverMoveRequirement`                  | Win without moving this creature               | End phase                          | Planned | Low |
| `MoveEveryTurnRequirement`              | Move at least once every turn                  | End phase                          | Planned | Low |

## Ability Targeting

| Name                                 | Description                                             | Event emitted by                      | Status | Priority |
| ------------------------------------ | ------------------------------------------------------- | ------------------------------------- | ------- | --- |
| `TargetEnemyCountRequirement`        | Target enemies X times                                  | `BattleActionResolver.ResolveAbility` | Planned | Priority |
| `TargetAllyCountRequirement`         | Target allies X times                                   | `BattleActionResolver.ResolveAbility` | Planned | Priority |
| `TargetSelfRequirement`              | Target self X times                                     | `BattleActionResolver.ResolveAbility` | Planned | High |
| `TargetMultipleUnitsRequirement`     | Hit or affect at least X units with one ability         | Effect resolution                     | Planned | High |
| `TargetMultipleEnemiesRequirement`   | Hit at least X enemies with one ability                 | Effect resolution                     | Planned | High |
| `TargetMultipleAlliesRequirement`    | Affect at least X allies with one ability               | Effect resolution                     | Planned | High |
| `HitEnemyAndAllySameCastRequirement` | Affect at least one enemy and one ally in the same cast | Effect resolution                     | Planned | Low |
| `HitEveryEnemyRequirement`           | Affect every living enemy at least once during a battle | End phase                             | Planned | Low |
| `NeverTargetAllyRequirement`         | Win without targeting allies                            | End phase                             | Planned | Low |
| `NeverTargetEnemyRequirement`        | Win without targeting enemies                           | End phase                             | Planned | Low |
| `TargetUnitWithStatusRequirement`    | Target a unit that has a specific status/tag            | `BattleActionResolver.ResolveAbility` | Planned | High |
| `TargetUnitWithoutStatusRequirement` | Target a unit with no statuses                          | `BattleActionResolver.ResolveAbility` | Planned | Low |
| `TargetLowHPEnemyRequirement`        | Target an enemy below X% HP                             | `BattleActionResolver.ResolveAbility` | Planned | High |
| `TargetFullHPEnemyRequirement`       | Target an enemy at full HP                              | `BattleActionResolver.ResolveAbility` | Planned | High |

## Revive / Defeat / Survival

| Name                              | Description                              | Event emitted by           | Status | Priority |
| --------------------------------- | ---------------------------------------- | -------------------------- | ------- | --- |
| `ReviveAllyRequirement`           | Revive allies X times                    | `ReviveEffect`             | Planned | High |
| `ReviveSelfRequirement`           | Revive self X times, if allowed          | `ReviveEffect`             | Planned | Low |
| `ReviveWithLowHPRequirement`      | Revive a target to X HP or less          | `ReviveEffect`             | Planned | Low |
| `ReviveToFullHPRequirement`       | Revive a target to full HP               | `ReviveEffect`             | Planned | Low |
| `WinAfterReviveRequirement`       | Win after this unit revived someone      | End phase                  | Planned | High |
| `WinAfterBeingRevivedRequirement` | Win after this unit was revived          | End phase                  | Planned | High |
| `DefeatedCountRequirement`        | Be defeated X times                      | `BattleContext.DefeatUnit` | Planned | High |
| `AvoidDefeatRequirement`          | Win without this unit being defeated     | End phase                  | Planned | High |
| `LastUnitStandingRequirement`     | Win while all other allies were defeated | End phase                  | Planned | Low |
| `SurviveAsLastAllyRequirement`    | Spend X turns as the last living ally    | Turn tracking              | Planned | Low |

## HP Thresholds / Risk Conditions

| Name                                   | Description                               | Event emitted by                   | Status | Priority |
| -------------------------------------- | ----------------------------------------- | ---------------------------------- | ------- | --- |
| `StartTurnBelowHPThresholdRequirement` | Start turn below X% HP                    | `BattleTurnRules.BeginTurn`        | Planned | High |
| `StartTurnAboveHPThresholdRequirement` | Start turn above X% HP                    | `BattleTurnRules.BeginTurn`        | Planned | High |
| `EndTurnBelowHPThresholdRequirement`   | End turn below X% HP                      | `BattleTurnRules.EndTurn`          | Planned | High |
| `EndTurnAboveHPThresholdRequirement`   | End turn above X% HP                      | `BattleTurnRules.EndTurn`          | Planned | High |
| `ReachCriticalHPRequirement`           | Reach X HP or fewer at any point          | `DamageTargetEffect`               | Planned | High |
| `RecoverFromCriticalHPRequirement`     | Go from below X% HP to above Y% HP        | `HealTargetEffect` + turn tracking | Planned | High |
| `StayAboveHPThresholdRequirement`      | Win without dropping below X% HP          | End phase                          | Planned | Low |
| `StayBelowHPThresholdRequirement`      | Win while ending all turns below X% HP    | End phase                          | Planned | Low |
| `FluctuateHPRequirement`               | Drop below X% HP then recover above Y% HP | HP tracking                        | Planned | Low |
| `EndBattleAtExactHPRequirement`        | Win with exactly X HP                     | End phase                          | Planned | Low |
| `EndBattleBelowHPRequirement`          | Win with X HP or less                     | End phase                          | Planned | High |
| `EndBattleAboveHPRequirement`          | Win with at least X% HP                   | End phase                          | Planned | High |

## Forms / Creature-Specific Conditions

| Name                           | Description                                  | Event emitted by                      | Status | Priority |
| ------------------------------ | -------------------------------------------- | ------------------------------------- | ------- | --- |
| `CastAbilityInFormRequirement` | Cast X abilities while in a specific form    | `BattleActionResolver.ResolveAbility` | Planned | High |
| `DealDamageInFormRequirement`  | Deal X damage while in a specific form       | `DamageTargetEffect`                  | Planned | High |
| `TakeDamageInFormRequirement`  | Take X damage while in a specific form       | `DamageTargetEffect`                  | Planned | High |
| `WinBattleInFormRequirement`   | Win X battles while in a specific form       | End phase                             | Planned | High |
| `UseFormTierRequirement`       | Complete a battle while at least form tier X | End phase                             | Planned | Priority |
| `UnlockWhileInFormRequirement` | Complete another requirement while in a form | Requirement wrapper                   | Planned | Low |
| `NeverChangeFormRequirement`   | Win without changing form                    | End phase                             | Planned | Low |
| `ChangeFormCountRequirement`   | Change form X times                          | Form-change event                     | Planned | High |

## Team / Ally Interaction

| Name                            | Description                                                          | Event emitted by                         | Status | Priority |
| ------------------------------- | -------------------------------------------------------------------- | ---------------------------------------- | ------- | --- |
| `BuffAllyRequirement`           | Apply a positive status/shield/resource gain to allies X times       | Effect events                            | Planned | High |
| `DebuffEnemyRequirement`        | Apply a negative status/resource loss to enemies X times             | Effect events                            | Planned | High |
| `ProtectAllyRequirement`        | Shield or heal an ally below X% HP                                   | `ApplyShieldEffect` / `HealTargetEffect` | Planned | High |
| `SaveAllyFromDefeatRequirement` | Heal/shield ally that would otherwise die soon or was at critical HP | Heal/shield event + HP threshold         | Planned | Low |
| `AssistKillRequirement`         | Damage enemy that is later killed by an ally                         | `DamageTargetEffect` + `DefeatUnit`      | Planned | High |
| `ReceiveAssistRequirement`      | Kill enemy previously damaged/debuffed by ally                       | `DefeatUnit`                             | Planned | High |
| `ActAfterAllyRequirement`       | Take a turn immediately after an ally                                | `BattleTurnRules.BeginTurn`              | Planned | Low |
| `ActAfterEnemyRequirement`      | Take a turn immediately after an enemy                               | `BattleTurnRules.BeginTurn`              | Planned | Low |
| `ComboWithAllyRequirement`      | Cast an ability after an ally applied a specific status/effect       | Ability cast + prior event tracking      | Planned | Low |

## Positioning / Formation

| Name                                     | Description                                             | Event emitted by            | Status | Priority |
| ---------------------------------------- | ------------------------------------------------------- | --------------------------- | ------- | --- |
| `StartTurnSurroundedRequirement`         | Start turn with X enemies within Y cells                | `BattleTurnRules.BeginTurn` | Planned | High |
| `EndTurnSurroundedRequirement`           | End turn with X enemies within Y cells                  | `BattleTurnRules.EndTurn`   | Planned | High |
| `StartTurnNearMultipleAlliesRequirement` | Start turn near X allies                                | `BattleTurnRules.BeginTurn` | Planned | High |
| `EndTurnNearMultipleAlliesRequirement`   | End turn near X allies                                  | `BattleTurnRules.EndTurn`   | Planned | High |
| `StartTurnIsolatedRequirement`           | Start turn with no units within X cells                 | `BattleTurnRules.BeginTurn` | Planned | Low |
| `EndTurnIsolatedRequirement`             | End turn with no units within X cells                   | `BattleTurnRules.EndTurn`   | Planned | Low |
| `MaintainDistanceFromEnemyRequirement`   | Never end a turn within X cells of an enemy             | End phase                   | Planned | Low |
| `StayNearAllyRequirement`                | End every turn within X cells of an ally                | End phase                   | Planned | Low |
| `FlankEnemyRequirement`                  | End turn on opposite side of enemy relative to ally     | `BattleTurnRules.EndTurn`   | Planned | Low |
| `LineUpWithEnemyRequirement`             | End turn aligned with enemy on same row/column/diagonal | `BattleTurnRules.EndTurn`   | Planned | Low |

## Range / Line of Sight

| Name                                  | Description                                           | Event emitted by                      | Status | Priority |
|---------------------------------------|-------------------------------------------------------|---------------------------------------|--------- | --- |
| `HitFromLongRangeRequirement` 		| Hit a target from at least X cells away 				| `DamageTargetEffect` + cast context 	| Planned | High |
| `HitFromMeleeRangeRequirement` 		| Hit a target while adjacent 							| `DamageTargetEffect` + cast context 	| Planned | High |
| `HitWithoutLineOfSightRequirement` 	| Hit a target without direct line of sight, if allowed | Ability resolution 					| Planned | Low |
| `MaintainRangeRequirement` 			| End every turn at least X cells from enemies 			| End phase + turn tracking 			| Planned | Low |
| `CloseGapRequirement` 				| Move from outside X range to adjacent to an enemy 	| `BattleActionResolver.ResolveMove`	| Planned | High |

## Turn Bar / Initiative / Stamina

| Name                                 | Description                                              | Event emitted by              | Status | Priority |
| ------------------------------------ | -------------------------------------------------------- | ----------------------------- | ------- | --- |
| `TakeTurnCountRequirement`           | Take X turns                                             | `BattleTurnRules.BeginTurn`   | Planned | High |
| `ActBeforeAnyEnemyRequirement`       | Be the first unit to act                                 | `BattleTurnRules.BeginTurn`   | Planned | High |
| `ActBeforeSpecificUnitRequirement`   | Act before a specific enemy/ally                         | `BattleTurnRules.BeginTurn`   | Planned | Low |
| `TakeConsecutiveTurnsRequirement`    | Take X turns before a specific enemy acts                | Turn tracking                 | Planned | Low |
| `DelayEnemyTurnBarRequirement`       | Reduce/delay enemy turn bar by X                         | `AdjustTurnBarTimeEffect`     | Planned | High |
| `AdvanceAllyTurnBarRequirement`      | Advance ally turn bar by X                               | `AdjustTurnBarTimeEffect`     | Planned | High |
| `StealTurnBarTimeRequirement`        | Steal X turn-bar time                                    | `StealResourceEffect`         | Planned | Low |
| `IncreaseTurnBarDurationRequirement` | Increase turn-bar duration by X                          | `AdjustTurnBarDurationEffect` | Planned | Low |
| `DecreaseTurnBarDurationRequirement` | Decrease turn-bar duration by X                          | `AdjustTurnBarDurationEffect` | Planned | Low |
| `EndTurnWithFullTurnBarRequirement`  | End turn with turn bar full/ready                        | `BattleTurnRules.EndTurn`     | Planned | Low |
| `ReachTurnBarReadyRequirement`       | Become ready X times                                     | Initiative update             | Planned | High |
| `PreventEnemyTurnRequirement`        | Delay enemy so it misses/does not act before battle ends | End phase + turn tracking     | Planned | Low |

## Displacement / Forced Movement

| Name                                   | Description                                        | Event emitted by     | Status | Priority |
| -------------------------------------- | -------------------------------------------------- | -------------------- | ------- | --- |
| `PushEnemyRequirement`                 | Force-move enemies X times                         | `MoveStatus`         | Planned | High |
| `PushAllyRequirement`                  | Force-move allies X times                          | `MoveStatus`         | Planned | Low |
| `MaxForceMoveDistanceRequirement`      | Force-move a unit at least X cells in one effect   | `MoveStatus`         | Planned | Low |
| `ForceMoveTotalDistanceRequirement`    | Force-move units X total cells                     | `MoveStatus`         | Planned | High |
| `PushEnemyTowardAllyRequirement`       | Force-move enemy closer to an ally                 | `MoveStatus`         | Planned | Low |
| `PushEnemyAwayFromSelfRequirement`     | Push enemy away from source                        | `MoveStatus`         | Planned | Low |
| `PushEnemyIntoRangeRequirement`        | Push enemy into range of an ally/source            | `MoveStatus`         | Planned | Low |
| `PushEnemyOutOfRangeRequirement`       | Push enemy out of range of an ally/source          | `MoveStatus`         | Planned | Low |
| `PushEnemyAdjacentToObjectRequirement` | Force-move enemy adjacent to an interactive object | `MoveStatus`         | Planned | Low |
| `SwapPositionRequirement`              | Successfully swap positions X times                | `SwapPositionEffect` | Planned | High |
| `SwapWithEnemyRequirement`             | Swap position with enemy X times                   | `SwapPositionEffect` | Planned | Low |
| `SwapWithAllyRequirement`              | Swap position with ally X times                    | `SwapPositionEffect` | Planned | Low |
| `TeleportRequirement`                  | Teleport X times                                   | `TeleportEffect`     | Planned | High |
| `TeleportDistanceRequirement`          | Teleport X total cells                             | `TeleportEffect`     | Planned | High |
| `MaxTeleportDistanceRequirement`       | Teleport at least X cells in one effect            | `TeleportEffect`     | Planned | Low |
| `TeleportAdjacentToEnemyRequirement`   | Teleport and end adjacent to an enemy              | `TeleportEffect`     | Planned | Low |
| `TeleportAwayFromEnemyRequirement`     | Teleport farther from nearest enemy                | `TeleportEffect`     | Planned | Low |

## Interactive Objects

| Name                                      | Description                                      | Event emitted by                | Status | Priority |
| ----------------------------------------- | ------------------------------------------------ | ------------------------------- | ------- | --- |
| `PlaceObjectCountRequirement`             | Place X interactive objects                      | `PlaceInteractiveObjectEffect`  | Planned | High |
| `PlaceSpecificObjectRequirement`          | Place a specific object X times                  | `PlaceInteractiveObjectEffect`  | Planned | High |
| `PlaceObjectWithTagRequirement`           | Place objects with a specific tag X times        | `PlaceInteractiveObjectEffect`  | Planned | High |
| `RemoveObjectCountRequirement`            | Remove X interactive objects                     | `RemoveInteractiveObjectEffect` | Planned | High |
| `RemoveSpecificObjectRequirement`         | Remove a specific object X times                 | `RemoveInteractiveObjectEffect` | Planned | High |
| `RemoveObjectWithTagRequirement`          | Remove objects with a specific tag X times       | `RemoveInteractiveObjectEffect` | Planned | High |
| `PlaceObjectNearEnemyRequirement`         | Place object within X cells of enemy             | `PlaceInteractiveObjectEffect`  | Planned | Low |
| `PlaceObjectNearAllyRequirement`          | Place object within X cells of ally              | `PlaceInteractiveObjectEffect`  | Planned | Low |
| `PlaceObjectOnSpecificCellRequirement`    | Place object on a specific cell/zone/tagged cell | `PlaceInteractiveObjectEffect`  | Planned | Low |
| `EndTurnNearObjectRequirement`            | End turn within X cells of an object with a tag  | `BattleTurnRules.EndTurn`       | Planned | Low |
| `ObjectSurvivesUntilBattleEndRequirement` | Place object that remains until victory          | End phase                       | Planned | Low |
| `EnemyEndsNearObjectRequirement`          | Enemy ends turn near your placed object          | Enemy turn end                  | Planned | Low |
| `RemoveEnemyObjectRequirement`            | Remove object placed by enemy side               | `RemoveInteractiveObjectEffect` | Planned | Low |

## Buff / Status Uptime

| Name                                      | Description                                           | Event emitted by      | Status | Priority |
|-------------------------------------------|-------------------------------------------------------|-----------------------|--------- | --- |
| `MaintainBuffForTurnsRequirement` 		| Keep a positive status active for X turns 			| Turn tracking 		| Planned | Low |
| `MaintainDebuffOnEnemyRequirement` 		| Keep a negative status on an enemy for X turns 		| Turn tracking 		| Planned | Low |
| `EndBattleWithBuffRequirement` 			| Win while affected by a specific positive status 		| End phase 			| Planned | High |
| `EndBattleWithEnemyDebuffedRequirement` 	| Win while at least one enemy has a specific debuff 	| End phase 			| Planned | High |
| `RefreshStatusRequirement` 				| Refresh or extend a status X times 					| Status refresh event 	| Planned | High |

## Damage Prevention / Mitigation

| Name                              | Description                                           | Event emitted by              | Status | Priority |
|-----------------------------------|-------------------------------------------------------|-------------------------------|--------- | --- |
| `ReduceIncomingDamageRequirement` | Prevent X damage through armor/resistance/reduction 	| Damage calculation event 		| Planned | High |
| `DodgeAttackRequirement` 			| Avoid X attacks through dodge/evasion 				| Attack resolution 			| Planned | Low |
| `BlockAttackRequirement` 			| Block X attacks 										| Attack resolution 			| Planned | Low |
| `ResistStatusRequirement` 		| Resist or ignore X status applications 				| Status application resolution | Planned | Low |
| `PreventAllyDamageRequirement` 	| Prevent X damage to allies through shields/reduction 	| Damage prevention event 		| Planned | Low |

## Meta / Composite Requirements

These are not emitted by one event; they combine other requirements.

| Name                             | Description                                            | Event emitted by      | Status | Priority |
| -------------------------------- | ------------------------------------------------------ | --------------------- | ------- | --- |
| `AndRequirement`                 | Complete all child requirements                        | Requirement evaluator | Planned | Priority |
| `OrRequirement`                  | Complete any child requirement                         | Requirement evaluator | Planned | Priority |
| `NotRequirement`                 | Complete if child requirement never occurs             | Requirement evaluator | Planned | High |
| `SequenceRequirement`            | Complete events in a specific order                    | Requirement evaluator | Planned | Low |
| `WithinTurnsRequirement`         | Complete child requirement within X turns              | Requirement evaluator | Planned | High |
| `AfterTurnRequirement`           | Complete child requirement after turn X                | Requirement evaluator | Planned | High |
| `BeforeTakingDamageRequirement`  | Complete child before taking damage                    | Requirement evaluator | Planned | Low |
| `WhileStatusedRequirement`       | Complete child while source has a status/tag           | Requirement evaluator | Planned | High |
| `WhileLowHPRequirement`          | Complete child while below HP threshold                | Requirement evaluator | Planned | High |
| `WithSpecificAbilityRequirement` | Complete child using a specific ability                | Requirement evaluator | Planned | Priority |
| `WithSpecificTargetRequirement`  | Complete child against ally/enemy/self/specific target | Requirement evaluator | Planned | High |
