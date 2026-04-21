using System;

public sealed class BattleCoordinator
{
	public event Action<BattlePhaseType> PhaseChanged;

	public BattlePhaseType CurrentPhaseType { get; private set; } = BattlePhaseType.Setup;
	public bool HasActivePhase { get; private set; }

	public void TransitionTo(BattlePhaseType phaseType)
	{
		CurrentPhaseType = phaseType;
		HasActivePhase = true;
		PhaseChanged?.Invoke(phaseType);
	}
}
