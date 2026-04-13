using System;

[Serializable]
public class WorldGenerator
{
	public Chunk GenerateChunk(ChunkCoordinates coordinates)
	{
		Chunk chunk = new Chunk();
		PopulateChunk(chunk, coordinates);
		return chunk;
	}

	protected virtual void PopulateChunk(Chunk chunk, ChunkCoordinates coordinates)
	{
	}
}
