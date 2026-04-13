using System;
using UnityEngine;

[Serializable]
public class SimpleDebugChunkGenerator
{
	[SerializeField] private int floorVoxelId;
	[SerializeField] private int borderNegativeXVoxelId = 1;
	[SerializeField] private int borderPositiveXVoxelId = 2;
	[SerializeField] private int borderNegativeZVoxelId = 3;
	[SerializeField] private int borderPositiveZVoxelId = 4;
	[SerializeField] private int raisedPlatformVoxelId;

	public ChunkData GenerateChunk(ChunkCoordinates coordinates)
	{
		var chunkData = new ChunkData();
		PopulateChunk(chunkData, coordinates);
		return chunkData;
	}

	public void PopulateChunk(ChunkData chunkData, ChunkCoordinates coordinates)
	{
		if (chunkData == null)
		{
			return;
		}

		for (int x = 0; x < ChunkData.FixedSizeX; x++)
		{
			for (int z = 0; z < ChunkData.FixedSizeZ; z++)
			{
				chunkData.SetCell(x, 0, z, new VoxelCell(floorVoxelId));
			}
		}

		for (int z = 0; z < ChunkData.FixedSizeZ; z++)
		{
			chunkData.SetCell(0, 1, z, new VoxelCell(borderNegativeXVoxelId));
			chunkData.SetCell(ChunkData.FixedSizeX - 1, 1, z, new VoxelCell(borderPositiveXVoxelId));
		}

		for (int x = 0; x < ChunkData.FixedSizeX; x++)
		{
			chunkData.SetCell(x, 1, 0, new VoxelCell(borderNegativeZVoxelId));
			chunkData.SetCell(x, 1, ChunkData.FixedSizeZ - 1, new VoxelCell(borderPositiveZVoxelId));
		}

		for (int x = 5; x <= 9; x++)
		{
			for (int z = 5; z <= 9; z++)
			{
				chunkData.SetCell(x, 1, z, new VoxelCell(raisedPlatformVoxelId));
			}
		}
	}
}
