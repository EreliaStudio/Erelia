using UnityEngine;

namespace Erelia.Battle.Board
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private MeshCollider meshCollider;
		[SerializeField] private MeshFilter maskMeshFilter;
		[SerializeField] private MeshRenderer maskMeshRenderer;
		[SerializeField] private Material[] maskMaterialsByType;

		public void SetRenderMesh(Mesh renderMesh)
		{
			if (meshFilter != null && meshFilter.sharedMesh != null)
			{
				DestroyMesh(meshFilter.sharedMesh);
				meshFilter.sharedMesh = null;
			}

			if (meshFilter != null)
			{
				meshFilter.sharedMesh = renderMesh;
			}
		}

		public void SetCollisionMesh(Mesh collisionMesh)
		{
			if (meshCollider != null && meshCollider.sharedMesh != null)
			{
				DestroyMesh(meshCollider.sharedMesh);
				meshCollider.sharedMesh = null;
			}

			if (meshCollider != null)
			{
				meshCollider.sharedMesh = collisionMesh;
			}
		}

		public void SetMaskMesh(Mesh maskMesh)
		{
			if (maskMeshFilter == null)
			{
				return;
			}

			if (maskMeshFilter.sharedMesh != null)
			{
				DestroyMesh(maskMeshFilter.sharedMesh);
				maskMeshFilter.sharedMesh = null;
			}

			maskMeshFilter.sharedMesh = maskMesh;

			if (maskMeshRenderer != null)
			{
				maskMeshRenderer.enabled = maskMesh != null && maskMesh.vertexCount > 0;
				if (maskMaterialsByType != null && maskMaterialsByType.Length > 0)
				{
					maskMeshRenderer.sharedMaterials = EnsureMaterialCount(maskMaterialsByType, maskMesh != null ? maskMesh.subMeshCount : 0);
				}
			}
		}

		public void ClearMaskMesh()
		{
			if (maskMeshFilter == null)
			{
				return;
			}

			if (maskMeshFilter.sharedMesh != null)
			{
				DestroyMesh(maskMeshFilter.sharedMesh);
				maskMeshFilter.sharedMesh = null;
			}

			if (maskMeshRenderer != null)
			{
				maskMeshRenderer.enabled = false;
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

		private static Material[] EnsureMaterialCount(Material[] source, int targetCount)
		{
			if (source == null)
			{
				return null;
			}

			if (targetCount <= 0 || source.Length == targetCount)
			{
				return source;
			}

			var resized = new Material[targetCount];
			int copyCount = Mathf.Min(source.Length, targetCount);
			for (int i = 0; i < copyCount; i++)
			{
				resized[i] = source[i];
			}
			return resized;
		}
	}
}
