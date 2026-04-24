using UnityEngine;

public sealed class PlacementPhaseController : BattlePhaseController
{
	[SerializeField] private GameObject placementHudRoot;
	[SerializeField] private CreatureTeamView playerTeamView;
	[SerializeField] private CreatureTeamView enemyTeamView;

	private PlacementPhase placementPhase;

	public override BattlePhaseType PhaseType => BattlePhaseType.Placement;

	public override void SetActive(bool isActive)
	{
		if (placementHudRoot != null)
		{
			placementHudRoot.SetActive(isActive);
		}

		base.SetActive(isActive);

		if (isActive)
		{
			BindTeams();
			SubscribePlayerCardClicks();
			return;
		}

		UnsubscribePlayerCardClicks();
	}

	protected override void OnBind()
	{
		ResolvePlacementPhase();
		BindTeams();
	}

	private void BindTeams()
	{
		if (BattleContext == null)
		{
			if (playerTeamView != null)
			{
				playerTeamView.Bind(null);
			}

			if (enemyTeamView != null)
			{
				enemyTeamView.Bind(null);
			}

			return;
		}

		playerTeamView?.Bind(BattleContext.PlayerUnits);
		enemyTeamView?.Bind(BattleContext.EnemyUnits);
	}

	private void SubscribePlayerCardClicks()
	{
		if (playerTeamView == null)
		{
			return;
		}

		UnsubscribePlayerCardClicks();

		int cardCount = playerTeamView.GetCardCount();
		for (int index = 0; index < cardCount; index++)
		{
			CreatureCardView card = playerTeamView.GetCard(index);
			card?.AddLeftClickListener(HandlePlayerCardLeftClicked);
		}
	}

	private void UnsubscribePlayerCardClicks()
	{
		if (playerTeamView == null)
		{
			return;
		}

		int cardCount = playerTeamView.GetCardCount();
		for (int index = 0; index < cardCount; index++)
		{
			CreatureCardView card = playerTeamView.GetCard(index);
			card?.RemoveLeftClickListener(HandlePlayerCardLeftClicked);
		}
	}

	private void HandlePlayerCardLeftClicked(BattleUnit unit)
	{
		ResolvePlacementPhase();
		placementPhase?.TrySelectPlayerUnit(unit);
	}

	private void ResolvePlacementPhase()
	{
		if (placementPhase != null)
		{
			return;
		}

		if (Orchestrator != null &&
			Orchestrator.TryGetPhase(BattlePhaseType.Placement, out IBattlePhase phase))
		{
			placementPhase = phase as PlacementPhase;
		}
	}
}
