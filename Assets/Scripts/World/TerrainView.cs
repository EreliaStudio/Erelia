using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainView : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;
	[SerializeField] private MeshRenderer meshRenderer;
	[SerializeField] private MeshCollider meshCollider;

	public MeshFilter MeshFilter => meshFilter;
	public MeshRenderer MeshRenderer => meshRenderer;
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

	public void SetVisible(bool visible)
	{
		gameObject.SetActive(visible);
	}

	private void Awake()
	{
		if (meshFilter == null)
		{
			Logger.LogError("[TerrainView] MeshFilter is not assigned in the inspector. Please assign a MeshFilter to the TerrainView component.", Logger.Severity.Critical, this);
		}

		if (meshRenderer == null)
		{
			Logger.LogError("[TerrainView] MeshRenderer is not assigned in the inspector. Please assign a MeshRenderer to the TerrainView component.", Logger.Severity.Critical, this);
		}

		if (meshCollider == null)
		{
			Logger.LogError("[TerrainView] MeshCollider is not assigned in the inspector. Please assign a MeshCollider to the TerrainView component.", Logger.Severity.Critical, this);
		}

		ApplyRendererSettings();
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

	private void ApplyRendererSettings()
	{
		if (meshRenderer == null)
		{
			return;
		}

		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.receiveShadows = false;
		meshRenderer.lightProbeUsage = LightProbeUsage.Off;
		meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
		meshRenderer.allowOcclusionWhenDynamic = true;
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
