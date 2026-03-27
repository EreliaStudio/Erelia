using UnityEngine;

namespace Erelia.Exploration
{
	[System.Serializable]
	public sealed class ExplorationState
	{
		public Erelia.Exploration.World.WorldState World;

		public Erelia.Exploration.Player.ExplorationPlayerState Player;

		public Vector3 SafePosition { get; private set; }

		public bool HasSafePosition { get; private set; }

		public ExplorationState()
		{
			World = new Erelia.Exploration.World.WorldState();
			Player = new Erelia.Exploration.Player.ExplorationPlayerState();
		}

		public void SetSafePosition(Vector3 position)
		{
			SafePosition = position;
			HasSafePosition = true;
		}

		public bool TryGetSafePosition(out Vector3 position)
		{
			position = SafePosition;
			return HasSafePosition;
		}
	}
}

