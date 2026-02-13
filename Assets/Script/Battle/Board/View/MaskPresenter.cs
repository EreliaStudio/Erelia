using UnityEngine;

namespace Battle.Board.View
{
	public class MaskPresenter : MonoBehaviour
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
			Mesh mesh = Core.Utils.Mesher.VoxelMaskRenderMesher.Build(data.Cells, data.MaskCells);
			mesh.name = "VoxelMaskRenderMesh";
			meshFilter.sharedMesh = mesh;
		}
	}
}
