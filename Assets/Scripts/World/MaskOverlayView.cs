using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MaskOverlayView : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;
	[SerializeField] private MeshRenderer meshRenderer;

	public MeshFilter MeshFilter => meshFilter;
	public MeshRenderer MeshRenderer => meshRenderer;

	public void SetMaskMesh(Mesh mesh)
	{
		DestroyMesh(meshFilter.sharedMesh);
		meshFilter.sharedMesh = mesh;
		if (meshRenderer != null)
		{
			meshRenderer.enabled = mesh != null && mesh.vertexCount > 0;
		}
	}

	public void SetVisible(bool visible)
	{
		gameObject.SetActive(visible);
	}

	private void Awake()
	{
		if (meshFilter == null)
		{
			Logger.LogError("[MaskOverlayView] MeshFilter is not assigned in the inspector. Please assign a MeshFilter to the MaskOverlayView component.", Logger.Severity.Critical, this);
		}

		if (meshRenderer == null)
		{
			Logger.LogError("[MaskOverlayView] MeshRenderer is not assigned in the inspector. Please assign a MeshRenderer to the MaskOverlayView component.", Logger.Severity.Critical, this);
		}

		ApplyRendererSettings();
	}

	private void OnDestroy()
	{
		if (meshFilter != null)
		{
			DestroyMesh(meshFilter.sharedMesh);
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
