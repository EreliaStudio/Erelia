using UnityEngine;

namespace Erelia.World.Chunk
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private MeshCollider meshCollider;

		public void SetRenderMesh(Mesh renderMesh)
		{
			if (meshFilter != null && meshFilter.sharedMesh != null)
			{
				DestroyMesh(meshFilter.sharedMesh);
				meshFilter.sharedMesh = null;
			}

			meshFilter.sharedMesh = renderMesh;
			Erelia.Logger.Log("[Erelia.World.Chunk.View] Render mesh applied.");
		}

		public void SetCollisionMesh(Mesh collisionMesh)
		{
			
			if (meshCollider != null && meshCollider.sharedMesh != null)
			{
				DestroyMesh(meshCollider.sharedMesh);
				meshCollider.sharedMesh = null;
			}
			meshCollider.sharedMesh = collisionMesh;
			Erelia.Logger.Log("[Erelia.World.Chunk.View] Collision mesh applied.");
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

