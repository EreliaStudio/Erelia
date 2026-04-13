using System.Collections.Generic;

public sealed class WorldLoadResult
{
	public readonly ChunkCoordinates CenterChunk;
	public readonly List<ChunkCoordinates> LoadedChunks = new List<ChunkCoordinates>();
	public readonly List<ChunkCoordinates> UnloadedChunks = new List<ChunkCoordinates>();

	public WorldLoadResult(ChunkCoordinates centerChunk)
	{
		CenterChunk = centerChunk;
	}
}
