using System.Collections.Generic;
using UnityEngine;

namespace Exploration.World.Controller
{
	public class BodyCollider : MonoBehaviour
	{
		[SerializeField] private Exploration.Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);

		private readonly Dictionary<Exploration.World.Chunk.Model.Coordinates, Exploration.World.Chunk.Controller.BodyCollider> controllers = new Dictionary<Exploration.World.Chunk.Model.Coordinates, Exploration.World.Chunk.Controller.BodyCollider>();

		private void Awake()
		{
			
		}

		public void Configure(Exploration.Player.Controller.KeyboardMotionController controller, Vector3Int range)
		{
			playerController = controller;
			viewRange = range;
		}

		private void Start()
		{
			RefreshActive();
		}

		public void RefreshActive()
		{
			if (Utils.ServiceLocator.Instance.WorldService == null)
			{
				Debug.LogError("World.Collider: World.Service is not available (ServiceLocator missing).");
				return;
			}

			if (playerController == null)
			{
				Debug.LogError("World.Collider: Player controller is not assigned.");
				return;
			}

			Exploration.World.Chunk.Model.Coordinates center = Exploration.World.Chunk.Model.Coordinates.FromWorld(playerController.transform.position);

			var needed = new HashSet<Exploration.World.Chunk.Model.Coordinates>();
			for (int x = -viewRange.x; x <= viewRange.x; x++)
			{
				for (int y = -viewRange.y; y <= viewRange.y; y++)
				{
					for (int z = -viewRange.z; z <= viewRange.z; z++)
					{
						var coord = new Exploration.World.Chunk.Model.Coordinates(center.X + x, center.Y + y, center.Z + z);
						needed.Add(coord);
						EnsureChunk(coord);
					}
				}
			}

			var toRemove = new List<Exploration.World.Chunk.Model.Coordinates>();
			foreach (KeyValuePair<Exploration.World.Chunk.Model.Coordinates, Exploration.World.Chunk.Controller.BodyCollider> pair in controllers)
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

		public Exploration.World.Chunk.Controller.BodyCollider EnsureChunk(Exploration.World.Chunk.Model.Coordinates coord)
		{
			if (controllers.TryGetValue(coord, out Exploration.World.Chunk.Controller.BodyCollider existing))
			{
				return existing;
			}

			Exploration.World.Chunk.Model.Data data = Utils.ServiceLocator.Instance.WorldService.GetOrCreateChunk(coord);
			if (data == null)
			{
				return null;
			}

			var chunkObject = new GameObject("ChunkCollider " + coord);
			chunkObject.transform.SetParent(transform, false);
			chunkObject.transform.localPosition = new Vector3(
				coord.X * Exploration.World.Chunk.Model.Data.SizeX,
				coord.Y * Exploration.World.Chunk.Model.Data.SizeY,
				coord.Z * Exploration.World.Chunk.Model.Data.SizeZ
			);

			var controller = chunkObject.AddComponent<Exploration.World.Chunk.Controller.BodyCollider>();
			controller.Initialize(coord, data);
			controllers.Add(coord, controller);

			return controller;
		}

		public void RemoveChunk(Exploration.World.Chunk.Model.Coordinates coord)
		{
			if (!controllers.TryGetValue(coord, out Exploration.World.Chunk.Controller.BodyCollider controller))
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
