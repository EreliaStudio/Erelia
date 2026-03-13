using UnityEngine;

namespace Erelia.Exploration
{
	/// <summary>
	/// Serializable container holding exploration-related runtime data.
	/// Constructs default world/player models and keeps them together for context storage.
	/// </summary>
	/// <remarks>
	/// Bundles the exploration world model and player model for easy storage in context.
	/// </remarks>
	[System.Serializable]
	public sealed class Data
	{
		/// <summary>
		/// World model instance for exploration.
		/// </summary>
		public Erelia.Exploration.World.Model WorldModel;

		/// <summary>
		/// Player model instance for exploration.
		/// </summary>
		public Erelia.Exploration.Player.Model PlayerModel;

		/// <summary>
		/// Last safe exploration position that defeat should return to.
		/// </summary>
		public Vector3 SafePosition { get; private set; }

		/// <summary>
		/// Indicates whether a safe exploration position has been defined.
		/// </summary>
		public bool HasSafePosition { get; private set; }

		/// <summary>
		/// Creates a new exploration data container with default models.
		/// </summary>
		public Data()
		{
			// Initialize default models so exploration systems are ready.
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
