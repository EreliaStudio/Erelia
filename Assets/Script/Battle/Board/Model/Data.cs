namespace Battle.Board.Model
{
	public class Data
	{
		public int SizeX = 0;
		public int SizeY = 0;
		public int SizeZ = 0;
		public Core.Voxel.Model.Cell[,,] Cells;
		public Core.Mask.Model.Cell[,,] MaskCells;

		public Data(Core.Voxel.Model.Cell[,,] data)
		{
			SizeX = data.GetLength(0);
			SizeY = data.GetLength(1);
			SizeZ = data.GetLength(2);
			Cells = data;
			MaskCells = new Core.Mask.Model.Cell[SizeX, SizeY, SizeZ];
		}
	}
}
