using System.Collections.Generic;
using UnityEngine;

namespace Exploration.World.Chunk.Controller
{
	public class BushChunkCollider : MonoBehaviour
	{
		private readonly List<GameObject> colliderObjects = new List<GameObject>();

		public void Rebuild(Exploration.World.Chunk.Model.Data data)
		{
			Clear();

			if (data == null)
			{
				return;
			}

			List<Mesh> meshes = Core.Utils.Mesher.BushCollisionMesher.Build(data.Cells);
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
			go.transform.SetParent(transform, false);
			var collider = go.AddComponent<MeshCollider>();
			collider.sharedMesh = mesh;
			collider.convex = true;
			collider.isTrigger = true;
			go.AddComponent<Exploration.World.Chunk.Controller.BushTriggerReporter>();
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
