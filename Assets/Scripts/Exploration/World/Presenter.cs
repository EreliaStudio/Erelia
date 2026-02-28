using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Exploration.World
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Exploration.World.View worldView;
		[SerializeField] private int chunksPerFrame = 2;
		[SerializeField] private float updateIntervalSeconds = 0.05f;
		[SerializeField] private Erelia.Exploration.World.Chunk.Generation.IGenerator chunkGenerator;

		private Erelia.Exploration.World.Model worldModel;
		private readonly Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Presenter> presenters = new();
		private readonly Queue<Erelia.Exploration.World.Chunk.Coordinates> pendingChunks = new();
		private readonly HashSet<Vector2Int> queuedChunkKeys = new();
		private float updateTimer;

		public Erelia.Exploration.World.Model WorldModel => worldModel;

		private void Awake()
		{
			if (chunkGenerator == null)
			{
				throw new System.InvalidOperationException("Chunk generator must be assigned on World.Presenter.");
			}

			if (worldModel == null)
			{
				worldModel = Erelia.Context.Instance.ExplorationData?.WorldModel;
			}
		}

		public void SetModel(Erelia.Exploration.World.Model model)
		{
			if (model == null)
			{
				throw new System.ArgumentNullException(nameof(model), "World model cannot be null.");
			}

			worldModel = model;
			pendingChunks.Clear();
			queuedChunkKeys.Clear();
		}

		private void OnEnable()
		{
			Erelia.Event.Bus.Subscribe<Erelia.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		private void OnDisable()
		{
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		public Erelia.Exploration.World.Chunk.Model CreateChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (worldModel == null)
			{
				worldModel = Erelia.Context.Instance.ExplorationData?.WorldModel;
				if (worldModel == null)
				{
					Debug.LogWarning("[Erelia.Exploration.World.Presenter] World model is missing.");
					return null;
				}
			}

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

		private bool IsChunkLoaded(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			return presenters.ContainsKey(coordinates);
		}

		private Erelia.Exploration.World.Chunk.Model CreateChunkPresenter(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (worldModel == null)
			{
				throw new System.InvalidOperationException("World model must be assigned before creating chunk presenters.");
			}

			Erelia.Exploration.World.Chunk.Model model = worldModel.GetOrCreateChunk(coordinates);
			chunkGenerator.Generate(model, coordinates, worldModel);

			if (presenters.ContainsKey(coordinates))
			{
				return model;
			}

			Erelia.Exploration.World.Chunk.View view = worldView != null
				? worldView.CreateChunkView(coordinates)
				: null;

			var presenter = new Erelia.Exploration.World.Chunk.Presenter(model, view);
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
				var coords = pendingChunks.Dequeue();
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
			if (worldModel == null)
			{
				worldModel = Erelia.Context.Instance.ExplorationData?.WorldModel;
				if (worldModel == null)
				{
					Debug.LogWarning("[Erelia.Exploration.World.Presenter] World model is missing.");
					return;
				}
			}

			if (evt == null || evt.Coordinates == null)
			{
				return;
			}

			int radius = worldView != null ? worldView.ViewRadius : 0;
			if (radius <= 0)
			{
				return;
			}

			RebuildPending(evt.Coordinates.ToVector2Int(), radius);
		}

		private void RebuildPending(Vector2Int center, int radius)
		{
			pendingChunks.Clear();
			queuedChunkKeys.Clear();

			int radiusSquared = radius * radius;
			List<Erelia.Exploration.World.Chunk.Coordinates> candidates = new();

			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dz = -radius; dz <= radius; dz++)
				{
					if ((dx * dx) + (dz * dz) > radiusSquared)
					{
						continue;
					}

					var coords = new Erelia.Exploration.World.Chunk.Coordinates(center.x + dx, center.y + dz);

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

			foreach (var coords in candidates)
			{
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
