using UnityEngine;

public class ChunkPresenter : MonoBehaviour
{
	[SerializeField] private ChunkData chunkData = new ChunkData();
	[SerializeField] private TerrainView terrainView;
	[SerializeField] private MaskOverlayView overlayView;
	[SerializeField] private VoxelRegistry voxelRegistry;
	[SerializeField] private VoxelMaskRegistry voxelMaskRegistry;

	public ChunkData ChunkData => chunkData;
	public TerrainView TerrainView => terrainView;
	public MaskOverlayView OverlayView => overlayView;
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
		if (chunkData == null || voxelRegistry == null)
		{
			return;
		}

		RebuildTerrain();
		RebuildOverlay();
	}

	public void RebuildTerrain()
	{
		if (chunkData == null || voxelRegistry == null)
		{
			return;
		}

		terrainView.SetRenderMesh(VoxelMesher.BuildRenderMesh(chunkData.Cells, voxelRegistry));
		terrainView.SetCollisionMesh(VoxelMesher.BuildColliderMesh(chunkData.Cells, voxelRegistry, VoxelTraversal.Obstacle));
	}

	public void RebuildOverlay()
	{
		if (chunkData == null || voxelRegistry == null || voxelMaskRegistry == null)
		{
			overlayView.SetMaskMesh(null);
			return;
		}

		overlayView.SetMaskMesh(VoxelMesher.BuildMaskMesh(chunkData.Cells, chunkData.MaskLayer, voxelRegistry, voxelMaskRegistry));
	}

	public void SetTerrainVisible(bool visible)
	{
		terrainView.SetVisible(visible);
	}

	public void SetOverlayVisible(bool visible)
	{
		overlayView.SetVisible(visible);
	}

	public void SetViewsVisible(bool visible)
	{
		SetTerrainVisible(visible);
		SetOverlayVisible(visible);
	}

	private void Awake()
	{
		if (terrainView == null)
		{
			Logger.LogError("[ChunkPresenter] TerrainView is not assigned in the inspector. Please assign a TerrainView child to the ChunkPresenter component.", Logger.Severity.Critical, this);
		}

		if (overlayView == null)
		{
			Logger.LogError("[ChunkPresenter] MaskOverlayView is not assigned in the inspector. Please assign a MaskOverlayView child to the ChunkPresenter component.", Logger.Severity.Critical, this);
		}

		Rebuild();
	}
}
