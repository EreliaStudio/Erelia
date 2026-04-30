# Expanded Feat Requirement List

This document expands the current combat feat requirement catalogue with additional requirement ideas, event suggestions, and implementation priorities.

All entries follow the same pattern:

- **Event emitted by**: where in the code the event is produced
- **Accumulation**:
  - `Additive`: progress sums across events
  - `Maximum`: progress takes the best single event
  - `Set Count`: progress counts distinct values
  - `Composite`: progress is evaluated from child requirements
- **Duration**:
  - `Ability`: evaluate the requirement inside one ability resolution or hit
  - `Turn`: evaluate the requirement inside one unit turn
  - `Fight`: evaluate the requirement across one battle, then reset if incomplete
  - `Game`: persist progress across fights
- **Scope**:
  - `Battle`: progress exists only inside one combat
  - `Profile`: progress persists across multiple fights
- **Status**: `Implemented` / `Planned`

---

## Core Design Distinction

A major distinction to add to the system is the requirement duration window. The requirement class should describe the measured behavior once; `FeatRequirement.Duration` decides which time window makes that behavior valid.

```text
Ability duration = progress must fit inside one ability resolution or hit.
Turn duration = progress must fit inside one unit turn.
Fight duration = progress must fit inside one battle.
Game duration = progress persists across multiple fights.
```

Examples:

```text
Cast Fireball 3 times in one battle.
```

```text
Cast Fireball 100 times across all battles.
```

Both should use `CastAbilityCountRequirement`; the first uses `Duration.Fight`, and the second uses `Duration.Game`. This avoids creating separate lifetime/fight/turn classes for the same measured behavior.

Events should remain the combat log: what happened, when, from whom, to whom, and with which ability/effect. Duration-specific behavior belongs in requirement parsing at the end of the fight, not in separate event emission paths for every duration variant.

`FeatRequirement` also owns the repeat count:

```text
RequiredRepeatCount = how many duration windows must satisfy this requirement.
FeatRequirementProgress.CompletedRepeatCount = how many windows have already satisfied it.
```

Examples:

```text
Deal 100 damage with Duration.Fight and RequiredRepeatCount = 3
= deal at least 100 damage in a fight three separate times.
```

```text
Deal 50 damage with Duration.Ability and RequiredRepeatCount = 2
= have two ability windows that each dealt at least 50 damage.
```

Children should only answer "how much progress does this typed event contribute?" Base requirement code groups events into ability, turn, fight, or game windows, sorts those windows by progress, and counts repeat completions.

---

# Cross-Battle / Profile Progression

These are useful for long-term unlocks, mastery nodes, creature evolution, or spell upgrades.

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `CastAbilityCountRequirement` with `Duration.Game` | Cast any or a specific ability X times across multiple fights | `BattleActionResolver.ResolveAbility` | Additive | Profile | Implemented |
| `DealDamageRequirement` with `Duration.Game` | Deal X total damage across multiple fights | `DamageTargetEffect` | Additive | Profile | Implemented |
| `WinBattleCountRequirement` | Win X battles with this creature | End phase | Additive | Profile | Planned |
| `DefeatEnemyLifetimeRequirement` | Defeat X enemies across multiple fights | `BattleContext.DefeatUnit` | Additive | Profile | Planned |
| `TakeDamageLifetimeRequirement` | Take X total damage across multiple fights | `DamageTargetEffect` | Additive | Profile | Planned |
| `HealLifetimeRequirement` | Heal X total HP across multiple fights | `HealTargetEffect` | Additive | Profile | Planned |
| `ApplyStatusLifetimeRequirement` | Apply statuses X total times across multiple fights | `ApplyStatusEffect` | Additive | Profile | Planned |
| `MoveDistanceLifetimeRequirement` | Move X total cells across multiple fights | `BattleActionResolver.ResolveMove` | Additive | Profile | Planned |
| `SurviveBattleCountRequirement` | Survive X battles | End phase | Additive | Profile | Planned |
| `UseSpecificEffectLifetimeRequirement` | Trigger a specific effect type X times across multiple fights | Any effect event | Additive | Profile | Planned |

---

# Ability Casting

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `CastAbilityCountRequirement` with `Duration.Fight` | Cast any or a specific ability X times during one battle | `BattleActionResolver.ResolveAbility` | Additive | Battle | Implemented |
| `CastAbilityCountRequirement` with `Duration.Turn` | Cast any or a specific ability at least X times in one turn | Turn summary / `BattleTurnRules.EndTurn` | Maximum | Battle | Implemented |
| `CastDifferentAbilitiesRequirement` | Cast X different abilities during one battle | `BattleActionResolver.ResolveAbility` | Set Count | Battle | Planned |
| `CastSameAbilityConsecutivelyRequirement` | Cast the same ability X turns in a row | `BattleActionResolver.ResolveAbility` + turn tracking | Maximum | Battle | Planned |
| `CastAbilityWithoutMovingRequirement` | Cast an ability during a turn where the unit did not move | `BattleTurnRules.EndTurn` | Additive | Battle | Planned |
| `CastAbilityAfterMovingRequirement` | Cast an ability after moving in the same turn | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `CastAbilityBeforeMovingRequirement` | Cast an ability before moving in the same turn | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `CastAbilityAtMaxRangeRequirement` | Hit/cast an ability at its maximum possible range | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `CastAbilityAtMinimumRangeRequirement` | Cast an ability adjacent or at minimum allowed range | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `CastAbilityOnFirstTurnRequirement` | Cast an ability on this unit's first turn | `BattleActionResolver.ResolveAbility` | Maximum | Battle | Planned |
| `CastAbilityEveryTurnRequirement` | Cast at least one ability on every turn this unit takes | End phase | Maximum | Battle | Planned |
| `WinAfterCastingSpecificAbilityRequirement` | Win a battle after casting a specific ability at least once | End phase | Maximum | Battle/Profile | Planned |
| `CastAbilityWithNoAPRemainingRequirement` | Cast an ability that leaves the unit with 0 AP | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `CastAbilityWhileLowHPRequirement` | Cast an ability while below X% HP | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `CastAbilityWhileStatusedRequirement` | Cast an ability while affected by a specific status/tag | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `CastAbilityFromSpecificFormRequirement` | Cast ability while in a specific form | `BattleActionResolver.ResolveAbility` | Additive | Battle/Profile | Planned |

