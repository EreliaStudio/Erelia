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

	public void Assign(WorldData targetWorldData)
	{
		worldData = targetWorldData ?? new WorldData();
	}

	public bool TryGetChunkPresenter(ChunkCoordinates coordinates, out ChunkPresenter presenter)
	{
		if (Chunks.TryGetValue(coordinates, out presenter) && presenter != null)
		{
			return true;
		}

		if (chunkPrefab == null)
		{
			presenter = null;
			return false;
		}

		if (!worldData.TryGetChunk(coordinates, out Chunk chunk))
		{
			presenter = null;
			return false;
		}

		presenter = Instantiate(chunkPrefab, transform);
		presenter.transform.position = new Vector3(
			coordinates.X * Chunk.FixedSizeX, 0, coordinates.Z * Chunk.FixedSizeZ);
		presenter.gameObject.name = $"Chunk {coordinates}";

		Chunks[coordinates] = presenter;
		presenter.Assign(chunk);
		return true;
	}

	public bool HasChunkPresenter(ChunkCoordinates coordinates)
	{
		return Chunks.TryGetValue(coordinates, out ChunkPresenter presenter) && presenter != null;
	}
}
