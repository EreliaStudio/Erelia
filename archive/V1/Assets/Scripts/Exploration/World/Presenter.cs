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

		private Erelia.Exploration.World.WorldState world;

		private readonly Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Presenter> presenters = new();

		private readonly Queue<Erelia.Exploration.World.Chunk.Coordinates> pendingChunks = new();

		private readonly HashSet<Vector2Int> queuedChunkKeys = new();

		private float updateTimer;

		public Erelia.Exploration.World.WorldState World => world;

		private void Awake()
		{
			if (chunkGenerator == null)
			{
				throw new System.InvalidOperationException("Chunk generator must be assigned on World.Presenter.");
			}

			if (world == null)
			{
				world = Erelia.Core.GameContext.Instance.Exploration?.World;
				if (world != null && !world.HasChunkGenerator)
				{
					world.SetChunkGenerator(chunkGenerator);
				}
			}
		}

		public void SetWorld(Erelia.Exploration.World.WorldState worldState)
		{
			if (worldState == null)
			{
				throw new System.ArgumentNullException(nameof(worldState), "World state cannot be null.");
			}

			world = worldState;
			if (!world.HasChunkGenerator)
			{
				world.SetChunkGenerator(chunkGenerator);
			}
			pendingChunks.Clear();
			queuedChunkKeys.Clear();
		}

		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		public Erelia.Exploration.World.Chunk.ChunkData CreateChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (world == null)
			{
				world = Erelia.Core.GameContext.Instance.Exploration?.World;
				if (world == null)
				{
					Debug.LogWarning("[Erelia.Exploration.World.Presenter] World state is missing.");
					return null;
				}
			}

			if (IsChunkLoaded(coordinates))
			{
				return world.GetOrCreateChunk(coordinates);
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

		private Erelia.Exploration.World.Chunk.ChunkData CreateChunkPresenter(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (world == null)
			{
				throw new System.InvalidOperationException("World state must be assigned before creating chunk presenters.");
			}

			Erelia.Exploration.World.Chunk.ChunkData chunk = world.GetOrCreateChunk(coordinates);

			if (presenters.ContainsKey(coordinates))
			{
				return chunk;
			}

			Erelia.Exploration.World.Chunk.View view = worldView != null
				? worldView.CreateChunkView(coordinates)
				: null;

			var presenter = new Erelia.Exploration.World.Chunk.Presenter(chunk, view);
			presenter.Bind();
			presenter.ForceRebuild();

			presenters.Add(coordinates, presenter);
			return chunk;
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

		private void OnPlayerChunkMotion(Erelia.Core.Event.PlayerChunkMotion evt)
		{
			if (world == null)
			{
				world = Erelia.Core.GameContext.Instance.Exploration?.World;
				if (world == null)
				{
					Debug.LogWarning("[Erelia.Exploration.World.Presenter] World state is missing.");
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


