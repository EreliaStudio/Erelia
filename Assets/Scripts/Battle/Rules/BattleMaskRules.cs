using System.Collections.Generic;
using UnityEngine;

public static class BattleMaskRules
{
	public static IReadOnlyList<Vector3Int> GetMovementRangeCells(BattleContext battleContext, TurnContext turnContext)
	{
		return BattleActionValidator.GetReachableCells(battleContext, turnContext);
	}

	public static IReadOnlyList<Vector3Int> GetAttackRangeCells(BattleContext battleContext, BattleUnit sourceUnit, Ability ability)
	{
		if (sourceUnit == null || ability == null)
		{
			return System.Array.Empty<Vector3Int>();
		}

		int bonusRange = System.Math.Max(0, sourceUnit.BattleAttributes?.BonusRange.Value ?? 0);
		return GetAttackRangeCells(battleContext, sourceUnit.BoardPosition, ability.Range, bonusRange);
	}

	public static IReadOnlyList<Vector3Int> GetAttackRangeCells(
		BattleContext battleContext,
		Vector3Int sourceCell,
		Ability.RangeDefinition range,
		int bonusRange = 0)
	{
		return BattleRangeRules.GetCellsInRange(battleContext, sourceCell, range, bonusRange);
	}

	public static IReadOnlyList<Vector3Int> GetAreaOfEffectCells(BattleContext battleContext, Ability ability, Vector3Int anchorCell)
	{
		return BattleTargetingRules.GetAffectedCells(battleContext, ability, anchorCell);
	}

	public static void ApplyMask(BoardOverlayState overlayState, IReadOnlyList<Vector3Int> cells, VoxelMask mask, bool clearExisting = true)
	{
		if (overlayState == null)
		{
			return;
		}

		if (clearExisting)
		{
			overlayState.Clear(mask);
		}

		overlayState.ApplyMask(cells, mask);
	}

	public static void ClearPreviewMasks(BoardOverlayState overlayState)
	{
		ClearMask(overlayState, VoxelMask.AttackRange);
		ClearMask(overlayState, VoxelMask.MovementRange);
		ClearMask(overlayState, VoxelMask.AreaOfEffect);
	}

	public static void ClearMask(BoardOverlayState overlayState, VoxelMask mask)
	{
		overlayState?.Clear(mask);
	}
}
