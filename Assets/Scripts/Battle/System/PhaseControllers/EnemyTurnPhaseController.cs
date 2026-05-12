public sealed class EnemyTurnPhaseController : BattlePhaseController
{
	public override BattlePhaseType PhaseType => BattlePhaseType.EnemyTurn;

	protected override void OnActivate()
	{
		HighlightActiveUnitCard(CreatureCardView.State.ActiveEnemy);
	}

	protected override void OnDeactivate()
	{
		RefreshCardAliveStates();
	}
}
