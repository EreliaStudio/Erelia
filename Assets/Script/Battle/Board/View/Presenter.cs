using UnityEngine;

namespace Battle.Board.View
{
	public class Presenter : MonoBehaviour
	{
		[SerializeField] private Material voxelMaterial = null;
		[SerializeField] private Material cellMaskMaterial = null;

		private void Awake()
		{
			
		}

		public void Configure(Material inputVoxelMaterial, Material inputCellMaskMaterial)
		{
			voxelMaterial = inputVoxelMaterial;
			cellMaskMaterial = inputCellMaskMaterial;
		}
	}
}