---

# Ability Targeting

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `TargetEnemyCountRequirement` | Target enemies X times | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `TargetAllyCountRequirement` | Target allies X times | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `TargetSelfRequirement` | Target self X times | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `TargetMultipleUnitsRequirement` | Hit or affect at least X units with one ability | `BattleActionResolver.ResolveAbility` / effect resolution | Maximum | Battle | Planned |
| `TargetMultipleEnemiesRequirement` | Hit at least X enemies with one ability | Effect resolution | Maximum | Battle | Planned |
| `TargetMultipleAlliesRequirement` | Affect at least X allies with one ability | Effect resolution | Maximum | Battle | Planned |
| `HitEnemyAndAllySameCastRequirement` | Affect at least one enemy and one ally in the same cast | Effect resolution | Maximum | Battle | Planned |
| `HitEveryEnemyRequirement` | Affect every living enemy at least once during a battle | End phase | Maximum | Battle | Planned |
| `NeverTargetAllyRequirement` | Win without targeting allies | End phase | Maximum | Battle | Planned |
| `NeverTargetEnemyRequirement` | Win without targeting enemies | End phase | Maximum | Battle | Planned |
| `TargetUnitWithStatusRequirement` | Target a unit that has a specific status/tag | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `TargetUnitWithoutStatusRequirement` | Target a unit with no statuses | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `TargetLowHPEnemyRequirement` | Target an enemy below X% HP | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |
| `TargetFullHPEnemyRequirement` | Target an enemy at full HP | `BattleActionResolver.ResolveAbility` | Additive | Battle | Planned |

---

# Damage Dealing

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `DealDamageRequirement` with `Duration.Ability` | Deal at least X damage in a single hit or ability event | `DamageTargetEffect` | Maximum | Battle | Implemented |
| `DealDamageRequirement` with `Duration.Fight` | Deal a total of X damage across the whole battle | `DamageTargetEffect` | Additive | Battle | Implemented |
| `DealDamageRequirement` with `Duration.Game` | Deal X total damage across multiple fights | `DamageTargetEffect` | Additive | Profile | Implemented |
| `MaxSingleHitDamageRequirement` | Compatibility path; prefer `DealDamageRequirement` with `Duration.Ability` | `DamageTargetEffect` | Maximum | Battle | Legacy |
| `DealDamageInOneTurnRequirement` | Deal at least X total damage within a single turn | Turn summary | Maximum | Battle | Planned |
| `DealDamageWithSpecificAbilityRequirement` | Deal X damage using a specific ability | `DamageTargetEffect` | Additive | Battle/Profile | Planned |
| `DealDamageWithSpecificDamageKindRequirement` | Deal X physical/magical/etc. damage | `DamageTargetEffect` | Additive | Battle/Profile | Planned |
| `DealDamageToStatusedTargetRequirement` | Deal X damage to targets with a specific status/tag | `DamageTargetEffect` | Additive | Battle | Planned |
| `DealDamageToUnstatusedTargetRequirement` | Deal X damage to targets with no statuses | `DamageTargetEffect` | Additive | Battle | Planned |
| `DealDamageWhileStatusedRequirement` | Deal X damage while the source has a specific status/tag | `DamageTargetEffect` | Additive | Battle | Planned |
| `DealDamageWhileLowHPRequirement` | Deal X damage while below a HP threshold | `DamageTargetEffect` | Additive | Battle | Planned |
| `DealDamageAtFullHPRequirement` | Deal X damage while at full HP | `DamageTargetEffect` | Additive | Battle | Planned |
| `DealDamageFromRangeRequirement` | Deal damage from at least X cells away | `DamageTargetEffect` / cast context | Maximum / Additive | Battle | Planned |
| `DealDamageAdjacentRequirement` | Deal damage while adjacent to the target | `DamageTargetEffect` / cast context | Additive | Battle | Planned |
| `DealDamageWithoutTakingDamageRequirement` | Deal X damage before taking any damage | `DamageTargetEffect` + damage taken tracking | Additive | Battle | Planned |
| `DealDamageAfterTakingDamageRequirement` | Deal X damage after being damaged this turn/battle | `DamageTargetEffect` | Additive | Battle | Planned |
| `DamageEveryEnemyRequirement` | Damage every enemy at least once | End phase | Maximum | Battle | Planned |
| `DamageSameEnemyMultipleTimesRequirement` | Damage the same enemy X times | `DamageTargetEffect` | Additive per target | Battle | Planned |
| `DamageDifferentEnemiesRequirement` | Damage X different enemies | `DamageTargetEffect` | Set Count | Battle | Planned |
| `NoDamageTakenWhileDealingDamageRequirement` | Win after dealing X damage and taking 0 damage | End phase | Maximum | Battle | Planned |

---

