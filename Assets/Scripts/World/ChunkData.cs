using System;

[Serializable]
public class ChunkData : VoxelGrid
{
	public const int FixedSizeX = 16;
	public const int FixedSizeY = 16;
	public const int FixedSizeZ = 16;

	public readonly VoxelMaskCell[,,] MaskCells;

	public ChunkData() : base(FixedSizeX, FixedSizeY, FixedSizeZ)
	{
		MaskCells = new VoxelMaskCell[FixedSizeX, FixedSizeY, FixedSizeZ];
		for (int x = 0; x < FixedSizeX; x++)
		{
			for (int y = 0; y < FixedSizeY; y++)
			{
				for (int z = 0; z < FixedSizeZ; z++)
				{
					MaskCells[x, y, z] = new VoxelMaskCell();
				}
			}
		}
	}

	public VoxelCell GetCell(int x, int y, int z)
	{
		return Cells[x, y, z];
	}

	public void SetCell(int x, int y, int z, VoxelCell cell)
	{
		Cells[x, y, z] = cell;
	}

	public VoxelMaskCell GetMaskCell(int x, int y, int z)
	{
		return MaskCells[x, y, z];
	}

	public void ClearMasks()
	{
		for (int x = 0; x < FixedSizeX; x++)
		{
			for (int y = 0; y < FixedSizeY; y++)
			{
				for (int z = 0; z < FixedSizeZ; z++)
				{
					MaskCells[x, y, z].Masks.Clear();
				}
			}
		}
	}
}
