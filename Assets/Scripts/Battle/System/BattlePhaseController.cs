using UnityEngine;

public abstract class BattlePhaseController : MonoBehaviour
{
	[SerializeField] private GameObject phaseHud;

	protected BattleOrchestrator Orchestrator { get; private set; }
	protected BattleMode BattleMode => Orchestrator?.BattleMode;
	protected BattleContext BattleContext => Orchestrator?.BattleContext;
	protected TurnContext TurnContext => BattleContext?.CurrentTurn;
	protected BattleCoordinator Coordinator => Orchestrator?.Coordinator;
	protected CreatureTeamView PlayerTeamView => Orchestrator?.PlayerTeamView;
	protected CreatureTeamView EnemyTeamView => Orchestrator?.EnemyTeamView;
	protected bool IsPhaseActive { get; private set; }

	public abstract BattlePhaseType PhaseType { get; }

	protected virtual void Awake()
	{
		OnAwake();
	}

	protected virtual void Start()
	{
		OnStart();
	}

	public void Bind(BattleOrchestrator orchestrator)
	{
		Orchestrator = orchestrator;
		OnBind();
	}

	public void Activate()
	{
		IsPhaseActive = true;
		gameObject.SetActive(true);
		if (phaseHud != null) phaseHud.SetActive(true);
		BindTeams();
		OnActivate();
	}

	public void Deactivate()
	{
		if (!IsPhaseActive && !gameObject.activeSelf)
		{
			return;
		}

		IsPhaseActive = false;
		OnDeactivate();
		if (phaseHud != null) phaseHud.SetActive(false);
		gameObject.SetActive(false);
	}

	protected virtual void OnAwake() { }
	protected virtual void OnStart() { }
	protected virtual void OnBind() { }
	protected virtual void OnActivate() { }
	protected virtual void OnDeactivate() { }

	private void BindTeams()
	{
		BattleContext context = BattleContext;

		if (PlayerTeamView != null)
		{
			PlayerTeamView.Bind(context?.PlayerUnits);
		}

		if (EnemyTeamView != null)
		{
			EnemyTeamView.Bind(context?.EnemyUnits);
		}
	}
}
