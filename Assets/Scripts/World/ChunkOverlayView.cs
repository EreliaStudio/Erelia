using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkOverlayView : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private Material material;

	public MeshFilter MeshFilter => meshFilter;
	public MeshRenderer MeshRenderer => meshRenderer;
	public Material Material => material;

	public void SetOverlayMesh(Mesh mesh)
	{
		CacheReferences();
		DestroyMesh(meshFilter.sharedMesh);
		meshFilter.sharedMesh = mesh;
	}

	public void SetVisible(bool visible)
	{
		gameObject.SetActive(visible);
	}

	private void Reset()
	{
		CacheReferences();
		ApplyMaterial();
	}

	private void OnValidate()
	{
		CacheReferences();
		ApplyMaterial();
	}

	private void Awake()
	{
		CacheReferences();
		ApplyMaterial();
	}

	private void OnDestroy()
	{
		if (meshFilter != null)
		{
			DestroyMesh(meshFilter.sharedMesh);
		}
	}

	private void CacheReferences()
	{
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
		}

		if (meshRenderer == null)
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}
	}

	private void ApplyMaterial()
	{
		if (meshRenderer != null && material != null)
		{
			meshRenderer.sharedMaterial = material;
		}
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
