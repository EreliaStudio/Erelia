using System;
using System.Collections.Generic;

[Serializable]
public class WorldData
{
	public WorldGenerator Generator = new WorldGenerator();

	[NonSerialized]
	public readonly Dictionary<ChunkCoordinates, Chunk> Chunks = new Dictionary<ChunkCoordinates, Chunk>();

	public Chunk GetChunk(ChunkCoordinates coordinates)
	{
		if (Chunks.TryGetValue(coordinates, out Chunk result))
		{
			return result;
		}

		result = Generator.GenerateChunk(coordinates);
		Chunks[coordinates] = result;
		return result;
	}

	public bool HasChunk(ChunkCoordinates coordinates)
	{
		return Chunks.ContainsKey(coordinates);
	}
}
