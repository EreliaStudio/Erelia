using UnityEngine;

public class BoardView : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;

	public MeshFilter MeshFilter => meshFilter;

	public void SetMaskMesh(Mesh mesh)
	{
		DestroyMesh(meshFilter.sharedMesh);
		meshFilter.sharedMesh = mesh;
	}

	private void OnDestroy()
	{
		DestroyMesh(meshFilter.sharedMesh);
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
