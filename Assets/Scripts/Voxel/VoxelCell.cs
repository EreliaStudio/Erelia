using System;

[Serializable]
public struct VoxelCell
{
	public int Id;
	public Orientation Orientation;
	public FlipOrientation FlipOrientation;

	public VoxelCell(int id)
		: this(id, Orientation.PositiveX, FlipOrientation.PositiveY)
	{
	}

	public VoxelCell(int id, Orientation orientation)
		: this(id, orientation, FlipOrientation.PositiveY)
	{
	}

	public VoxelCell(int id, Orientation orientation, FlipOrientation flipOrientation)
	{
		Id = id;
		Orientation = orientation;
		FlipOrientation = flipOrientation;
	}
}
