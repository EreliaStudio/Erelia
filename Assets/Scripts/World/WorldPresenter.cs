using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldPresenter : MonoBehaviour
{
	[SerializeField] private WorldData worldData = new WorldData();
	[SerializeField] private ChunkPresenter chunkPrefab;

	public WorldData WorldData => worldData;
	public ChunkPresenter ChunkPrefab => chunkPrefab;

	[NonSerialized]
	public readonly Dictionary<ChunkCoordinates, ChunkPresenter> Chunks = new Dictionary<ChunkCoordinates, ChunkPresenter>();

	public void Initialize(WorldData targetWorldData)
	{
		worldData = targetWorldData;
	}

	public ChunkPresenter GetChunkPresenter(ChunkCoordinates coordinates)
	{
		if (Chunks.TryGetValue(coordinates, out ChunkPresenter result) && result != null)
		{
			return result;
		}

		result = Instantiate(chunkPrefab, transform);
		result.transform.position = new Vector3(coordinates.X * Chunk.FixedSizeX, 0, coordinates.Z * Chunk.FixedSizeZ);
		result.gameObject.name = $"Chunk {coordinates}";

		Chunks[coordinates] = result;

		result.Assign(worldData.GetChunk(coordinates));

		return result;
	}

	public bool HasChunkPresenter(ChunkCoordinates coordinates)
	{
		return Chunks.TryGetValue(coordinates, out ChunkPresenter presenter) && presenter != null;
	}
}
