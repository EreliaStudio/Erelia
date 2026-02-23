using System.Collections.Generic;
using UnityEngine;

namespace Erelia.World
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.World.View worldView;
		[SerializeField] private int chunksPerFrame = 2;
		[SerializeField] private float updateIntervalSeconds = 0.05f;
		[SerializeField] private Erelia.World.Chunk.Generation.IGenerator chunkGenerator;

		private Erelia.World.Model worldModel;
		private readonly Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Presenter> presenters = new Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Presenter>();
		private readonly Queue<Erelia.World.Chunk.Coordinates> pendingChunks = new Queue<Erelia.World.Chunk.Coordinates>();
		private readonly HashSet<Vector2Int> queuedChunkKeys = new HashSet<Vector2Int>();
		private float updateTimer;

		public Erelia.World.Model WorldModel => worldModel;

		private void Awake()
		{
			if (chunkGenerator == null)
			{
				throw new System.InvalidOperationException("Chunk generator must be assigned on World.Presenter.");
			}

			worldModel = new Erelia.World.Model();
		}

		private void OnEnable()
		{
			Erelia.Event.Bus.Subscribe<Erelia.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		private void OnDisable()
		{
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		public Erelia.World.Chunk.Model CreateChunk(Erelia.World.Chunk.Coordinates coordinates)
		{
			if (IsChunkLoaded(coordinates))
			{
				return worldModel.GetOrCreateChunk(coordinates);
			}

			return CreateChunkPresenter(coordinates);
		}

		private void Update()
		{
			if (pendingChunks.Count <= 0)
			{
				return;
			}

			if (updateIntervalSeconds <= 0f)
			{
				ProcessPending(Mathf.Max(1, chunksPerFrame));
				return;
			}

			updateTimer += Time.deltaTime;
			if (updateTimer >= updateIntervalSeconds)
			{
				updateTimer = 0f;
				ProcessPending(Mathf.Max(1, chunksPerFrame));
			}
		}

		private bool IsChunkLoaded(Erelia.World.Chunk.Coordinates coordinates)
		{
			return presenters.ContainsKey(coordinates);
		}

		private Erelia.World.Chunk.Model CreateChunkPresenter(Erelia.World.Chunk.Coordinates coordinates)
		{
			if (worldModel == null)
			{
				if (chunkGenerator == null)
				{
					throw new System.InvalidOperationException("Chunk generator must be assigned on World.Presenter.");
				}

				worldModel = new Erelia.World.Model();
			}

			Erelia.World.Chunk.Model model = worldModel.GetOrCreateChunk(coordinates);
			chunkGenerator?.Generate(model, coordinates, worldModel);

			if (presenters.ContainsKey(coordinates))
			{
				return model;
			}

			Erelia.World.Chunk.View view = worldView != null ? worldView.CreateChunkView(coordinates) : null;

			var presenter = new Erelia.World.Chunk.Presenter(model, view);
			presenter.Bind();
			presenter.ForceRebuild();

			presenters.Add(coordinates, presenter);
			return model;
		}

		private void ProcessPending(int maxCount)
		{
			int created = 0;
			while (created < maxCount && pendingChunks.Count > 0)
			{
				Erelia.World.Chunk.Coordinates coords = pendingChunks.Dequeue();
				queuedChunkKeys.Remove(coords.ToVector2Int());

				if (IsChunkLoaded(coords))
				{
					continue;
				}

				CreateChunkPresenter(coords);
				created++;
			}
		}

		private void OnPlayerChunkMotion(Erelia.Event.PlayerChunkMotion evt)
		{
			if (evt == null || evt.Coordinates == null)
			{
				return;
			}

			Erelia.World.Chunk.Coordinates coordinates = evt.Coordinates;
			int radius = worldView != null ? worldView.ViewRadius : 0;
			if (radius <= 0)
			{
				return;
			}

			RebuildPending(coordinates.ToVector2Int(), radius);
		}

		private void RebuildPending(Vector2Int center, int radius)
		{
			pendingChunks.Clear();
			queuedChunkKeys.Clear();

			int radiusSquared = radius * radius;
			List<Erelia.World.Chunk.Coordinates> candidates = new List<Erelia.World.Chunk.Coordinates>();

			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dz = -radius; dz <= radius; dz++)
				{
					if ((dx * dx) + (dz * dz) > radiusSquared)
					{
						continue;
					}

					int x = center.x + dx;
					int z = center.y + dz;
					Erelia.World.Chunk.Coordinates coords = new Erelia.World.Chunk.Coordinates(x, z);

					if (IsChunkLoaded(coords))
					{
						continue;
					}

					candidates.Add(coords);
				}
			}

			candidates.Sort((a, b) =>
			{
				int da = (a.X - center.x) * (a.X - center.x) + (a.Z - center.y) * (a.Z - center.y);
				int db = (b.X - center.x) * (b.X - center.x) + (b.Z - center.y) * (b.Z - center.y);
				return da.CompareTo(db);
			});

			for (int i = 0; i < candidates.Count; i++)
			{
				Erelia.World.Chunk.Coordinates coords = candidates[i];
				Vector2Int key = coords.ToVector2Int();
				if (queuedChunkKeys.Add(key))
				{
					pendingChunks.Enqueue(coords);
				}
			}
		}

		private void OnDestroy()
		{
			foreach (var pair in presenters)
			{
				pair.Value.Unbind();
			}
			presenters.Clear();
		}
	}
}
