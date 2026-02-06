using UnityEngine;

namespace World.Chunk
{
	[CreateAssetMenu(fileName = "WorldChunkGenerator", menuName = "World/TestGenerator")]
	public class TestGenerator : World.Chunk.IGenerator 
	{
		public override World.Chunk.Data Generate(World.Chunk.Coordinates coordinate)
		{
			var chunk = new Data();

			for (int x = 0; x < Data.SizeX; x++)
			{
				for (int y = 0; y < Data.SizeY; y++)
				{
					for (int z = 0; z < Data.SizeZ; z++)
					{
						int localY = y;
						int dataId = -1;

						if (localY == 0)
						{
							dataId = 0;
						}
						else if (localY == 1)
						{
							if (x == 0 || z == 0)
							{
								dataId = 1;
							}
							else if ((x == 2 || z == 2) && x != 0 && z != 0)
							{
								dataId = 2;
							}
							else if (x == 4 || z == 4)
							{
								dataId = 3;
							}
							else if (coordinate.X == 1 && coordinate.Y == 0 && coordinate.Z == 1)
							{
								dataId = 4;
							}
						}

						chunk.Cells[x, y, z] = new World.Chunk.Cell(dataId);
					}
				}
			}

			return chunk;
		}
	}
}
