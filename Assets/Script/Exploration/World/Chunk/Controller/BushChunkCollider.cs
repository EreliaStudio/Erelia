using System.Collections.Generic;
using UnityEngine;

namespace World.Chunk.Controller
{
	public class BushChunkCollider : MonoBehaviour
	{
		private Transform root = null;

		private readonly List<GameObject> colliderObjects = new List<GameObject>();

		private void Awake()
		{
			EnsureRoot();
		}

		private void Reset()
		{
			EnsureRoot();
		}

		private void EnsureRoot()
		{
			if (root != null)
			{
				return;
			}

			var go = new GameObject("BushColliders");
			go.transform.SetParent(transform, false);
			root = go.transform;
		}

		public void Rebuild(World.Chunk.Model.Data data)
		{
			Clear();

			if (data == null)
			{
				return;
			}

			List<Mesh> meshes = Utils.Mesher.BushCollisionMesher.Build(data.Cells);
			for (int i = 0; i < meshes.Count; i++)
			{
				CreateCollider(meshes[i]);
			}
		}

		private void CreateCollider(Mesh mesh)
		{
			if (mesh == null || mesh.vertexCount == 0)
			{
				return;
			}

			var go = new GameObject(mesh.name);
			go.transform.SetParent(root, false);
			var collider = go.AddComponent<MeshCollider>();
			collider.sharedMesh = mesh;
			collider.convex = true;
			collider.isTrigger = true;
			go.AddComponent<World.Chunk.Controller.BushTriggerReporter>();
			colliderObjects.Add(go);
		}

		private void Clear()
		{
			for (int i = 0; i < colliderObjects.Count; i++)
			{
				if (colliderObjects[i] != null)
				{
					Destroy(colliderObjects[i]);
				}
			}
			colliderObjects.Clear();
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}
