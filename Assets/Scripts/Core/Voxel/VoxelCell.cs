using System;

[Serializable]
public class VoxelCell
{
	public int Id = -1;
	public VoxelOrientation Orientation = VoxelOrientation.PositiveX;
	public VoxelFlipOrientation FlipOrientation = VoxelFlipOrientation.PositiveY;

	public bool IsEmpty => Id < 0;

	public VoxelCell()
	{
	}

	public VoxelCell(int id)
	{
		Id = id;
	}

	public VoxelCell(int id, VoxelOrientation orientation, VoxelFlipOrientation flipOrientation = VoxelFlipOrientation.PositiveY)
	{
		Id = id;
		Orientation = orientation;
		FlipOrientation = flipOrientation;
	}
}
