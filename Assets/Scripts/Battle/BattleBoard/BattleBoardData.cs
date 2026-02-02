using System;
using UnityEngine;

[Serializable]
public class BattleBoardData
{
    private Vector3Int originCell;
    private int sizeX;
    private int sizeY;
    private int sizeZ;
    private VoxelCell[,,] voxels;
    private BattleCell[,,] maskCells;

    public Vector3Int OriginCell => originCell;
    public int SizeX => sizeX;
    public int SizeY => sizeY;
    public int SizeZ => sizeZ;
    public VoxelCell[,,] Voxels => voxels;
    public BattleCell[,,] MaskCells => maskCells;

    public BattleBoardData()
    {
    }

    public BattleBoardData(Vector3Int originCell, int sizeX, int sizeY, int sizeZ)
    {
        Initialize(originCell, sizeX, sizeY, sizeZ);
    }

    public void Initialize(Vector3Int originCell, int sizeX, int sizeY, int sizeZ)
    {
        this.originCell = originCell;
        this.sizeX = Mathf.Max(0, sizeX);
        this.sizeY = Mathf.Max(0, sizeY);
        this.sizeZ = Mathf.Max(0, sizeZ);
        voxels = new VoxelCell[this.sizeX, this.sizeY, this.sizeZ];
        maskCells = new BattleCell[this.sizeX, this.sizeY, this.sizeZ];
        InitializeMaskCells();
    }

    public void EnsureInitialized()
    {
        if (voxels == null || voxels.GetLength(0) != sizeX || voxels.GetLength(1) != sizeY || voxels.GetLength(2) != sizeZ)
        {
            Initialize(originCell, sizeX, sizeY, sizeZ);
        }
    }

    public bool TryGetVoxel(int x, int y, int z, out VoxelCell cell)
    {
        cell = default;
        if (voxels == null || x < 0 || x >= SizeX || y < 0 || y >= SizeY || z < 0 || z >= SizeZ)
        {
            return false;
        }

        cell = Voxels[x, y, z];
        return true;
    }

    public void SetVoxel(int x, int y, int z, VoxelCell cell)
    {
        if (voxels == null || x < 0 || x >= SizeX || y < 0 || y >= SizeY || z < 0 || z >= SizeZ)
        {
            return;
        }

        Voxels[x, y, z] = cell;
    }

    public bool TryGetMaskCell(int x, int y, int z, out BattleCell cell)
    {
        cell = null;
        if (maskCells == null || x < 0 || x >= SizeX || y < 0 || y >= SizeY || z < 0 || z >= SizeZ)
        {
            return false;
        }

        cell = MaskCells[x, y, z];
        return cell != null;
    }

    public void AddMask(int x, int y, int z, BattleCellMask mask)
    {
        if (!TryGetMaskCell(x, y, z, out BattleCell cell))
        {
            return;
        }

        cell.AddMask(mask);
    }

    public void RemoveMask(int x, int y, int z, BattleCellMask mask)
    {
        if (!TryGetMaskCell(x, y, z, out BattleCell cell))
        {
            return;
        }

        cell.RemoveMask(mask);
    }

    public void ClearAllMasks()
    {
        if (maskCells == null)
        {
            return;
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                for (int z = 0; z < SizeZ; z++)
                {
                    MaskCells[x, y, z]?.ClearMasks();
                }
            }
        }
    }

    public void ClearMask(BattleCellMask mask)
    {
        if (maskCells == null)
        {
            return;
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                for (int z = 0; z < SizeZ; z++)
                {
                    MaskCells[x, y, z]?.RemoveMask(mask);
                }
            }
        }
    }

    public bool TryGetSurfaceY(int x, int z, int airId, VoxelRegistry registry, out int surfaceY)
    {
        surfaceY = -1;
        if (voxels == null || x < 0 || x >= SizeX || z < 0 || z >= SizeZ)
        {
            return false;
        }

        for (int y = SizeY - 1; y >= 0; y--)
        {
            int id = Voxels[x, y, z].Id;
            if (id == airId)
            {
                continue;
            }

            if (registry == null || !registry.TryGetVoxel(id, out Voxel voxel) || voxel == null)
            {
                surfaceY = y + 1;
                return surfaceY < SizeY;
            }

            if (voxel.Traversal == VoxelTraversal.Walkable)
            {
                continue;
            }

            surfaceY = y + 1;
            return surfaceY < SizeY;
        }

        return false;
    }

    private void InitializeMaskCells()
    {
        if (maskCells == null)
        {
            return;
        }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                for (int z = 0; z < SizeZ; z++)
                {
                    MaskCells[x, y, z] = new BattleCell();
                }
            }
        }
    }
}
