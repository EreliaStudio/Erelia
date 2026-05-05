public static class TamingProgressService
{
	public static void AwardWonBattleTamingRewards(PlayerData p_playerData, BattleOutcome p_outcome)
	{
		if (p_playerData == null || p_outcome == null || p_outcome.Winner != BattleSide.Player)
		{
			return;
		}

		for (int index = 0; index < p_outcome.ImpressedCreatures.Count; index++)
		{
			p_playerData.AddCreatureToTeamOrStorage(p_outcome.ImpressedCreatures[index]);
		}
	}
}
