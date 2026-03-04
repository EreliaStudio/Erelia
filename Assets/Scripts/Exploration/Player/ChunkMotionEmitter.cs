using UnityEngine;

namespace Erelia.Exploration.Player
{
	/// <summary>
	/// Emits a <see cref="Erelia.Core.Event.PlayerChunkMotion"/> event when the player changes chunk.
	/// Subscribes to player motion, tracks the current chunk, and emits when the chunk changes (including initial).
	/// </summary>
	public sealed class ChunkMotionEmitter : MonoBehaviour
	{
		/// <summary>
		/// Last known chunk coordinates.
		/// </summary>
		private Erelia.Exploration.World.Chunk.Coordinates currentChunk = Erelia.Exploration.World.Chunk.Coordinates.Zero;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Initialize to a different chunk so first update triggers an event.
			Erelia.Exploration.World.Chunk.Coordinates current = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(transform.position);
			currentChunk = current - new Erelia.Exploration.World.Chunk.Coordinates(1, 1);
		}

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Listen to player motion events.
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Stop listening to player motion events.
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		/// <summary>
		/// Unity callback invoked on the first frame.
		/// </summary>
		private void Start()
		{
			// Emit the initial chunk if it differs from the cached value.
			Erelia.Exploration.World.Chunk.Coordinates current = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(transform.position);
			if (!current.Equals(currentChunk))
			{
				currentChunk = current;
				Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerChunkMotion(current));
			}
		}

		/// <summary>
		/// Handles player motion events and emits chunk change events.
		/// </summary>
		/// <param name="evt">Player motion event.</param>
		private void OnPlayerMotion(Erelia.Core.Event.PlayerMotion evt)
		{
			// Ignore null events.
			if (evt == null)
			{
				return;
			}

			// Compute current chunk and compare against cached value.
			Erelia.Exploration.World.Chunk.Coordinates current = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(evt.WorldPosition);
			if (current.Equals(currentChunk))
			{
				return;
			}

			// Update cache and emit a chunk motion event.
			currentChunk = current;
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerChunkMotion(current));
		}
	}
}
