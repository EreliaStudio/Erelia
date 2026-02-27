namespace Erelia.Event
{
	public sealed class PlayerChunkMotion : GenericEvent
	{
		public Erelia.Exploration.World.Chunk.Coordinates Coordinates { get; }

		public PlayerChunkMotion(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			Coordinates = coordinates;
		}
	}
}
