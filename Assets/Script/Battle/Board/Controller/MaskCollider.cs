using System.Collections.Generic;
using UnityEngine;

namespace Battle.Board.Controller
{
	public class MaskCollider : MonoBehaviour
	{
		private readonly List<GameObject> colliderObjects = new List<GameObject>();

		public void Rebuild(Battle.Board.Model.Data data)
		{
			Clear();

			if (data == null)
			{
				return;
			}

			// List<Mesh> meshes = Utils.Mesher.MaskCollisionMesher.Build(data.MaskCells);
			// for (int i = 0; i < meshes.Count; i++)
			// {
			// 	CreateCollider(meshes[i]);
			// }
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
			collider.isTrigger = false;
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
