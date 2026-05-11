using System.Collections.Generic;
using UnityEngine;

public sealed class BattleService
{
	private readonly GameContext gameContext;

	private BattleContext activeBattleContext;

	public BattleService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public BattleContext ActiveBattleContext => activeBattleContext;

	public void Initialize()
	{
		EventCenter.BattleLaunchRequested += OnBattleLaunchRequested;
		EventCenter.BattleStarted += OnBattleStarted;
		EventCenter.BattleUnitRemovalRequested += OnBattleUnitRemovalRequested;
	}

	public void Shutdown()
	{
		EventCenter.BattleLaunchRequested -= OnBattleLaunchRequested;
		EventCenter.BattleStarted -= OnBattleStarted;
		EventCenter.BattleUnitRemovalRequested -= OnBattleUnitRemovalRequested;
		activeBattleContext = null;
	}

	public void ResolveBattle(BattleContext p_battleContext, BattleSide p_winner)
	{
		if (p_battleContext == null || !ReferenceEquals(activeBattleContext, p_battleContext))
		{
			return;
		}

		EventCenter.EmitBattleResolved(p_battleContext, p_winner);
		activeBattleContext = null;
	}

	private void OnBattleStarted(BattleContext p_battleContext)
	{
		if (activeBattleContext == null)
		{
			activeBattleContext = p_battleContext;
		}
	}

	private void OnBattleLaunchRequested(
		BoardConfiguration p_boardConfiguration,
		Vector3 p_battleOriginWorldPosition,
		Vector3Int? p_playerReturnWorldCell,
		IReadOnlyList<EncounterUnit> p_enemyUnits,
		PlacementStyle p_placementStyle,
		bool p_allowsTaming)
	{
		if (p_boardConfiguration == null)
		{
			return;
		}

		if (ServiceLocator.Instance?.WorldService == null ||
			!ServiceLocator.Instance.WorldService.TryBuildBattleBoard(p_boardConfiguration, p_battleOriginWorldPosition, out BoardData boardData))
		{
			Logger.LogError("[BattleService] Could not build battle board for launch request.", Logger.Severity.Warning);
			return;
		}

		IReadOnlyList<CreatureUnit> playerTeam = ServiceLocator.Instance?.PlayerService?.GetActiveTeam()
			?? System.Array.Empty<CreatureUnit>();

		activeBattleContext = new BattleContext(
			playerTeam,
			p_enemyUnits,
			boardData,
			p_placementStyle,
			p_allowsTaming,
			p_playerReturnWorldCell ?? Vector3Int.FloorToInt(p_battleOriginWorldPosition));

		EventCenter.EmitBattleStarted(activeBattleContext);
	}

	private void OnBattleUnitRemovalRequested(BattleContext p_battleContext, BattleUnit p_unit)
	{
		if (!ReferenceEquals(p_battleContext, activeBattleContext) || p_unit == null)
		{
			return;
		}

		activeBattleContext.RemoveUnit(p_unit);
	}
}
