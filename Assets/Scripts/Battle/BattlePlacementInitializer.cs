using System.Collections.Generic;
using UnityEngine;

public static class BattlePlacementInitializer
{
	public static PlacementAreas ResolveAreas(BoardData p_board)
	{
		if (p_board == null)
		{
			return new PlacementAreas(null, null);
		}

		return p_board.PlacementStyle switch
		{
			BoardConfiguration.PlacementStyle.HalfBoard => ResolveHalfBoardAreas(p_board),
			_ => ResolveHalfBoardAreas(p_board)
		};
	}

	public static void PlaceUnits(BattleContext p_context, IReadOnlyList<BattleUnit> p_units, IReadOnlyList<Vector3Int> p_cells)
	{
		if (p_context == null || p_units == null || p_cells == null || p_units.Count == 0 || p_cells.Count == 0)
		{
			return;
		}

		for (int unitIndex = 0; unitIndex < p_units.Count; unitIndex++)
		{
			BattleUnit unit = p_units[unitIndex];
			if (unit == null)
			{
				continue;
			}

			for (int cellIndex = 0; cellIndex < p_cells.Count; cellIndex++)
			{
				if (p_context.TryPlaceUnit(unit, p_cells[cellIndex]))
				{
					break;
				}
			}
		}
	}

	private static PlacementAreas ResolveHalfBoardAreas(BoardData p_board)
	{
		var playerCells = new List<Vector3Int>();
		var enemyCells = new List<Vector3Int>();
		int splitX = Mathf.CeilToInt(p_board.Terrain.SizeX * 0.5f);

		for (int x = 0; x < p_board.Terrain.SizeX; x++)
		{
			for (int z = 0; z < p_board.Terrain.SizeZ; z++)
			{
				for (int y = 0; y < p_board.Terrain.SizeY; y++)
				{
					Vector3Int cell = new Vector3Int(x, y, z);
					if (!p_board.IsStandable(cell))
					{
						continue;
					}

					if (x < splitX)
					{
						playerCells.Add(cell);
					}
					else
					{
						enemyCells.Add(cell);
					}
				}
			}
		}

		return new PlacementAreas(playerCells, enemyCells);
	}
}
