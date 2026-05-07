public static class BattleOutcomeRules
{
	public static bool TryComputeWinner(BattleContext battleContext, out BattleSide winner)
	{
		winner = BattleSide.Neutral;
		if (battleContext == null)
		{
			return false;
		}

		bool hasPlayerUnits = battleContext.HasLivingUnits(BattleSide.Player);
		bool hasEnemyUnits = battleContext.HasLivingUnits(BattleSide.Enemy);

		if (hasPlayerUnits && !hasEnemyUnits)
		{
			winner = BattleSide.Player;
		}
		else if (hasEnemyUnits && !hasPlayerUnits)
		{
			winner = BattleSide.Enemy;
		}

		return true;
	}
}
