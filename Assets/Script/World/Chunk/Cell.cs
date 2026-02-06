namespace World.Chunk
{
	public class Cell
	{
		public int Id;
		public Orientation Orientation;
		public FlipOrientation FlipOrientation;

		public Cell(int id)
			: this(id, Orientation.PositiveX, FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Orientation orientation)
			: this(id, orientation, FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Orientation orientation, FlipOrientation flipOrientation)
		{
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}
	}
}