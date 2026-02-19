namespace Voxel
{
	public class Cell
	{
		public int Id;
		public Voxel.Orientation Orientation;
		public Voxel.FlipOrientation FlipOrientation;

		public Cell(int id)
			: this(id, Voxel.Orientation.PositiveX, Voxel.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Voxel.Orientation orientation)
			: this(id, orientation, Voxel.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Voxel.Orientation orientation, Voxel.FlipOrientation flipOrientation)
		{
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}

		public static Voxel.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, Voxel.Cell defaultCell = null)
		{
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Pack sizes must be > 0.");
			}

			var cells = new Voxel.Cell[sizeX, sizeY, sizeZ];
			Voxel.Cell seed = defaultCell ?? new Voxel.Cell(-1);
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					for (int k = 0; k < sizeZ; k++)
					{
						cells[i, j, k] = new Voxel.Cell(seed.Id, seed.Orientation, seed.FlipOrientation);
					}
				}
			}

			return cells;
		}
	}
}