# Critical / Finisher / Kill Requirements

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `KillCountRequirement` | Defeat at least X enemies during the battle | `BattleContext.DefeatUnit` | Additive | Battle | Planned |
| `KillLifetimeRequirement` | Defeat X enemies across multiple battles | `BattleContext.DefeatUnit` | Additive | Profile | Planned |
| `KillWithSpecificAbilityRequirement` | Defeat an enemy using a specific ability | `DamageTargetEffect` / `DefeatUnit` | Additive | Battle/Profile | Planned |
| `KillWithSpecificDamageKindRequirement` | Defeat an enemy using physical/magical/etc. damage | `DamageTargetEffect` / `DefeatUnit` | Additive | Battle | Planned |
| `KillWithOneHitRequirement` | Kill an enemy that was at full HP with one hit | `DamageTargetEffect` | Maximum | Battle | Planned |
| `OverkillRequirement` | Deal at least X excess damage beyond remaining HP | `DamageTargetEffect` | Maximum | Battle | Planned |
| `ExecuteLowHPEnemyRequirement` | Kill an enemy that was below X% HP | `DamageTargetEffect` / `DefeatUnit` | Additive | Battle | Planned |
| `KillWhileLowHPRequirement` | Kill an enemy while source is below X% HP | `DefeatUnit` | Additive | Battle | Planned |
| `KillWhileAtFullHPRequirement` | Kill an enemy while source is at full HP | `DefeatUnit` | Additive | Battle | Planned |
| `KillStatusedEnemyRequirement` | Kill an enemy affected by a specific status/tag | `DefeatUnit` | Additive | Battle/Profile | Planned |
| `KillWithoutMovingRequirement` | Kill an enemy during a turn where source did not move | `DefeatUnit` / turn summary | Additive | Battle | Planned |
| `KillAfterMovingRequirement` | Kill an enemy after moving this turn | `DefeatUnit` / turn state | Additive | Battle | Planned |
| `KillMultipleEnemiesOneTurnRequirement` | Kill X enemies in one turn | Turn summary | Maximum | Battle | Planned |
| `KillMultipleEnemiesOneCastRequirement` | Kill X enemies with one ability cast | Ability resolution summary | Maximum | Battle | Planned |
| `FirstBloodRequirement` | Be the first unit to defeat an enemy in battle | `DefeatUnit` | Maximum | Battle | Planned |
| `LastHitRequirement` | Land the killing blow on X enemies | `DefeatUnit` | Additive | Battle/Profile | Planned |
| `SoloKillRequirement` | Kill an enemy that only this creature damaged | `DefeatUnit` + damage attribution | Additive | Battle | Planned |

---

# Damage Taking

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `TakeDamageRequirement` | Take a total of X damage across the whole battle | `DamageTargetEffect` | Additive | Battle | Implemented |
| `MaxSingleHitTakenRequirement` | Take at least X damage in one hit | `DamageTargetEffect` | Maximum | Battle | Planned |
| `SurviveHitRequirement` | Survive a hit that dealt at least X damage | `DamageTargetEffect` | Maximum | Battle | Planned |
| `TakeDamageAndSurviveRequirement` | Take X total damage and finish battle alive | End phase | Maximum | Battle | Planned |
| `TakeDamageFromSpecificKindRequirement` | Take X physical/magical/etc. damage | `DamageTargetEffect` | Additive | Battle/Profile | Planned |
| `TakeDamageFromSpecificAbilityRequirement` | Take X damage from a specific ability | `DamageTargetEffect` | Additive | Battle | Planned |
| `TakeDamageWhileShieldedRequirement` | Take damage while having at least one active shield | `DamageTargetEffect` | Additive | Battle | Planned |
| `TakeDamageWhileStatusedRequirement` | Take damage while affected by a specific status/tag | `DamageTargetEffect` | Additive | Battle | Planned |
| `TakeDamageWhileLowHPRequirement` | Take damage while already below X% HP | `DamageTargetEffect` | Additive | Battle | Planned |
| `SurviveAtOneHPRequirement` | Survive damage and remain at exactly 1 HP | `DamageTargetEffect` | Maximum | Battle | Planned |
| `LoseHalfHPInOneHitRequirement` | Lose at least X% max HP in one hit | `DamageTargetEffect` | Maximum | Battle | Planned |
| `TakeNoDamageForTurnsRequirement` | Avoid damage for X own turns | Turn tracking | Maximum | Battle | Planned |
| `TakeNoDamageRequirement` | Finish battle without taking damage | End phase | Maximum | Battle | Planned |
| `TakeDamageEveryTurnRequirement` | Take damage on every turn this creature acts | End phase | Maximum | Battle | Planned |
| `TankDamageForAlliesRequirement` | Take X damage while allies remain alive | `DamageTargetEffect` | Additive | Battle | Planned |

---

# Healing

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `HealHealthRequirement` | Heal a total of X HP across the battle | `HealTargetEffect` | Additive | Battle | Implemented |
| `HealLifetimeRequirement` | Heal X total HP across multiple battles | `HealTargetEffect` | Additive | Profile | Planned |
| `MaxSingleHealRequirement` | Restore at least X HP in a single heal | `HealTargetEffect` | Maximum | Battle | Planned |
| `HealSpecificAllyRequirement` | Heal a specific ally X total HP | `HealTargetEffect` | Additive | Battle | Planned |
| `HealSelfRequirement` | Heal self X total HP | `HealTargetEffect` | Additive | Battle/Profile | Planned |
| `HealOtherAllyRequirement` | Heal allies other than self X total HP | `HealTargetEffect` | Additive | Battle/Profile | Planned |
| `HealFromBelowThresholdRequirement` | Heal a unit that was below X% HP | `HealTargetEffect` | Additive / Maximum | Battle | Planned |
| `FullyHealAllyRequirement` | Bring an ally from below X HP/% back to full HP | `HealTargetEffect` | Maximum | Battle | Planned |
| `OverhealRequirement` | Attempt to heal X amount beyond missing HP | `HealTargetEffect` | Additive | Battle | Planned |
| `HealAfterTakingDamageRequirement` | Heal after the target took damage this turn | `HealTargetEffect` + turn state | Additive | Battle | Planned |
| `HealBeforeTakingDamageRequirement` | Heal a target before they take damage later in the same turn | Turn summary | Additive | Battle | Planned |
| `HealStatusedAllyRequirement` | Heal an ally affected by a specific status/tag | `HealTargetEffect` | Additive | Battle | Planned |
| `HealUnstatusedAllyRequirement` | Heal an ally with no statuses | `HealTargetEffect` | Additive | Battle | Planned |
| `HealAllAlliesRequirement` | Heal every ally at least once during battle | End phase | Maximum | Battle | Planned |
| `WinAfterHealingRequirement` | Win after healing at least X HP | End phase | Maximum | Battle | Planned |
| `HealWithoutDamagingRequirement` | Heal X HP in a battle where this creature deals no damage | End phase | Maximum | Battle | Planned |

---

