using System.Collections.Generic;
using UnityEngine;

namespace Erelia.World
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.World.View worldView;
		[SerializeField] private int chunksPerFrame = 2;
		[SerializeField] private float updateIntervalSeconds = 0.05f;

		private Erelia.World.Model worldModel;
		private readonly Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Presenter> presenters =
			new Dictionary<Erelia.World.Chunk.Coordinates, Erelia.World.Chunk.Presenter>();
		private readonly Queue<Erelia.World.Chunk.Coordinates> pendingChunks = new Queue<Erelia.World.Chunk.Coordinates>();
		private readonly HashSet<Vector2Int> queuedChunkKeys = new HashSet<Vector2Int>();
		private Vector2Int lastCenterChunk;
		private bool hasCenterChunk;
		private float updateTimer;

		private void Awake()
		{
			Erelia.Logger.Log("[Erelia.World.Presenter] Awake - initializing world model.");
			worldModel = new Erelia.World.Model(new Erelia.World.Chunk.Generation.SimpleDebugChunkGenerator());
		}

		private void OnEnable()
		{
			Erelia.Events.PlayerChunkChanged += OnPlayerChunkChanged;
		}

		private void OnDisable()
		{
			Erelia.Events.PlayerChunkChanged -= OnPlayerChunkChanged;
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
				Erelia.Logger.RaiseWarning("[Erelia.World.Presenter] World model was null. Recreating.");
				worldModel = new Erelia.World.Model(new Erelia.World.Chunk.Generation.SimpleDebugChunkGenerator());
			}

			Erelia.World.Chunk.Model model = worldModel.GetOrCreateChunk(coordinates);

			if (presenters.ContainsKey(coordinates))
			{
				Erelia.Logger.Log("[Erelia.World.Presenter] Chunk presenter already exists for coordinates " + coordinates + ".");
				return model;
			}

			if (worldView == null)
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Presenter] World view is not assigned. Chunk view will be null for " + coordinates + ".");
			}

			Erelia.World.Chunk.View view = worldView != null ? worldView.CreateChunkView(coordinates) : null;
			
			if (view == null)
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Presenter] Chunk view could not be created for " + coordinates + ".");
			}

			var presenter = new Erelia.World.Chunk.Presenter(model, view);
			presenter.Bind();
			presenter.ForceRebuild();

			presenters.Add(coordinates, presenter);
			Erelia.Logger.Log("[Erelia.World.Presenter] Chunk presenter created for " + coordinates + ".");
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

		private void OnPlayerChunkChanged(Erelia.World.Chunk.Coordinates coordinates)
		{
			if (coordinates == null)
			{
				return;
			}

			int radius = worldView != null ? worldView.ViewRadius : 0;
			if (radius <= 0)
			{
				return;
			}

			Vector2Int centerKey = coordinates.ToVector2Int();
			if (!hasCenterChunk || centerKey != lastCenterChunk || pendingChunks.Count == 0)
			{
				RebuildPending(centerKey, radius);
				lastCenterChunk = centerKey;
				hasCenterChunk = true;
			}
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
			Erelia.Logger.Log("[Erelia.World.Presenter] OnDestroy - unbinding chunk presenters.");
			foreach (var pair in presenters)
			{
				pair.Value.Unbind();
			}
			presenters.Clear();
		}
	}
}


