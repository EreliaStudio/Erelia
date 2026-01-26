using UnityEngine;

public class ChunkView : MonoBehaviour
{
	public ChunkCoord Coord { get; private set; }
	public Chunk Chunk { get; private set; }

	private ChunkMesher mesher;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public void Initialize(ChunkCoord coord, Chunk chunk, ChunkMesher mesherInstance, Material material)
	{
		Coord = coord;
		Chunk = chunk;
		mesher = mesherInstance;

		EnsureComponents();
		if (material != null)
		{
			meshRenderer.sharedMaterial = material;
		}

		RebuildMesh();
	}

	public void RebuildMesh()
	{
		if (Chunk == null || mesher == null)
		{
			return;
		}

		meshFilter.sharedMesh = mesher.BuildMesh(Chunk);
	}

	private void EnsureComponents()
	{
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
		}

		if (meshRenderer == null)
		{
			meshRenderer = GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
			}
		}
	}
}
