using System.Collections.Generic;
using UnityEngine;

public class WorldPresenter : MonoBehaviour
{
	[SerializeField] private WorldData worldData = new WorldData();
	[SerializeField] private ChunkPresenter chunkPrefab;
	[SerializeField] private WorldLoader worldLoader = new WorldLoader();

	private readonly Dictionary<ChunkCoordinates, ChunkPresenter> chunkPresenters = new Dictionary<ChunkCoordinates, ChunkPresenter>();

	public WorldData WorldData => worldData;
	public WorldLoader WorldLoader => worldLoader;
	public VoxelRegistry VoxelRegistry => chunkPrefab != null ? chunkPrefab.VoxelRegistry : null;

	[ContextMenu("Load Around Origin")]
	public void LoadAroundOrigin()
	{
		ApplyLoadResult(SetCenterChunk(new ChunkCoordinates(0, 0)));
	}

	private void Awake()
	{
		if (worldData == null)
		{
			worldData = new WorldData();
		}
	}

	private void OnEnable()
	{
		EventCenter.PlayerChunkChanged += OnPlayerChunkChanged;
	}

	private void OnDisable()
	{
		EventCenter.PlayerChunkChanged -= OnPlayerChunkChanged;
	}

	private void OnDestroy()
	{
		ClearChunkPresenters();
		if (worldData != null)
		{
			worldData.Clear();
		}
	}

	private void OnPlayerChunkChanged(ChunkCoordinates centerChunk)
	{
		ApplyLoadResult(SetCenterChunk(centerChunk));
	}

	private void Update()
	{
		if (worldData == null || worldLoader == null || !worldLoader.HasPendingChunks)
		{
			return;
		}

		ApplyLoadResult(worldLoader.ProcessPending(worldData, Time.deltaTime));
	}

	private WorldLoadResult SetCenterChunk(ChunkCoordinates centerChunk)
	{
		if (worldData == null)
		{
			worldData = new WorldData();
		}

		if (worldLoader == null)
		{
			worldLoader = new WorldLoader();
		}

		return worldLoader.SetCenterChunk(worldData, centerChunk);
	}

	private void ApplyLoadResult(WorldLoadResult loadResult)
	{
		if (loadResult == null)
		{
			return;
		}

		for (int i = 0; i < loadResult.UnloadedChunks.Count; i++)
		{
			DestroyChunkPresenter(loadResult.UnloadedChunks[i]);
		}

		if (chunkPrefab == null)
		{
			return;
		}

		for (int i = 0; i < loadResult.LoadedChunks.Count; i++)
		{
			ChunkCoordinates coordinates = loadResult.LoadedChunks[i];
			if (!worldData.TryGetChunk(coordinates, out ChunkData chunkData) || chunkData == null)
			{
				continue;
			}

			CreateChunkPresenter(coordinates).Assign(chunkData);
		}
	}

	private ChunkPresenter CreateChunkPresenter(ChunkCoordinates coordinates)
	{
		if (chunkPresenters.TryGetValue(coordinates, out ChunkPresenter existingPresenter) && existingPresenter != null)
		{
			return existingPresenter;
		}

		ChunkPresenter presenter = Instantiate(chunkPrefab, transform);
		presenter.transform.localPosition = new Vector3(
			coordinates.X * ChunkData.FixedSizeX,
			0f,
			coordinates.Z * ChunkData.FixedSizeZ);
		presenter.gameObject.name = $"Chunk {coordinates}";
		chunkPresenters[coordinates] = presenter;
		return presenter;
	}

	public bool TryGetChunk(ChunkCoordinates coordinates, out ChunkData chunkData)
	{
		if (worldData != null && worldData.TryGetChunk(coordinates, out chunkData))
		{
			return true;
		}

		chunkData = null;
		return false;
	}

	public void ClearChunkMasks(ChunkCoordinates coordinates)
	{
		if (worldData == null || !worldData.TryGetChunk(coordinates, out ChunkData chunkData) || chunkData == null)
		{
			return;
		}

		chunkData.ClearMasks();
	}

	public void ClearAllChunkMasks()
	{
		if (worldData == null)
		{
			return;
		}

		foreach (KeyValuePair<ChunkCoordinates, ChunkData> entry in worldData.Chunks)
		{
			entry.Value?.ClearMasks();
		}
	}

	public bool TryAddMask(Vector3Int worldPosition, VoxelMask mask)
	{
		if (mask == VoxelMask.None || worldData == null)
		{
			return false;
		}

		if (!worldData.TryGetChunk(worldPosition, out _, out Vector3Int localPosition, out ChunkData chunkData) || chunkData == null)
		{
			return false;
		}

		return chunkData.MaskLayer.TryAddMask(localPosition, mask);
	}

	public void RebuildChunkOverlay(ChunkCoordinates coordinates)
	{
		if (chunkPresenters.TryGetValue(coordinates, out ChunkPresenter presenter) && presenter != null)
		{
			presenter.RebuildOverlay();
		}
	}

	public void RebuildAllChunkOverlays()
	{
		foreach (ChunkPresenter presenter in chunkPresenters.Values)
		{
			if (presenter != null)
			{
				presenter.RebuildOverlay();
			}
		}
	}

	private void DestroyChunkPresenter(ChunkCoordinates coordinates)
	{
		if (!chunkPresenters.TryGetValue(coordinates, out ChunkPresenter presenter) || presenter == null)
		{
			chunkPresenters.Remove(coordinates);
			return;
		}

		if (Application.isPlaying)
		{
			Destroy(presenter.gameObject);
		}
		else
		{
			DestroyImmediate(presenter.gameObject);
		}

		chunkPresenters.Remove(coordinates);
	}

	private void ClearChunkPresenters()
	{
		var coordinates = new List<ChunkCoordinates>(chunkPresenters.Keys);
		for (int i = 0; i < coordinates.Count; i++)
		{
			DestroyChunkPresenter(coordinates[i]);
		}
	}
}
