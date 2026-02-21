using System.Collections.Generic;
using UnityEngine;

namespace Erelia.World.Chunk
{
	public sealed class BushView : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter;
		[SerializeField] private MeshRenderer meshRenderer;
		[SerializeField] private MeshCollider meshCollider;

		private readonly List<MeshCollider> collisionColliders = new List<MeshCollider>();
		private Transform collisionRoot;

		public event System.Action<Collider> TriggerEntered;

		private void Awake()
		{
			EnsureTrigger();
		}

		private void OnValidate()
		{
			EnsureTrigger();
		}

		public void ApplyMeshes(Mesh renderMesh, List<Mesh> collisionMeshes)
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

			CreateCollisionColliders(collisionMeshes);
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

			DestroyCollisionColliders();
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

		private void CreateCollisionColliders(List<Mesh> collisionMeshes)
		{
			if (collisionMeshes == null || collisionMeshes.Count == 0)
			{
				return;
			}

			EnsureCollisionRoot();
			for (int i = 0; i < collisionMeshes.Count; i++)
			{
				Mesh mesh = collisionMeshes[i];
				if (mesh == null)
				{
					continue;
				}

				GameObject child = new GameObject($"BushCollision_{i}");
				child.transform.SetParent(collisionRoot, false);
				MeshCollider collider = child.AddComponent<MeshCollider>();
				collider.sharedMesh = mesh;
				collider.convex = true;
				collider.isTrigger = true;
				collisionColliders.Add(collider);
			}
		}

		private void EnsureCollisionRoot()
		{
			if (collisionRoot != null)
			{
				return;
			}

			GameObject root = new GameObject("BushCollisionRoot");
			root.transform.SetParent(transform, false);
			collisionRoot = root.transform;
		}

		private void DestroyCollisionColliders()
		{
			for (int i = 0; i < collisionColliders.Count; i++)
			{
				MeshCollider collider = collisionColliders[i];
				if (collider == null)
				{
					continue;
				}

				if (collider.sharedMesh != null)
				{
					DestroyMesh(collider.sharedMesh);
					collider.sharedMesh = null;
				}

				if (Application.isPlaying)
				{
					Destroy(collider.gameObject);
				}
				else
				{
					DestroyImmediate(collider.gameObject);
				}
			}

			collisionColliders.Clear();
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
