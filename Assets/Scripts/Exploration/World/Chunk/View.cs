using UnityEngine;

namespace Erelia.Exploration.World.Chunk
{
	/// <summary>
	/// View component that holds render and collision meshes for a chunk.
	/// Owns mesh components and replaces render/collision meshes while cleaning up old ones.
	/// </summary>
	public sealed class View : MonoBehaviour
	{
		/// <summary>
		/// Mesh filter used for rendering.
		/// </summary>
		[SerializeField] private MeshFilter meshFilter;

		/// <summary>
		/// Mesh renderer used for rendering.
		/// </summary>
		[SerializeField] private MeshRenderer meshRenderer;

		/// <summary>
		/// Mesh collider used for physics.
		/// </summary>
		[SerializeField] private MeshCollider meshCollider;

		/// <summary>
		/// Assigns a new render mesh and disposes the previous one.
		/// </summary>
		/// <param name="renderMesh">New render mesh.</param>
		public void SetRenderMesh(Mesh renderMesh)
		{
			// Clean up previous render mesh.
			if (meshFilter != null && meshFilter.sharedMesh != null)
			{
				DestroyMesh(meshFilter.sharedMesh);
				meshFilter.sharedMesh = null;
			}

			// Assign new mesh.
			meshFilter.sharedMesh = renderMesh;
		}

		/// <summary>
		/// Assigns a new collision mesh and disposes the previous one.
		/// </summary>
		/// <param name="collisionMesh">New collision mesh.</param>
		public void SetCollisionMesh(Mesh collisionMesh)
		{
			// Clean up previous collision mesh.
			if (meshCollider != null && meshCollider.sharedMesh != null)
			{
				DestroyMesh(meshCollider.sharedMesh);
				meshCollider.sharedMesh = null;
			}
			
			// Assign new mesh.
			meshCollider.sharedMesh = collisionMesh;
		}

		/// <summary>
		/// Destroys a mesh using the proper Unity API depending on play mode.
		/// </summary>
		/// <param name="mesh">Mesh to destroy.</param>
		private static void DestroyMesh(Mesh mesh)
		{
			// Ignore null meshes.
			if (mesh == null)
			{
				return;
			}

			// Use the correct destroy method depending on play mode.
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

