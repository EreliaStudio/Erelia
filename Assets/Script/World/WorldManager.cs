using UnityEngine;

namespace World
{
	public class WorldManager : MonoBehaviour
	{
		[SerializeField] private Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);
		[SerializeField] private Material chunkMaterial = null;

		private World.View.WorldView worldView = null;
		private World.Controller.WorldController worldController = null;

		private void Awake()
		{
			InitializeWorldView();
			InitializeWorldController();

			//Need to subscribe to the event inside player service, thought the service locator, to call for the worldView and worldController update of the visible chunks
		}

		private void InitializeWorldView()
		{
			var go = new GameObject("WorldView");
			go.transform.SetParent(transform, false);
			worldView = go.AddComponent<World.View.WorldView>();
			worldView.Configure(chunkMaterial, playerController, viewRange);
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
