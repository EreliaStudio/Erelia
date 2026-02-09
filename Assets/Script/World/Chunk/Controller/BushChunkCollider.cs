using System.Collections.Generic;
using UnityEngine;

namespace World.Controller
{
	public class BushChunkCollider : MonoBehaviour
	{
		[SerializeField] private Transform root = null;
		[SerializeField] private bool isTrigger = true;

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

		public void Initialize(World.Chunk.Model.Data data)
		{
			Rebuild(data);
		}

		public void Rebuild(World.Chunk.Model.Data data)
		{
			Clear();

			if (data == null)
			{
				return;
			}

			List<Mesh> meshes = World.Chunk.Controller.BushCollisionMesher.Build(data.Cells);
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
			if (isTrigger)
			{
				collider.convex = true;
			}
			collider.isTrigger = isTrigger;
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
