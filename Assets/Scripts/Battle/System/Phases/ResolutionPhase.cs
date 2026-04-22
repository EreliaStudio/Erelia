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

		BattleActionResolver.Resolve(BattleContext, TurnContext, action);
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

		if (action is not EndTurnAction && Orchestrator.CanContinueActiveTurn())
		{
			Coordinator.TransitionTo(Orchestrator.GetCurrentTurnPhaseType());
			return;
		}

		if (action is not EndTurnAction)
		{
			BattleTurnRules.EndTurn(BattleContext, action.SourceUnit);
		}

		TurnContext?.End();

		if (Orchestrator.TryBeginNextTurn(out BattlePhaseType nextPhase))
		{
			Coordinator.TransitionTo(nextPhase);
			return;
		}

		Coordinator.TransitionTo(BattlePhaseType.End);
	}
}
