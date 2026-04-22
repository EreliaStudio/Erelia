using System.Collections.Generic;
using UnityEngine;

public static class BoardDataBuilder
{
	public static BoardData Build(WorldData worldData, VoxelRegistry voxelRegistry, Vector3Int anchorWorldPosition, BoardConfiguration configuration)
	{
		if (worldData == null || voxelRegistry == null || configuration == null)
		{
			return null;
		}


		Vector3Int size = configuration.GetSize();
		Vector3Int worldOrigin = configuration.GetWorldOrigin(anchorWorldPosition);

		BoardTerrainLayer terrain = new BoardTerrainLayer(size.x, size.y, size.z);
		BoardData board = new BoardData(terrain, new BoardNavigationLayer(), new BoardRuntimeRegistry(), worldOrigin);
		board.AssignVoxelRegistry(voxelRegistry);

		for (int x = 0; x < size.x; x++)
		{
			for (int y = 0; y < size.y; y++)
			{
				for (int z = 0; z < size.z; z++)
				{
					Vector3Int localPosition = new Vector3Int(x, y, z);
					Vector3Int worldPosition = worldOrigin + localPosition;
					terrain.Cells[x, y, z] = worldData.TryGetCell(worldPosition, out VoxelCell cell)
						? new VoxelCell(cell)
						: null;
				}
			}
		}

		board.RebuildNavigation();
		board.AssignBorderLocalCells(BuildBorder(board, size));
		return board;
	}

	private static List<Vector3Int> BuildBorder(BoardData board, Vector3Int size)
	{
		var borderLocalCells = new List<Vector3Int>();
		if (board == null)
		{
			return borderLocalCells;
		}

		for (int x = 0; x < size.x; x++)
		{
			for (int z = 0; z < size.z; z++)
			{
				if (x != 0 && x != size.x - 1 && z != 0 && z != size.z - 1)
				{
					continue;
				}

				for (int y = 0; y < size.y; y++)
				{
					Vector3Int localPosition = new Vector3Int(x, y, z);
					if (!board.Navigation.IsStandable(localPosition))
					{
						continue;
					}

					board.Terrain.MaskLayer.TryAddMask(localPosition, VoxelMask.BattleAreaBorder);
					borderLocalCells.Add(localPosition);
				}
			}
		}

		return borderLocalCells;
	}
}
