using System;

public sealed class BattleOrchestrator
{
	public BattlePhase CurrentPhase { get; private set; }

	public void Tick(float p_deltaTime)
	{
		CurrentPhase?.Tick(p_deltaTime);
	}

	public void TransitionTo(BattlePhase p_nextPhase)
	{
		if (p_nextPhase == null)
		{
			throw new ArgumentNullException(nameof(p_nextPhase));
		}

		if (ReferenceEquals(CurrentPhase, p_nextPhase))
		{
			return;
		}

		CurrentPhase?.Exit();
		CurrentPhase = p_nextPhase;
		CurrentPhase.Enter();
	}
}
