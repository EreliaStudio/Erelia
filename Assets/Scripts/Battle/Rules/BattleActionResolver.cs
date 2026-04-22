using System;
using System.Collections.Generic;

public static class BattleActionResolver
{
	public static bool Resolve(BattleContext battleContext, TurnContext turnContext, BattleAction action)
	{
		if (battleContext == null || turnContext == null || action?.SourceUnit == null)
		{
			return false;
		}

		return action switch
		{
			MoveAction moveAction => ResolveMove(battleContext, turnContext, moveAction),
			AbilityAction abilityAction => ResolveAbility(battleContext, turnContext, abilityAction),
			EndTurnAction endTurnAction => ResolveEndTurn(battleContext, endTurnAction),
			_ => false
		};
	}

	private static bool ResolveMove(BattleContext battleContext, TurnContext turnContext, MoveAction action)
	{
		if (!BattleActionValidator.TryGetMovementCost(battleContext, turnContext, action.Destination, out int movementCost))
		{
			return false;
		}

		BattleStatusRules.ApplyHook(action.SourceUnit, battleContext, StatusHookPoint.BeforeMove, action.SourceUnit);

		if (!battleContext.TryMoveUnit(action.SourceUnit, action.Destination))
		{
			return false;
		}

		TrackedResourceDelta sourceDelta = Track(action.SourceUnit);
		action.SourceUnit.BattleAttributes.MovementPoints.Decrease(movementCost);
		EmitLossHooks(battleContext, action.SourceUnit, sourceDelta);
		battleContext.Stats.RecordMove(action.SourceUnit);

		BattleStatusRules.ApplyHook(action.SourceUnit, battleContext, StatusHookPoint.AfterMove, action.SourceUnit);
		return true;
	}

	private static bool ResolveAbility(BattleContext battleContext, TurnContext turnContext, AbilityAction action)
	{
		if (!BattleActionValidator.CanUseAbility(battleContext, turnContext, action.Ability))
		{
			return false;
		}

		if (!AreTargetsValid(battleContext, turnContext, action))
		{
			return false;
		}

		List<BattleUnit> trackedUnits = BuildTrackedUnits(battleContext, action);
		Dictionary<BattleUnit, TrackedResourceDelta> deltasByUnit = SnapshotTrackedUnits(trackedUnits);

		BattleStatusRules.ApplyHook(action.SourceUnit, battleContext, StatusHookPoint.BeforeCastingAnAbility, action.SourceUnit);

		int actionPointCost = Math.Max(0, action.Ability.Cost?.Ability ?? 0);
		int movementPointCost = Math.Max(0, action.Ability.Cost?.Movement ?? 0);
		action.SourceUnit.BattleAttributes.ActionPoints.Decrease(actionPointCost);
		action.SourceUnit.BattleAttributes.MovementPoints.Decrease(movementPointCost);

		if (action.TargetCells == null || action.TargetCells.Count == 0)
		{
			return false;
		}

		ApplyAbilityEffects(action, battleContext);

		BattleStatusRules.ApplyHook(action.SourceUnit, battleContext, StatusHookPoint.AfterCastingAnAbility, action.SourceUnit);

		for (int index = 0; index < trackedUnits.Count; index++)
		{
			BattleUnit unit = trackedUnits[index];
			if (unit == null || !deltasByUnit.TryGetValue(unit, out TrackedResourceDelta delta))
			{
				continue;
			}

			EmitLossHooks(battleContext, unit, delta, action.SourceUnit);
			RecordStatDeltas(battleContext, action.SourceUnit, unit, delta);
		}

		battleContext.Stats.RecordAbilityCast(action.SourceUnit);
		ProcessDefeatedUnits(battleContext, trackedUnits, deltasByUnit);
		return true;
	}

	private static bool ResolveEndTurn(BattleContext battleContext, EndTurnAction action)
	{
		if (battleContext == null || action?.SourceUnit == null)
		{
			return false;
		}

		BattleTurnRules.EndTurn(battleContext, action.SourceUnit);
		return true;
	}

	private static bool AreTargetsValid(BattleContext battleContext, TurnContext turnContext, AbilityAction action)
	{
		if (action.TargetCells == null || action.TargetCells.Count == 0)
		{
			return false;
		}

		for (int index = 0; index < action.TargetCells.Count; index++)
		{
			if (!BattleActionValidator.CanTargetCell(battleContext, turnContext, action.Ability, action.TargetCells[index]))
			{
				return false;
			}
		}

		return true;
	}

	private static void ApplyAbilityEffects(AbilityAction action, BattleContext battleContext)
	{
		if (action?.Ability?.Effects == null)
		{
			return;
		}

		for (int anchorIndex = 0; anchorIndex < action.TargetCells.Count; anchorIndex++)
		{
			Vector3Int anchorCell = action.TargetCells[anchorIndex];
			IReadOnlyList<Vector3Int> affectedCells = BattleTargetingRules.GetAffectedCells(battleContext, action.Ability, action.TargetCells[anchorIndex]);
			for (int cellIndex = 0; cellIndex < affectedCells.Count; cellIndex++)
			{
				Vector3Int affectedCell = affectedCells[cellIndex];
				IReadOnlyList<BattleObject> objectsAtCell = BattleTargetingRules.GetObjectsAtCell(battleContext, affectedCell);

				if (objectsAtCell.Count == 0)
				{
					BattleAbilityExecutionContext emptyCellContext = CreateExecutionContext(action, battleContext, anchorCell, affectedCell, null);
					for (int effectIndex = 0; effectIndex < action.Ability.Effects.Count; effectIndex++)
					{
						action.Ability.Effects[effectIndex]?.Apply(emptyCellContext);
					}
					continue;
				}

				for (int objectIndex = 0; objectIndex < objectsAtCell.Count; objectIndex++)
				{
					BattleAbilityExecutionContext context = CreateExecutionContext(action, battleContext, anchorCell, affectedCell, objectsAtCell[objectIndex]);
					for (int effectIndex = 0; effectIndex < action.Ability.Effects.Count; effectIndex++)
					{
						action.Ability.Effects[effectIndex]?.Apply(context);
					}
				}
			}
		}
	}

