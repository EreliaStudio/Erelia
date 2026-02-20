namespace Erelia.Voxel
{
	public class Cell
	{
		public int Id;
		public Erelia.Voxel.Orientation Orientation;
		public Erelia.Voxel.FlipOrientation FlipOrientation;

		public Cell(int id)
			: this(id, Erelia.Voxel.Orientation.PositiveX, Erelia.Voxel.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Erelia.Voxel.Orientation orientation)
			: this(id, orientation, Erelia.Voxel.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Erelia.Voxel.Orientation orientation, Erelia.Voxel.FlipOrientation flipOrientation)
		{
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}

		public static Erelia.Voxel.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, Erelia.Voxel.Cell defaultCell = null)
		{
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Pack sizes must be > 0.");
			}

			var cells = new Erelia.Voxel.Cell[sizeX, sizeY, sizeZ];
			Erelia.Voxel.Cell seed = defaultCell ?? new Erelia.Voxel.Cell(-1);
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					for (int k = 0; k < sizeZ; k++)
					{
						cells[i, j, k] = new Erelia.Voxel.Cell(seed.Id, seed.Orientation, seed.FlipOrientation);
					}
				}
			}

			return cells;
		}
	}
}

