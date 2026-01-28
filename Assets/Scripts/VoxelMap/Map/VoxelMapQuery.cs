using UnityEngine;

public static class VoxelMapQuery
{
    public static bool TryGetVoxelId(VoxelMap map, Vector3Int worldCell, out int id)
    {
        id = 0;
        if (map == null || map.Data == null)
        {
            return false;
        }

        ChunkCoord coord = new ChunkCoord(
            Mathf.FloorToInt((float)worldCell.x / Chunk.SizeX),
            Mathf.FloorToInt((float)worldCell.y / Chunk.SizeY),
            Mathf.FloorToInt((float)worldCell.z / Chunk.SizeZ));

        Chunk chunk = map.Data.GetOrCreateChunk(coord);
        if (chunk == null)
        {
            return false;
        }

        int localX = Mod(worldCell.x, Chunk.SizeX);
        int localY = Mod(worldCell.y, Chunk.SizeY);
        int localZ = Mod(worldCell.z, Chunk.SizeZ);

        id = chunk.Voxels[localX, localY, localZ].Id;
        return true;
    }

    public static bool TryGetVoxelCell(VoxelMap map, Vector3Int worldCell, out VoxelCell cell)
    {
        cell = default;
        if (map == null || map.Data == null)
        {
            return false;
        }

        ChunkCoord coord = new ChunkCoord(
            Mathf.FloorToInt((float)worldCell.x / Chunk.SizeX),
            Mathf.FloorToInt((float)worldCell.y / Chunk.SizeY),
            Mathf.FloorToInt((float)worldCell.z / Chunk.SizeZ));

        Chunk chunk = map.Data.GetOrCreateChunk(coord);
        if (chunk == null)
        {
            return false;
        }

        int localX = Mod(worldCell.x, Chunk.SizeX);
        int localY = Mod(worldCell.y, Chunk.SizeY);
        int localZ = Mod(worldCell.z, Chunk.SizeZ);

        cell = chunk.Voxels[localX, localY, localZ];
        return true;
    }

    public static bool TryGetVoxel(VoxelMap map, Vector3Int worldCell, out Voxel voxel, out int id)
    {
        voxel = null;
        id = 0;

        if (!TryGetVoxelId(map, worldCell, out id))
        {
            return false;
        }

        VoxelRegistry registry = map.Registry;
        if (registry == null)
        {
            return false;
        }

        return registry.TryGetVoxel(id, out voxel) && voxel != null;
    }

    public static bool IsFullVoxel(Voxel voxel)
    {
        return voxel is CubeVoxel;
    }

    private static int Mod(int value, int size)
    {
        int result = value % size;
        return result < 0 ? result + size : result;
    }
}
