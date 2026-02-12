using UnityEngine;

namespace Battle.Board.View
{
	public class Presenter : MonoBehaviour
	{
		private Battle.Board.View.MaskPresenter maskPresenter = null;
		private Battle.Board.View.VoxelPresenter voxelPresenter = null;

		private void Awake()
		{
			InitializeVoxelPresenter();
			InitializeMaskPresenter();
		}

		private void InitializeVoxelPresenter()
		{
			var go = new GameObject("VoxelPresenter");
			go.transform.SetParent(transform, false);
			voxelPresenter = go.AddComponent<Battle.Board.View.VoxelPresenter>();
		}

		private void InitializeMaskPresenter()
		{
			var go = new GameObject("MaskPresenter");
			go.transform.SetParent(transform, false);
			maskPresenter = go.AddComponent<Battle.Board.View.MaskPresenter>();
		}

		public void Initialize(Material voxelMaterial, Material cellMaskMaterial)
		{
			voxelPresenter.Initialize(voxelMaterial);
			maskPresenter.Initialize(cellMaskMaterial);
		}

		public void Rebuild(Battle.Board.Model.Data data)
		{
			RebuildVoxels(data);
			RebuildMask(data);
		}

		public void RebuildVoxels(Battle.Board.Model.Data data)
		{
			voxelPresenter.Rebuild(data);
		}

		public void RebuildMask(Battle.Board.Model.Data data)
		{
			maskPresenter.Rebuild(data);
		}
	}
}
