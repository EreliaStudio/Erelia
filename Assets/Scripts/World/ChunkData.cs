using System;

[Serializable]
public class ChunkData : VoxelGrid
{
	public const int FixedSizeX = 16;
	public const int FixedSizeY = 16;
	public const int FixedSizeZ = 16;

	public readonly VoxelMaskLayer MaskLayer;

	public ChunkData() : base(FixedSizeX, FixedSizeY, FixedSizeZ)
	{
		MaskLayer = new VoxelMaskLayer(FixedSizeX, FixedSizeY, FixedSizeZ);
	}

	public VoxelCell GetCell(int x, int y, int z)
	{
		return Cells[x, y, z];
	}

	public void SetCell(int x, int y, int z, VoxelCell cell)
	{
		Cells[x, y, z] = cell;
	}

	public void ClearMasks()
	{
		MaskLayer.Clear();
	}
}
