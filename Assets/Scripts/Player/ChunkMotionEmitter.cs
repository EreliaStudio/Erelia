using UnityEngine;

namespace Erelia.Player
{
	public sealed class ChunkMotionEmitter : MonoBehaviour
	{
		private Erelia.World.Chunk.Coordinates currentChunk = Erelia.World.Chunk.Coordinates.Zero;

		private void Awake()
		{
			Erelia.World.Chunk.Coordinates current = Erelia.World.Chunk.Coordinates.FromWorld(transform.position);
			currentChunk = current - new Erelia.World.Chunk.Coordinates(1, 1);
		}

		private void OnEnable()
		{
			Erelia.Event.Bus.Subscribe<Erelia.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void OnDisable()
		{
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void Start()
		{
			Erelia.World.Chunk.Coordinates current = Erelia.World.Chunk.Coordinates.FromWorld(transform.position);
			if (!current.Equals(currentChunk))
			{
				currentChunk = current;
				Erelia.Event.Bus.Emit(new Erelia.Event.PlayerChunkMotion(current));
			}
		}

		private void OnPlayerMotion(Erelia.Event.PlayerMotion evt)
		{
			if (evt == null)
			{
				return;
			}

			Erelia.World.Chunk.Coordinates current = Erelia.World.Chunk.Coordinates.FromWorld(evt.WorldPosition);
			if (current.Equals(currentChunk))
			{
				return;
			}

			currentChunk = current;
			Erelia.Event.Bus.Emit(new Erelia.Event.PlayerChunkMotion(current));
		}
	}
}
