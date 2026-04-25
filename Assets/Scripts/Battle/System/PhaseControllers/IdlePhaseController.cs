using UnityEngine;

public sealed class IdlePhaseController : BattlePhaseController
{
	private IdlePhase idlePhase;

	public override BattlePhaseType PhaseType => BattlePhaseType.Idle;

	protected override void OnBind()
	{
		if (Orchestrator.TryGetPhase(BattlePhaseType.Idle, out IBattlePhase phase))
		{
			idlePhase = phase as IdlePhase;
		}
	}

	private void Update()
	{
		idlePhase?.Tick(Time.deltaTime);
	}
}
