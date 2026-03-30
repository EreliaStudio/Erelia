using System;

[Serializable]
public class Board : VoxelGrid
{
	public readonly VoxelMaskCell[,,] MaskCells;

	public Board() : base(0, 0, 0)
	{
		MaskCells = new VoxelMaskCell[0, 0, 0];
	}

	public Board(int sizeX, int sizeY, int sizeZ) : base(sizeX, sizeY, sizeZ)
	{
		MaskCells = new VoxelMaskCell[sizeX, sizeY, sizeZ];

		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					MaskCells[x, y, z] = new VoxelMaskCell();
				}
			}
		}
	}
}
