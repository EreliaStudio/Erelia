using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Exploration.World
{
	/// <summary>
	/// Presenter responsible for streaming and rendering world chunks around the player.
	/// Listens for player chunk motion, queues nearby chunks, and instantiates chunk presenters over time.
	/// </summary>
	public sealed class Presenter : MonoBehaviour
	{
		/// <summary>
		/// View component used to spawn chunk visuals.
		/// </summary>
		[SerializeField] private Erelia.Exploration.World.View worldView;

		/// <summary>
		/// Maximum number of chunks created per update tick.
		/// </summary>
		[SerializeField] private int chunksPerFrame = 2;

		/// <summary>
		/// Minimum time interval between chunk creation updates.
		/// </summary>
		[SerializeField] private float updateIntervalSeconds = 0.05f;

		/// <summary>
		/// Chunk generator used to populate new chunks.
		/// </summary>
		[SerializeField] private Erelia.Exploration.World.Chunk.Generation.IGenerator chunkGenerator;

		/// <summary>
		/// Current world model.
		/// </summary>
		private Erelia.Exploration.World.Model worldModel;

		/// <summary>
		/// Active chunk presenters keyed by coordinates.
		/// </summary>
		private readonly Dictionary<Erelia.Exploration.World.Chunk.Coordinates, Erelia.Exploration.World.Chunk.Presenter> presenters = new();

		/// <summary>
		/// Queue of chunks pending creation.
		/// </summary>
		private readonly Queue<Erelia.Exploration.World.Chunk.Coordinates> pendingChunks = new();

		/// <summary>
		/// Set of queued chunk keys to avoid duplicates.
		/// </summary>
		private readonly HashSet<Vector2Int> queuedChunkKeys = new();

		/// <summary>
		/// Timer used to throttle chunk creation.
		/// </summary>
		private float updateTimer;

		/// <summary>
		/// Gets the active world model.
		/// </summary>
		public Erelia.Exploration.World.Model WorldModel => worldModel;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Ensure a generator is assigned.
			if (chunkGenerator == null)
			{
				throw new System.InvalidOperationException("Chunk generator must be assigned on World.Presenter.");
			}

			// Try to bind model from context if not assigned.
			if (worldModel == null)
			{
				worldModel = Erelia.Core.Context.Instance.ExplorationData?.WorldModel;
				if (worldModel != null && !worldModel.HasChunkGenerator)
				{
					worldModel.SetChunkGenerator(chunkGenerator);
				}
			}
		}

		/// <summary>
		/// Assigns the world model to this presenter.
		/// </summary>
		/// <param name="model">World model to use.</param>
		public void SetModel(Erelia.Exploration.World.Model model)
		{
			// Validate input to avoid invalid state.
			if (model == null)
			{
				throw new System.ArgumentNullException(nameof(model), "World model cannot be null.");
			}

			// Store model and ensure generator is set.
			worldModel = model;
			if (!worldModel.HasChunkGenerator)
			{
				worldModel.SetChunkGenerator(chunkGenerator);
			}
			// Reset pending queues.
			pendingChunks.Clear();
			queuedChunkKeys.Clear();
		}

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Listen for player chunk motion.
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Stop listening for player chunk motion.
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerChunkMotion>(OnPlayerChunkMotion);
		}

		/// <summary>
		/// Creates or retrieves a chunk and ensures it has a presenter if needed.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <returns>Chunk model instance, or <c>null</c> if no world model is available.</returns>
		public Erelia.Exploration.World.Chunk.Model CreateChunk(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			// Resolve world model if needed.
			if (worldModel == null)
			{
				worldModel = Erelia.Core.Context.Instance.ExplorationData?.WorldModel;
				if (worldModel == null)
				{
					Debug.LogWarning("[Erelia.Exploration.World.Presenter] World model is missing.");
					return null;
				}
			}

			// If already loaded, return existing model.
			if (IsChunkLoaded(coordinates))
			{
				return worldModel.GetOrCreateChunk(coordinates);
			}

			// Otherwise create a presenter and return the model.
			return CreateChunkPresenter(coordinates);
		}

		/// <summary>
		/// Unity update loop for processing pending chunk creation.
		/// </summary>
		private void Update()
		{
			if (pendingChunks.Count <= 0)
			{
				return;
			}

			// If no interval is set, process immediately each frame.
			if (updateIntervalSeconds <= 0f)
			{
				ProcessPending(Mathf.Max(1, chunksPerFrame));
				return;
			}

			// Throttle chunk creation based on the interval.
			updateTimer += Time.deltaTime;
			if (updateTimer >= updateIntervalSeconds)
			{
				updateTimer = 0f;
				ProcessPending(Mathf.Max(1, chunksPerFrame));
			}
		}

		/// <summary>
		/// Checks whether a chunk presenter already exists.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <returns><c>true</c> if loaded; otherwise <c>false</c>.</returns>
		private bool IsChunkLoaded(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			return presenters.ContainsKey(coordinates);
		}

		/// <summary>
		/// Creates a chunk presenter and binds it to a model.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <returns>Chunk model instance.</returns>
		private Erelia.Exploration.World.Chunk.Model CreateChunkPresenter(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (worldModel == null)
			{
				throw new System.InvalidOperationException("World model must be assigned before creating chunk presenters.");
			}

			// Create or load the chunk model.
			Erelia.Exploration.World.Chunk.Model model = worldModel.GetOrCreateChunk(coordinates);

			// If already loaded, return the model.
			if (presenters.ContainsKey(coordinates))
			{
				return model;
			}

			// Create the view and presenter.
			Erelia.Exploration.World.Chunk.View view = worldView != null
				? worldView.CreateChunkView(coordinates)
				: null;

			var presenter = new Erelia.Exploration.World.Chunk.Presenter(model, view);
			presenter.Bind();
			presenter.ForceRebuild();

			// Cache presenter.
			presenters.Add(coordinates, presenter);
			return model;
		}

		/// <summary>
		/// Processes pending chunk creations up to the specified count.
		/// </summary>
		/// <param name="maxCount">Maximum number of chunks to create.</param>
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

				// Create the presenter and increment count.
				CreateChunkPresenter(coords);
				created++;
			}
		}

		/// <summary>
		/// Handles player chunk motion events to update the visible chunk queue.
		/// </summary>
		/// <param name="evt">Player chunk motion event.</param>
		private void OnPlayerChunkMotion(Erelia.Core.Event.PlayerChunkMotion evt)
		{
			// Resolve world model if needed.
			if (worldModel == null)
			{
				worldModel = Erelia.Core.Context.Instance.ExplorationData?.WorldModel;
				if (worldModel == null)
				{
					Debug.LogWarning("[Erelia.Exploration.World.Presenter] World model is missing.");
					return;
				}
			}

			// Validate event payload.
			if (evt == null || evt.Coordinates == null)
			{
				return;
			}

			// Use view radius to determine queue size.
			int radius = worldView != null ? worldView.ViewRadius : 0;
			if (radius <= 0)
			{
				return;
			}

			// Rebuild the pending queue around the new center.
			RebuildPending(evt.Coordinates.ToVector2Int(), radius);
		}

		/// <summary>
		/// Rebuilds the pending chunk queue around a center point.
		/// </summary>
		/// <param name="center">Center chunk coordinate.</param>
		/// <param name="radius">Radius in chunks.</param>
		private void RebuildPending(Vector2Int center, int radius)
		{
			// Reset queues.
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

					// Convert to chunk coordinates and skip if already loaded.
					var coords = new Erelia.Exploration.World.Chunk.Coordinates(center.x + dx, center.y + dz);

					if (IsChunkLoaded(coords))
					{
						continue;
					}

					candidates.Add(coords);
				}
			}

			// Sort candidates by distance to center (closest first).
			candidates.Sort((a, b) =>
			{
				int da = (a.X - center.x) * (a.X - center.x) + (a.Z - center.y) * (a.Z - center.y);
				int db = (b.X - center.x) * (b.X - center.x) + (b.Z - center.y) * (b.Z - center.y);
				return da.CompareTo(db);
			});

			// Enqueue candidates while preventing duplicates.
			foreach (var coords in candidates)
			{
				Vector2Int key = coords.ToVector2Int();
				if (queuedChunkKeys.Add(key))
				{
					pendingChunks.Enqueue(coords);
				}
			}
		}

		/// <summary>
		/// Unity callback invoked when the component is destroyed.
		/// </summary>
		private void OnDestroy()
		{
			// Unbind all chunk presenters.
			foreach (var pair in presenters)
			{
				pair.Value.Unbind();
			}

			// Clear cached presenters.
			presenters.Clear();
		}
	}
}
