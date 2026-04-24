using UnityEngine;

public sealed class IdlePhaseController : BattlePhaseController
{
	[SerializeField] private GameObject idleHudRoot;
	[SerializeField] private CreatureTeamView playerTeamView;
	[SerializeField] private CreatureTeamView enemyTeamView;

	private IdlePhase idlePhase;

	public override BattlePhaseType PhaseType => BattlePhaseType.Idle;

	private void Update()
	{
		idlePhase?.Tick(Time.deltaTime);
	}

	public override void SetActive(bool isActive)
	{
		if (idleHudRoot != null)
		{
			idleHudRoot.SetActive(isActive);
		}

		base.SetActive(isActive);

		if (isActive)
		{
			ResolveIdlePhase();
			BindTeams();
		}
	}

	protected override void OnBind()
	{
		ResolveIdlePhase();
		BindTeams();
	}

	private void ResolveIdlePhase()
	{
		if (idlePhase != null)
		{
			return;
		}

		if (Orchestrator != null &&
			Orchestrator.TryGetPhase(BattlePhaseType.Idle, out IBattlePhase phase))
		{
			idlePhase = phase as IdlePhase;
		}
	}

	private void BindTeams()
	{
		if (BattleContext == null)
		{
			playerTeamView?.Bind(null);
			enemyTeamView?.Bind(null);
			return;
		}

		playerTeamView?.Bind(BattleContext.PlayerUnits);
		enemyTeamView?.Bind(BattleContext.EnemyUnits);
	}
}
