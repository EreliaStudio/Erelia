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
							VoxelCell sourceCell = chunk.Cells[localX, y, localZ];
							board.Cells[boardX, y, boardZ] = sourceCell == null ? null : new VoxelCell(sourceCell);
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
		board.UpdateCellGraph(p_voxelRegistry);
		return board;
	}
}
