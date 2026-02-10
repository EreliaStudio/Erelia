namespace Voxel.Model
{
	public class Cell
	{
		public int Id;
		public Voxel.Model.Orientation Orientation;
		public Voxel.Model.FlipOrientation FlipOrientation;

		public Cell(int id)
			: this(id, Voxel.Model.Orientation.PositiveX, Voxel.Model.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Voxel.Model.Orientation orientation)
			: this(id, orientation, Voxel.Model.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Voxel.Model.Orientation orientation, Voxel.Model.FlipOrientation flipOrientation)
		{
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}
	}
}
