using UnityEngine;
using Utils;

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

		private void OnEnable()
		{
			ServiceLocator.Instance.BattleBoardService.Data.OnVoxelEdition += RebuildVoxels;
			ServiceLocator.Instance.BattleBoardService.Data.OnMaskEdition += RebuildMask;
			RebuildVoxels(ServiceLocator.Instance.BattleBoardService.Data);
			RebuildMask(ServiceLocator.Instance.BattleBoardService.Data);
		}

		private void OnDisable()
		{
			ServiceLocator.Instance.BattleBoardService.Data.OnVoxelEdition -= RebuildVoxels;
			ServiceLocator.Instance.BattleBoardService.Data.OnMaskEdition -= RebuildMask;
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
			Debug.Log("Rebuilding voxels in Presenter");
			voxelPresenter.Rebuild(data);
		}

		public void RebuildMask(Battle.Board.Model.Data data)
		{
			maskPresenter.Rebuild(data);
		}
	}
}
