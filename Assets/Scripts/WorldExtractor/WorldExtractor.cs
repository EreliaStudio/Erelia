using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public static class WorldExtractor
{
	private static Board Extract(WorldData p_worldData, Vector3Int p_anchorCoordinates, WorldExtractionConfiguration p_configuration)
	{
		int size = p_configuration.Size;
		if (size <= 0)
		{
			return new Board();
		}

		Board board = new Board(size, Chunk.FixedSizeY, size);

		Vector2Int minWorld = new Vector2Int(p_anchorCoordinates.x, p_anchorCoordinates.z);
		Vector2Int maxWorld = minWorld + new Vector2Int(size - 1, size - 1);

		ChunkCoordinates minChunkCoordinates = ChunkCoordinates.FromWorldPosition(minWorld.x, minWorld.y);
		ChunkCoordinates maxChunkCoordinates = ChunkCoordinates.FromWorldPosition(maxWorld.x, maxWorld.y);

		for (int chunkX = minChunkCoordinates.X; chunkX <= maxChunkCoordinates.X; chunkX++)
		{
			for (int chunkZ = minChunkCoordinates.Z; chunkZ <= maxChunkCoordinates.Z; chunkZ++)
			{
				ChunkCoordinates chunkCoordinates = new ChunkCoordinates(chunkX, chunkZ);
				Chunk chunk = p_worldData.GetChunk(chunkCoordinates);
				if (chunk == null)
				{
					continue;
				}

				Vector2Int chunkMinWorld = new Vector2Int(chunkX * Chunk.FixedSizeX, chunkZ * Chunk.FixedSizeZ);
				Vector2Int chunkMaxWorld = chunkMinWorld + new Vector2Int(Chunk.FixedSizeX - 1, Chunk.FixedSizeZ - 1);

				Vector2Int overlapMinWorld = new Vector2Int(Math.Max(minWorld.x, chunkMinWorld.x), Math.Max(minWorld.y, chunkMinWorld.y));
				Vector2Int overlapMaxWorld = new Vector2Int(Math.Min(maxWorld.x, chunkMaxWorld.x), Math.Min(maxWorld.y, chunkMaxWorld.y));

				for (int worldX = overlapMinWorld.x; worldX <= overlapMaxWorld.x; worldX++)
				{
					int localX = worldX - chunkMinWorld.x;
					int boardX = worldX - minWorld.x;

					for (int worldZ = overlapMinWorld.y; worldZ <= overlapMaxWorld.y; worldZ++)
					{
						int localZ = worldZ - chunkMinWorld.y;
						int boardZ = worldZ - minWorld.y;

						for (int y = 0; y < Chunk.FixedSizeY; y++)
						{
							board.Cells[boardX, y, boardZ] = CloneCell(chunk.Cells[localX, y, localZ]);
						}
					}
				}
			}
		}

		return board;
	}

	public static Board Extract(WorldData p_worldData, Vector3Int p_anchorCoordinates, WorldExtractionConfiguration p_configuration, VoxelRegistry p_voxelRegistry)
	{
		Board board = Extract(p_worldData, p_anchorCoordinates, p_configuration);
		board.ReachableCells = GetReachableCells(board, p_voxelRegistry);
		return board;
	}

	private static List<Vector3Int> GetReachableCells(Board p_board, VoxelRegistry p_voxelRegistry)
	{
		List<Vector3Int> reachableCells = new List<Vector3Int>();

		for (int x = 0; x < p_board.SizeX; x++)
		{
			for (int y = 0; y < p_board.SizeY - 2; y++)
			{
				for (int z = 0; z < p_board.SizeZ; z++)
				{
					if (IsReachableCell(p_board, x, y, z, p_voxelRegistry))
					{
						reachableCells.Add(new Vector3Int(x, y, z));
					}
				}
			}
		}

		return reachableCells;
	}

	private static bool IsReachableCell(Board p_board, int p_x, int p_y, int p_z, VoxelRegistry p_voxelRegistry)
	{
		return IsWalkable(p_board.Cells[p_x, p_y, p_z], p_voxelRegistry) &&
			IsAirOrWalkable(p_board.Cells[p_x, p_y + 1, p_z], p_voxelRegistry) &&
			IsAirOrWalkable(p_board.Cells[p_x, p_y + 2, p_z], p_voxelRegistry);
	}

	private static bool IsAirOrWalkable(VoxelCell p_cell, VoxelRegistry p_voxelRegistry)
	{
		return p_cell == null || p_cell.IsEmpty || IsWalkable(p_cell, p_voxelRegistry);
	}

	private static bool IsWalkable(VoxelCell p_cell, VoxelRegistry p_voxelRegistry)
	{
		if (p_cell == null || p_cell.IsEmpty)
		{
			return false;
		}

		if (!p_voxelRegistry.TryGetVoxel(p_cell.Id, out VoxelDefinition voxelDefinition) || voxelDefinition == null)
		{
			return false;
		}

		return voxelDefinition.Data != null && voxelDefinition.Data.Traversal == VoxelTraversal.Walkable;
	}

	private static VoxelCell CloneCell(VoxelCell p_sourceCell)
	{
		if (p_sourceCell == null)
		{
			return null;
		}

		return new VoxelCell(p_sourceCell.Id, p_sourceCell.Orientation, p_sourceCell.FlipOrientation);
	}
}
