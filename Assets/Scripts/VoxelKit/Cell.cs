namespace VoxelKit
{
	public class Cell
	{
		public int Id;
		public VoxelKit.Orientation Orientation;
		public VoxelKit.FlipOrientation FlipOrientation;

		public Cell(int id)
			: this(id, VoxelKit.Orientation.PositiveX, VoxelKit.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, VoxelKit.Orientation orientation)
			: this(id, orientation, VoxelKit.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, VoxelKit.Orientation orientation, VoxelKit.FlipOrientation flipOrientation)
		{
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}

		public static VoxelKit.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, VoxelKit.Cell defaultCell = null)
		{
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Pack sizes must be > 0.");
			}

			var cells = new VoxelKit.Cell[sizeX, sizeY, sizeZ];
			VoxelKit.Cell seed = defaultCell ?? new VoxelKit.Cell(-1);
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					for (int k = 0; k < sizeZ; k++)
					{
						cells[i, j, k] = new VoxelKit.Cell(seed.Id, seed.Orientation, seed.FlipOrientation);
					}
				}
			}

			return cells;
		}
	}
}


