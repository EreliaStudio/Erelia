public sealed class ResolutionPhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.Resolution;

	public override void Enter()
	{
		BattleAction action = Orchestrator.ConsumePendingAction();
		if (action == null)
		{
			Coordinator.TransitionTo(Orchestrator.GetCurrentTurnPhaseType());
			return;
		}

		bool resolved = BattleActionResolver.Resolve(BattleContext, TurnContext, action);
		if (!resolved)
		{
			Coordinator.TransitionTo(Orchestrator.GetCurrentTurnPhaseType());
			return;
		}

		TransitionAfterResolution(action);
	}

	private void TransitionAfterResolution(BattleAction action)
	{
		if (BattleContext == null ||
			!BattleContext.HasLivingUnits(BattleSide.Player) ||
			!BattleContext.HasLivingUnits(BattleSide.Enemy))
		{
			TurnContext?.End();
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		if (action is not EndTurnAction)
		{
			Coordinator.TransitionTo(Orchestrator.GetCurrentTurnPhaseType());
			return;
		}
 
		TurnContext?.End();

		Coordinator.TransitionTo(BattlePhaseType.Idle);
	}
}
