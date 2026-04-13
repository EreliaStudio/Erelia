using UnityEngine;

public class ChunkPresenter : MonoBehaviour
{
	[SerializeField] private Chunk chunk = new Chunk();
	[SerializeField] private ChunkView chunkView;
	[SerializeField] private VoxelRegistry voxelRegistry;

	public Chunk Chunk => chunk;
	public ChunkView View => chunkView;

	public void Assign(Chunk targetChunk)
	{
		chunk = targetChunk;
		Rebuild();
	}

	public void Rebuild()
	{
		if (chunk == null)
		{
			return ;
		}

		chunkView.SetRenderMesh(VoxelMesher.BuildRenderMesh(chunk.Cells, voxelRegistry));
		chunkView.SetCollisionMesh(VoxelMesher.BuildColliderMesh(chunk.Cells, voxelRegistry));
	}

	private void Awake()
	{
		Rebuild();
	}
}
