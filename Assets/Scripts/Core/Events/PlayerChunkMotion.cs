namespace Erelia.Core.Event
{
	/// <summary>
	/// Event emitted when the player enters a different chunk.
	/// </summary>
	public sealed class PlayerChunkMotion : GenericEvent
	{
		/// <summary>
		/// Chunk coordinates the player moved into.
		/// </summary>
		public Erelia.Exploration.World.Chunk.Coordinates Coordinates { get; }

		/// <summary>
		/// Creates a new player chunk motion event.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates the player entered.</param>
		public PlayerChunkMotion(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			Coordinates = coordinates;
		}
	}
}
