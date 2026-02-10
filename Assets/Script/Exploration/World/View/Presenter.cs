using System.Collections.Generic;
using UnityEngine;

namespace World.View
{
	public class Presenter : MonoBehaviour
	{
		[SerializeField] private Material voxelMaterial = null;
		[SerializeField] private Player.Controller.KeyboardMotionController playerController = null;
		[SerializeField] private Vector3Int viewRange = new Vector3Int(1, 0, 1);

		private readonly Dictionary<World.Chunk.Model.Coordinates, World.Chunk.View.Presenter> views = new Dictionary<World.Chunk.Model.Coordinates, World.Chunk.View.Presenter>();

		private void Awake()
		{
			
		}

		public void Configure(Material material, Player.Controller.KeyboardMotionController controller, Vector3Int range)
		{
			voxelMaterial = material;
			playerController = controller;
			viewRange = range;
		}

		private void Start()
		{
			RefreshVisible();
		}

		public void RefreshVisible()
		{
			if (Utils.ServiceLocator.Instance.WorldService == null)
			{
				Debug.LogError("World.Presenter: World.Service is not available (ServiceLocator missing).");
				return;
			}

			if (voxelMaterial == null)
			{
				Debug.LogError("World.Presenter: Chunk material is not assigned.");
				return;
			}

			if (playerController == null)
			{
				Debug.LogError("World.Presenter: Player controller is not assigned.");
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
			foreach (KeyValuePair<World.Chunk.Model.Coordinates, World.Chunk.View.Presenter> pair in views)
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

		public World.Chunk.View.Presenter EnsureChunk(World.Chunk.Model.Coordinates coord)
		{
			if (views.TryGetValue(coord, out World.Chunk.View.Presenter existing))
			{
				return existing;
			}

			World.Chunk.Model.Data data = Utils.ServiceLocator.Instance.WorldService.GetOrCreateChunk(coord);
			if (data == null)
			{
				return null;
			}

			var chunkObject = new GameObject("ChunkPresenter " + coord);
			chunkObject.transform.SetParent(transform, false);
			chunkObject.transform.localPosition = new Vector3(
				coord.X * World.Chunk.Model.Data.SizeX,
				coord.Y * World.Chunk.Model.Data.SizeY,
				coord.Z * World.Chunk.Model.Data.SizeZ
			);

			var view = chunkObject.AddComponent<World.Chunk.View.Presenter>();
			view.Initialize(coord, data, voxelMaterial);
			views.Add(coord, view);

			return view;
		}

		public void RemoveChunk(World.Chunk.Model.Coordinates coord)
		{
			if (!views.TryGetValue(coord, out World.Chunk.View.Presenter view))
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
