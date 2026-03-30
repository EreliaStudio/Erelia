using System;
using System.Collections.Generic;

[Serializable]
public class WorldData
{
	public WorldGenerator Generator = new WorldGenerator();

	[NonSerialized]
	public readonly Dictionary<ChunkCoordinates, Chunk> Chunks = new Dictionary<ChunkCoordinates, Chunk>();

	public bool TryGetChunk(ChunkCoordinates coordinates, out Chunk chunk)
	{
		if (Chunks.TryGetValue(coordinates, out chunk) && chunk != null)
		{
			return true;
		}

		chunk = Generator.GenerateChunk(coordinates);
		Chunks[coordinates] = chunk;
		return chunk != null;
	}

	public bool HasChunk(ChunkCoordinates coordinates)
	{
		return Chunks.TryGetValue(coordinates, out Chunk chunk) && chunk != null;
	}
}
