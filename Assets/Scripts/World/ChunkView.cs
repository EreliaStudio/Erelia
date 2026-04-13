using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkView : MonoBehaviour
{
	private const string DefaultMaterialResourcePath = "Voxel/Definition/VoxelMaterial";

	[SerializeField] private MeshFilter meshFilter;
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private MeshCollider meshCollider;
	[SerializeField] private Material material;

	public MeshFilter MeshFilter => meshFilter;
	public MeshRenderer MeshRenderer => meshRenderer;
	public MeshCollider MeshCollider => meshCollider;
	public Material Material => material;

	public void SetRenderMesh(Mesh mesh)
	{
		CacheReferences();
		DestroyMesh(meshFilter.sharedMesh);
		meshFilter.sharedMesh = mesh;
	}

	public void SetCollisionMesh(Mesh mesh)
	{
		CacheReferences();
		DestroyMesh(meshCollider.sharedMesh);
		meshCollider.sharedMesh = mesh;
	}

	private void Reset()
	{
		CacheReferences();
		LoadDefaultMaterial();
		ApplyMaterial();
	}

	private void OnValidate()
	{
		CacheReferences();
		LoadDefaultMaterial();
		ApplyMaterial();
	}

	private void Awake()
	{
		CacheReferences();
		LoadDefaultMaterial();
		ApplyMaterial();
	}

	private void OnDestroy()
	{
		if (meshFilter != null)
		{
			DestroyMesh(meshFilter.sharedMesh);
		}

		if (meshCollider != null)
		{
			DestroyMesh(meshCollider.sharedMesh);
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

		if (meshCollider == null)
		{
			meshCollider = GetComponent<MeshCollider>();
		}
	}

	private void ApplyMaterial()
	{
		if (meshRenderer != null && material != null)
		{
			meshRenderer.sharedMaterial = material;
		}
	}

	private void LoadDefaultMaterial()
	{
		if (material == null)
		{
			material = Resources.Load<Material>(DefaultMaterialResourcePath);
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