	private static BattleAbilityExecutionContext CreateExecutionContext(
		AbilityAction action,
		BattleContext battleContext,
		Vector3Int anchorCell,
		Vector3Int affectedCell,
		BattleObject targetObject)
	{
		return new BattleAbilityExecutionContext
		{
			BattleContext = battleContext,
			Ability = action?.Ability,
			SourceObject = action?.SourceUnit,
			TargetObject = targetObject,
			AnchorCell = anchorCell,
			AffectedCell = affectedCell
		};
	}

	private static void ApplyEffectList(IReadOnlyList<Effect> effects, BattleAbilityExecutionContext context)
	{
		if (effects == null || context == null)
		{
			return;
		}

		for (int effectIndex = 0; effectIndex < effects.Count; effectIndex++)
		{
			effects[effectIndex]?.Apply(context);
		}
	}

	private static List<BattleUnit> BuildTrackedUnits(BattleContext battleContext, AbilityAction action)
	{
		List<BattleUnit> trackedUnits = new List<BattleUnit>();
		AddTrackedUnit(trackedUnits, action?.SourceUnit);

		if (battleContext == null || action?.TargetCells == null)
		{
			return trackedUnits;
		}

		for (int anchorIndex = 0; anchorIndex < action.TargetCells.Count; anchorIndex++)
		{
			IReadOnlyList<Vector3Int> affectedCells = BattleTargetingRules.GetAffectedCells(battleContext, action.Ability, action.TargetCells[anchorIndex]);
			for (int cellIndex = 0; cellIndex < affectedCells.Count; cellIndex++)
			{
				IReadOnlyList<BattleObject> objectsAtCell = BattleTargetingRules.GetObjectsAtCell(battleContext, affectedCells[cellIndex]);
				for (int objectIndex = 0; objectIndex < objectsAtCell.Count; objectIndex++)
				{
					AddTrackedUnit(trackedUnits, objectsAtCell[objectIndex] as BattleUnit);
				}
			}
		}

		return trackedUnits;
	}

	private static Dictionary<BattleUnit, TrackedResourceDelta> SnapshotTrackedUnits(List<BattleUnit> units)
	{
		Dictionary<BattleUnit, TrackedResourceDelta> snapshots = new Dictionary<BattleUnit, TrackedResourceDelta>();
		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit == null || snapshots.ContainsKey(unit))
			{
				continue;
			}

			snapshots[unit] = Track(unit);
		}

		return snapshots;
	}

	private static void AddTrackedUnit(List<BattleUnit> units, BattleUnit unit)
	{
		if (unit != null && !units.Contains(unit))
		{
			units.Add(unit);
		}
	}

	private static TrackedResourceDelta Track(BattleUnit unit)
	{
		return new TrackedResourceDelta(
			unit?.BattleAttributes.Health.Current ?? 0,
			unit?.BattleAttributes.ActionPoints.Current ?? 0,
			unit?.BattleAttributes.MovementPoints.Current ?? 0);
	}

	private static void EmitLossHooks(BattleContext battleContext, BattleUnit unit, TrackedResourceDelta before, BattleObject caster = null)
	{
		if (unit == null)
		{
			return;
		}

		if (unit.BattleAttributes.Health.Current < before.Health)
		{
			BattleStatusRules.ApplyHook(unit, battleContext, StatusHookPoint.OnHPLoss, caster);
		}

		if (unit.BattleAttributes.ActionPoints.Current < before.ActionPoints)
		{
			BattleStatusRules.ApplyHook(unit, battleContext, StatusHookPoint.OnAPLoss, caster);
		}

		if (unit.BattleAttributes.MovementPoints.Current < before.MovementPoints)
		{
			BattleStatusRules.ApplyHook(unit, battleContext, StatusHookPoint.OnMPLoss, caster);
		}
	}

	private static void ProcessDefeatedUnits(BattleContext battleContext, List<BattleUnit> trackedUnits, Dictionary<BattleUnit, TrackedResourceDelta> snapshotsBefore)
	{
		for (int index = 0; index < trackedUnits.Count; index++)
		{
			BattleUnit unit = trackedUnits[index];
			if (unit == null || !unit.IsDefeated)
			{
				continue;
			}

			if (snapshotsBefore.TryGetValue(unit, out TrackedResourceDelta before) && before.Health <= 0)
			{
				continue;
			}

			battleContext.DefeatUnit(unit);
		}
	}

	private static void RecordStatDeltas(BattleContext battleContext, BattleUnit source, BattleUnit target, TrackedResourceDelta before)
	{
		if (source == null || target == null)
		{
			return;
		}

		int healthDelta = before.Health - target.BattleAttributes.Health.Current;
		if (healthDelta > 0 && target.Side != source.Side)
		{
			battleContext.Stats.RecordDamageDealt(source, healthDelta);
		}
		else if (healthDelta < 0 && target.Side == source.Side)
		{
			battleContext.Stats.RecordHealingDone(source, -healthDelta);
		}
	}

	private readonly struct TrackedResourceDelta
	{
		public TrackedResourceDelta(int health, int actionPoints, int movementPoints)
		{
			Health = health;
			ActionPoints = actionPoints;
			MovementPoints = movementPoints;
		}

		public int Health { get; }
		public int ActionPoints { get; }
		public int MovementPoints { get; }
	}
}
