using UnityEngine;

namespace Erelia.Exploration
{
	[System.Serializable]
	public sealed class Data
	{
		public Erelia.Exploration.World.Model WorldModel;

		public Erelia.Exploration.Player.Model PlayerModel;

		public Vector3 SafePosition { get; private set; }

		public bool HasSafePosition { get; private set; }

		public Data()
		{
			WorldModel = new Erelia.Exploration.World.Model();
			PlayerModel = new Erelia.Exploration.Player.Model();
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
