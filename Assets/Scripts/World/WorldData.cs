using System;
using System.Collections.Generic;

[Serializable]
public class WorldData
{
	public readonly Dictionary<ChunkCoordinates, ChunkData> Chunks = new Dictionary<ChunkCoordinates, ChunkData>();

	public void SetChunk(ChunkCoordinates coordinates, ChunkData chunkData)
	{
		if (chunkData == null)
		{
			Chunks.Remove(coordinates);
			return;
		}

		Chunks[coordinates] = chunkData;
	}

	public bool TryGetChunk(ChunkCoordinates coordinates, out ChunkData chunkData)
	{
		return Chunks.TryGetValue(coordinates, out chunkData);
	}

	public bool HasChunk(ChunkCoordinates coordinates)
	{
		return Chunks.ContainsKey(coordinates);
	}

	public void Clear()
	{
		Chunks.Clear();
	}
}
