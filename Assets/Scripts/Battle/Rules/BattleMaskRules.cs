using System.Collections.Generic;
using UnityEngine;

public static class BattleMaskRules
{
	public static IReadOnlyList<Vector3Int> ApplyMovementRangeMask(BattleContext battleContext, TurnContext turnContext)
	{
		ClearMask(battleContext, VoxelMask.MovementRange);
		IReadOnlyList<Vector3Int> cells = BattleActionValidator.GetReachableCells(battleContext, turnContext);
		ApplyMask(battleContext, cells, VoxelMask.MovementRange);
		return cells;
	}

	public static IReadOnlyList<Vector3Int> ApplyAttackRangeMask(BattleContext battleContext, BattleUnit sourceUnit, Ability ability)
	{
		if (sourceUnit == null || ability == null)
		{
			ClearMask(battleContext, VoxelMask.AttackRange);
			return System.Array.Empty<Vector3Int>();
		}

		int bonusRange = System.Math.Max(0, sourceUnit.BattleAttributes?.BonusRange.Value ?? 0);
		return ApplyAttackRangeMask(battleContext, sourceUnit.BoardPosition, ability.Range, bonusRange);
	}

	public static IReadOnlyList<Vector3Int> ApplyAttackRangeMask(
		BattleContext battleContext,
		Vector3Int sourceCell,
		Ability.RangeDefinition range,
		int bonusRange = 0)
	{
		ClearMask(battleContext, VoxelMask.AttackRange);
		IReadOnlyList<Vector3Int> cells = BattleRangeRules.GetCellsInRange(battleContext, sourceCell, range, bonusRange);
		ApplyMask(battleContext, cells, VoxelMask.AttackRange);
		return cells;
	}

	public static IReadOnlyList<Vector3Int> ApplyAreaOfEffectMask(BattleContext battleContext, Ability ability, Vector3Int anchorCell)
	{
		ClearMask(battleContext, VoxelMask.AreaOfEffect);
		IReadOnlyList<Vector3Int> cells = BattleTargetingRules.GetAffectedCells(battleContext, ability, anchorCell);
		ApplyMask(battleContext, cells, VoxelMask.AreaOfEffect);
		return cells;
	}

	public static IReadOnlyList<Vector3Int> ApplyPlacementMask(BoardData board, IReadOnlyList<Vector3Int> cells)
	{
		if (board == null)
		{
			return System.Array.Empty<Vector3Int>();
		}

		board.ClearMask(VoxelMask.Placement);
		board.ApplyMask(cells, VoxelMask.Placement);
		return cells ?? System.Array.Empty<Vector3Int>();
	}

	public static void ClearPreviewMasks(BattleContext battleContext)
	{
		ClearMask(battleContext, VoxelMask.AttackRange);
		ClearMask(battleContext, VoxelMask.MovementRange);
		ClearMask(battleContext, VoxelMask.AreaOfEffect);
	}

	public static void ClearMask(BattleContext battleContext, VoxelMask mask)
	{
		battleContext?.Board?.ClearMask(mask);
	}

	private static void ApplyMask(BattleContext battleContext, IReadOnlyList<Vector3Int> cells, VoxelMask mask)
	{
		battleContext?.Board?.ApplyMask(cells, mask);
	}
}
