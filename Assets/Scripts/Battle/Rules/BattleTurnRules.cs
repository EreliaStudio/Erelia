using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleTurnRules
{
	private const float TurnBarEpsilon = 0.0001f;

	public static void BeginTurn(BattleContext battleContext, BattleUnit activeUnit)
	{
		if (battleContext?.CurrentTurn == null || activeUnit == null)
		{
			return;
		}

		EmitTurnStartPositionEvent(battleContext, activeUnit);

		battleContext.CurrentTurn.Begin(activeUnit);
		activeUnit.BattleAttributes.TurnBar.SetCurrent(activeUnit.BattleAttributes.TurnBar.Max);

		BattleStatusRules.ApplyHook(CreateHookContext(
			battleContext,
			StatusHookPoint.TurnStart,
			activeUnit,
			activeUnit,
			activeUnit));
		BattleUnitRules.ResolvePendingDefeats(battleContext, activeUnit);
	}

	public static void EndTurn(BattleContext battleContext, BattleUnit activeUnit)
	{
		if (battleContext == null || activeUnit == null)
		{
			return;
		}

		BattleStatusRules.ApplyHook(CreateHookContext(
			battleContext,
			StatusHookPoint.TurnEnd,
			activeUnit,
			activeUnit,
			activeUnit));
		BattleStatusRules.AdvanceTurnDurations(activeUnit);
		activeUnit.BattleAttributes.AdvanceShieldDurations();
		battleContext.Board?.Runtime?.AdvanceObjectDurations();
		ResetTurnResources(activeUnit);
		activeUnit.BattleAttributes.TurnBar.SetCurrent(0f);
		BattleUnitRules.ResolvePendingDefeats(battleContext, activeUnit);

		EmitTurnEndPositionEvent(battleContext, activeUnit);
		EventCenter.EmitBattleTurnEnded(battleContext, activeUnit);
	}

	private static void ResetTurnResources(BattleUnit activeUnit)
	{
		if (activeUnit?.BattleAttributes == null)
		{
			return;
		}

		activeUnit.BattleAttributes.ActionPoints.SetCurrent(activeUnit.BattleAttributes.ActionPoints.Max);
		activeUnit.BattleAttributes.MovementPoints.SetCurrent(activeUnit.BattleAttributes.MovementPoints.Max);
	}

	public static bool CanContinueTurn(BattleContext battleContext, TurnContext turnContext)
	{
		BattleUnit activeUnit = turnContext?.ActiveUnit;
		if (battleContext == null || activeUnit == null || activeUnit.IsDefeated)
		{
			return false;
		}

		if (BattleActionValidator.GetReachableCells(battleContext, turnContext).Count > 0)
		{
			return true;
		}

		if (activeUnit.Abilities == null)
		{
			return false;
		}

		for (int index = 0; index < activeUnit.Abilities.Count; index++)
		{
			Ability ability = activeUnit.Abilities[index];
			if (!BattleActionValidator.CanUseAbility(battleContext, turnContext, ability))
			{
				continue;
			}

			if (BattleActionValidator.GetValidTargets(battleContext, turnContext, ability).Count > 0 ||
				BattleActionValidator.GetValidTargetCells(battleContext, turnContext, ability).Count > 0)
			{
				return true;
			}
		}

		return false;
	}

	public static void AdvanceTurnBars(BattleContext battleContext, float deltaTime)
	{
		if (battleContext == null || deltaTime <= 0f)
		{
			return;
		}

		AdvanceTurnBars(battleContext.PlayerUnits, deltaTime);
		AdvanceTurnBars(battleContext.EnemyUnits, deltaTime);
	}

	public static bool TrySelectNextReadyUnit(BattleContext battleContext, out BattleUnit unit)
	{
		unit = null;
		if (battleContext == null)
		{
			return false;
		}

		return TrySelectReadyUnit(battleContext, out unit);
	}

	public static bool TryFindNextActiveUnit(BattleContext battleContext, out BattleUnit unit)
	{
		unit = null;
		if (battleContext == null)
		{
			return false;
		}

		if (TrySelectReadyUnit(battleContext, out unit))
		{
			return true;
		}

		float elapsedTime = ComputeTimeUntilNextReadyUnit(battleContext);
		if (elapsedTime <= TurnBarEpsilon)
		{
			return false;
		}

		AdvanceTurnBars(battleContext.PlayerUnits, elapsedTime);
		AdvanceTurnBars(battleContext.EnemyUnits, elapsedTime);
		return TrySelectReadyUnit(battleContext, out unit);
	}

	private static bool TrySelectReadyUnit(BattleContext battleContext, out BattleUnit unit)
	{
		unit = null;
		if (battleContext == null)
		{
			return false;
		}

		List<BattleUnit> readyUnits = new List<BattleUnit>();
		CollectReadyUnits(battleContext.PlayerUnits, readyUnits);
		CollectReadyUnits(battleContext.EnemyUnits, readyUnits);

		if (readyUnits.Count == 0)
		{
			return false;
		}

		unit = ResolveTie(battleContext, readyUnits);
		return unit != null;
	}

	private static void CollectReadyUnits(IReadOnlyList<BattleUnit> source, List<BattleUnit> readyUnits)
	{
		for (int index = 0; index < source.Count; index++)
		{
			BattleUnit candidate = source[index];
			if (candidate != null && !candidate.IsDefeated && IsTurnReady(candidate))
			{
				readyUnits.Add(candidate);
			}
		}
	}

	private static BattleUnit ResolveTie(BattleContext battleContext, List<BattleUnit> readyUnits)
	{
		if (battleContext == null || readyUnits == null || readyUnits.Count == 0)
		{
			return null;
		}

		BattleUnit selectedPlayerUnit = FindFirstReadyUnit(battleContext.PlayerUnits, readyUnits);
		if (selectedPlayerUnit != null)
		{
			return selectedPlayerUnit;
		}

		return FindFirstReadyUnit(battleContext.EnemyUnits, readyUnits);
	}

	private static BattleUnit FindFirstReadyUnit(IReadOnlyList<BattleUnit> orderedUnits, List<BattleUnit> readyUnits)
	{
		if (orderedUnits == null || readyUnits == null)
		{
			return null;
		}

		for (int index = 0; index < orderedUnits.Count; index++)
		{
			BattleUnit candidate = orderedUnits[index];
			if (candidate != null && readyUnits.Contains(candidate))
			{
				return candidate;
			}
		}

		return null;
	}

	private static float ComputeTimeUntilNextReadyUnit(BattleContext battleContext)
	{
		float minimumRemainingTime = float.PositiveInfinity;
		minimumRemainingTime = ComputeMinTimeFromList(battleContext.PlayerUnits, minimumRemainingTime);
		minimumRemainingTime = ComputeMinTimeFromList(battleContext.EnemyUnits, minimumRemainingTime);
		return float.IsPositiveInfinity(minimumRemainingTime) ? 0f : minimumRemainingTime;
	}

	private static float ComputeMinTimeFromList(IReadOnlyList<BattleUnit> units, float currentMin)
	{
		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit == null || unit.IsDefeated)
			{
				continue;
			}

			float staminaRatio = unit.BattleAttributes.StaminaRatio.Value;
			if (staminaRatio <= 0f)
			{
				continue;
			}

			float remaining = Mathf.Max(0f, unit.BattleAttributes.TurnBar.Max - unit.BattleAttributes.TurnBar.Current);
			currentMin = Mathf.Min(currentMin, remaining / staminaRatio);
		}

		return currentMin;
	}

	private static void AdvanceTurnBars(IReadOnlyList<BattleUnit> units, float elapsedTime)
	{
		if (units == null || elapsedTime <= 0f)
		{
			return;
		}

		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit == null || unit.IsDefeated)
			{
				continue;
			}

			float staminaRatio = unit.BattleAttributes.StaminaRatio.Value;
			if (staminaRatio <= 0f)
			{
				continue;
			}

			unit.BattleAttributes.TurnBar.Increase(elapsedTime * staminaRatio);
		}
	}

	private static bool IsTurnReady(BattleUnit unit)
	{
		return unit != null &&
			unit.BattleAttributes != null &&
			unit.BattleAttributes.TurnBar.Current >= unit.BattleAttributes.TurnBar.Max - TurnBarEpsilon;
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

	private static void EmitTurnStartPositionEvent(BattleContext battleContext, BattleUnit activeUnit)
	{
		if (!activeUnit.HasBoardPosition ||
			!battleContext.Board.Runtime.TryGetPosition(activeUnit, out Vector3Int unitPos))
		{
			return;
		}

		ComputeClosestDistances(battleContext, activeUnit, unitPos,
			out int closestAlly, out int closestEnemy);

		if (closestAlly == int.MaxValue && closestEnemy == int.MaxValue)
		{
			return;
		}

		BattleEventReporter.Emit(new TurnStartedEvent
		{
			Caster = activeUnit,
			ClosestAllyDistance = closestAlly,
			ClosestEnemyDistance = closestEnemy
		});
	}

	private static void EmitTurnEndPositionEvent(BattleContext battleContext, BattleUnit activeUnit)
	{
		if (!activeUnit.HasBoardPosition ||
			!battleContext.Board.Runtime.TryGetPosition(activeUnit, out Vector3Int unitPos))
		{
			return;
		}

		ComputeClosestDistances(battleContext, activeUnit, unitPos,
			out int closestAlly, out int closestEnemy);

		if (closestAlly == int.MaxValue && closestEnemy == int.MaxValue)
		{
			return;
		}

		BattleEventReporter.Emit(new TurnEndedEvent
		{
			Caster = activeUnit,
			ClosestAllyDistance = closestAlly,
			ClosestEnemyDistance = closestEnemy
		});
	}

	private static void ComputeClosestDistances(
		BattleContext battleContext,
		BattleUnit activeUnit,
		Vector3Int unitPos,
		out int closestAlly,
		out int closestEnemy)
	{
		closestAlly = int.MaxValue;
		closestEnemy = int.MaxValue;

		ComputeClosestDistancesInList(battleContext.PlayerUnits, activeUnit, unitPos, battleContext, ref closestAlly, ref closestEnemy);
		ComputeClosestDistancesInList(battleContext.EnemyUnits, activeUnit, unitPos, battleContext, ref closestAlly, ref closestEnemy);
	}

	private static void ComputeClosestDistancesInList(
		IReadOnlyList<BattleUnit> p_list,
		BattleUnit activeUnit,
		Vector3Int unitPos,
		BattleContext battleContext,
		ref int closestAlly,
		ref int closestEnemy)
	{
		for (int index = 0; index < p_list.Count; index++)
		{
			BattleUnit other = p_list[index];
			if (other == null || other == activeUnit || other.IsDefeated || !other.HasBoardPosition)
			{
				continue;
			}

			if (!battleContext.Board.Runtime.TryGetPosition(other, out Vector3Int otherPos))
			{
				continue;
			}

			int dist = Math.Abs(unitPos.x - otherPos.x) + Math.Abs(unitPos.z - otherPos.z);
			if (other.Side == activeUnit.Side)
			{
				closestAlly = Math.Min(closestAlly, dist);
			}
			else
			{
				closestEnemy = Math.Min(closestEnemy, dist);
			}
		}
	}

}
