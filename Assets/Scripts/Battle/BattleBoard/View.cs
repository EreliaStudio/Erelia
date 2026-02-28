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
				if (maskMeshRenderer.enabled && maskMeshRenderer.sharedMaterial == null)
				{
					Debug.LogWarning("[Erelia.Battle.Board.View] Assign a mask material on the mask mesh renderer.");
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
	}
}
