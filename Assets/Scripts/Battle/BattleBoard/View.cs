using UnityEngine;

namespace Erelia.Battle.Board
{
	/// <summary>
	/// View component that owns the board render, collision, and mask meshes.
	/// Replaces meshes on update and disposes previous instances.
	/// </summary>
	public sealed class View : MonoBehaviour
	{
		/// <summary>
		/// Mesh filter used for board rendering.
		/// </summary>
		[SerializeField] private MeshFilter meshFilter;
		/// <summary>
		/// Mesh renderer used for board rendering.
		/// </summary>
		[SerializeField] private MeshRenderer meshRenderer;
		/// <summary>
		/// Mesh collider used for board collisions.
		/// </summary>
		[SerializeField] private MeshCollider meshCollider;
		/// <summary>
		/// Mesh filter used for mask overlay rendering.
		/// </summary>
		[SerializeField] private MeshFilter maskMeshFilter;
		/// <summary>
		/// Mesh renderer used for mask overlay rendering.
		/// </summary>
		[SerializeField] private MeshRenderer maskMeshRenderer;

		/// <summary>
		/// Assigns a new render mesh and disposes the previous one.
		/// </summary>
		public void SetRenderMesh(Mesh renderMesh)
		{
			// Replace the render mesh safely.
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

		/// <summary>
		/// Assigns a new collision mesh and disposes the previous one.
		/// </summary>
		public void SetCollisionMesh(Mesh collisionMesh)
		{
			// Replace the collision mesh safely.
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

		/// <summary>
		/// Assigns a new mask mesh and updates the mask renderer.
		/// </summary>
		public void SetMaskMesh(Mesh maskMesh)
		{
			// Replace the mask mesh and toggle its renderer.
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

		/// <summary>
		/// Clears the mask mesh and hides the mask renderer.
		/// </summary>
		public void ClearMaskMesh()
		{
			// Remove the current mask mesh.
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

		/// <summary>
		/// Destroys a mesh using the correct API for play mode.
		/// </summary>
		private static void DestroyMesh(Mesh mesh)
		{
			// Ignore null meshes.
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
