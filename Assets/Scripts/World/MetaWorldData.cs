using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MetaWorldData
{
	public readonly Dictionary<ChunkCoordinates, ChunkMetaData> Chunks =
		new Dictionary<ChunkCoordinates, ChunkMetaData>();

	public void SetChunkMeta(ChunkCoordinates coordinates, ChunkMetaData chunkMetaData)
	{
		if (chunkMetaData == null)
		{
			Chunks.Remove(coordinates);
			return;
		}

		Chunks[coordinates] = chunkMetaData;
	}

	public bool TryGetChunkMeta(ChunkCoordinates coordinates, out ChunkMetaData chunkMetaData)
	{
		return Chunks.TryGetValue(coordinates, out chunkMetaData);
	}

	public bool TryGetChunkMeta(Vector3Int worldPosition, out ChunkCoordinates coordinates, out ChunkMetaData chunkMetaData)
	{
		coordinates = ChunkCoordinates.FromWorldVoxelPosition(worldPosition);
		return TryGetChunkMeta(coordinates, out chunkMetaData);
	}

	public bool HasChunkMeta(ChunkCoordinates coordinates)
	{
		return Chunks.ContainsKey(coordinates);
	}

	public void Clear()
	{
		Chunks.Clear();
	}
}
