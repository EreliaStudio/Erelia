using UnityEngine;

namespace Battle.Board.View
{
	public class VoxelPresenter : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter = null;
		[SerializeField] private MeshRenderer meshRenderer = null;

		private void Awake()
		{
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}

		public void Initialize(Material voxelMaterial)
		{
			meshRenderer.sharedMaterial = voxelMaterial;
		}

		public void Rebuild(Battle.Board.Model.Data data)
		{
			if (data == null)
			{
				meshFilter.sharedMesh = null;
				return;
			}
			Mesh mesh = Core.Utils.Mesher.VoxelRenderMesher.Build(data.Cells);
			mesh.name = "VoxelRenderMesh";
			meshFilter.sharedMesh = mesh;
		}
	}
}
