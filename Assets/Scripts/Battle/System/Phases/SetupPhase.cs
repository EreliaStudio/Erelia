public sealed class SetupPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.Setup;

	public override void Enter()
	{
		if (BattleContext == null)
		{
			return;
		}

		if (!BattleContext.HasLivingUnits(BattleSide.Player) || !BattleContext.HasLivingUnits(BattleSide.Enemy))
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		InitializeTurnBars();
		Coordinator.TransitionTo(BattlePhaseType.Placement);
	}

	private void InitializeTurnBars()
	{
		InitializeTurnBarsForList(BattleContext.PlayerUnits);
		InitializeTurnBarsForList(BattleContext.EnemyUnits);
	}

	private static void InitializeTurnBarsForList(System.Collections.Generic.IReadOnlyList<BattleUnit> units)
	{
		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit == null || unit.IsDefeated)
			{
				continue;
			}

			unit.BattleAttributes.TurnBar.SetCurrent(0f);
		}
	}
}
