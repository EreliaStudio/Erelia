namespace Battle.Board.Model
{
	class Data
	{
		public int SizeX = 0;
		public int SizeY = 0;
		public int SizeZ = 0;
		public Voxel.Model.Cell[,,] Cells;
		public Battle.Board.Model.Cell[,,] MaskCells;

		public Data(Voxel.Model.Cell[,,] data)
		{
			SizeX = data.GetLength(0);
			SizeY = data.GetLength(1);
			SizeZ = data.GetLength(2);
			Cells = data;
			MaskCells = new Battle.Board.Model.Cell[SizeX, SizeY, SizeZ];
		}
	}
}
