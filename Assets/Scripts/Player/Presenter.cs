using UnityEngine;

namespace Erelia.Player
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Player.View view;

		private Erelia.Player.Model model;

		private void Awake()
		{
			model = new Erelia.Player.Model();
			Erelia.World.Chunk.Coordinates current = Erelia.World.Chunk.Coordinates.FromWorld(view.Target.position);
			model.SetChunk(current - new World.Chunk.Coordinates(1, 1));
		}

		private void Update()
		{
			if (view == null)
			{
				return;
			}

			Erelia.World.Chunk.Coordinates current = Erelia.World.Chunk.Coordinates.FromWorld(view.Target.position);
			if (model.SetChunk(current))
			{
				Erelia.Events.PlayerChunkChanged?.Invoke(current);
			}
		}
	}
}
