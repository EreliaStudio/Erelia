using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattlePlacementRules
{
	public readonly struct PlacementZones
	{
		public PlacementZones(IReadOnlyList<Vector3Int> playerCells, IReadOnlyList<Vector3Int> enemyCells)
		{
			PlayerCells = playerCells ?? Array.Empty<Vector3Int>();
			EnemyCells = enemyCells ?? Array.Empty<Vector3Int>();
		}

		public IReadOnlyList<Vector3Int> PlayerCells { get; }
		public IReadOnlyList<Vector3Int> EnemyCells { get; }
	}

	public static bool TryBuildPlacementZones(BattleContext battleContext, out PlacementZones zones)
	{
		zones = default;
		if (battleContext?.Board == null)
		{
			return false;
		}

		return battleContext.PlacementStyle switch
		{
			PlacementStyle.HalfBoard => TryBuildHalfBoardZones(battleContext.Board, out zones),
			_ => false
		};
	}

	public static IReadOnlyList<BattleUnit> GetUnitsWaitingForPlacement(BattleContext battleContext, BattleSide side)
	{
		List<BattleUnit> units = new List<BattleUnit>();
		if (battleContext == null)
		{
			return units;
		}

		foreach (BattleUnit unit in battleContext.GetUnits(side))
		{
			if (unit == null || unit.IsDefeated || unit.HasBoardPosition)
			{
				continue;
			}

			units.Add(unit);
		}

		return units;
	}

	public static IReadOnlyList<Vector3Int> GetValidPlacementCells(
		BattleContext battleContext,
		BattleUnit unit,
		IReadOnlyList<Vector3Int> allowedCells)
	{
		List<Vector3Int> validCells = new List<Vector3Int>();
		if (battleContext?.Board == null || unit == null || allowedCells == null)
		{
			return validCells;
		}

		for (int index = 0; index < allowedCells.Count; index++)
		{
			Vector3Int cell = allowedCells[index];
			if (battleContext.Board.CanPlace(unit, cell))
			{
				validCells.Add(cell);
			}
		}

		return validCells;
	}

	public static bool CanPlaceUnit(
		BattleContext battleContext,
		BattleUnit unit,
		Vector3Int cell,
		IReadOnlyList<Vector3Int> allowedCells)
	{
		if (battleContext?.Board == null || unit == null || allowedCells == null)
		{
			return false;
		}

		if (!ContainsCell(allowedCells, cell))
		{
			return false;
		}

		return battleContext.Board.CanPlace(unit, cell);
	}

	public static bool TryAutoPlaceUnitsRandomly(
		BattleContext battleContext,
		IReadOnlyList<BattleUnit> units,
		IReadOnlyList<Vector3Int> allowedCells)
	{
		if (battleContext?.Board == null || units == null || allowedCells == null)
		{
			return false;
		}

		List<BattleUnit> unitsToPlace = new List<BattleUnit>();
		for (int index = 0; index < units.Count; index++)
		{
			BattleUnit unit = units[index];
			if (unit != null && !unit.IsDefeated && !unit.HasBoardPosition)
			{
				unitsToPlace.Add(unit);
			}
		}

		if (unitsToPlace.Count == 0)
		{
			return true;
		}

		List<Vector3Int> availableCells = new List<Vector3Int>();
		BattleUnit placementProbe = unitsToPlace[0];
		for (int index = 0; index < allowedCells.Count; index++)
		{
			Vector3Int cell = allowedCells[index];
			if (battleContext.Board.CanPlace(placementProbe, cell))
			{
				availableCells.Add(cell);
			}
		}

		if (availableCells.Count < unitsToPlace.Count)
		{
			return false;
		}

		Shuffle(availableCells);

		for (int index = 0; index < unitsToPlace.Count; index++)
		{
			if (!battleContext.TryPlaceUnit(unitsToPlace[index], availableCells[index]))
			{
				return false;
			}
		}

		return true;
	}

	public static void ApplyPlacementMask(BoardData board, IReadOnlyList<Vector3Int> cells)
	{
		BattleMaskRules.ApplyPlacementMask(board, cells);
	}

	private static bool TryBuildHalfBoardZones(BoardData board, out PlacementZones zones)
	{
		zones = default;
		if (board?.Navigation?.Nodes == null)
		{
			return false;
		}

		List<Vector3Int> playerCells = new List<Vector3Int>();
		List<Vector3Int> enemyCells = new List<Vector3Int>();
		int splitZ = board.Terrain.SizeZ / 2;

		IReadOnlyList<VoxelTraversalGraph.Node> nodes = board.Navigation.Nodes;
		for (int index = 0; index < nodes.Count; index++)
		{
			VoxelTraversalGraph.Node node = nodes[index];
			if (node == null || !board.IsStandable(node.Position))
			{
				continue;
			}

			if (node.Position.z < splitZ)
			{
				playerCells.Add(node.Position);
			}
			else
			{
				enemyCells.Add(node.Position);
			}
		}

		SortPlacementCells(playerCells);
		SortPlacementCells(enemyCells);

		if (playerCells.Count == 0 || enemyCells.Count == 0)
		{
			return false;
		}

		zones = new PlacementZones(playerCells, enemyCells);
		return true;
	}

	private static void SortPlacementCells(List<Vector3Int> cells)
	{
		if (cells == null)
		{
			return;
		}

		cells.Sort((left, right) =>
		{
			int zComparison = left.z.CompareTo(right.z);
			if (zComparison != 0)
			{
				return zComparison;
			}

			int xComparison = left.x.CompareTo(right.x);
			if (xComparison != 0)
			{
				return xComparison;
			}

			return left.y.CompareTo(right.y);
		});
	}

	private static bool ContainsCell(IReadOnlyList<Vector3Int> cells, Vector3Int targetCell)
	{
		if (cells == null)
		{
			return false;
		}

		for (int index = 0; index < cells.Count; index++)
		{
			if (cells[index] == targetCell)
			{
				return true;
			}
		}

		return false;
	}

	private static void Shuffle(List<Vector3Int> cells)
	{
		if (cells == null)
		{
			return;
		}

		for (int index = cells.Count - 1; index > 0; index--)
		{
			int swapIndex = UnityEngine.Random.Range(0, index + 1);
			(cells[index], cells[swapIndex]) = (cells[swapIndex], cells[index]);
		}
	}
}
