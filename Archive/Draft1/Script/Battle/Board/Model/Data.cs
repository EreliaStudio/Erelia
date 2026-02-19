using System;

namespace Battle.Board.Model
{
	public class Data
	{
		public int SizeX = 0;
		public int SizeY = 0;
		public int SizeZ = 0;
		public Core.Voxel.Model.Cell[,,] Cells;
		public Core.Mask.Model.Cell[,,] MaskCells;

		public event Action<Battle.Board.Model.Data> OnVoxelEdition;
		public event Action<Battle.Board.Model.Data> OnMaskEdition;

		public Data(Core.Voxel.Model.Cell[,,] data)
		{
			SizeX = data.GetLength(0);
			SizeY = data.GetLength(1);
			SizeZ = data.GetLength(2);
			Cells = data;
			MaskCells = new Core.Mask.Model.Cell[SizeX, SizeY, SizeZ];

			for (int i = 0; i < SizeX; i++)
			{
				for (int j = 0; j < SizeY; j++)
				{
					for (int k = 0; k < SizeZ; k++)
					{
						MaskCells[i, j, k] = new Core.Mask.Model.Cell();
					}
				}
			}
		}

		public void Validate()
		{
			ValidateVoxel();
			ValidateMask();
		}

		public void ValidateVoxel()
		{
			OnVoxelEdition?.Invoke(this);
		}

		public void ValidateMask()
		{
			OnMaskEdition?.Invoke(this);
		}
	}
}
