using System;

[Serializable]
public struct VoxelCell
{
    public int Id;
    public Orientation Orientation;

    public VoxelCell(int id)
        : this(id, Orientation.PositiveX)
    {
    }

    public VoxelCell(int id, Orientation orientation)
    {
        Id = id;
        Orientation = orientation;
    }
}
