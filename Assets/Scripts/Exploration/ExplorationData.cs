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
		/// Creates a new exploration data container with default models.
		/// </summary>
		public Data()
		{
			// Initialize default models so exploration systems are ready.
			WorldModel = new Erelia.Exploration.World.Model();
			PlayerModel = new Erelia.Exploration.Player.Model();
		}
	}
}
