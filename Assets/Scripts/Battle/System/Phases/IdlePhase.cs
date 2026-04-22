public sealed class IdlePhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.Idle;

	public override void Enter()
	{
		if (Orchestrator.TryBeginNextTurn(out BattlePhaseType nextPhase))
		{
			Coordinator.TransitionTo(nextPhase);
			return;
		}

		Coordinator.TransitionTo(BattlePhaseType.End);
	}
}
