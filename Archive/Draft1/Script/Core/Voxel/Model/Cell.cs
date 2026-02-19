namespace Core.Voxel.Model
{
	public class Cell
	{
		public int Id;
		public Core.Voxel.Model.Orientation Orientation;
		public Core.Voxel.Model.FlipOrientation FlipOrientation;

		public Cell(int id)
			: this(id, Core.Voxel.Model.Orientation.PositiveX, Core.Voxel.Model.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Core.Voxel.Model.Orientation orientation)
			: this(id, orientation, Core.Voxel.Model.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Core.Voxel.Model.Orientation orientation, Core.Voxel.Model.FlipOrientation flipOrientation)
		{
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}
	}
}
