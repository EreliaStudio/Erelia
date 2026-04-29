public sealed class IdlePhase : BattlePhase
{
	public override BattlePhaseType PhaseType => BattlePhaseType.Idle;

	public override void Enter()
	{
		if (!HasAnyLivingUnit())
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		if (BattleTurnRules.TryFindNextActiveUnit(BattleContext, out BattleUnit readyUnit))
		{
			BeginTurnFor(readyUnit);
		}
	}

	public void Tick(float deltaTime)
	{
		if (BattleContext == null)
		{
			return;
		}

		if (!HasAnyLivingUnit())
		{
			Coordinator.TransitionTo(BattlePhaseType.End);
			return;
		}

		BattleTurnRules.AdvanceTurnBars(BattleContext, deltaTime);

		if (BattleTurnRules.TrySelectNextReadyUnit(BattleContext, out BattleUnit readyUnit))
		{
			BeginTurnFor(readyUnit);
		}
	}

	private void BeginTurnFor(BattleUnit unit)
	{
		if (Orchestrator.TryBeginTurn(unit, out BattlePhaseType nextPhase))
		{
			Coordinator.TransitionTo(nextPhase);
			return;
		}

		Coordinator.TransitionTo(BattlePhaseType.End);
	}

	private bool HasAnyLivingUnit()
	{
		if (BattleContext?.AllUnits == null)
		{
			return false;
		}

		for (int index = 0; index < BattleContext.AllUnits.Count; index++)
		{
			BattleUnit unit = BattleContext.AllUnits[index];
			if (unit != null && !unit.IsDefeated)
			{
				return true;
			}
		}

		return false;
	}
}
