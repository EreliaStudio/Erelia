using System.Collections.Generic;
using UnityEngine;

public class WorldPresenter : MonoBehaviour
{
	[SerializeField] private WorldData worldData = new WorldData();
	[SerializeField] private ChunkPresenter chunkPrefab;
	[SerializeField] private SimpleDebugChunkGenerator generator = new SimpleDebugChunkGenerator();
	[SerializeField] private Vector2Int minChunk = Vector2Int.zero;
	[SerializeField] private Vector2Int maxChunk = Vector2Int.zero;
	[SerializeField] private bool buildOnAwake = true;

	private readonly Dictionary<ChunkCoordinates, ChunkPresenter> chunkPresenters = new Dictionary<ChunkCoordinates, ChunkPresenter>();

	public WorldData WorldData => worldData;

	[ContextMenu("Build Debug World")]
	public void BuildDebugWorld()
	{
		if (worldData == null)
		{
			worldData = new WorldData();
		}

		if (generator == null)
		{
			generator = new SimpleDebugChunkGenerator();
		}

		if (chunkPrefab == null)
		{
			return;
		}

		ClearChunkPresenters();
		worldData.Clear();

		for (int chunkX = minChunk.x; chunkX <= maxChunk.x; chunkX++)
		{
			for (int chunkZ = minChunk.y; chunkZ <= maxChunk.y; chunkZ++)
			{
				var coordinates = new ChunkCoordinates(chunkX, chunkZ);
				ChunkData chunkData = generator.GenerateChunk(coordinates);

				worldData.SetChunk(coordinates, chunkData);
				CreateChunkPresenter(coordinates).Assign(chunkData);
			}
		}
	}

	private void Awake()
	{
		if (buildOnAwake)
		{
			BuildDebugWorld();
		}
	}

	private ChunkPresenter CreateChunkPresenter(ChunkCoordinates coordinates)
	{
		ChunkPresenter presenter = Instantiate(chunkPrefab, transform);
		presenter.transform.localPosition = new Vector3(
			coordinates.X * ChunkData.FixedSizeX,
			0f,
			coordinates.Z * ChunkData.FixedSizeZ);
		presenter.gameObject.name = $"Chunk {coordinates}";
		chunkPresenters[coordinates] = presenter;
		return presenter;
	}

	private void ClearChunkPresenters()
	{
		foreach (ChunkPresenter presenter in chunkPresenters.Values)
		{
			if (presenter == null)
			{
				continue;
			}

			if (Application.isPlaying)
			{
				Destroy(presenter.gameObject);
			}
			else
			{
				DestroyImmediate(presenter.gameObject);
			}
		}

		chunkPresenters.Clear();
	}
}
