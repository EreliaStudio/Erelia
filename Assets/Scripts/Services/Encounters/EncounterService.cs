public sealed class EncounterService
{
	private readonly GameContext gameContext;

	public EncounterService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public void Initialize()
	{
		EventCenter.BattleResolved += OnBattleResolved;
	}

	public void Shutdown()
	{
		EventCenter.BattleResolved -= OnBattleResolved;
	}

	public void RequestBattle(
		BoardConfiguration p_boardConfiguration,
		UnityEngine.Vector3 p_battleOriginWorldPosition,
		System.Collections.Generic.IReadOnlyList<EncounterUnit> p_enemyUnits,
		PlacementStyle p_placementStyle,
		bool p_allowsTaming)
	{
		if (p_boardConfiguration == null)
		{
			return;
		}

		EventCenter.EmitBattleLaunchRequested(
			p_boardConfiguration,
			p_battleOriginWorldPosition,
			p_enemyUnits,
			p_placementStyle,
			p_allowsTaming);
	}

	private void OnBattleResolved(BattleContext p_battleContext, BattleSide p_winner)
	{
		if (p_battleContext == null)
		{
			return;
		}

		EventCenter.EmitEncounterResolved(p_battleContext, p_winner);
	}
}
