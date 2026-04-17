using System.Collections.Generic;
using UnityEngine;

public class WorldPresenter : MonoBehaviour
{
	[SerializeField] private ChunkPresenter chunkPrefab;

	private readonly Dictionary<ChunkCoordinates, ChunkPresenter> chunkPresenters = new Dictionary<ChunkCoordinates, ChunkPresenter>();
	private WorldContext worldContext;

	public WorldContext WorldContext => worldContext;
	public WorldData WorldData => worldContext?.WorldData;
	public MetaWorldData MetaWorldData => worldContext?.MetaWorldData;
	public WorldLoader WorldLoader => worldContext?.WorldLoader;
	public MetaWorldGenerator MetaWorldGenerator => worldContext?.MetaWorldGenerator;
	public VoxelRegistry VoxelRegistry => chunkPrefab != null ? chunkPrefab.VoxelRegistry : null;

	private void Awake()
	{
		if (chunkPrefab == null)
		{
			Logger.LogError("[WorldPresenter] ChunkPrefab is not assigned in the inspector. Please assign a ChunkPresenter prefab to the WorldPresenter component.", Logger.Severity.Critical, this);
		}
	}

	public void Bind(WorldContext targetWorldContext)
	{
		if (ReferenceEquals(worldContext, targetWorldContext))
		{
			RefreshLoadedChunkPresenters();
			return;
		}

		ClearChunkPresenters();
		worldContext = targetWorldContext;
		RefreshLoadedChunkPresenters();
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
	}

	private void OnPlayerChunkChanged(ChunkCoordinates centerChunk)
	{
		ApplyLoadResult(SetCenterChunk(centerChunk));
	}

	private void Update()
	{
		if (worldContext == null || worldContext.WorldData == null || worldContext.WorldLoader == null || !worldContext.WorldLoader.HasPendingChunks)
		{
			return;
		}

		ApplyLoadResult(worldContext.WorldLoader.ProcessPending(worldContext.WorldData, Time.deltaTime));
	}

	private WorldLoadResult SetCenterChunk(ChunkCoordinates centerChunk)
	{
		if (worldContext == null || worldContext.WorldData == null || worldContext.WorldLoader == null)
		{
			return null;
		}

		return worldContext.WorldLoader.SetCenterChunk(worldContext.WorldData, centerChunk);
	}

	private void ApplyLoadResult(WorldLoadResult loadResult)
	{
		if (loadResult == null)
		{
			return;
		}

		for (int i = 0; i < loadResult.UnloadedChunks.Count; i++)
		{
			ChunkCoordinates coordinates = loadResult.UnloadedChunks[i];
			DestroyChunkPresenter(coordinates);
			worldContext?.MetaWorldData?.SetChunkMeta(coordinates, null);
		}

		if (chunkPrefab == null)
		{
			return;
		}

		for (int i = 0; i < loadResult.LoadedChunks.Count; i++)
		{
			ChunkCoordinates coordinates = loadResult.LoadedChunks[i];
			if (worldContext == null ||
			    !worldContext.WorldData.TryGetChunk(coordinates, out ChunkData chunkData) ||
			    chunkData == null)
			{
				continue;
			}

			if (worldContext.MetaWorldData != null && !worldContext.MetaWorldData.HasChunkMeta(coordinates))
			{
				worldContext.MetaWorldData.SetChunkMeta(coordinates, worldContext.MetaWorldGenerator != null
					? worldContext.MetaWorldGenerator.GenerateChunkMeta(coordinates)
					: new ChunkMetaData());
			}

			CreateChunkPresenter(coordinates).Assign(chunkData);
		}
	}

	public void LoadImmediatelyAroundWorldCell(Vector3Int worldCell)
	{
		if (worldContext == null)
		{
			return;
		}

		ApplyLoadResult(SetCenterChunk(ChunkCoordinates.FromWorldVoxelPosition(worldCell)));

		if (worldContext.WorldLoader == null || worldContext.WorldData == null)
		{
			return;
		}

		float stepDeltaTime = worldContext.WorldLoader.UpdateIntervalSeconds > 0f ? worldContext.WorldLoader.UpdateIntervalSeconds : 0f;
		while (worldContext.WorldLoader.HasPendingChunks)
		{
			ApplyLoadResult(worldContext.WorldLoader.ProcessPending(worldContext.WorldData, stepDeltaTime));
		}
	}

	[ContextMenu("Load Around Origin")]
	public void LoadAroundOrigin()
	{
		ApplyLoadResult(SetCenterChunk(new ChunkCoordinates(0, 0)));
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

	private void RefreshLoadedChunkPresenters()
	{
		if (worldContext?.WorldData == null || chunkPrefab == null)
		{
			return;
		}

		foreach (KeyValuePair<ChunkCoordinates, ChunkData> entry in worldContext.WorldData.Chunks)
		{
			if (entry.Value != null)
			{
				CreateChunkPresenter(entry.Key).Assign(entry.Value);
			}
		}
	}

	public bool TryGetChunk(ChunkCoordinates coordinates, out ChunkData chunkData)
	{
		if (worldContext?.WorldData != null && worldContext.WorldData.TryGetChunk(coordinates, out chunkData))
		{
			return true;
		}

		chunkData = null;
		return false;
	}

	public void ClearChunkMasks(ChunkCoordinates coordinates)
	{
		if (worldContext?.WorldData == null || !worldContext.WorldData.TryGetChunk(coordinates, out ChunkData chunkData) || chunkData == null)
		{
			return;
		}

		chunkData.ClearMasks();
	}

	public void ClearAllChunkMasks()
	{
		if (worldContext?.WorldData == null)
		{
			return;
		}

		foreach (KeyValuePair<ChunkCoordinates, ChunkData> entry in worldContext.WorldData.Chunks)
		{
			entry.Value?.ClearMasks();
		}
	}

	public bool TryAddMask(Vector3Int worldPosition, VoxelMask mask)
	{
		if (mask == VoxelMask.None || worldContext?.WorldData == null)
		{
			return false;
		}

		if (!worldContext.WorldData.TryGetChunk(worldPosition, out _, out Vector3Int localPosition, out ChunkData chunkData) || chunkData == null)
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

	public void ShowBattleAreaBorder(IEnumerable<Vector3Int> borderWorldCells)
	{
		ClearAllChunkMasks();

		if (borderWorldCells != null)
		{
			foreach (Vector3Int worldCell in borderWorldCells)
			{
				TryAddMask(worldCell, VoxelMask.BattleAreaBorder);
			}
		}

		RebuildAllChunkOverlays();
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
