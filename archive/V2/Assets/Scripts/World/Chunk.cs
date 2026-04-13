using System;

[Serializable]
public class Chunk : VoxelGrid
{
	public const int FixedSizeX = 16;
	public const int FixedSizeY = 16;
	public const int FixedSizeZ = 16;

	public Chunk() : base(FixedSizeX, FixedSizeY, FixedSizeZ)
	{
	}
}
