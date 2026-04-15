using UnityEngine;

public class ChunkPresenter : MonoBehaviour
{
	[SerializeField] private ChunkData chunkData = new ChunkData();
	[SerializeField] private ChunkView terrainView;
	[SerializeField] private ChunkOverlayView overlayView;
	[SerializeField] private VoxelRegistry voxelRegistry;
	[SerializeField] private VoxelMaskRegistry voxelMaskRegistry;

	public ChunkData ChunkData => chunkData;
	public ChunkView View => terrainView;
	public ChunkView TerrainView => terrainView;
	public ChunkOverlayView OverlayView => overlayView;
	public VoxelRegistry VoxelRegistry => voxelRegistry;
	public VoxelMaskRegistry VoxelMaskRegistry => voxelMaskRegistry;

	public void Assign(ChunkData targetChunkData)
	{
		chunkData = targetChunkData;
		Rebuild();
	}

	[ContextMenu("Rebuild Chunk")]
	public void Rebuild()
	{
		CacheReferences();

		if (chunkData == null || voxelRegistry == null)
		{
			return;
		}

		RebuildTerrain();
		RebuildOverlay();
	}

	public void RebuildTerrain()
	{
		if (terrainView == null || chunkData == null || voxelRegistry == null)
		{
			return;
		}

		terrainView.SetRenderMesh(VoxelMesher.BuildRenderMesh(chunkData.Cells, voxelRegistry));
		terrainView.SetCollisionMesh(VoxelMesher.BuildColliderMesh(chunkData.Cells, voxelRegistry, VoxelTraversal.Obstacle));
	}

	public void RebuildOverlay()
	{
		if (overlayView == null)
		{
			return;
		}

		if (chunkData == null || voxelRegistry == null || voxelMaskRegistry == null)
		{
			overlayView.SetOverlayMesh(null);
			return;
		}

		overlayView.SetOverlayMesh(VoxelMesher.BuildMaskMesh(chunkData.Cells, chunkData.MaskLayer, voxelRegistry, voxelMaskRegistry));
	}

	public void SetTerrainVisible(bool visible)
	{
		CacheReferences();
		if (terrainView != null)
		{
			terrainView.SetVisible(visible);
		}
	}

	public void SetOverlayVisible(bool visible)
	{
		CacheReferences();
		if (overlayView != null)
		{
			overlayView.SetVisible(visible);
		}
	}

	public void SetViewsVisible(bool visible)
	{
		SetTerrainVisible(visible);
		SetOverlayVisible(visible);
	}

	private void Reset()
	{
		CacheReferences();
	}

	private void Awake()
	{
		Rebuild();
	}

	private void CacheReferences()
	{
		if (terrainView == null)
		{
			terrainView = GetComponentInChildren<ChunkView>(true);
		}

		if (overlayView == null)
		{
			overlayView = GetComponentInChildren<ChunkOverlayView>(true);
		}
	}
}
