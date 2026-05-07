public sealed class EndPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.End;

	public override void Enter()
	{
		BattleOutcomeRules.TryComputeWinner(BattleContext, out BattleSide winner);
		BattleService battleService = ServiceLocator.Instance?.BattleService;
		if (battleService != null)
		{
			battleService.ResolveBattle(BattleContext, winner);
			return;
		}

		if (BattleContext != null)
		{
			EventCenter.EmitBattleResolved(BattleContext, winner);
		}
	}
}
