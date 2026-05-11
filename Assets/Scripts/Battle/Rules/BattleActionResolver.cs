using System;
using System.Collections.Generic;
using UnityEngine;

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
		if (!BattleActionValidator.CanMove(battleContext, turnContext, action))
		{
			return false;
		}

		BattleStatusRules.ApplyHook(CreateHookContext(
			battleContext,
			StatusHookPoint.BeforeConsumingResources,
			action.SourceUnit,
			action.SourceUnit,
			action.SourceUnit,
			action));

		if (!battleContext.TryMoveUnit(action.SourceUnit, action.Destination))
		{
			return false;
		}

		BattleResourceChangeResult movementPointChange = BattleResourceRules.ChangeMovementPoints(
			battleContext,
			action.SourceUnit,
			action.SourceUnit,
			-action.MovementPointCost);
		BattleEventReporter.Emit(new DistanceTravelledEvent { Caster = action.SourceUnit, Distance = action.MovementPointCost });
		if (movementPointChange.LossAmount > 0)
		{
			BattleEventReporter.Emit(new ResourceConsumedEvent { Caster = action.SourceUnit, Resource = ResourceConsumedEvent.ResourceKind.MovementPoints, Amount = movementPointChange.LossAmount });
		}

		BattleStatusRules.ApplyHook(CreateHookContext(
			battleContext,
			StatusHookPoint.AfterConsumingResources,
			action.SourceUnit,
			action.SourceUnit,
			action.SourceUnit,
			action));
		BattleUnitRules.ResolvePendingDefeats(battleContext, action.SourceUnit);
		return true;
	}

	private static bool ResolveAbility(BattleContext battleContext, TurnContext turnContext, AbilityAction action)
	{
		if (!BattleActionValidator.CanUseAbilityAction(battleContext, turnContext, action))
		{
			return false;
		}

		if (!AreTargetsValid(battleContext, turnContext, action))
		{
			return false;
		}

		BattleStatusRules.ApplyHook(CreateHookContext(
			battleContext,
			StatusHookPoint.BeforeConsumingResources,
			action.SourceUnit,
			action.SourceUnit,
			action.SourceUnit,
			action));

		BattleResourceChangeResult actionPointChange = BattleResourceRules.ChangeActionPoints(
			battleContext,
			action.SourceUnit,
			action.SourceUnit,
			-action.ActionPointCost);
		BattleResourceChangeResult movementPointChange = BattleResourceRules.ChangeMovementPoints(
			battleContext,
			action.SourceUnit,
			action.SourceUnit,
			-action.MovementPointCost);

		if (actionPointChange.LossAmount > 0)
		{
			BattleEventReporter.Emit(new ResourceConsumedEvent { Caster = action.SourceUnit, Resource = ResourceConsumedEvent.ResourceKind.ActionPoints, Amount = actionPointChange.LossAmount });
		}

		if (movementPointChange.LossAmount > 0)
		{
			BattleEventReporter.Emit(new ResourceConsumedEvent { Caster = action.SourceUnit, Resource = ResourceConsumedEvent.ResourceKind.MovementPoints, Amount = movementPointChange.LossAmount });
		}

		ApplyAbilityEffects(action, battleContext);
		BattleUnitRules.ResolvePendingDefeats(battleContext, action.SourceUnit, action.Ability);

		BattleStatusRules.ApplyHook(CreateHookContext(
			battleContext,
			StatusHookPoint.AfterConsumingResources,
			action.SourceUnit,
			action.SourceUnit,
			action.SourceUnit,
			action));
		BattleUnitRules.ResolvePendingDefeats(battleContext, action.SourceUnit, action.Ability);

		turnContext.RecordAbilityCast(action.Ability);
		BattleEventReporter.Emit(new AbilityCastEvent
		{
			Caster = action.SourceUnit,
			SourceAbility = action.Ability,
			TargetDistance = ComputeNearestTargetDistance(action)
		});

		EventCenter.EmitBattleAbilityResolved(battleContext, action.SourceUnit);
		return true;
	}

	private static int ComputeNearestTargetDistance(AbilityAction action)
	{
		if (action?.SourceUnit == null ||
			!action.SourceUnit.HasBoardPosition ||
			action.TargetCells == null ||
			action.TargetCells.Count == 0)
		{
			return 0;
		}

		Vector3Int casterCell = action.SourceUnit.BoardPosition;
		int minimumDistance = int.MaxValue;

		for (int index = 0; index < action.TargetCells.Count; index++)
		{
			minimumDistance = Math.Min(minimumDistance, ManhattanDistance(casterCell, action.TargetCells[index]));
		}

		return minimumDistance == int.MaxValue ? 0 : minimumDistance;
	}

	private static int ManhattanDistance(Vector3Int from, Vector3Int to)
	{
		return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y) + Math.Abs(from.z - to.z);
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
			if (!BattleActionValidator.CanTargetCellWithAbility(battleContext, turnContext, action.Ability, action.TargetCells[index]))
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

	private static BattleHookContext CreateHookContext(
		BattleContext battleContext,
		StatusHookPoint hookPoint,
		BattleUnit hookOwner,
		BattleObject sourceObject,
		BattleObject targetObject,
		BattleAction action = null)
	{
		return new BattleHookContext
		{
			BattleContext = battleContext,
			HookPoint = hookPoint,
			HookOwner = hookOwner,
			SourceObject = sourceObject,
			TargetObject = targetObject,
			Action = action
		};
	}
}
