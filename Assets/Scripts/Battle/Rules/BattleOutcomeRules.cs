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
			CollectLivingUnits(battleContext.PlayerUnits),
			CollectLivingUnits(battleContext.EnemyUnits),
			battleContext.Stats);

		return true;
	}

	private static List<BattleUnit> CollectLivingUnits(IReadOnlyList<BattleUnit> units)
	{
		List<BattleUnit> living = new List<BattleUnit>();
		if (units == null)
		{
			return living;
		}

		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit != null && !unit.IsDefeated)
			{
				living.Add(unit);
			}
		}

		return living;
	}
}
