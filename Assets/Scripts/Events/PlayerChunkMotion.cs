namespace Erelia.Event
{
	public sealed class PlayerChunkMotion : GenericEvent
	{
		public Erelia.World.Chunk.Coordinates Coordinates { get; }

		public PlayerChunkMotion(Erelia.World.Chunk.Coordinates coordinates)
		{
			Coordinates = coordinates;
		}
	}
}
