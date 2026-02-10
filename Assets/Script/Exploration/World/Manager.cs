using UnityEngine;

namespace World
{
	public class Manager : MonoBehaviour
	{
		[SerializeField] private Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);
		[SerializeField] private Material voxelMaterial = null;

		private World.View.WorldView worldView = null;
		private World.Controller.WorldController worldController = null;

		private void Awake()
		{
			InitializeWorldView();
			InitializeWorldController();
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
			worldView.RefreshVisible();
			worldController.RefreshActive();
		}

		private void InitializeWorldView()
		{
			var go = new GameObject("WorldView");
			go.transform.SetParent(transform, false);
			worldView = go.AddComponent<World.View.WorldView>();
			worldView.Configure(voxelMaterial, playerController, viewRange);
		}

		private void InitializeWorldController()
		{
			var go = new GameObject("WorldController");
			go.transform.SetParent(transform, false);
			worldController = go.AddComponent<World.Controller.WorldController>();
			worldController.Configure(playerController, viewRange);
		}
	}
}
