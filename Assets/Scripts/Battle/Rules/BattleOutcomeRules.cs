using System.Collections.Generic;

public static class BattleOutcomeRules
{
	public static bool TryComputeOutcome(BattleContext battleContext, out BattleOutcome outcome)
	{
		outcome = null;
		if (battleContext == null)
		{
			return false;
		}

		bool hasPlayerUnits = battleContext.HasLivingUnits(BattleSide.Player);
		bool hasEnemyUnits = battleContext.HasLivingUnits(BattleSide.Enemy);

		BattleSide winner;
		if (hasPlayerUnits && !hasEnemyUnits)
		{
			winner = BattleSide.Player;
		}
		else if (hasEnemyUnits && !hasPlayerUnits)
		{
			winner = BattleSide.Enemy;
		}
		else
		{
			winner = BattleSide.Neutral;
		}

		outcome = new BattleOutcome(
			winner,
			CollectActiveUnits(battleContext.PlayerUnits),
			CollectActiveUnits(battleContext.EnemyUnits),
			battleContext.Stats,
			CreateRecruits(battleContext.TamedUnits));

		return true;
	}

	private static List<CreatureUnit> CreateRecruits(IReadOnlyList<BattleUnit> p_tamedUnits)
	{
		List<CreatureUnit> recruits = new List<CreatureUnit>();

		if (p_tamedUnits == null)
		{
			return recruits;
		}

		for (int index = 0; index < p_tamedUnits.Count; index++)
		{
			CreatureUnit recruit = TamingRules.CreateRecruitFromImpressedUnit(p_tamedUnits[index]);
			if (recruit != null)
			{
				recruits.Add(recruit);
			}
		}

		return recruits;
	}

	private static List<BattleUnit> CollectActiveUnits(IReadOnlyList<BattleUnit> units)
	{
		List<BattleUnit> living = new List<BattleUnit>();
		if (units == null)
		{
			return living;
		}

		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit != null && unit.IsActiveInBattle)
			{
				living.Add(unit);
			}
		}

		return living;
	}
}