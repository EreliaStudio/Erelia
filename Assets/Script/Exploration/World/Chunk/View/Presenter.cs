using UnityEngine;

namespace Exploration.World.Chunk.View
{
	public class Presenter : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter = null;
		[SerializeField] private MeshRenderer meshRenderer = null;

		public Exploration.World.Chunk.Model.Coordinates Coordinates { get; private set; }

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();
		}

		private void CacheComponents()
		{
			if (meshFilter == null)
			{
				meshFilter = GetComponent<MeshFilter>();
				if (meshFilter == null)
				{
					meshFilter = gameObject.AddComponent<MeshFilter>();
				}
			}

			if (meshRenderer == null)
			{
				meshRenderer = GetComponent<MeshRenderer>();
				if (meshRenderer == null)
				{
					meshRenderer = gameObject.AddComponent<MeshRenderer>();
				}
			}
		}

		public void Initialize(Exploration.World.Chunk.Model.Coordinates coord, Exploration.World.Chunk.Model.Data data, Material material)
		{
			Coordinates = coord;
			if (material != null)
			{
				meshRenderer.sharedMaterial = material;
			}

			Rebuild(data);
		}

		public void Rebuild(Exploration.World.Chunk.Model.Data data)
		{
			if (data == null)
			{
				meshFilter.sharedMesh = null;
				Debug.LogWarning("Chunk.Presenter: Rebuild received null data for " + Coordinates, this);
				return;
			}

			Mesh mesh = Core.Utils.Mesher.VoxelRenderMesher.Build(data.Cells);
			mesh.name = "ChunkMesh " + Coordinates;
			meshFilter.sharedMesh = mesh;
		}
	}
}