# Shields

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `ApplyShieldRequirement` | Apply X total shield amount | `ApplyShieldEffect` | Additive | Battle | Implemented |
| `ApplySpecificShieldRequirement` | Apply X shield amount of a specific kind | `ApplyShieldEffect` | Additive | Battle | Implemented / Planned |
| `MaxSingleShieldRequirement` | Apply at least X shield in one effect | `ApplyShieldEffect` | Maximum | Battle | Planned |
| `ApplyShieldCountRequirement` | Apply shield effects X times | `ApplyShieldEffect` | Additive | Battle/Profile | Planned |
| `ApplyShieldToAllyRequirement` | Apply X shield amount to allies | `ApplyShieldEffect` | Additive | Battle | Planned |
| `ApplyShieldToSelfRequirement` | Apply X shield amount to self | `ApplyShieldEffect` | Additive | Battle | Planned |
| `ApplyShieldWhileLowHPRequirement` | Apply shield while below X% HP | `ApplyShieldEffect` | Additive | Battle | Planned |
| `AbsorbDamageWithShieldRequirement` | Absorb X total damage using shields | shield absorption in `BattleAttributes` | Additive | Battle/Profile | Implemented / Planned |
| `MaxShieldAbsorbOneHitRequirement` | Absorb X damage with shield in one hit | shield absorption | Maximum | Battle | Implemented / Planned |
| `ShieldBrokenRequirement` | Have X shields broken | shield absorption | Additive | Battle/Profile | Implemented / Planned |
| `BreakEnemyShieldRequirement` | Break X enemy shields | shield absorption / damage source attribution | Additive | Battle | Planned |
| `EndTurnWithShieldRequirement` | End a turn with at least X shield amount active | `BattleTurnRules.EndTurn` | Maximum | Battle | Planned |
| `WinWithShieldRemainingRequirement` | Win while still having shield active | End phase | Maximum | Battle | Planned |
| `PreventLethalDamageWithShieldRequirement` | Shield absorbs damage that would otherwise defeat the unit | shield absorption + HP check | Maximum | Battle | Planned |
| `MaintainShieldForTurnsRequirement` | Keep any shield active for X turns | Turn tracking | Maximum | Battle | Planned |

---

# Status Effects

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `ApplyStatusCountRequirement` | Apply any status X times | `ApplyStatusEffect` | Additive | Battle/Profile | Planned |
| `ApplySpecificStatusCountRequirement` | Apply a specific status X times | `ApplyStatusEffect` | Additive | Battle/Profile | Planned |
| `ApplyStatusWithTagRequirement` | Apply statuses with a specific tag X times | `ApplyStatusEffect` | Additive | Battle/Profile | Planned |
| `ApplyMultipleStatusesOneTurnRequirement` | Apply X statuses in one turn | Turn summary | Maximum | Battle | Planned |
| `ApplyMultipleStatusesOneCastRequirement` | Apply X statuses with one ability | Ability resolution summary | Maximum | Battle | Planned |
| `ApplyStatusToMultipleEnemiesRequirement` | Apply a status to X enemies in one cast/turn | `ApplyStatusEffect` | Maximum | Battle | Planned |
| `ConsumeStatusRequirement` | Consume X total stacks of statuses | `ConsumeStatus` | Additive | Battle/Profile | Planned |
| `ConsumeSpecificStatusRequirement` | Consume X stacks of a specific status | `ConsumeStatus` | Additive | Battle/Profile | Planned |
| `RemoveStatusStackRequirement` | Remove X total status stacks | `RemoveStatusEffect` / `CleanseEffect` | Additive | Battle/Profile | Planned |
| `CleanseStatusRequirement` | Cleanse X statuses or stacks | `CleanseEffect` | Additive | Battle/Profile | Planned |
| `CleanseAllyRequirement` | Cleanse statuses from allies X times | `CleanseEffect` | Additive | Battle | Planned |
| `CleanseSelfRequirement` | Cleanse self X times | `CleanseEffect` | Additive | Battle/Profile | Planned |
| `HaveStatusAtTurnStartRequirement` | Start turn with a specific status/tag | `BattleTurnRules.BeginTurn` | Additive / Maximum | Battle | Planned |
| `HaveNoStatusAtTurnStartRequirement` | Start turn with no active statuses | `BattleTurnRules.BeginTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnWithStatusRequirement` | End turn with a specific status/tag | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnWithNoStatusRequirement` | End turn with no active statuses | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `MaintainStatusForTurnsRequirement` | Keep a status active for X turns | Turn tracking | Maximum | Battle | Planned |
| `ReachStatusStackRequirement` | Reach X stacks of a specific status | `ApplyStatusEffect` / turn check | Maximum | Battle | Planned |
| `WinWithStatusRequirement` | Win while affected by a specific status/tag | End phase | Maximum | Battle | Planned |
| `WinWithoutStatusRequirement` | Win without ever receiving a specific status/tag | End phase | Maximum | Battle | Planned |

---

# Resource Usage

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `SpendActionPointsRequirement` | Spend X total AP | AP decrease event / ability resolution | Additive | Battle/Profile | Planned |
| `SpendMovementPointsRequirement` | Spend X total MP | MP decrease event / move resolution | Additive | Battle/Profile | Planned |
| `SpendAllActionPointsRequirement` | End a turn with 0 AP after spending AP | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `SpendAllMovementPointsRequirement` | End a turn with 0 MP after moving | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnWithNoResourcesRequirement` | End a turn with 0 AP and 0 MP | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnWithFullActionPointsRequirement` | End turn without spending AP | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnWithFullMovementRequirement` | End turn without spending MP | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `GainActionPointsRequirement` | Gain X AP from effects | `ResourceChangeEffect` / `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `GainMovementPointsRequirement` | Gain X MP from effects | `ResourceChangeEffect` / `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `ReduceEnemyActionPointsRequirement` | Reduce enemy AP by X | `ResourceChangeEffect` / `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `ReduceEnemyMovementPointsRequirement` | Reduce enemy MP by X | `ResourceChangeEffect` / `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `StealHealthRequirement` | Steal X health | `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `StealActionPointsRequirement` | Steal X AP | `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `StealMovementPointsRequirement` | Steal X MP | `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `StealRangeRequirement` | Steal X bonus range | `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `EndTurnWithExactResourceRequirement` | End turn with exactly X AP/MP | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `UseNoResourcesTurnRequirement` | Take a turn without spending AP or MP | `BattleTurnRules.EndTurn` | Additive | Battle | Planned |

