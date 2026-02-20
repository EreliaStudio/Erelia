using UnityEngine;

namespace Erelia.World.Chunk
{
	public sealed class BushView : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private MeshCollider meshCollider;

		public event System.Action<Collider> TriggerEntered;

		private void Awake()
		{
			EnsureTrigger();
		}

		private void OnValidate()
		{
			EnsureTrigger();
		}

		public void ApplyMeshes(Mesh renderMesh, Mesh collisionMesh)
		{
			DisposeMeshes();

			if (meshFilter != null)
			{
				meshFilter.sharedMesh = renderMesh;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.BushView] MeshFilter is not assigned.");
			}

			if (meshCollider != null)
			{
				meshCollider.sharedMesh = collisionMesh;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.BushView] MeshCollider is not assigned.");
			}
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

		private void OnTriggerEnter(Collider other)
		{
			TriggerEntered?.Invoke(other);
		}

		private void EnsureTrigger()
		{
			if (meshCollider != null)
			{
				meshCollider.isTrigger = true;
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
