using System;

internal static class BattleResourceRules
{
	internal static BattleResourceChangeResult ChangeHealth(
		BattleContext battleContext,
		BattleUnit unit,
		BattleUnit casterUnit,
		int amount)
	{
		ObservableResource health = unit?.BattleAttributes?.Health;
		if (amount > 0 && health != null)
		{
			amount = Math.Min(amount, Math.Max(0, health.Max - health.Current));
		}

		return ChangeResource(
			battleContext,
			unit,
			casterUnit,
			amount,
			health,
			lossHookPoint: StatusHookPoint.OnHPLoss,
			gainHookPoint: StatusHookPoint.OnHPGain);
	}

	internal static BattleResourceChangeResult ChangeActionPoints(
		BattleContext battleContext,
		BattleUnit unit,
		BattleUnit casterUnit,
		int amount)
	{
		return ChangeResource(
			battleContext,
			unit,
			casterUnit,
			amount,
			unit?.BattleAttributes?.ActionPoints,
			lossHookPoint: StatusHookPoint.OnAPLoss,
			gainHookPoint: StatusHookPoint.OnAPGain);
	}

	internal static BattleResourceChangeResult ChangeMovementPoints(
		BattleContext battleContext,
		BattleUnit unit,
		BattleUnit casterUnit,
		int amount)
	{
		return ChangeResource(
			battleContext,
			unit,
			casterUnit,
			amount,
			unit?.BattleAttributes?.MovementPoints,
			lossHookPoint: StatusHookPoint.OnMPLoss,
			gainHookPoint: StatusHookPoint.OnMPGain);
	}

	private static BattleResourceChangeResult ChangeResource(
		BattleContext battleContext,
		BattleUnit unit,
		BattleUnit casterUnit,
		int amount,
		ObservableResource resource,
		StatusHookPoint lossHookPoint,
		StatusHookPoint gainHookPoint)
	{
		if (unit == null || resource == null || amount == 0)
		{
			return default;
		}

		int before = resource.Current;
		int target = before + amount;
		target = target < 0 ? 0 : target;
		resource.SetCurrentAllowOverflow(target);

		int appliedDelta = resource.Current - before;
		if (appliedDelta == 0)
		{
			return default;
		}

		bool crossedToDefeated = resource == unit.BattleAttributes.Health &&
			before > 0 &&
			resource.Current <= 0;

		BattleResourceChangeResult result = new BattleResourceChangeResult(
			unit,
			casterUnit,
			amount,
			appliedDelta,
			crossedToDefeated);
		battleContext?.RecordResourceChange(result);

		BattleStatusRules.ApplyHook(new BattleHookContext
		{
			BattleContext = battleContext,
			HookPoint = appliedDelta < 0 ? lossHookPoint : gainHookPoint,
			HookOwner = unit,
			SourceObject = casterUnit ?? unit,
			TargetObject = unit
		});

		return result;
	}
}
