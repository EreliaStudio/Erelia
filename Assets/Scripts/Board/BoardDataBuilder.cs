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

		HashSet<Vector2Int> borderColumns = BuildBorderColumns(size);
		board.RebuildNavigation(borderColumns);
		board.AssignBorderLocalCells(BuildBorder(terrain, size, voxelRegistry, borderColumns));
		return board;
	}

	private static HashSet<Vector2Int> BuildBorderColumns(Vector3Int size)
	{
		var columns = new HashSet<Vector2Int>();
		for (int x = 0; x < size.x; x++)
		{
			for (int z = 0; z < size.z; z++)
			{
				if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1)
				{
					columns.Add(new Vector2Int(x, z));
				}
			}
		}

		return columns;
	}

	private static List<Vector3Int> BuildBorder(BoardTerrainLayer terrain, Vector3Int size, VoxelRegistry voxelRegistry, HashSet<Vector2Int> borderColumns)
	{
		var borderLocalCells = new List<Vector3Int>();
		foreach (Vector2Int column in borderColumns)
		{
			for (int y = 0; y < size.y - 2; y++)
			{
				Vector3Int localPosition = new Vector3Int(column.x, y, column.y);
				if (VoxelTraversalUtility.IsReachableCell(terrain, localPosition, voxelRegistry))
				{
					borderLocalCells.Add(localPosition);
				}
			}
		}

		return borderLocalCells;
	}
}
