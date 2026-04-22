using UnityEngine;

public abstract class BattlePhaseController : MonoBehaviour
{
	protected BattleOrchestrator Orchestrator { get; private set; }
	protected BattleMode BattleMode => Orchestrator?.BattleMode;
	protected BattleContext BattleContext => Orchestrator?.BattleContext;
	protected TurnContext TurnContext => BattleContext?.CurrentTurn;
	protected BattleCoordinator Coordinator => Orchestrator?.Coordinator;

	public abstract BattlePhaseType PhaseType { get; }

	public void Bind(BattleOrchestrator orchestrator)
	{
		Orchestrator = orchestrator;
		OnBind();
	}

	public virtual void SetActive(bool isActive)
	{
		gameObject.SetActive(isActive);
	}

	protected virtual void OnBind()
	{
	}
}
