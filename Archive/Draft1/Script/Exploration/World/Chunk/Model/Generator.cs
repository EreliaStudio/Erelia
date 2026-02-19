using UnityEngine;

namespace Exploration.World.Chunk.Model
{
	[CreateAssetMenu(menuName = "Exploration/World/TestGenerator", fileName = "NewWorldGenerator")]
	public class TestGenerator : Exploration.World.Chunk.Model.AbstractGenerator 
	{
		public override Exploration.World.Chunk.Model.Data Generate(Exploration.World.Chunk.Model.Coordinates coordinate)
		{
			var chunk = new Data();

			for (int x = 0; x < Data.SizeX; x++)
			{
				for (int y = 0; y < Data.SizeY; y++)
				{
					for (int z = 0; z < Data.SizeZ; z++)
					{
						int dataId = -1;

						if (y == 0)
						{
							dataId = 0;
						}
						else if (y == 1)
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

						chunk.Cells[x, y, z] = new Core.Voxel.Model.Cell(dataId);
					}
				}
			}

			return chunk;
		}
	}
}
