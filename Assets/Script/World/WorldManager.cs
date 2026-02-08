using UnityEngine;

namespace World
{
	public class WorldManager : MonoBehaviour
	{
		[SerializeField] private Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Transform target = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);
		[SerializeField] private Material chunkMaterial = null;

		[SerializeField] private World.View.WorldView worldView = null;
		[SerializeField] private World.Controller.WorldController worldController = null;

		private void Awake()
		{
			ResolveTarget();
			InitializeWorldView();
			InitializeWorldController();
		}

		private void ResolveTarget()
		{
			if (playerController != null)
			{
				target = playerController.transform;
			}
		}

		private void InitializeWorldView()
		{
			var go = new GameObject("WorldView");
			go.transform.SetParent(transform, false);
			worldView = go.AddComponent<World.View.WorldView>();
			worldView.Configure(chunkMaterial, target, playerController, viewRange);
		}

		private void InitializeWorldController()
		{
			var go = new GameObject("WorldController");
			go.transform.SetParent(transform, false);
			worldController = go.AddComponent<World.Controller.WorldController>();
			worldController.Configure(target, playerController, viewRange);
		}
	}
}
