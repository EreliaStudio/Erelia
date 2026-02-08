using System.Collections.Generic;
using UnityEngine;

namespace World.View
{
	public class WorldView : MonoBehaviour
	{
		[SerializeField] private Material chunkMaterial = null;
		[SerializeField] private Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);

		private readonly Dictionary<World.Chunk.Model.Coordinates, World.View.ChunkView> views = new Dictionary<World.Chunk.Model.Coordinates, World.View.ChunkView>();
		private World.Service worldService = null;

		private void Awake()
		{
			worldService = Utils.ServiceLocator.Instance.WorldService;
		}

		public void Configure(Material material, Player.Controller.KeyboardMotionController controller, Vector3Int range)
		{
			chunkMaterial = material;
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
			RefreshVisible();
		}

		public void RefreshVisible()
		{
			if (worldService == null)
			{
				Debug.LogError("WorldView: World.Service is not available (ServiceLocator missing).");
				return;
			}

			if (chunkMaterial == null)
			{
				Debug.LogError("WorldView: Chunk material is not assigned.");
				return;
			}

			if (playerController == null)
			{
				Debug.LogError("WorldView: Player controller is not assigned.");
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
			foreach (KeyValuePair<World.Chunk.Model.Coordinates, World.View.ChunkView> pair in views)
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
			RefreshVisible();
		}

		public World.View.ChunkView EnsureChunk(World.Chunk.Model.Coordinates coord)
		{
			if (views.TryGetValue(coord, out World.View.ChunkView existing))
			{
				return existing;
			}

			World.Chunk.Model.Data data = worldService.GetOrCreateChunk(coord);
			if (data == null)
			{
				return null;
			}

			var chunkObject = new GameObject("ChunkView " + coord);
			chunkObject.transform.SetParent(transform, false);
			chunkObject.transform.localPosition = new Vector3(
				coord.X * World.Chunk.Model.Data.SizeX,
				coord.Y * World.Chunk.Model.Data.SizeY,
				coord.Z * World.Chunk.Model.Data.SizeZ
			);

			var view = chunkObject.AddComponent<World.View.ChunkView>();
			view.Initialize(coord, data, chunkMaterial);
			views.Add(coord, view);

			return view;
		}

		public void RemoveChunk(World.Chunk.Model.Coordinates coord)
		{
			if (!views.TryGetValue(coord, out World.View.ChunkView view))
			{
				return;
			}

			if (view != null)
			{
				Destroy(view.gameObject);
			}

			views.Remove(coord);
		}
	}
}
