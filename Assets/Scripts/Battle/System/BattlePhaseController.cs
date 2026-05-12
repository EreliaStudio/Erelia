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

	protected void HighlightActiveUnitCard(CreatureCardView.State activeState)
	{
		BattleUnit activeUnit = TurnContext?.ActiveUnit;
		HighlightActiveUnitCardInTeam(PlayerTeamView, BattleContext?.PlayerUnits, activeUnit, activeState);
		HighlightActiveUnitCardInTeam(EnemyTeamView, BattleContext?.EnemyUnits, activeUnit, activeState);
	}

	protected void RefreshCardAliveStates()
	{
		RefreshTeamAliveStates(PlayerTeamView, BattleContext?.PlayerUnits);
		RefreshTeamAliveStates(EnemyTeamView, BattleContext?.EnemyUnits);
	}

	private static void RefreshTeamAliveStates(CreatureTeamView teamView, System.Collections.Generic.IReadOnlyList<BattleUnit> units)
	{
		if (teamView == null) return;
		int cardCount = teamView.GetCardCount();
		for (int i = 0; i < cardCount; i++)
		{
			CreatureCardView card = teamView.GetCard(i);
			if (card == null) continue;
			BattleUnit unit = units != null && i < units.Count ? units[i] : null;
			if (unit == null) card.SetColorMode(CreatureCardView.State.Empty);
			else card.SetColorMode(unit.IsDefeated ? CreatureCardView.State.Defeated : CreatureCardView.State.Alive);
		}
	}

	private static void HighlightActiveUnitCardInTeam(CreatureTeamView teamView, System.Collections.Generic.IReadOnlyList<BattleUnit> units, BattleUnit activeUnit, CreatureCardView.State activeState)
	{
		if (teamView == null) return;
		int cardCount = teamView.GetCardCount();
		for (int i = 0; i < cardCount; i++)
		{
			CreatureCardView card = teamView.GetCard(i);
			if (card == null) continue;
			BattleUnit unit = units != null && i < units.Count ? units[i] : null;
			card.SetColorMode(GetTurnCardState(unit, activeUnit, activeState));
		}
	}

	private static CreatureCardView.State GetTurnCardState(BattleUnit unit, BattleUnit activeUnit, CreatureCardView.State activeState)
	{
		if (unit == null) return CreatureCardView.State.Empty;
		if (ReferenceEquals(unit, activeUnit)) return activeState;
		return unit.IsDefeated ? CreatureCardView.State.Defeated : CreatureCardView.State.Alive;
	}

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
