using UnityEngine;

namespace World.Chunk.Model
{
	public class Data
	{
		public const int SizeX = 16;
		public const int SizeY = 64;
		public const int SizeZ = 16;

		public World.Chunk.Model.Cell[,,] Cells = new World.Chunk.Model.Cell[SizeX, SizeY, SizeZ];

		public Data()
		{
			for (int i = 0; i < SizeX; i++)
			{
				for (int j = 0; j < SizeY; j++)
				{
					for (int k = 0; k < SizeZ; k++)
					{
						Cells[i, j, k] = new World.Chunk.Model.Cell(-1);
					}
				}
			}
		}
	}
}