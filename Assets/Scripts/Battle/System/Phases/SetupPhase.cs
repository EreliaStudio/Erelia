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
		for (int index = 0; index < BattleContext.AllUnits.Count; index++)
		{
			BattleUnit unit = BattleContext.AllUnits[index];
			if (unit == null || unit.IsDefeated)
			{
				continue;
			}

			unit.BattleAttributes.TurnBar.SetCurrent(0f);
		}
	}
}
