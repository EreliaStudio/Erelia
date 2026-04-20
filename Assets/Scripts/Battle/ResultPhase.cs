using System;

public sealed class ResultPhase : BattlePhase
{
	public ResultPhase(BattleContext p_context) : base(p_context)
	{
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.Result;

	public event Action ResultAcknowledged;

	public override void Enter()
	{
		ResultAcknowledged?.Invoke();
	}
}
