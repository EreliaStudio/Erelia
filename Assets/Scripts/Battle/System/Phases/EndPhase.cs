public sealed class EndPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.End;

	public override void Enter()
	{
		BattleOutcomeRules.TryComputeOutcome(BattleContext, out BattleOutcome outcome);
		EventCenter.EmitBattleEnded(outcome);
	}
}
