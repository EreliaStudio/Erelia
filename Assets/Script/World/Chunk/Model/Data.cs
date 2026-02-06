using UnityEngine;

namespace World.Chunk.Model
{
	public class Data
	{
		public const int SizeX = 16;
		public const int SizeY = 64;
		public const int SizeZ = 16;

		public World.Chunk.Cell[,,] Cells = new World.Chunk.Cell[SizeX, SizeY, SizeZ];

		public Data()
		{
			for (int i = 0; i < SizeX; i++)
			{
				for (int j = 0; j < SizeX; j++)
				{
					for (int k = 0; k < SizeX; k++)
					{
						Cells[i, j, k] = new World.Chunk.Cell(-1);
					}
				}
			}
		}
	}
}