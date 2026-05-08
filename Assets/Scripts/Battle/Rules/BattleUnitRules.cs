using System.Collections.Generic;

internal static class BattleUnitRules
{
	internal static void ResolvePendingDefeats(
		BattleContext battleContext,
		BattleUnit sourceUnit = null,
		Ability sourceAbility = null)
	{
		if (battleContext == null)
		{
			return;
		}

		List<BattleResourceChangeResult> changes = battleContext.ConsumePendingResourceChanges();
		for (int index = 0; index < changes.Count; index++)
		{
			BattleResourceChangeResult change = changes[index];
			if (!change.CrossedToDefeated)
			{
				continue;
			}

			DefeatUnit(
				battleContext,
				change.Unit,
				change.CasterUnit ?? sourceUnit,
				sourceAbility);
		}
	}

	internal static bool DefeatUnit(
		BattleContext battleContext,
		BattleUnit unit,
		BattleUnit sourceUnit = null,
		Ability sourceAbility = null)
	{
		if (battleContext == null || unit == null || !unit.IsDefeated)
		{
			return false;
		}

		battleContext.DefeatUnit(unit);

		if (sourceUnit != null && unit != sourceUnit)
		{
			BattleEventReporter.Emit(new UnitDefeatedEvent
			{
				Caster = sourceUnit,
				Target = unit,
				SourceAbility = sourceAbility
			});
		}

		return true;
	}
}
