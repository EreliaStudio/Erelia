using UnityEngine;

public class ChunkPresenter : MonoBehaviour
{
	[SerializeField] private ChunkData chunkData = new ChunkData();
	[SerializeField] private ChunkView chunkView;
	[SerializeField] private VoxelRegistry voxelRegistry;

	public ChunkData ChunkData => chunkData;
	public ChunkView View => chunkView;

	public void Assign(ChunkData targetChunkData)
	{
		chunkData = targetChunkData;
		Rebuild();
	}

	[ContextMenu("Rebuild Chunk")]
	public void Rebuild()
	{
		if (chunkView == null)
		{
			chunkView = GetComponent<ChunkView>();
		}

		if (chunkData == null || chunkView == null || voxelRegistry == null)
		{
			return;
		}

		chunkView.SetRenderMesh(VoxelMesher.BuildRenderMesh(chunkData.Cells, voxelRegistry));
		chunkView.SetCollisionMesh(VoxelMesher.BuildColliderMesh(chunkData.Cells, voxelRegistry));
	}

	private void Reset()
	{
		if (chunkView == null)
		{
			chunkView = GetComponent<ChunkView>();
		}
	}

	private void Awake()
	{
		Rebuild();
	}
}