---

# Movement

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `TotalDistanceTravelledRequirement` | Travel X total cells during battle | `BattleActionResolver.ResolveMove` | Additive | Battle/Profile | Planned |
| `MaxDistanceInOneTurnRequirement` | Move X cells in one turn | Turn summary | Maximum | Battle | Planned |
| `MaxDistanceInOneMoveRequirement` | Move X cells in one move action | `BattleActionResolver.ResolveMove` | Maximum | Battle | Planned |
| `MoveCountRequirement` | Perform X move actions | `BattleActionResolver.ResolveMove` | Additive | Battle/Profile | Planned |
| `MoveTowardEnemyRequirement` | End move closer to nearest enemy | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveAwayFromEnemyRequirement` | End move farther from nearest enemy | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveTowardAllyRequirement` | End move closer to nearest ally | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveAwayFromAllyRequirement` | End move farther from nearest ally | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveAdjacentToEnemyRequirement` | Move and end adjacent to an enemy | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveAdjacentToAllyRequirement` | Move and end adjacent to an ally | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveFromAdjacentToEnemyRequirement` | Start adjacent to enemy and move away | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveWithoutEndingNearEnemyRequirement` | Move and end at least X cells from every enemy | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `MoveThroughSpecificCellRequirement` | Move through a specific type/tagged cell | `BattleActionResolver.ResolveMove` | Additive | Battle | Planned |
| `VisitCellsRequirement` | Visit X different cells during battle | Move tracking | Set Count | Battle | Planned |
| `EndTurnOnSpecificCellRequirement` | End a turn on a specific board cell/tag/zone | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `NeverMoveRequirement` | Win without moving this creature | End phase | Maximum | Battle | Planned |
| `MoveEveryTurnRequirement` | Move at least once every turn | End phase | Maximum | Battle | Planned |

---

# Positioning / Formation

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `StartTurnSurroundedRequirement` | Start turn with X enemies within Y cells | `BattleTurnRules.BeginTurn` | Maximum | Battle | Planned |
| `EndTurnSurroundedRequirement` | End turn with X enemies within Y cells | `BattleTurnRules.EndTurn` | Maximum | Battle | Planned |
| `StartTurnNearMultipleAlliesRequirement` | Start turn near X allies | `BattleTurnRules.BeginTurn` | Maximum | Battle | Planned |
| `EndTurnNearMultipleAlliesRequirement` | End turn near X allies | `BattleTurnRules.EndTurn` | Maximum | Battle | Planned |
| `StartTurnIsolatedRequirement` | Start turn with no units within X cells | `BattleTurnRules.BeginTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnIsolatedRequirement` | End turn with no units within X cells | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `MaintainDistanceFromEnemyRequirement` | Never end a turn within X cells of an enemy | End phase | Maximum | Battle | Planned |
| `StayNearAllyRequirement` | End every turn within X cells of an ally | End phase | Maximum | Battle | Planned |
| `FlankEnemyRequirement` | End turn on opposite side of enemy relative to ally | `BattleTurnRules.EndTurn` | Additive | Battle | Planned |
| `LineUpWithEnemyRequirement` | End turn aligned with enemy on same row/column/diagonal | `BattleTurnRules.EndTurn` | Additive | Battle | Planned |

---

# Turn Bar / Initiative / Stamina

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `TakeTurnCountRequirement` | Take X turns during battle | `BattleTurnRules.BeginTurn` | Additive | Battle/Profile | Planned |
| `ActBeforeAnyEnemyRequirement` | Be the first unit to act | `BattleTurnRules.BeginTurn` | Maximum | Battle | Planned |
| `ActBeforeSpecificUnitRequirement` | Act before a specific enemy/ally | `BattleTurnRules.BeginTurn` | Maximum | Battle | Planned |
| `TakeConsecutiveTurnsRequirement` | Take X turns before a specific enemy acts | Turn tracking | Maximum | Battle | Planned |
| `DelayEnemyTurnBarRequirement` | Reduce/delay enemy turn bar by X | `AdjustTurnBarTimeEffect` | Additive | Battle/Profile | Planned |
| `AdvanceAllyTurnBarRequirement` | Advance ally turn bar by X | `AdjustTurnBarTimeEffect` | Additive | Battle/Profile | Planned |
| `StealTurnBarTimeRequirement` | Steal X turn-bar time | `StealResourceEffect` | Additive | Battle/Profile | Planned |
| `IncreaseTurnBarDurationRequirement` | Increase turn-bar duration by X | `AdjustTurnBarDurationEffect` | Additive | Battle/Profile | Planned |
| `DecreaseTurnBarDurationRequirement` | Decrease turn-bar duration by X | `AdjustTurnBarDurationEffect` | Additive | Battle/Profile | Planned |
| `EndTurnWithFullTurnBarRequirement` | End turn with turn bar full/ready | `BattleTurnRules.EndTurn` | Maximum | Battle | Planned |
| `ReachTurnBarReadyRequirement` | Become ready X times | initiative update | Additive | Battle/Profile | Planned |
| `PreventEnemyTurnRequirement` | Delay enemy so it misses/does not act before battle ends | End phase + turn tracking | Maximum | Battle | Planned |

---

# Displacement / Forced Movement

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `PushEnemyRequirement` | Force-move enemies X times | `MoveStatus` | Additive | Battle/Profile | Planned |
| `PushAllyRequirement` | Force-move allies X times | `MoveStatus` | Additive | Battle/Profile | Planned |
| `MaxForceMoveDistanceRequirement` | Force-move a unit at least X cells in one effect | `MoveStatus` | Maximum | Battle | Planned |
| `ForceMoveTotalDistanceRequirement` | Force-move units X total cells | `MoveStatus` | Additive | Battle/Profile | Planned |
| `PushEnemyTowardAllyRequirement` | Force-move enemy closer to an ally | `MoveStatus` | Additive | Battle | Planned |
| `PushEnemyAwayFromSelfRequirement` | Push enemy away from source | `MoveStatus` | Additive | Battle | Planned |
| `PushEnemyIntoRangeRequirement` | Push enemy into range of an ally/source | `MoveStatus` | Additive | Battle | Planned |
| `PushEnemyOutOfRangeRequirement` | Push enemy out of range of an ally/source | `MoveStatus` | Additive | Battle | Planned |
| `PushEnemyAdjacentToObjectRequirement` | Force-move enemy adjacent to an interactive object | `MoveStatus` | Additive | Battle | Planned |
| `SwapPositionRequirement` | Successfully swap positions X times | `SwapPositionEffect` | Additive | Battle/Profile | Planned |
| `SwapWithEnemyRequirement` | Swap position with enemy X times | `SwapPositionEffect` | Additive | Battle/Profile | Planned |
| `SwapWithAllyRequirement` | Swap position with ally X times | `SwapPositionEffect` | Additive | Battle/Profile | Planned |
| `TeleportRequirement` | Teleport X times | `TeleportEffect` | Additive | Battle/Profile | Planned |
| `TeleportDistanceRequirement` | Teleport X total cells | `TeleportEffect` | Additive | Battle/Profile | Planned |
| `MaxTeleportDistanceRequirement` | Teleport at least X cells in one effect | `TeleportEffect` | Maximum | Battle | Planned |
| `TeleportAdjacentToEnemyRequirement` | Teleport and end adjacent to an enemy | `TeleportEffect` | Additive | Battle | Planned |
| `TeleportAwayFromEnemyRequirement` | Teleport farther from nearest enemy | `TeleportEffect` | Additive | Battle | Planned |

---

# Interactive Objects

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `PlaceObjectCountRequirement` | Place X interactive objects | `PlaceInteractiveObjectEffect` | Additive | Battle/Profile | Planned |
| `PlaceSpecificObjectRequirement` | Place a specific object X times | `PlaceInteractiveObjectEffect` | Additive | Battle/Profile | Planned |
| `PlaceObjectWithTagRequirement` | Place objects with a specific tag X times | `PlaceInteractiveObjectEffect` | Additive | Battle/Profile | Planned |
| `RemoveObjectCountRequirement` | Remove X interactive objects | `RemoveInteractiveObjectEffect` | Additive | Battle/Profile | Planned |
| `RemoveSpecificObjectRequirement` | Remove a specific object X times | `RemoveInteractiveObjectEffect` | Additive | Battle/Profile | Planned |
| `RemoveObjectWithTagRequirement` | Remove objects with a specific tag X times | `RemoveInteractiveObjectEffect` | Additive | Battle/Profile | Planned |
| `PlaceObjectNearEnemyRequirement` | Place object within X cells of enemy | `PlaceInteractiveObjectEffect` | Additive | Battle | Planned |
| `PlaceObjectNearAllyRequirement` | Place object within X cells of ally | `PlaceInteractiveObjectEffect` | Additive | Battle | Planned |
| `PlaceObjectOnSpecificCellRequirement` | Place object on a specific cell/zone/tagged cell | `PlaceInteractiveObjectEffect` | Additive | Battle | Planned |
| `EndTurnNearObjectRequirement` | End turn within X cells of an object with a tag | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `ObjectSurvivesUntilBattleEndRequirement` | Place object that remains until victory | End phase | Maximum | Battle | Planned |
| `EnemyEndsNearObjectRequirement` | Enemy ends turn near your placed object | Enemy turn end | Additive | Battle | Planned |
| `RemoveEnemyObjectRequirement` | Remove object placed by enemy side | `RemoveInteractiveObjectEffect` | Additive | Battle | Planned |

---

# Revive / Defeat / Survival

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `ReviveAllyRequirement` | Revive allies X times | `ReviveEffect` | Additive | Battle/Profile | Planned |
| `ReviveSelfRequirement` | Revive self X times, if allowed | `ReviveEffect` | Additive | Battle/Profile | Planned |
| `ReviveWithLowHPRequirement` | Revive a target to X HP or less | `ReviveEffect` | Additive | Battle | Planned |
| `ReviveToFullHPRequirement` | Revive a target to full HP | `ReviveEffect` | Additive | Battle | Planned |
| `WinAfterReviveRequirement` | Win after this unit revived someone | End phase | Maximum | Battle | Planned |
| `WinAfterBeingRevivedRequirement` | Win after this unit was revived | End phase | Maximum | Battle | Planned |
| `DefeatedCountRequirement` | Be defeated X times across battles | `BattleContext.DefeatUnit` | Additive | Profile | Planned |
| `AvoidDefeatRequirement` | Win without this unit being defeated | End phase | Maximum | Battle/Profile | Planned |
| `LastUnitStandingRequirement` | Win while all other allies were defeated | End phase | Maximum | Battle | Planned |
| `SurviveAsLastAllyRequirement` | Spend X turns as the last living ally | Turn tracking | Maximum | Battle | Planned |

---

# HP Thresholds / Risk Conditions

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `StartTurnBelowHPThresholdRequirement` | Start turn below X% HP | `BattleTurnRules.BeginTurn` | Additive / Maximum | Battle | Planned |
| `StartTurnAboveHPThresholdRequirement` | Start turn above X% HP | `BattleTurnRules.BeginTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnBelowHPThresholdRequirement` | End turn below X% HP | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `EndTurnAboveHPThresholdRequirement` | End turn above X% HP | `BattleTurnRules.EndTurn` | Additive / Maximum | Battle | Planned |
| `ReachCriticalHPRequirement` | Reach X HP or fewer at any point | `DamageTargetEffect` | Maximum | Battle/Profile | Planned |
| `RecoverFromCriticalHPRequirement` | Go from below X% HP to above Y% HP | `HealTargetEffect` / turn tracking | Maximum | Battle | Planned |
| `StayAboveHPThresholdRequirement` | Win without dropping below X% HP | End phase | Maximum | Battle | Planned |
| `StayBelowHPThresholdRequirement` | Win while ending all turns below X% HP | End phase | Maximum | Battle | Planned |
| `FluctuateHPRequirement` | Drop below X% HP then recover above Y% HP | HP tracking | Maximum | Battle | Planned |
| `EndBattleAtExactHPRequirement` | Win with exactly X HP | End phase | Maximum | Battle | Planned |
| `EndBattleBelowHPRequirement` | Win with X HP or less | End phase | Maximum | Battle | Planned |
| `EndBattleAboveHPRequirement` | Win with at least X% HP | End phase | Maximum | Battle | Planned |

---

# Battle Outcome

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `WinBattleRequirement` | Win one battle with this creature present | End phase | Additive | Profile | Planned |
| `WinBattleCountRequirement` | Win X battles | End phase | Additive | Profile | Planned |
| `WinBattleWithFullHPRequirement` | Win with full HP | End phase | Maximum | Battle/Profile | Planned |
| `WinBattleWithoutTakingDamageRequirement` | Win without taking damage | End phase | Maximum | Battle/Profile | Planned |
| `WinBattleWithinXTurnsRequirement` | Win in X turns or fewer | End phase | Maximum | Battle | Planned |
| `WinBattleAfterXTurnsRequirement` | Win after at least X turns | End phase | Maximum | Battle | Planned |
| `AllAlliesSurviveRequirement` | Win with no ally defeated | End phase | Maximum | Battle | Planned |
| `OnlyThisUnitSurvivesRequirement` | Win with this unit as the only surviving ally | End phase | Maximum | Battle | Planned |
| `WinWithLowHPRequirement` | Win with X HP or fewer | End phase | Maximum | Battle/Profile | Planned |
| `WinWithoutHealingRequirement` | Win without receiving healing | End phase | Maximum | Battle/Profile | Planned |
| `WinWithoutShieldRequirement` | Win without receiving shields | End phase | Maximum | Battle/Profile | Planned |
| `WinWithoutMovingRequirement` | Win without this unit moving | End phase | Maximum | Battle/Profile | Planned |
| `WinWithoutCastingRequirement` | Win without this unit casting abilities | End phase | Maximum | Battle/Profile | Planned |
| `WinAfterTakingDamageRequirement` | Win after taking at least X damage | End phase | Maximum | Battle/Profile | Planned |
| `WinAfterDealingDamageRequirement` | Win after dealing at least X damage | End phase | Maximum | Battle/Profile | Planned |
| `WinAfterApplyingStatusRequirement` | Win after applying a specific status/tag | End phase | Maximum | Battle/Profile | Planned |
| `WinAgainstEnemyCountRequirement` | Win a battle with at least X enemies | End phase | Maximum | Battle | Planned |
| `WinWithTeamSizeRequirement` | Win with X or fewer allies | End phase | Maximum | Battle | Planned |

---

# Team / Ally Interaction

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `BuffAllyRequirement` | Apply a positive status/shield/resource gain to allies X times | Effect events | Additive | Battle/Profile | Planned |
| `DebuffEnemyRequirement` | Apply a negative status/resource loss to enemies X times | Effect events | Additive | Battle/Profile | Planned |
| `ProtectAllyRequirement` | Shield or heal an ally below X% HP | `ApplyShieldEffect` / `HealTargetEffect` | Additive | Battle/Profile | Planned |
| `SaveAllyFromDefeatRequirement` | Heal/shield ally that would otherwise die soon or was at critical HP | Heal/shield event + HP threshold | Maximum | Battle | Planned |
| `AssistKillRequirement` | Damage enemy that is later killed by an ally | `DamageTargetEffect` + `DefeatUnit` | Additive | Battle/Profile | Planned |
| `ReceiveAssistRequirement` | Kill enemy previously damaged/debuffed by ally | `DefeatUnit` | Additive | Battle/Profile | Planned |
| `ActAfterAllyRequirement` | Take a turn immediately after an ally | `BattleTurnRules.BeginTurn` | Additive | Battle | Planned |
| `ActAfterEnemyRequirement` | Take a turn immediately after an enemy | `BattleTurnRules.BeginTurn` | Additive | Battle | Planned |
| `ComboWithAllyRequirement` | Cast ability after an ally applied a specific status/effect | Ability cast + prior event tracking | Additive | Battle | Planned |

---

# Forms / Creature-Specific Conditions

Since the project has form tiers, these can be useful.

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `CastAbilityInFormRequirement` | Cast X abilities while in a specific form | `BattleActionResolver.ResolveAbility` | Additive | Battle/Profile | Planned |
| `DealDamageInFormRequirement` | Deal X damage while in a specific form | `DamageTargetEffect` | Additive | Battle/Profile | Planned |
| `TakeDamageInFormRequirement` | Take X damage while in a specific form | `DamageTargetEffect` | Additive | Battle/Profile | Planned |
| `WinBattleInFormRequirement` | Win X battles while in a specific form | End phase | Additive | Profile | Planned |
| `UseFormTierRequirement` | Complete a battle while at least form tier X | End phase | Maximum | Battle/Profile | Planned |
| `UnlockWhileInFormRequirement` | Complete another requirement while in a form | Requirement wrapper | Depends | Battle/Profile | Planned |
| `NeverChangeFormRequirement` | Win without changing form | End phase | Maximum | Battle | Planned |
| `ChangeFormCountRequirement` | Change form X times | Form-change event | Additive | Battle/Profile | Planned |

---

# Meta / Composite Requirements

These are not emitted by one event; they combine other requirements.

| Name | Description | Event emitted by | Accumulation | Scope | Status |
|---|---|---|---|---|---|
| `AndRequirement` | Complete all child requirements | Requirement evaluator | Composite | Battle/Profile | Planned |
| `OrRequirement` | Complete any child requirement | Requirement evaluator | Composite | Battle/Profile | Planned |
| `NotRequirement` | Complete if child requirement never occurs | Requirement evaluator | Composite | Battle/Profile | Planned |
| `SequenceRequirement` | Complete events in a specific order | Requirement evaluator | Sequential | Battle/Profile | Planned |
| `WithinTurnsRequirement` | Complete child requirement within X turns | Requirement evaluator | Wrapper | Battle | Planned |
| `AfterTurnRequirement` | Complete child requirement after turn X | Requirement evaluator | Wrapper | Battle | Planned |
| `BeforeTakingDamageRequirement` | Complete child before taking damage | Requirement evaluator | Wrapper | Battle | Planned |
| `WhileStatusedRequirement` | Complete child while source has a status/tag | Requirement evaluator | Wrapper | Battle/Profile | Planned |
| `WhileLowHPRequirement` | Complete child while below HP threshold | Requirement evaluator | Wrapper | Battle/Profile | Planned |
| `WithSpecificAbilityRequirement` | Complete child using a specific ability | Requirement evaluator | Wrapper | Battle/Profile | Planned |
| `WithSpecificTargetRequirement` | Complete child against ally/enemy/self/specific target | Requirement evaluator | Wrapper | Battle/Profile | Planned |

---

# Recommended Event Types to Add

To support most of the list without making every effect custom, add generic combat events.

## `AbilityCastEvent`

Emitted by `BattleActionResolver.ResolveAbility`.

```csharp
public sealed class AbilityCastEvent : FeatRequirement.EventBase
{
	public BattleUnit Caster;
	public Ability Ability;
	public IReadOnlyList<Vector3Int> TargetCells;
	public IReadOnlyList<BattleObject> Targets;
	public int TurnIndex;
	public int CastIndexThisTurn;
}
```

Supports:

- `CastAbilityCountRequirement`
- `CastSpecificAbilityCountRequirement`
- `CastMultipleAbilitiesInOneTurnRequirement`
- ability-specific lifetime mastery

---

## `MoveEvent`

Emitted by `BattleActionResolver.ResolveMove`.

```csharp
public sealed class MoveEvent : FeatRequirement.EventBase
{
	public BattleUnit Unit;
	public Vector3Int StartCell;
	public Vector3Int EndCell;
	public int Distance;
	public int TurnIndex;
}
```

Supports:

- total distance travelled
- max distance in one move
- move toward/away from enemy
- end adjacent to enemy/ally

---

## `TurnSummaryEvent`

Emitted by `BattleTurnRules.EndTurn`.

```csharp
public sealed class TurnSummaryEvent : FeatRequirement.EventBase
{
	public BattleUnit Unit;
	public int TurnIndex;

