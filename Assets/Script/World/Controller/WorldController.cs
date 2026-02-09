using System.Collections.Generic;
using UnityEngine;

namespace World.Controller
{
	public class WorldController : MonoBehaviour
	{
		[SerializeField] private Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);

		private readonly Dictionary<World.Chunk.Model.Coordinates, World.Controller.ChunkController> controllers = new Dictionary<World.Chunk.Model.Coordinates, World.Controller.ChunkController>();

		private void Awake()
		{
			
		}

		public void Configure(Player.Controller.KeyboardMotionController controller, Vector3Int range)
		{
			playerController = controller;
			viewRange = range;
		}

		private void OnEnable()
		{
			if (playerController != null)
			{
				playerController.ChunkCoordinateChanged += HandlePlayerChunkChanged;
			}
		}

		private void OnDisable()
		{
			if (playerController != null)
			{
				playerController.ChunkCoordinateChanged -= HandlePlayerChunkChanged;
			}
		}

		private void Start()
		{
			RefreshActive();
		}

		public void RefreshActive()
		{
			if (Utils.ServiceLocator.Instance.WorldService == null)
			{
				Debug.LogError("WorldController: World.Service is not available (ServiceLocator missing).");
				return;
			}

			if (playerController == null)
			{
				Debug.LogError("WorldController: Player controller is not assigned.");
				return;
			}

			World.Chunk.Model.Coordinates center = World.Chunk.Model.Coordinates.FromWorld(playerController.transform.position);

			var needed = new HashSet<World.Chunk.Model.Coordinates>();
			for (int x = -viewRange.x; x <= viewRange.x; x++)
			{
				for (int y = -viewRange.y; y <= viewRange.y; y++)
				{
					for (int z = -viewRange.z; z <= viewRange.z; z++)
					{
						var coord = new World.Chunk.Model.Coordinates(center.X + x, center.Y + y, center.Z + z);
						needed.Add(coord);
						EnsureChunk(coord);
					}
				}
			}

			var toRemove = new List<World.Chunk.Model.Coordinates>();
			foreach (KeyValuePair<World.Chunk.Model.Coordinates, World.Controller.ChunkController> pair in controllers)
			{
				if (!needed.Contains(pair.Key))
				{
					toRemove.Add(pair.Key);
				}
			}

			for (int i = 0; i < toRemove.Count; i++)
			{
				RemoveChunk(toRemove[i]);
			}
		}

		private void HandlePlayerChunkChanged(World.Chunk.Model.Coordinates coord)
		{
			RefreshActive();
		}

		public World.Controller.ChunkController EnsureChunk(World.Chunk.Model.Coordinates coord)
		{
			if (controllers.TryGetValue(coord, out World.Controller.ChunkController existing))
			{
				return existing;
			}

			World.Chunk.Model.Data data = Utils.ServiceLocator.Instance.WorldService.GetOrCreateChunk(coord);
			if (data == null)
			{
				return null;
			}

			var chunkObject = new GameObject("ChunkController " + coord);
			chunkObject.transform.SetParent(transform, false);
			chunkObject.transform.localPosition = new Vector3(
				coord.X * World.Chunk.Model.Data.SizeX,
				coord.Y * World.Chunk.Model.Data.SizeY,
				coord.Z * World.Chunk.Model.Data.SizeZ
			);

			var controller = chunkObject.AddComponent<World.Controller.ChunkController>();
			controller.Initialize(coord, data);
			controllers.Add(coord, controller);

			return controller;
		}

		public void RemoveChunk(World.Chunk.Model.Coordinates coord)
		{
			if (!controllers.TryGetValue(coord, out World.Controller.ChunkController controller))
			{
				return;
			}

			if (controller != null)
			{
				Destroy(controller.gameObject);
			}

			controllers.Remove(coord);
		}
	}
}
