using UnityEngine;

public class ChunkView : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;
	[SerializeField] private MeshCollider meshCollider;

	public MeshFilter MeshFilter => meshFilter;
	public MeshCollider MeshCollider => meshCollider;

	public void SetRenderMesh(Mesh mesh)
	{
		DestroyMesh(meshFilter.sharedMesh);
		meshFilter.sharedMesh = mesh;
	}

	public void SetCollisionMesh(Mesh mesh)
	{
		DestroyMesh(meshCollider.sharedMesh);
		meshCollider.sharedMesh = mesh;
	}

	private void OnDestroy()
	{
		DestroyMesh(meshFilter.sharedMesh);
		DestroyMesh(meshCollider.sharedMesh);
	}

	private static void DestroyMesh(Mesh mesh)
	{
		if (mesh == null)
		{
			return;
		}

		if (Application.isPlaying)
		{
			Destroy(mesh);
		}
		else
		{
			DestroyImmediate(mesh);
		}
	}
}