	public int AbilityCasts;
	public int MovementDistance;
	public int ActionPointsSpent;
	public int MovementPointsSpent;

	public int DamageDealt;
	public int DamageTaken;
	public int HealingDone;
	public int ShieldsApplied;
	public int StatusesApplied;

	public int RemainingActionPoints;
	public int RemainingMovementPoints;
}
```

This event is extremely useful because it supports many “in one turn” requirements without needing custom state in every requirement.

---

## `BattleSummaryEvent`

Emitted by end phase.

```csharp
public sealed class BattleSummaryEvent : FeatRequirement.EventBase
{
	public BattleUnit Unit;
	public bool WonBattle;
	public bool IsAliveAtEnd;

	public int TurnsTaken;
	public int DamageDealt;
	public int DamageTaken;
	public int HealingDone;
	public int Kills;
	public int DistanceMoved;
	public int AbilityCasts;

	public bool TookDamage;
	public bool WasDefeated;
	public bool WasRevived;
}
```

Supports:

- win requirements
- survive requirements
- no damage
- full HP win
- low HP win
- battle-scoped composite checks

---

## `EffectAppliedEvent`

Optional, but powerful. Emitted by every effect after it successfully changes something.

```csharp
public sealed class EffectAppliedEvent : FeatRequirement.EventBase
{
	public BattleUnit Source;
	public BattleUnit Target;
	public Ability Ability;

