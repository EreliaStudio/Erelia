using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleRangeRules
{
	public static bool IsCellInRange(BattleUnit source, Vector3Int cell, Ability ability)
	{
		if (source == null || ability == null || !source.HasBoardPosition)
		{
			return false;
		}

		int bonusRange = Math.Max(0, source.BattleAttributes?.BonusRange.Value ?? 0);
		return IsCellInRange(source.BoardPosition, cell, ability.Range, bonusRange);
	}

	public static bool IsCellInRange(Vector3Int sourceCell, Vector3Int cell, Ability.RangeDefinition range, int bonusRange = 0)
	{
		int rangeValue = Math.Max(0, range?.Value ?? 0) + Math.Max(0, bonusRange);
		Vector3Int delta = cell - sourceCell;
		int distanceX = Math.Abs(delta.x);
		int distanceZ = Math.Abs(delta.z);

		return (range?.Type ?? Ability.RangeDefinition.Shape.Circle) switch
		{
			Ability.RangeDefinition.Shape.Circle => distanceX + distanceZ <= rangeValue,
			Ability.RangeDefinition.Shape.Line => (distanceX == 0 || distanceZ == 0) && distanceX + distanceZ <= rangeValue,
			Ability.RangeDefinition.Shape.Diagonal => distanceX == distanceZ && distanceX <= rangeValue,
			_ => false
		};
	}

	public static IReadOnlyList<Vector3Int> GetCellsInRange(
		BattleContext battleContext,
		Vector3Int sourceCell,
		Ability.RangeDefinition range,
		int bonusRange = 0)
	{
		if (battleContext?.Board?.Navigation?.Nodes == null || !battleContext.Board.IsInside(sourceCell))
		{
			return Array.Empty<Vector3Int>();
		}

		List<Vector3Int> cells = new List<Vector3Int>();
		IReadOnlyList<VoxelTraversalGraph.Node> nodes = battleContext.Board.Navigation.Nodes;
		for (int index = 0; index < nodes.Count; index++)
		{
			VoxelTraversalGraph.Node node = nodes[index];
			if (node == null)
			{
				continue;
			}

			Vector3Int candidate = node.Position;
			if (!IsCellInRange(sourceCell, candidate, range, bonusRange))
			{
				continue;
			}

			if ((range?.RequireLineOfSight ?? false) &&
				!BattleLineOfSightRules.HasLineOfSight(battleContext, sourceCell, candidate))
			{
				continue;
			}

			cells.Add(candidate);
		}

		return cells;
	}
}
