using UnityEngine;

public class BattleBoard
{
    public Vector3Int OriginCell { get; }
    public int SizeX { get; }
    public int SizeY { get; }
    public int SizeZ { get; }
    public VoxelCell[,,] Voxels { get; }

    public BattleBoard(Vector3Int originCell, int sizeX, int sizeY, int sizeZ)
    {
        OriginCell = originCell;
        SizeX = Mathf.Max(0, sizeX);
        SizeY = Mathf.Max(0, sizeY);
        SizeZ = Mathf.Max(0, sizeZ);
        Voxels = new VoxelCell[SizeX, SizeY, SizeZ];
    }

    public bool TryGetVoxel(int x, int y, int z, out VoxelCell cell)
    {
        cell = default;
        if (x < 0 || x >= SizeX || y < 0 || y >= SizeY || z < 0 || z >= SizeZ)
        {
            return false;
        }

        cell = Voxels[x, y, z];
        return true;
    }

    public void SetVoxel(int x, int y, int z, VoxelCell cell)
    {
        if (x < 0 || x >= SizeX || y < 0 || y >= SizeY || z < 0 || z >= SizeZ)
        {
            return;
        }

        Voxels[x, y, z] = cell;
    }
}
