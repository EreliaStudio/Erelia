using UnityEngine;

namespace World
{
	public class Manager : MonoBehaviour
	{
		[SerializeField] private Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);
		[SerializeField] private Material voxelMaterial = null;

		private World.View.Presenter worldPresenter = null;
		private World.Controller.BodyCollider worldCollider = null;

		private void Awake()
		{
			InitializeWorldPresenter();
			InitializeWorldCollider();
		}

		private void OnEnable()
		{
			Utils.ServiceLocator.Instance.PlayerService.PlayerChunkCoordinateChanged += HandlePlayerChunkChanged;
		}

		private void OnDisable()
		{
			Utils.ServiceLocator.Instance.PlayerService.PlayerChunkCoordinateChanged -= HandlePlayerChunkChanged;
		}

		private void HandlePlayerChunkChanged(World.Chunk.Model.Coordinates coord)
		{
			worldPresenter.RefreshVisible();
			worldCollider.RefreshActive();
		}

		private void InitializeWorldPresenter()
		{
			var go = new GameObject("WorldPresenter");
			go.transform.SetParent(transform, false);
			worldPresenter = go.AddComponent<World.View.Presenter>();
			worldPresenter.Configure(voxelMaterial, playerController, viewRange);
		}

		private void InitializeWorldCollider()
		{
			var go = new GameObject("WorldCollider");
			go.transform.SetParent(transform, false);
			worldCollider = go.AddComponent<World.Controller.BodyCollider>();
			worldCollider.Configure(playerController, viewRange);
		}
	}
}
