namespace Erelia.Player
{
	public sealed class Model
	{
		public Erelia.World.Chunk.Coordinates CurrentChunk { get; private set; } = Erelia.World.Chunk.Coordinates.Zero;

		public bool SetChunk(Erelia.World.Chunk.Coordinates coordinates)
		{
			if (coordinates == null)
			{
				return false;
			}

			if (coordinates.Equals(CurrentChunk))
			{
				return false;
			}

			CurrentChunk = coordinates;
			return true;
		}
	}
}
