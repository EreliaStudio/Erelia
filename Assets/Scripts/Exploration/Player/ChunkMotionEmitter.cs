using UnityEngine;

namespace Erelia.Exploration.Player
{
	public sealed class ChunkMotionEmitter : MonoBehaviour
	{
		private Erelia.Exploration.World.Chunk.Coordinates currentChunk = Erelia.Exploration.World.Chunk.Coordinates.Zero;

		private void Awake()
		{
			Erelia.Exploration.World.Chunk.Coordinates current = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(transform.position);
			currentChunk = current - new Erelia.Exploration.World.Chunk.Coordinates(1, 1);
		}

		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void Start()
		{
			Erelia.Exploration.World.Chunk.Coordinates current = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(transform.position);
			if (!current.Equals(currentChunk))
			{
				currentChunk = current;
				Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerChunkMotion(current));
			}
		}

		private void OnPlayerMotion(Erelia.Core.Event.PlayerMotion evt)
		{
			if (evt == null)
			{
				return;
			}

			Erelia.Exploration.World.Chunk.Coordinates current = Erelia.Exploration.World.Chunk.Coordinates.FromWorld(evt.WorldPosition);
			if (current.Equals(currentChunk))
			{
				return;
			}

			currentChunk = current;
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerChunkMotion(current));
		}
	}
}
