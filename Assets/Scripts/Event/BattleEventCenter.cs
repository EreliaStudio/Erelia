using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class EventCenter
{
	public static event Action<BoardConfiguration, Vector3, IReadOnlyList<EncounterUnit>, PlacementStyle, bool> BattleLaunchRequested;
	public static event Action<BattleContext> BattleStarted;
	public static event Action<BattleEvent> BattleEventOccurred;
	public static event Action<BattleContext, BattleUnit> BattleAbilityResolved;
	public static event Action<BattleContext, BattleUnit> BattleTurnEnded;
	public static event Action<BattleContext, BattleUnit> BattleUnitRemovalRequested;
	public static event Action<BattleContext, BattleSide> BattleResolved;
	public static event Action<BattleActionCompositionContext> BattleActionCompositionStarted;
	public static event Action<BattleActionCompositionContext> BattleActionCompositionUpdated;
	public static event Action<BattleActionCompositionContext> BattleActionCompositionCanceled;
	public static event Action<BattleActionCompositionContext, BattleAction> BattleActionComposed;

	public static void EmitBattleLaunchRequested(
		BoardConfiguration p_boardConfiguration,
		Vector3 p_battleOriginWorldPosition,
		IReadOnlyList<EncounterUnit> p_enemyUnits,
		PlacementStyle p_placementStyle,
		bool p_allowsTaming)
	{
		BattleLaunchRequested?.Invoke(
			p_boardConfiguration,
			p_battleOriginWorldPosition,
			p_enemyUnits,
			p_placementStyle,
			p_allowsTaming);
	}

	public static void EmitBattleStarted(BattleContext p_battleContext)
	{
		BattleStarted?.Invoke(p_battleContext);
	}

	public static void EmitBattleEventOccurred(BattleEvent p_featEvent)
	{
		BattleEventOccurred?.Invoke(p_featEvent);
	}

	public static void EmitBattleAbilityResolved(BattleContext p_battleContext, BattleUnit p_sourceUnit)
	{
		BattleAbilityResolved?.Invoke(p_battleContext, p_sourceUnit);
	}

	public static void EmitBattleTurnEnded(BattleContext p_battleContext, BattleUnit p_unit)
	{
		BattleTurnEnded?.Invoke(p_battleContext, p_unit);
	}

	public static void EmitBattleUnitRemovalRequested(BattleContext p_battleContext, BattleUnit p_unit)
	{
		BattleUnitRemovalRequested?.Invoke(p_battleContext, p_unit);
	}

	public static void EmitBattleResolved(BattleContext p_battleContext, BattleSide p_winner)
	{
		BattleResolved?.Invoke(p_battleContext, p_winner);
	}

	public static void EmitBattleActionCompositionStarted(BattleActionCompositionContext p_context)
	{
		BattleActionCompositionStarted?.Invoke(p_context);
	}

	public static void EmitBattleActionCompositionUpdated(BattleActionCompositionContext p_context)
	{
		BattleActionCompositionUpdated?.Invoke(p_context);
	}

	public static void EmitBattleActionCompositionCanceled(BattleActionCompositionContext p_context)
	{
		BattleActionCompositionCanceled?.Invoke(p_context);
	}

	public static void EmitBattleActionComposed(BattleActionCompositionContext p_context, BattleAction p_action)
	{
		BattleActionComposed?.Invoke(p_context, p_action);
	}
}
