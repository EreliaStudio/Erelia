public abstract class BattlePhase : IBattlePhase
{
	protected BattleOrchestrator Orchestrator { get; private set; }
	protected BattleMode BattleMode => Orchestrator?.BattleMode;
	protected BattleContext BattleContext => Orchestrator?.BattleContext;
	protected BattleCoordinator Coordinator => Orchestrator?.Coordinator;

	public abstract BattlePhaseType PhaseType { get; }

	public void Bind(BattleOrchestrator orchestrator)
	{
		Orchestrator = orchestrator;
		OnBind();
	}

	public virtual void Enter()
	{
	}

	public virtual void Exit()
	{
	}

	protected virtual void OnBind()
	{
	}
}