	public IAbilityEffect Effect;
	public string EffectTypeName;

	public int NumericValue;
	public Vector3Int SourceCell;
	public Vector3Int TargetCell;
}
```

Supports generic conditions like:

```text
Use any teleport effect X times.
Use any shield effect X times.
Use any displacement effect X times.
Use an effect with tag "Fire" X times.
```

---

# Best Additions to Implement First

## 1. Ability mastery

```text
CastSpecificAbilityLifetimeRequirement
CastSpecificAbilityCountRequirement
CastSpecificAbilityInOneTurnRequirement
```

This gives clean “use spell to upgrade spell” progression.

Examples:

```text
Cast Ember 25 times across fights to unlock Fireball.
Cast Fireball 3 times in one battle to unlock Flame Burst.
```

---

## 2. Turn summary requirements

```text
DealDamageInOneTurnRequirement
CastMultipleAbilitiesInOneTurnRequirement
SpendAllActionPointsRequirement
SpendAllMovementPointsRequirement
EndTurnWithNoResourcesRequirement
```

These create tactical goals instead of pure grinding.

---

## 3. Status-specific requirements

```text
ApplySpecificStatusCountRequirement
ConsumeSpecificStatusRequirement
HaveStatusAtTurnStartRequirement
CleanseStatusRequirement
```

This makes status builds feel distinct.

---

## 4. Shield requirements

```text
ApplyShieldRequirement
AbsorbDamageWithShieldRequirement
ShieldBrokenRequirement
PreventLethalDamageWithShieldRequirement
```

This gives defensive builds real progression.

---

## 5. Battle outcome requirements

```text
WinBattleCountRequirement
WinBattleWithoutTakingDamageRequirement
WinBattleWithFullHPRequirement
WinBattleWithinXTurnsRequirement
```

These are perfect for milestone/unlock nodes.

---

# Suggested Naming Convention

Use one class with configuration instead of many nearly identical classes.

Instead of:

```text
CastFireballRequirement
CastIceShardRequirement
CastHealRequirement
```

Use:

```csharp
CastAbilityCountRequirement
{
	Ability = fireball,
	RequiredCount = 10,
	Scope = RequirementScope.Profile
}
```

Similarly:

```csharp
ResourceChangedRequirement
{
	Resource = ResourceKind.ActionPoint,
	Direction = ResourceChangeDirection.Lost,
	TargetSide = RequirementTargetSide.Enemy,
	RequiredAmount = 5
}
```

This keeps the system scalable.
