using UnityEngine;

namespace Erelia.World.Chunk
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private MeshCollider meshCollider;

		public void ApplyMeshes(Mesh renderMesh, Mesh collisionMesh)
		{
			DisposeMeshes();

			if (meshFilter != null)
			{
				meshFilter.sharedMesh = renderMesh;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.View] MeshFilter is not assigned.");
			}

			if (meshCollider != null)
			{
				meshCollider.sharedMesh = collisionMesh;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.View] MeshCollider is not assigned.");
			}

			Erelia.Logger.Log("[Erelia.World.Chunk.View] Meshes applied.");
		}

		public void DisposeMeshes()
		{
			if (meshFilter != null && meshFilter.sharedMesh != null)
			{
				DestroyMesh(meshFilter.sharedMesh);
				meshFilter.sharedMesh = null;
			}

			if (meshCollider != null && meshCollider.sharedMesh != null)
			{
				DestroyMesh(meshCollider.sharedMesh);
				meshCollider.sharedMesh = null;
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

