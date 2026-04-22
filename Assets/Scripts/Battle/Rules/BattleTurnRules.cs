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

		TrackedResourceDelta before = Track(activeUnit);
		battleContext.CurrentTurn.Begin(activeUnit);
		activeUnit.BattleAttributes.ActionPoints.Reset();
		activeUnit.BattleAttributes.MovementPoints.Reset();
		activeUnit.BattleAttributes.TurnBar.SetCurrent(activeUnit.BattleAttributes.TurnBar.Max);

		BattleStatusRules.ApplyHook(activeUnit, battleContext, StatusHookPoint.TurnStart, activeUnit);
		EmitLossHooks(battleContext, activeUnit, before);
	}

	public static void EndTurn(BattleContext battleContext, BattleUnit activeUnit)
	{
		if (battleContext == null || activeUnit == null)
		{
			return;
		}

		TrackedResourceDelta before = Track(activeUnit);
		BattleStatusRules.ApplyHook(activeUnit, battleContext, StatusHookPoint.TurnEnd, activeUnit);
		BattleStatusRules.AdvanceTurnDurations(activeUnit);
		battleContext.Board?.Runtime?.AdvanceObjectDurations();
		activeUnit.BattleAttributes.TurnBar.SetCurrent(0f);
		EmitLossHooks(battleContext, activeUnit, before);
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

	public static bool TryFindNextActiveUnit(BattleContext battleContext, out BattleUnit unit)
	{
		unit = null;
		if (battleContext == null)
		{
			return false;
		}

		List<BattleUnit> livingUnits = GetLivingUnits(battleContext);
		if (livingUnits.Count == 0)
		{
			return false;
		}

		if (TrySelectReadyUnit(battleContext, livingUnits, out unit))
		{
			return true;
		}

		float elapsedTime = ComputeTimeUntilNextReadyUnit(livingUnits);
		if (elapsedTime <= TurnBarEpsilon)
		{
			return false;
		}

		AdvanceTurnBars(livingUnits, elapsedTime);
		return TrySelectReadyUnit(battleContext, livingUnits, out unit);
	}

	private static List<BattleUnit> GetLivingUnits(BattleContext battleContext)
	{
		List<BattleUnit> units = new List<BattleUnit>();
		if (battleContext?.AllUnits == null)
		{
			return units;
		}

		for (int index = 0; index < battleContext.AllUnits.Count; index++)
		{
			BattleUnit candidate = battleContext.AllUnits[index];
			if (candidate == null || candidate.IsDefeated)
			{
				continue;
			}

			units.Add(candidate);
		}

		return units;
	}

	private static bool TrySelectReadyUnit(BattleContext battleContext, List<BattleUnit> candidates, out BattleUnit unit)
	{
		unit = null;
		if (battleContext == null || candidates == null)
		{
			return false;
		}

		List<BattleUnit> readyUnits = new List<BattleUnit>();
		for (int index = 0; index < candidates.Count; index++)
		{
			BattleUnit candidate = candidates[index];
			if (candidate != null && IsTurnReady(candidate))
			{
				readyUnits.Add(candidate);
			}
		}

		if (readyUnits.Count == 0)
		{
			return false;
		}

		unit = ResolveTie(battleContext, readyUnits);
		return unit != null;
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

	private static float ComputeTimeUntilNextReadyUnit(List<BattleUnit> units)
	{
		float minimumRemainingTime = float.PositiveInfinity;
		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit == null)
			{
				continue;
			}

			float remainingTime = Mathf.Max(0f, unit.BattleAttributes.TurnBar.Max - unit.BattleAttributes.TurnBar.Current);
			minimumRemainingTime = Mathf.Min(minimumRemainingTime, remainingTime);
		}

		return float.IsPositiveInfinity(minimumRemainingTime) ? 0f : minimumRemainingTime;
	}

	private static void AdvanceTurnBars(List<BattleUnit> units, float elapsedTime)
	{
		if (units == null || elapsedTime <= 0f)
		{
			return;
		}

		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit == null)
			{
				continue;
			}

			unit.BattleAttributes.TurnBar.Increase(elapsedTime);
		}
	}

	private static bool IsTurnReady(BattleUnit unit)
	{
		return unit != null &&
			unit.BattleAttributes != null &&
			unit.BattleAttributes.TurnBar.Current >= unit.BattleAttributes.TurnBar.Max - TurnBarEpsilon;
	}

	private static TrackedResourceDelta Track(BattleUnit unit)
	{
		return new TrackedResourceDelta(
			unit?.BattleAttributes.Health.Current ?? 0,
			unit?.BattleAttributes.ActionPoints.Current ?? 0,
			unit?.BattleAttributes.MovementPoints.Current ?? 0);
	}

	private static void EmitLossHooks(BattleContext battleContext, BattleUnit unit, TrackedResourceDelta before)
	{
		if (unit == null)
		{
			return;
		}

		if (unit.BattleAttributes.Health.Current < before.Health)
		{
			BattleStatusRules.ApplyHook(unit, battleContext, StatusHookPoint.OnHPLoss, unit);
		}

		if (unit.BattleAttributes.ActionPoints.Current < before.ActionPoints)
		{
			BattleStatusRules.ApplyHook(unit, battleContext, StatusHookPoint.OnAPLoss, unit);
		}

		if (unit.BattleAttributes.MovementPoints.Current < before.MovementPoints)
		{
			BattleStatusRules.ApplyHook(unit, battleContext, StatusHookPoint.OnMPLoss, unit);
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
