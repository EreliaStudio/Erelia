using System;

public abstract class BattlePhase
{
	protected BattlePhase(BattleContext p_context)
	{
		Context = p_context ?? throw new ArgumentNullException(nameof(p_context));
	}

	protected BattleContext Context { get; }

	public abstract BattlePhaseId PhaseId { get; }

	public virtual void Enter()
	{
	}

	public virtual void Tick(float p_deltaTime)
	{
	}

	public virtual void Exit()
	{
	}
}
